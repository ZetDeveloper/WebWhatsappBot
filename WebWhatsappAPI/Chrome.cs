using OpenQA.Selenium.Chrome;

namespace WebWhatsappBotCore.Chrome
{
    public class ChromeWApp : IWebWhatsappDriver
    {
        ChromeOptions chrome_options;
        /// <summary>
        /// Make a new ChromeWhatsapp Instance
        /// </summary>
        public ChromeWApp()
        {
            chrome_options = new ChromeOptions() ;
            chrome_options.LeaveBrowserRunning = false;
            chrome_options.AddExtension("dan.zip");
            chrome_options.AddArgument("--disable-web-security");
            chrome_options.AddArgument("--allow-running-insecure-content");
            chrome_options.AddArguments("--test-type");
            //chrome_options.AddArguments("--start-maximized");
            chrome_options.AddArguments("--disable-web-security");
            //chrome_options.AddArguments("--user-data-dir=D:\\chrome");
            chrome_options.AddArguments("--allow-file-access-from-files");
            chrome_options.AddArguments("--allow-running-insecure-content");
            chrome_options.AddArguments("--allow-cross-origin-auth-prompt");
            chrome_options.AddArguments("--allow-file-access");

        }

        /// <summary>
        /// Starts the chrome driver with settings
        /// </summary>
        public override void StartDriver()
        {
            HasStartedCheck();
            var drive = new ChromeDriver(chrome_options);
            base.StartDriver(drive);
        }
        /// <summary>
        /// Adds an extension
        /// Note: has to be before start of driver
        /// </summary>
        /// <param name="path"></param>
        public void AddExtension(string path)
        {
            HasStartedCheck();
            chrome_options.AddExtension(path);
        }
        /// <summary>
        /// Adds an base64 encoded extension
        /// Note: has to be before start of driver
        /// </summary>
        /// <param name="base64">the extension</param>
        public void AddExtensionBase64(string base64)
        {
            HasStartedCheck();
            chrome_options.AddEncodedExtension(base64);
        }
        /// <summary>
        /// Adds an argument when chrome is started
        /// Note: has to be before start of driver
        /// </summary>
        /// <param name="arg">the argument</param>
        public void AddStartArgument(params string[] arg)
        {
            HasStartedCheck();
            chrome_options.AddArguments(arg);
        }
    }
}
