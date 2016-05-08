using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace AlpariBinaryTransmitter
{
    class Program
    {




        static void Main(string[] args)
        {


            string Folder = @"D:\BinaryPositions";


            AlpariClient client = new AlpariClient(Folder);
            
            

           
                client.WaitConnection();

                client.ReadAndSendPositions();
                

                System.Threading.Thread.Sleep(5000);
                client.SendPosition("EURUSD", "Long", 1, 1.12186, 15);


                while (true)
                {
                    System.Threading.Thread.Sleep(1000);
                }

            


        }



       






    }
}
