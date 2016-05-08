using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace AlpariBinaryTransmitter
{
    public static class DebugLog
    {

        static string LogDirectory = "log";
        static StreamWriter File = null;
        static int Day = 0;

        public static void WriteLine(string text)
        {
            if (File == null)
            {
                CreateFile();
            }
            else if (DateTime.Now.Day != Day)
            {
                File.Close();
                CreateFile();
            }

            text = DateTime.Now.ToString() + "\t" + text;


            File.WriteLine(text);
            File.Flush();
            Console.WriteLine(text);



        }



        static void CreateFile()
        {

            
            Day = DateTime.Now.Day;

            string currentDir = LogDirectory + @"\" + DateTime.Now.ToString("yyyy-MM-dd");

            if (System.IO.Directory.Exists(currentDir) == false)
            {
                try
                {
                    System.IO.Directory.CreateDirectory(currentDir);
                }
                catch (Exception)
                {
                    return;
                }

            }

            File = new StreamWriter(currentDir + @"\" + DateTime.Now.ToString("HH.mm.ss") + ".log", true);


        }




    }
}
