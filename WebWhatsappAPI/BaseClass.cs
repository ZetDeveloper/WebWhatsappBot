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
using MongoDB.Driver;
using WebWhatsappBotCore.Models;
using System.Net;
using Newtonsoft.Json;

namespace WebWhatsappBotCore
{
    public abstract class IWebWhatsappDriver
    {
       
        public bool HasStarted { get; protected set; }
        protected IWebDriver driver;

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
            catch (Exception ex)
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
        ///  Base64ToImage
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


        bool useMongo = false;
        public string SendMessageRequest(String response)
        {
            Msg obj = JsonConvert.DeserializeObject<Msg>(response);

            Raise_RecievedMessage(obj.body, obj.from);

            MongoDBContext context = null;
            if (useMongo)
            {
                MongoDBContext.IsSSL = false;
                MongoDBContext.DatabaseName = "Whats";
                context = new MongoDBContext();
                context.Msg.InsertOne(obj);
            }

            return string.Format(response);
        }

        /// <summary>
        /// Scans for messages but only retreaves if person is in PeopleList
        /// </summary>
        public async void MessageScanner(bool useMongo = false)
        {
            this.useMongo = useMongo;

            MongoDBContext context = null;
            if (useMongo)
            {
                MongoDBContext.IsSSL = false;
                MongoDBContext.DatabaseName = "Whats";
                context = new MongoDBContext();

                
            }

           

            MicroServer microServer = new MicroServer(SendMessageRequest, "http://localhost:8080/zetdeveloper/");
            microServer.Run();

            IJavaScriptExecutor js = driver as IJavaScriptExecutor;
            js.ExecuteScript(Scripting.newMessageScan);
        }


        public void SendMessageToName(string name, string message)
        {
            IJavaScriptExecutor js = driver as IJavaScriptExecutor;
            Boolean? sendFast = false;
            int i = 0;
            do
            {
                sendFast = (Boolean?)js.ExecuteScript(Scripting.SendMessageByName, name, message) as Boolean?;
                Thread.Sleep(1000);
                i++;

                if (i >= 3)
                {
                    sendFast = true;
                }
            } while (sendFast == false);
        }

        public void SendMessageToNumber(string number, string message)
        {
            IJavaScriptExecutor js = driver as IJavaScriptExecutor;
            Boolean? sendFast = false;
            int i = 0;
            number = getIdFromNumber(number);
            do
            {
                sendFast = (Boolean?)js.ExecuteScript(Scripting.SendMessageByID, number, message) as Boolean?;
                Thread.Sleep(1000);
                i++;

                if (i >= 3)
                {
                    sendFast = true;
                }
            } while (sendFast == false);
        }

        public bool createGroupFast(string nombreGrupo, string[] numerosGrupo)
        {

            for (int i = 0; i < numerosGrupo.Length; i++)
            {
                numerosGrupo[i] = getIdFromNumber(numerosGrupo[i]);
            }


            IJavaScriptExecutor js = driver as IJavaScriptExecutor;
            Boolean? created = false;
            created = (Boolean?)js.ExecuteScript(Scripting.createGroup, nombreGrupo, string.Join(",", numerosGrupo)) as Boolean?;
            return true;
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

        String code = "";

        private string getIdFromNumber(string number)
        {
            if (number.Contains("@")) return number;
            return code + number + "@c.us";
        }

        public void SendMessageToNumber(string number, string name, string message)
        {
            IJavaScriptExecutor js = driver as IJavaScriptExecutor;

            Boolean? sendFast = (Boolean?)js.ExecuteScript(Scripting.SendMessageByID, getIdFromNumber(number), message) as Boolean?;

            
        }


        public void getGroups()
        {
            IJavaScriptExecutor js = driver as IJavaScriptExecutor;

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
            }
        }

        protected void HasStartedCheck(bool Invert = false)
        {
            if (HasStarted ^ Invert)
            {
                throw new NotSupportedException(String.Format("Driver has {0} already started", Invert ? "not" : ""));
            }
        }
    }
}