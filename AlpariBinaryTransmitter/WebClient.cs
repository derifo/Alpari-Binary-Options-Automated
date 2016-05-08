using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Collections;
using System.Data;
using System.Xml;
using System.Xml.Serialization;

namespace AlpariBinaryTransmitter
{
    public class WebClient
    {

        bool ProxyEnabled = false;
        string ProxyHost = "127.0.0.1";
        int ProxyPort = 8888;

        public CookieCollection Cookies = null;
        bool ShowCookies = false;
        
        string CookiesFilename = "cookies.xml";
        string UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:41.0) Gecko/20100101 Firefox/41.0";



        public WebClient(string _UserAgent = null, string _CookiesFilename = null)
        {
            if (_UserAgent != null) { UserAgent = _UserAgent; }
            if (_CookiesFilename != null) { CookiesFilename = _CookiesFilename; }

            Cookies = new CookieCollection();

            ImportCookies();
        
        
        
        }



        public string Request(string URL, SortedDictionary<string, string> PostData = null, string FilenameToSave = null)
        {
            string res = string.Empty;


            try
            {
                DebugLog.WriteLine("Sending web request to " + URL);

                if (ShowCookies)
                {
                    DebugLog.WriteLine("Cookies to send: ");
                    PrintCookies();
                }
                

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);

                request.Referer = URL;
                request.UserAgent = UserAgent;
                request.CookieContainer = new CookieContainer();
                request.AllowAutoRedirect = false;

                foreach (Cookie c in Cookies)
                {
                    request.CookieContainer.Add(c);
                }


                if (ProxyEnabled)
                {
                    WebProxy proxy = new WebProxy(ProxyHost, ProxyPort);
                    proxy.BypassProxyOnLocal = false;
                    request.Proxy = proxy;

                }


                request.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => { return true; };


                if (PostData == null)
                {
                    request.Method = "GET";
                }
                else
                {

                    string[] PostDataParams = new string[PostData.Count];
                    int i = 0;
                    foreach (KeyValuePair<string, string> p in PostData)
                    {
                        PostDataParams[i] = WebUtility.UrlEncode(p.Key) + "=" + WebUtility.UrlEncode(p.Value);
                        i++;
                    }

                    string PostDataString = string.Join("&", PostDataParams);
                    byte[] byteArray = Encoding.UTF8.GetBytes(PostDataString);


                    request.Method = "POST";
                    request.ContentType = "application/x-www-form-urlencoded";
                    request.ContentLength = byteArray.Length;

                    Stream postDataStream = request.GetRequestStream();
                    postDataStream.Write(byteArray, 0, byteArray.Length);
                    postDataStream.Close();



                }





                WebResponse response = request.GetResponse();
                HttpWebResponse HWResponse = (HttpWebResponse)response;
                MakeCookies(HWResponse.Cookies);



                Stream dataStream = response.GetResponseStream();

                if (FilenameToSave == null)
                {
                    StreamReader reader = new StreamReader(dataStream);
                    res = reader.ReadToEnd();
                    reader.Close();
                }
                else
                {
                    try
                    {
                        File.Delete(FilenameToSave);
                    }
                    catch (Exception) { }
            
                    Stream outputStream = File.OpenWrite(FilenameToSave);

                    byte[] buffer = new byte[4096];
                    int bytesRead;
                    do
                    {
                        bytesRead = dataStream.Read(buffer, 0, buffer.Length);
                        outputStream.Write(buffer, 0, bytesRead);

                    } while (bytesRead != 0);
                    
                    outputStream.Close();

                }
                
                
                dataStream.Close();
                response.Close();

            }
            catch (Exception e)
            {
                DebugLog.WriteLine("WebClient.Request error: " + e.Message);
                return string.Empty;
            }






            return res;
        }








        void ExportCookies()
        {

            try
            {
                List<CookieForExport> cookieList = new List<CookieForExport>();
                foreach (Cookie c in Cookies)
                {
                    CookieForExport ce = new CookieForExport();
                    ce.Name = c.Name;
                    ce.Value = c.Value;
                    ce.Path = c.Path;
                    ce.Domain = c.Domain;
                    cookieList.Add(ce);
                }

                System.Xml.Serialization.XmlSerializer writer = new System.Xml.Serialization.XmlSerializer(typeof(List<CookieForExport>));
                System.IO.FileStream file = System.IO.File.Create(CookiesFilename);
                writer.Serialize(file, cookieList);
                file.Close();
            }
            catch (Exception e)
            {
                DebugLog.WriteLine("Cookies export error: " + e.Message);
            }

        }




        void ImportCookies()
        {

            try
            {

                System.Xml.Serialization.XmlSerializer reader = new System.Xml.Serialization.XmlSerializer(typeof(List<CookieForExport>));
                System.IO.StreamReader file = new System.IO.StreamReader(CookiesFilename);

                List<CookieForExport> cookieList = (List<CookieForExport>)reader.Deserialize(file);
                file.Close();

                Cookies = new CookieCollection();
                foreach (CookieForExport ce in cookieList)
                {
                    Cookie c = new Cookie();
                    c.Name = ce.Name;
                    c.Value = ce.Value;
                    c.Domain = ce.Domain;
                    c.Path = ce.Path;
                    Cookies.Add(c);

                }




            }
            catch (Exception e)
            {
                if (ShowCookies)
                {
                    DebugLog.WriteLine("Cookies import error: " + e.Message);
                }
               
            }

            if (ShowCookies)
            {
                DebugLog.WriteLine("Cookies imported: ");
                PrintCookies();
            }
            



        }




        void MakeCookies(CookieCollection NewCookies)
        {
            bool updated = false;
            List<int> CookiesToRemove = new List<int>();

            foreach (Cookie c in NewCookies)
            {
                Cookie exisiting = GetCookieByName(c.Name);

                if (exisiting == null)
                {
                    Cookies.Add(c);
                    updated = true;
                }
                else
                {

                    exisiting.Value = c.Value;
                    updated = true;

                }
            }

            CookieCollection OldCookies = Cookies;
            Cookies = new CookieCollection();

            foreach (Cookie c in OldCookies)
            {
                if (c.Value != "deleted")
                {
                    Cookies.Add(c);
                }
                
            }


            ExportCookies();

            if (ShowCookies)
            {
                if (updated)
                {
                    DebugLog.WriteLine("Cookies updated: ");
                    PrintCookies();
                }
                else
                {
                    DebugLog.WriteLine("Cookies not updated");
                }


            }





        }

        Cookie GetCookieByName(string Name)
        {



            Cookie res = null;


            if (Cookies == null)
            {
                Cookies = new CookieCollection();
                return res;
            }



            foreach (Cookie c in Cookies)
            {
                if (c.Name == Name)
                {
                    res = c;
                    break;
                }

            }

            return res;
        }


        void PrintCookies()
        {
            DebugLog.WriteLine("--------------------------");

            foreach (Cookie c in Cookies)
            {
                DebugLog.WriteLine("Cookie: ---> " + c.ToString());

            }
            DebugLog.WriteLine("--------------------------");
        }





    }
}
