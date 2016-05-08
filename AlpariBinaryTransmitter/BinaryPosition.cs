using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;


namespace AlpariBinaryTransmitter
{
    [XmlInclude(typeof(BinaryPosition))]
    [Serializable]
    public class BinaryPosition
    {
        public DateTime OpenTime = DateTime.MinValue;
        public DateTime CloseTime = DateTime.MinValue;
        public double OpenPrice = 0;
        public double ClosePrice = 0;
        public string Type = string.Empty;
        public double Amount = 0;
        public int AlpariID = 0;
        public string OriginalID = string.Empty;
        public string Symbol = string.Empty;
        public int LifeTimeMinutes = 0;
        public string Status = string.Empty;

        public int PayoutWin = 0;
        public int PayoutLose = 0;
        public int PayoutParity = 0;

        public bool ResultIsWin = false;
        public bool ResultIsLoss = false;
        public bool ResultIsParity = false;


        public static void Export(BinaryPosition pos, string Filename)
        {

            try
            {

                System.Xml.Serialization.XmlSerializer writer = new System.Xml.Serialization.XmlSerializer(typeof(List<BinaryPosition>));

                System.IO.FileStream file = System.IO.File.Create(Filename);

                writer.Serialize(file, pos);
                file.Close();




            }
            catch (Exception) { }



        }

        public static BinaryPosition Import(string Filename)
        {

            BinaryPosition pos = null;

            try
            {

                System.Xml.Serialization.XmlSerializer reader = new System.Xml.Serialization.XmlSerializer(typeof(List<BinaryPosition>));
                System.IO.StreamReader file = new System.IO.StreamReader(Filename);

                pos = (BinaryPosition)reader.Deserialize(file);
                file.Close();

            }
            catch (Exception e) { }


            return pos;
        }


    }
}
