using DanielExample.Rest;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using WebWhatsappBotCore;

namespace DanielExample
{
    class Program
    {
        static void Main(string[] args)
        {
            Program x = new Program();
            x.MainS(null);
        }
        IWebWhatsappDriver _driver;
        void MainS(string[] args)
        {
            Console.WriteLine("1. Start");
            Console.WriteLine("2. Configure");
            string x = Console.ReadLine();
            switch (x)
            {
                case "Start":
                case "1":
                    Start(new WebWhatsappBotCore.Chrome.ChromeWApp());

                    break;
                case "Configure":
                case "2":
                   
                    break;
                default:
                    Main(null);
                    break;

            }
            Console.WriteLine("Done");
            Console.ReadKey();
        }

        void Start(IWebWhatsappDriver driver)
        {
            _driver = driver;
            driver.StartDriver();
            
            driver.OnMsgRecieved += OnMsgRec;
            Task.Run(() => driver.MessageScanner());

           

            Console.WriteLine("Use CTRL+C to exit");
            
        }

        private void OnMsgRec(IWebWhatsappDriver.MsgArgs arg)
        {
            Console.WriteLine(arg.Sender + " Wrote: " + arg.Msg + " at " + arg.TimeStamp);

            //get the name of pokemon by numer LOL
            if (arg.Msg.ToLower().StartsWith("/pokemon"))
            {
                var gitHubApi = RestService.For<IPokemonApi>("http://pokeapi.co/api/v2");

                String idPokemon = arg.Msg.ToLower().Split(' ')[1];

                var octocat = gitHubApi.GetPokemon(idPokemon);
               
                _driver.SendMessageToNumber(arg.Sender, "your pokemon is: " + octocat.Result.Forms[0].Name);
            }

            if (arg.Msg.ToLower().StartsWith("/group"))
            {
                string[] argr = arg.Msg.Split('-');

                String nombreGrupo = argr[1].Trim();
                string[] numerosGrupo = argr[2].Split(' ');
                numerosGrupo = numerosGrupo.Where(x => x != "").ToArray();

                _driver.createGroupFast(nombreGrupo, numerosGrupo);
                Thread.Sleep(500);
                _driver.SendMessageToName(nombreGrupo, nombreGrupo);


            }

            if (arg.Msg.ToLower().StartsWith("/number"))
            {
                _driver.SendMessageToNumber(arg.Msg.ToLower().Split('-')[1], arg.Msg.ToLower().Split('-')[2], arg.Msg.Split('-')[3]);
            }

          
        }
    }
}
