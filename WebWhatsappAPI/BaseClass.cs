using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.Events;
using Polly;
using OpenQA.Selenium.Interactions;
using System.Drawing.Imaging;
using OpenQA.Selenium.Support.UI;
using System.Runtime.InteropServices;
using System.Windows.Forms;



namespace WebWhatsappBotCore
{
    public abstract class IWebWhatsappDriver
    {
        /// <summary>
        /// Current settings
        /// </summary>
        public ChatSettings Settings = new ChatSettings();

        public bool HasStarted { get; protected set; }
        protected IWebDriver driver;

        /// <summary>
        /// A refrence to the Selenium WebDriver used; Selenium.WebDriver required
        /// </summary>
        public IWebDriver WebDriver
        {
            get
            {
                if (driver != null)
                {
                    return driver;
                }
                throw new NullReferenceException("Can't use WebDriver before StartDriver()");
            }
        }

        private EventFiringWebDriver _eventDriver;

        /// <summary>
        /// An event WebDriver from selenium; Selenium.Support package required
        /// </summary>
        public EventFiringWebDriver EventDriver
        {
            get
            {
                if (_eventDriver != null)
                {
                    return _eventDriver;
                }
                throw new NullReferenceException("Can't use Event Driver before StartDriver()");
            }
        }

        /// <summary>
        /// The settings of the an driver
        /// </summary>
        public class ChatSettings
        {
            public bool AllowGET = true; //TODO: implement(what?)
            public bool AutoSaveSettings = true; //Save Chatsettings and AutoSaveSettings generally on
            public bool SaveMessages = false; //TODO: implement
            public AutoSaveSettings SaveSettings = new AutoSaveSettings();
        }

        /// <summary>
        /// The save settings of the an driver
        /// </summary>
        public class AutoSaveSettings
        {
            public uint Interval = 3600; //every hour
            public ulong BackupInterval = 3600 * 24 * 7; //every week
            public bool Backups = false; //Save backups which can be manually restored //TODO: implement
            public bool SaveCookies = true; //Save Cookies with Save

            public IReadOnlyCollection<Cookie> SavedCookies; //For later usage
        }

        /// <summary>
        /// Arguments used by Msg event
        /// </summary>
        public class MsgArgs : EventArgs
        {
            public MsgArgs(string message, string sender)
            {
                TimeStamp = DateTime.Now;
                Msg = message;
                Sender = sender;
            }

            public string Msg { get; }

            public string Sender { get; }

            public DateTime TimeStamp { get; }
        }

        public delegate void MsgRecievedEventHandler(MsgArgs e);

        public event MsgRecievedEventHandler OnMsgRecieved;

        protected void Raise_RecievedMessage(string Msg, string Sender)
        {
            OnMsgRecieved?.Invoke(new MsgArgs(Msg, Sender));
        }


        /// <summary>
        /// Returns if the Login page and QR has loaded
        /// </summary>
        /// <returns></returns>
        public bool OnLoginPage()
        {
            try
            {
                if (driver.FindElement(By.XPath("//div[@class='qr-wrapper-container']")) != null)
                {
                    return true;
                }
            }
            catch(Exception ex)
            {
                return false;
            }
            return false;
        }

        /// <summary>
        /// Check's if we get the notification "PhoneNotConnected"
        /// </summary>
        /// <returns>bool; true if connected</returns>
        public bool IsPhoneConnected()
        {
            try
            {
                if (driver.FindElement(By.ClassName("icon-alert-phone")) != null)
                {
                    return false;
                }
            }
            catch
            {
                return true;
            }
            return true;
        }


        /// <summary>
        /// Gets raw QR string 
        /// </summary>
        /// <returns>sting(base64) of the image; returns null if not available</returns>
        private string GetQRImageRAW()
        {
            try
            {
                var qrcode = driver.FindElement(By.XPath("//img[@alt='Scan me!']"));
                var outp = qrcode.GetAttribute("src");
                outp = outp.Substring(22); //DELETE HEADER
                return outp;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets an C# image of the QR on the homepage
        /// </summary>
        /// <returns>QR image; returns null if not available</returns>
        public Image GetQrImage()
        {
            var pol = Policy<Image>
                .Handle<Exception>()
                .WaitAndRetry(new[]
                {
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(3)
                });

            return pol.Execute(() =>
            {
                var base64Image = GetQRImageRAW();

                if (base64Image == null)
                    throw new Exception("Image not found");

                return Base64ToImage(base64Image);
            });
        }

        /// <summary>
        /// https://stackoverflow.com/a/18827264
        /// </summary>
        /// <param name="base64String">Base 64 string</param>
        /// <returns>an image</returns>
        private Image Base64ToImage(string base64String)
        {
            // Convert base 64 string to byte[]
            var imageBytes = Convert.FromBase64String(base64String);
            // Convert byte[] to Image
            using (var ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
            {
                var image = Image.FromStream(ms, true);
                return image;
            }
        }

        /// <summary>
        /// Scans for messages but only retreaves if person is in PeopleList
        /// </summary>
        /// <param name="PeopleList">List of People to filter on(case-sensitive)</param>
        /// <param name="isBlackList"> is it a black- or whitelist (default whitelist)</param>
        /// <returns>Nothing</returns>
        public async void MessageScanner(string[] PeopleList, bool isBlackList = false)
        {
            while (true)
            {
                IReadOnlyCollection<IWebElement> unread = driver.FindElements(By.ClassName("unread"));
                foreach (IWebElement x in unread.ToArray())//just in case
                {
                    var y = x.FindElement(By.ClassName("ellipsify"));
                    if (PeopleList.Contains(y.GetAttribute("title")) != isBlackList)
                     {
                        try
                        {
                            x.Click();
                            await Task.Delay(200);
                            var Pname = "";
                            var message_text = GetLastestText(out Pname);
                            Raise_RecievedMessage(message_text, Pname);
                        }catch(Exception ex)
                        {

                        }
                    }
                }
                await Task.Delay(50); 
            }
        }

        /// <summary>
        /// Checks for messages which enables OnMsgRecieved event
        /// </summary>
        /// <returns>Nothing</returns>
        public async void MessageScanner()
        {
            while (true)
            {
                IReadOnlyCollection<IWebElement> unread = driver.FindElements(By.ClassName("unread-count"));
                if (unread.Count < 1)
                {
                    Thread.Sleep(50); //we don't wan't too much overhead
                    continue;
                }
                try
                {
                    unread.ElementAt(0).Click(); //Goto (first) Unread chat
                }
                catch (Exception)
                {
                } //DEAL with Stale elements
                await Task.Delay(200); //Let it load
                var Pname = "";
                var message_text = GetLastestText(out Pname);
                Raise_RecievedMessage(message_text, Pname);
            }
        }

        /// <summary>
        /// Starts selenium driver, while loading a save file
        /// Note: these functions don't make drivers
        /// </summary>
        /// <param name="driver">The driver</param>
        /// <param name="savefile">Path to savefile</param>
        public virtual void StartDriver(IWebDriver driver, string savefile)
        {
            StartDriver(driver);
            if (File.Exists(savefile))
            {
                Console.WriteLine("Trying to restore settings");
                Settings = Extensions.ReadFromBinaryFile<ChatSettings>("Save.bin");
                if (Settings.SaveSettings.SaveCookies)
                {
                    Settings.SaveSettings.SavedCookies.LoadCookies(driver);
                }
            }
            else
            {
                Settings = new ChatSettings();
            }
        }

        /// <summary>
        /// Starts selenium driver(only really used internally or virtually)
        /// Note: these functions don't make drivers
        /// </summary>
        public virtual void StartDriver()
        {
            //can't start a driver twice
            HasStartedCheck();
            HasStarted = true;
        }

        /// <summary>
        /// Starts selenium driver
        /// Note: these functions don't make drivers
        /// </summary>
        /// <param name="driver">The selenium driver</param>
        public virtual void StartDriver(IWebDriver driver)
        {
            this.driver = driver;
            driver.Navigate().GoToUrl("https://web.whatsapp.com");
            _eventDriver = new EventFiringWebDriver(WebDriver);
        }



        /// <summary>
        /// Saves to file
        /// </summary>
        protected virtual void AutoSave()
        {
            if (!Settings.AutoSaveSettings)
                return;
            if (Settings.SaveSettings.SaveCookies)
            {
                Settings.SaveSettings.SavedCookies = driver.Manage().Cookies.AllCookies;
            }
            Settings.WriteToBinaryFile("Save.bin");
            if (!Settings.SaveSettings.Backups) return;
            Directory.CreateDirectory("./Backups");
            Settings.WriteToBinaryFile($"./Backups/Settings_{DateTime.UtcNow:yyyy-MM-dd-HH-mm-ss}.bin");
        }

        /// <summary>
        /// Saves settings and more to file
        /// </summary>
        /// <param name="FileName">Path/Filename to make the file (e.g. save1.bin)</param>
        public virtual void Save(string FileName)
        {
            if (!Settings.AutoSaveSettings)
                return;
            if (Settings.SaveSettings.SaveCookies)
            {
                Settings.SaveSettings.SavedCookies = driver.Manage().Cookies.AllCookies;
            }
            Settings.WriteToBinaryFile(FileName);
            if (Settings.SaveSettings.Backups)
            {
                Directory.CreateDirectory("./Backups");
                Settings.WriteToBinaryFile(String.Format("./Backups/Settings_{0}.bin",
                    DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss")));
            }
        }

        /// <summary>
        /// Loads a file containing Settings and cookies
        /// </summary>
        /// <param name="FileName">path to Filename</param>
        public virtual void Load(string FileName)
        {
            Settings = Extensions.ReadFromBinaryFile<ChatSettings>(FileName);
            Settings.SaveSettings.SavedCookies.LoadCookies(driver);
        }

        /// <summary>
        /// Gets the latest test
        /// </summary>
        /// <param name="Pname">[Optional output] the person that send the message</param>
        /// <returns></returns>
        public string GetLastestText(out string Pname) //TODO: return IList<string> of all unread messages
        {
            var chat = driver.FindElement(By.ClassName("active"));
            var nametag = chat.FindElement(By.ClassName("ellipsify"));
            Pname = nametag.GetAttribute("title");
            IReadOnlyCollection<IWebElement> messages = null;
            try
            {
                messages = driver.FindElements(By.ClassName("msg"));
                    //.FindElements(By.XPath("*"));
            }
            catch (Exception)
            {
            } //DEAL with Stale elements
            var newmessage = messages.OrderBy(x => x.Location.Y).Reverse().First(); //Get latest message
            try
            {
                var message_text_raw = newmessage.FindElement(By.ClassName("selectable-text"));
                return Regex.Replace(message_text_raw.Text, "<!--(.*?)-->", "");
            }
            catch
            {
                
            }

            try
            {
                var message_text_raw = newmessage.FindElement(By.ClassName("image-thumb-body"));
                String logoSRC = message_text_raw.GetAttribute("src");

                IJavaScriptExecutor js = driver as IJavaScriptExecutor;

                string title = (string)js.ExecuteScript(@"
                    var y = document.getElementsByClassName('image-thumb-body');
                    var c = document.createElement('canvas');
                    var ctx = c.getContext('2d');
                    var img = null;

                    for(var i=0; i < y.length; i++){
                        if(y[i].src= '" + logoSRC + @"'){
                            img = y[i]
                        }
                    }

                    c.height=img.height * 2;
                    c.width=img.width * 2;
                    ctx.drawImage(img, 0, 0,img.width * 2, img.height * 2);
                    var base64String = c.toDataURL();
                    return base64String;
                ") as string;

                var base64 = title.Split(',').Last();
                using (var stream = new MemoryStream(Convert.FromBase64String(base64)))
                {
                    using (var bitmap = new Bitmap(stream))
                    {
                        var filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ImageName.png");
                        bitmap.Save(filepath, ImageFormat.Png);
                    }
                }

                return Regex.Replace(message_text_raw.Text, "<!--(.*?)-->", "");
            }
            catch
            {
                return "Error";
            }
        }

        /// <summary>
        /// Gets Messages from Active/person's conversaton
        /// <param>Order not garanteed</param>
        /// </summary>
        /// <param name="Pname">[Optional input] the person to get messages from</param>
        /// <returns>Unordered List of messages</returns>
        public IEnumerable<string> GetMessages(string Pname = null)
        {
            if (Pname != null)
            {
                SetActivePerson(Pname);
            }
            IReadOnlyCollection<IWebElement> messages = null;
            try
            {
                messages = driver.FindElement(By.ClassName("message-list")).FindElements(By.XPath("*"));
            }
            catch (Exception)
            {
            } //DEAL with Stale elements
            foreach (var x in messages)
            {
                var message_text_raw = x.FindElement(By.ClassName("selectable-text"));
                yield return Regex.Replace(message_text_raw.Text, "<!--(.*?)-->", "");
            }
        }

        /// <summary>
        /// Gets messages ordered "newest first"
        /// </summary>
        /// <param name="Pname">[Optional input] person to get messages from</param>
        /// <returns>Ordered List of string's</returns>
        public List<string> GetMessagesOrdered(string Pname = null)
        {
            if (Pname != null)
            {
                SetActivePerson(Pname);
            }
            IReadOnlyCollection<IWebElement> messages = null;
            try
            {
                messages = driver.FindElement(By.ClassName("message-list")).FindElements(By.XPath("*"));
            }
            catch (Exception)
            {
            } //DEAL with Stale elements
            var outp = new List<string>();
            foreach (var x in messages.OrderBy(x => x.Location.Y).Reverse())
            {
                var message_text_raw = x.FindElement(By.ClassName("selectable-text"));
                outp.Add(Regex.Replace(message_text_raw.Text, "<!--(.*?)-->", ""));
            }
            return outp;
        }

        /// <summary>
        /// Send message to person
        /// </summary>
        /// <param name="message">string to send</param>
        /// <param name="person">person to send to (if null send to active)</param>
        public void SendMessage(string message, string person = null)
        {
            if (person != null)
            {
                SetActivePerson(person);
            }
            var outp = message.ToWhatsappText();
            var chatbox = driver.FindElement(By.ClassName("block-compose"));

            Actions actions = new Actions(driver);
            actions.MoveToElement(chatbox);
            actions.Click();
            actions.SendKeys(message);
           
            actions.SendKeys(OpenQA.Selenium.Keys.Enter);
            actions.Build().Perform();

            /*chatbox.Click();
            
            chatbox.SendKeys(outp);
            chatbox.SendKeys(Keys.Enter);*/
        }

        public void SetActivePersonFirst(String name)
        {
            IReadOnlyCollection<IWebElement> AllChats = driver.FindElements(By.ClassName("chat-title"));
            foreach (var we in AllChats)
            {
                var Title = we.FindElement(By.ClassName("emojitext"));
                if (Title.GetAttribute("title").ToLower() == name.ToLower())
                {
                    Title.Click();
                    Thread.Sleep(300);
                    return;
                }
            }
            Console.WriteLine("Can't find person, not sending");
        }


        public void SendMessageToNumber(string number, string name, string message)
        {
            var chatbox = driver.FindElement(By.ClassName("icon-search-morph"));
            Actions actions = new Actions(driver);
            actions.MoveToElement(chatbox);
            actions.Click();
            actions.Build().Perform();

            var chatbox2 = driver.FindElement(By.CssSelector("#side > div.chatlist-panel-search > div > label > input"));
            //var javascriptDriver = this.driver as IJavaScriptExecutor;
           
            
            Thread.Sleep(500);

            //javascriptDriver.ExecuteScript("document.getElementsByClassName('input-search copyable-text selectable-text')[0].value = '" + number + "'");

            actions.MoveToElement(chatbox2);
            actions.Click();
            actions.SendKeys(number);
            
            actions.Build().Perform();

            Thread.Sleep(1500);

            SetActivePersonFirst(name);

            SendMessage(message);
        }


        private void DropImage(string dropBoxId, string filePath)
        {
            var javascriptDriver = this.driver as IJavaScriptExecutor;
            var inputId = dropBoxId + "FileUpload";

            // append input to HTML to add file path
            javascriptDriver.ExecuteScript(inputId + " = window.$('<input id=\"" + inputId + "\"/>').attr({type:'file'}).appendTo('body');");
            this.driver.FindElement(By.Id(inputId)).SendKeys(filePath);

            // fire mock event pointing to inserted file path
            javascriptDriver.ExecuteScript("e = $.Event('drop'); e.originalEvent = {dataTransfer : { files : " + inputId + ".get(0).files } }; $('#" + dropBoxId + "').trigger(e);");
        }

        void DropFile(IWebElement target, string filePath, int offsetX = 0, int offsetY = 0)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException(filePath);

            //IWebDriver driver = ((RemoteWebElement)target).WrappedDriver;
            IJavaScriptExecutor jse = (IJavaScriptExecutor)driver;
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));

            string JS_DROP_FILE = @"
        var target = arguments[0],
            offsetX = arguments[1],
            offsetY = arguments[2],
            document = target.ownerDocument || document,
            window = document.defaultView || window;

        var input = document.createElement('INPUT');
        input.type = 'file';
        input.style.display = 'none';
        input.onchange = function () {
          target.scrollIntoView(true);

          var rect = target.getBoundingClientRect(),
              x = rect.left + (offsetX || (rect.width >> 1)),
              y = rect.top + (offsetY || (rect.height >> 1)),
              dataTransfer = { files: this.files };

          ['dragenter', 'dragover', 'drop'].forEach(function (name) {
            var evt = document.createEvent('MouseEvent');
            evt.initMouseEvent(name, !0, !0, window, 0, 0, 0, x, y, !1, !1, !1, !1, 0, null);
            evt.dataTransfer = dataTransfer;
            target.dispatchEvent(evt);
          });

          setTimeout(function () { document.body.removeChild(input); }, 25);
        };
        document.body.appendChild(input);
        return input;
        ";

            IWebElement input = (IWebElement)jse.ExecuteScript(JS_DROP_FILE, target, offsetX, offsetY);
            input.SendKeys(filePath);
            wait.Until(ExpectedConditions.StalenessOf(input));
        }

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        public void UploadFile(string fileName)
        {
            var dialogHWnd = FindWindow(null, "Abrir");
            var setFocus = true;
            if (setFocus)
            {
                Thread.Sleep(500);
                SendKeys.SendWait(fileName);
                Thread.Sleep(5000);
                SendKeys.SendWait("{RIGHT}");
                SendKeys.SendWait("{ENTER}");
                //SendKeys.SendWait("{TAB}");
                //SendKeys.Send("^A");
                //SendKeys.SendWait("{ENTER}");
            }
        }

        public void SendMessageImage(string message, string person = null)
        {
            if (person != null)
            {
                SetActivePerson(person);
            }
            var outp = message.ToWhatsappText();
            var chatbox = driver.FindElement(By.CssSelector("#main > header > div.pane-chat-controls > div > div:nth-child(2) > div"));
            //DropFile(chatbox, @"C:\Users\Zet\Pictures\credencial\11937449_1496953950602168_2839239085099833248_n.jpg");

            Actions actions = new Actions(driver);
            actions.MoveToElement(chatbox);
            actions.Click();
            //actions.ContextClick();
            actions.Build().Perform();
            Thread.Sleep(500);

            var chatbox2 = driver.FindElement(By.XPath("//*[@id='main']/header/div[3]/div/div[2]/span/div/div/ul/li[1]/button"));
            actions.MoveToElement(chatbox2);
            actions.Click();
            //actions.SendKeys(@"C:\Users\Zet\Pictures\credencial\11937449_1496953950602168_2839239085099833248_n.jpg");
            actions.Build().Perform();

            UploadFile(@"C:\Users\Zet\Pictures\ano.jpg");
            /*actions.SendKeys(message);
            actions.SendKeys(Keys.Enter);
            actions.Build().Perform();*/

            /*chatbox.Click();
            
            chatbox.SendKeys(outp);
            chatbox.SendKeys(Keys.Enter);*/
        }

        /// <summary>
        /// Set's Active person/chat by name
        /// <para>useful for default chat type of situations</para>
        /// </summary>
        /// <param name="person">the person to set active</param>
        public void SetActivePerson(string person)
        {
            IReadOnlyCollection<IWebElement> AllChats = driver.FindElements(By.ClassName("chat-title"));
            foreach (var we in AllChats)
            {
                var Title = we.FindElement(By.ClassName("emojitext"));
                if (Title.GetAttribute("title") == person)
                {
                    Title.Click();
                    Thread.Sleep(300);
                    return;
                }
            }
            Console.WriteLine("Can't find person, not sending");
        }

        /// <summary>
        /// Get's all chat names so you can make a selection menu
        /// </summary>
        /// <returns>Unorderd string 'Enumerable'</returns>
        public IEnumerable<string> GetAllChatNames()
        {
            HasStartedCheck();
            IReadOnlyCollection<IWebElement> AllChats = driver.FindElement(By.ClassName("chatlist")).FindElements(By.ClassName("chat-title"));
            foreach (var we in AllChats)
            {
                var Title = we.FindElement(By.ClassName("emojitext"));
                yield return Title.GetAttribute("title");
            }
        }

        /// <summary>
        /// only for internal use; throws exception if the driver has already started(can be inverted)
        /// </summary>
        protected void HasStartedCheck(bool Invert = false)
        {
            if (HasStarted ^ Invert)
            {
                throw new NotSupportedException(String.Format("Driver has {0} already started", Invert ? "not" : ""));
            }
        }
    }
}