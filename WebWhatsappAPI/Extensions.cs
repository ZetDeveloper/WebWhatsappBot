using System.Collections.Generic;

namespace WebWhatsappBotCore
{
    public static class Extensions
    {
        public static string ToWhatsappText(this string inp) //Makes sure newlines don't submit
        {
            return inp.Replace("\n", (OpenQA.Selenium.Keys.Shift + OpenQA.Selenium.Keys.Enter + OpenQA.Selenium.Keys.LeftShift))
                .Replace(':', (char)0xFF1A); //colon to full-width colon(prevents random emoji)
        }

        /// <summary>
        /// Writes the given object instance to a binary file.
        /// <para>Object type (and all child types) must be decorated with the [Serializable] attribute.</para>
        /// <para>To prevent a variable from being serialized, decorate it with the [NonSerialized] attribute; cannot be applied to properties.</para>
        /// </summary>
        /// <typeparam name="T">The type of object being written to the XML file.</typeparam>
        /// <param name="filePath">The file path to write the object instance to.</param>
        /// <param name="objectToWrite">The object instance to write to the XML file.</param>
        /// <param name="append">If false the file will be overwritten if it already exists. If true the contents will be appended to the file.</param>
        public static void WriteToBinaryFile<T>(this T objectToWrite, string filePath, bool append = false)
        {
            using (System.IO.Stream stream = System.IO.File.Open(filePath, append ? System.IO.FileMode.Append : System.IO.FileMode.Create))
                new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter().Serialize(stream, objectToWrite);
        }

        /// <summary>
        /// Reads an object instance from a binary file.
        /// </summary>
        /// <typeparam name="T">The type of object to read from the XML.</typeparam>
        /// <param name="filePath">The file path to read the object instance from.</param>
        /// <returns>Returns a new instance of the object read from the binary file.</returns>
        public static T ReadFromBinaryFile<T>(string filePath)
        {
            using (System.IO.Stream stream = System.IO.File.Open(filePath, System.IO.FileMode.Open))
                return (T)(new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter().Deserialize(stream));
        }

        /// <summary>
        /// Load "Saved" Cookies into browser
        /// </summary>
        /// <param name="Cookies">An read-only collection of cookies</param>
        /// <param name="driver">The browser driver</param>
        public static void LoadCookies(this IReadOnlyCollection<OpenQA.Selenium.Cookie> Cookies, OpenQA.Selenium.IWebDriver driver)
        {
            foreach (OpenQA.Selenium.Cookie cookie in Cookies)
            {
                driver.Manage().Cookies.AddCookie(cookie);
            }
        }


    }
}
