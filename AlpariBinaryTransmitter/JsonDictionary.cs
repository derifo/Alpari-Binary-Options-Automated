using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Dynamic;
using Newtonsoft.Json;
using Newtonsoft;
using Newtonsoft.Json.Schema;
using System.IO;


namespace AlpariBinaryTransmitter
{
    public static class JsonDictionary
    {


        public static SortedDictionary<string, object> Deserialize(string json)
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(json));
            SortedDictionary<string, object> result = ReadObject(reader);
            result = (SortedDictionary<string, object>)result["0"];
            return result;

        }


        static SortedDictionary<string, object> ReadObject(JsonTextReader reader)
        {
            SortedDictionary<string, object> result = new SortedDictionary<string, object>();

            int i = 0;
            string Key = string.Empty;
            object Value = null;


            while (reader.Read())
            {
                string TokenType = reader.TokenType.ToString();

                if (TokenType == "PropertyName")
                {
                    Key = reader.Value.ToString();
                }
                else if (TokenType == "EndArray" || TokenType == "EndObject")
                {
                    break;
                }
                else
                {
                    if (TokenType == "StartObject" || TokenType == "StartArray")
                    {
                        Value = ReadObject(reader);
                    }
                    else
                    {
                        Value = reader.Value;
                    }

                    if (string.IsNullOrEmpty(Key)) { Key = i.ToString(); }

                    if (Value != null)
                    {
                        result.Add(Key, Value);
                    }


                    i++;
                    Key = string.Empty;
                    Value = null;
                }

            }


            return result;
        }







    }

}
