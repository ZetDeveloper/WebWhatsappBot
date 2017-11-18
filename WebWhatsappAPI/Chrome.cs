using OpenQA.Selenium.Chrome;

namespace WebWhatsappBotCore.Chrome
{
    public class ChromeWApp : IWebWhatsappDriver
    {
        ChromeOptions ChromeOP;
        /// <summary>
        /// Make a new ChromeWhatsapp Instance
        /// </summary>
        public ChromeWApp()
        {
            ChromeOP = new ChromeOptions() { LeaveBrowserRunning = false};
        }

        /// <summary>
        /// Starts the chrome driver with settings
        /// </summary>
        public override void StartDriver()
        {
            HasStartedCheck();
            var drive = new ChromeDriver(ChromeOP);
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
            ChromeOP.AddExtension(path);
        }
        /// <summary>
        /// Adds an base64 encoded extension
        /// Note: has to be before start of driver
        /// </summary>
        /// <param name="base64">the extension</param>
        public void AddExtensionBase64(string base64)
        {
            HasStartedCheck();
            ChromeOP.AddEncodedExtension(base64);
        }
        /// <summary>
        /// Adds an argument when chrome is started
        /// Note: has to be before start of driver
        /// </summary>
        /// <param name="arg">the argument</param>
        public void AddStartArgument(params string[] arg)
        {
            HasStartedCheck();
            ChromeOP.AddArguments(arg);
        }
    }
}
