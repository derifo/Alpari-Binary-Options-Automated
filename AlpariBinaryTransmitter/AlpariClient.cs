using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Collections;
using System.Data;
using System.Xml;
using System.Xml.Serialization;
using Akumu.Antigate;
using Json;

namespace AlpariBinaryTransmitter
{
    [XmlInclude(typeof(CookieForExport))]
    [Serializable]
    public class CookieForExport
    {

        public string Name;
        public string Value;
        public string Domain;
        public string Path;


    }





    public class AlpariClient
    {



        bool Demo = false;


        string Username = "your_alpari_email";
        string Password = "your_alpari_pwd";

        

        string AntiCaptchaKey = "your_anti_captcha_key";


        string MainFolder;
        string RequestsToSendFolder;
        string RequestResultsFolder;
        string ClosedPositionsFolder;


        string Token;

        int TradeGroupID;
        int CurrencyID;
        int AccountID;

        double _Balance;
        public double Balance { get { return _Balance; } } 
        

        WebSocket ws;
        WebClient WebClient;

        public bool Connected = false;
        bool Error = false;
        
        bool NewPositionInProgress = false;

        bool NewPositionError = false;
        string NewPositionErrorMessage = string.Empty;
        double NewPositionOpenPrice = 0;
        bool NewPositionResponseReceived = false;
        int NewPositionID = 0;

        public List<BinaryPosition> OpenPositions;

        TradeSettings TradeSettings;



        int _RequestID = 0;
        int RequestID { get { _RequestID++; return _RequestID; } }

        public AlpariClient(string Folder, string _Username, string _Password, string _UserAgent = null)
        {
            Username = _Username;
            Password = _Password;

            MainFolder = Folder;
            RequestsToSendFolder = Folder + @"\RequestsToSend";
            RequestResultsFolder = Folder + @"\RequestResults";
            ClosedPositionsFolder = Folder + @"\ClosedPositions";

            TradeSettings = new TradeSettings();

            WebClient = new WebClient(_UserAgent);
            OpenPositions = new List<BinaryPosition>();
            

        }



        public AlpariClient(string Folder)
        {
            MainFolder = Folder;
            RequestsToSendFolder = Folder + @"\RequestsToSend";
            RequestResultsFolder = Folder + @"\RequestResults";
            ClosedPositionsFolder = Folder + @"\ClosedPositions";

            TradeSettings = new TradeSettings();

            WebClient = new WebClient();
            OpenPositions = new List<BinaryPosition>();
            
            
        }

        public void WaitConnection()
        {

            while (!Connected)
            {
                Connect();
                System.Threading.Thread.Sleep(1000);
            }


        }



        public bool Connect()
        {
            DebugLog.WriteLine("Alpari client starting!");


            if (!Demo)
            {
                

                if (!LoginToWebsite())
                {
                    DebugLog.WriteLine("Error logging to website");
                    return false;
                }
                


                if (!GetToken())
                {
                    DebugLog.WriteLine("No token");
                    return false;
                }


            }
            else
            {

                DebugLog.WriteLine("DEMO MODE");
                Token = "token";

            }






            //return true;

            using (ws = new WebSocket("wss://my.alpari.ru/bo_ws/"))
            {

                ws.OnMessage += (sender, e) =>
                {

                    //DebugLog.WriteLine("Server says: " + e.Data);

                    var Data = JsonParser.Deserialize(e.Data);
                    OnMessage(Data, e.Data);
                    

                };

                ws.OnClose += (sender, e) =>
                {
                    Connected = false;
                    DebugLog.WriteLine("CONNECTION CLOSED");

                };

                ws.OnError += (sender, e) =>
                {

                    DebugLog.WriteLine("Error: " + e.Message);
                    Connected = false;
                    
                };

            }


            foreach (Cookie c in WebClient.Cookies)
            {
                ws.SetCookie(new WebSocketSharp.Net.Cookie(c.Name, c.Value, c.Path, c.Domain));
            }



            ws.Connect();


            ws.Send("{\"type\":\"REQUEST\",\"action\":\"auth\",\"body\":{\"token\":\"" + Token + "\"},\"rid\":" + RequestID + "}");
            ws.Send("{\"type\":\"REQUEST\",\"action\":\"time\",\"body\":{},\"rid\":" + RequestID + "}");

            while (!Connected && !Error)
            {
                System.Threading.Thread.Sleep(1000);
            }

            if (Connected)
            {
                ws.Send("{\"type\":\"REQUEST\",\"action\":\"account\",\"body\":{},\"rid\":" + RequestID + "}");
                ws.Send("{\"type\":\"REQUEST\",\"action\":\"+account\",\"body\":{},\"rid\":" + RequestID + "}");
                ws.Send("{\"type\":\"REQUEST\",\"action\":\"dictionary\",\"body\":{},\"rid\":" + RequestID + "}");
                ws.Send("{\"type\":\"REQUEST\",\"action\":\"+trade_state\",\"body\":{},\"rid\":" + RequestID + "}");
                ws.Send("{\"type\":\"REQUEST\",\"action\":\"trade_state\",\"body\":{},\"rid\":" + RequestID + "}");
                ws.Send("{\"type\":\"REQUEST\",\"action\":\"trade_settings\",\"body\":{},\"rid\":" + RequestID + "}");
                ws.Send("{\"type\":\"REQUEST\",\"action\":\"+trade_settings\",\"body\":{},\"rid\":" + RequestID + "}");

                ws.Send("{\"type\":\"REQUEST\",\"action\":\"+rate\",\"body\":{\"asset_id\":1},\"rid\":" + RequestID + "}");
                ws.Send("{\"type\":\"REQUEST\",\"action\":\"rates/history\",\"body\":{\"asset_id\":1,\"bar_size\":15,\"limit\":181},\"rid\":" + RequestID + "}");
                ws.Send("{\"type\":\"REQUEST\",\"action\":\"rates/last\",\"body\":{},\"rid\":" + RequestID + "}");



                ws.Send("{\"type\":\"REQUEST\",\"action\":\"options\",\"body\":{},\"rid\":" + RequestID + "}");
                ws.Send("{\"type\":\"REQUEST\",\"action\":\"+option\",\"body\":{},\"rid\":" + RequestID + "}");

                ws.Send("{\"type\":\"REQUEST\",\"action\":\"traders_choice\",\"body\":{\"option_type_id\":\"1\",\"asset_id\":\"1\",\"time_frame_id\":\"3\"},\"rid\":" + RequestID + "}");
                ws.Send("{\"type\":\"REQUEST\",\"action\":\"+traders_choice\",\"body\":{\"option_type_id\":\"1\",\"asset_id\":\"1\",\"time_frame_id\":\"3\"},\"rid\":" + RequestID + "}");


                ws.Send("{\"type\":\"REQUEST\",\"action\":\"report\",\"body\":{\"limit\":10,\"offset\":0},\"rid\":" + RequestID + "}");

                Task t = Task.Run(() =>
                {
                    while (true)
                    {
                        WaitConnection();
                        ws.Send("{\"type\":\"REQUEST\",\"action\":\"time\",\"body\":{},\"rid\":" + RequestID + "}");
                        System.Threading.Thread.Sleep(3000);

                    }



                } );


            }




            




            if (Connected)
            {
                DebugLog.WriteLine("\r\n**********************************************\r\nAlpari Websocket connected!!!\r\n**********************************************");
                return true;
            }




            return false;



        }





        void OnMessage(dynamic Data, string JsonSource)
        {
            if (Data.action != "rate" && Data.action != "time" && Data.action != "traders_choice")
            {
                DebugLog.WriteLine("Server says: " + JsonSource);
            }

            if (Data.type == "RESPONSE")
            {

                if (Data.action == "auth")
                {
                    CheckWSAuth(Data);
                }
                if (Data.action == "option/buy")
                {
                    OnNewPositionResponse(Data);
                }
                if (Data.action == "dictionary")
                {
                    SetDictionary(Data);
                }

                


            }

            if (Data.type == "EVENT")
            {

                if (Data.action == "account")
                {
                    SetAccountInfo(Data);
                }
                if (Data.action == "rate")
                {
                    OnTick(Data);
                }
                if (Data.action == "option")
                {
                    OnOptionInfoEvent(Data);

                }
            }


            if (Data.action == "trade_settings")
            {
                SetTradeSettings(JsonSource);
            }



        }


        void OnOptionInfoEvent(dynamic Data)
        {

            try
            {
                int AlpariID = (int)Data.body.id;
                int ResultID = (int)Data.body.result_id;

                BinaryPosition pos = FindPositionByAlpariID(AlpariID);

                if (ResultID == TradeSettings.OptionResultsDictionary["win"])
                {
                    pos.ResultIsWin = true;
                }
                else if (ResultID == TradeSettings.OptionResultsDictionary["loss"])
                {
                    pos.ResultIsLoss = true;
                }
                else if (ResultID == TradeSettings.OptionResultsDictionary["loss"])
                {
                    pos.ResultIsParity = true;
                }

                pos.Status = "Closed";
                pos.ClosePrice = (double)Data.body.expiry_rate.value;
                pos.CloseTime = DateTime.UtcNow;

                BinaryPosition.Export(pos, ClosedPositionsFolder + "/" + pos.OriginalID);

                OpenPositions.Remove(pos);



            }
            catch (Exception e) { }


        }






        BinaryPosition FindPositionByAlpariID(int AlpariID)
        {
            BinaryPosition pos = null;

            foreach (BinaryPosition p in OpenPositions)
            {
                if (p.AlpariID == AlpariID)
                {
                    pos = p;
                    break;
                }
            }



            return pos;
        }




        void OnTick(dynamic Data)
        {
            try
            {
                Console.WriteLine("Tick: " + Data.body.value);

            }
            catch (Exception e) { DebugLog.WriteLine("Tick reading error: " + e.Message); }
        }



        void SetTradeSettings(string JsonSource)
        {

            #region json_example2
            /*
             * {"type":"RESPONSE","action":"trade_settings","body":{"22":5000,"23":5000,"settings":{"1":{"11":{"8":[175],"5":[160],"6":[165],"7":[170]},"12":{"1":[160],"2":[165],"3":[165],"4":[170],"5":[175],"6":[180],"7":[185],"8":[200]},"13":{"8":[175],"5":[160],"6":[165],"7":[170]},"14":{"8":[175],"5":[160],"6":[165],"7":[170]},"15":{"8":[175],"5":[160],"6":[165],"7":[170]},"16":{"8":[175],"5":[160],"6":[165],"7":[170]},"17":{"8":[175],"5":[160],"6":[165],"7":[170]},"18":{"8":[175],"5":[160],"6":[165],"7":[170]},"19":{"8":[175],"5":[160],"6":[165],"7":[170]},"1":{"1":[160],"2":[165],"3":[165],"4":[170],"5":[175],"6":[180],"7":[185],"8":[200]},"2":{"1":[160],"2":[165],"3":[165],"4":[170],"5":[175],"6":[180],"7":[185],"8":[200]},"3":{"1":[160],"2":[165],"3":[165],"4":[170],"5":[175],"6":[180],"7":[185],"8":[200]},"4":{"1":[160],"2":[165],"3":[165],"4":[170],"5":[175],"6":[180],"7":[185],"8":[200]},"5":{"1":[160],"2":[165],"3":[165],"4":[170],"5":[175],"6":[180],"7":[185],"8":[200]},"6":{"1":[160],"2":[165],"3":[165],"4":[170],"5":[175],"6":[180],"7":[185],"8":[200]},"7":{"1":[160],"2":[165],"3":[165],"4":[170],"5":[175],"6":[180],"7":[185],"8":[200]},"20":{"1":[160],"2":[165],"3":[165],"4":[170],"5":[175],"6":[180],"7":[185],"8":[200]},"10":{"1":[160],"2":[165],"3":[165],"4":[170],"5":[175],"6":[180],"7":[185],"8":[200]},"21":{"1":[160],"2":[165],"3":[165],"4":[170],"5":[175],"6":[180],"7":[185],"8":[200]}},"2":{"11":{"8":[175],"5":[160],"6":[165],"7":[170]},"12":{"8":[200],"3":[170],"4":[175],"5":[180],"6":[185],"7":[190]},"13":{"8":[175],"5":[160],"6":[165],"7":[170]},"14":{"8":[175],"5":[160],"6":[165],"7":[170]},"15":{"8":[175],"5":[160],"6":[165],"7":[170]},"16":{"8":[175],"5":[160],"6":[165],"7":[170]},"17":{"8":[175],"5":[160],"6":[165],"7":[170]},"18":{"8":[175],"5":[160],"6":[165],"7":[170]},"19":{"8":[175],"5":[160],"6":[165],"7":[170]},"1":{"8":[200],"3":[170],"4":[175],"5":[180],"6":[185],"7":[190]},"2":{"8":[200],"3":[170],"4":[175],"5":[180],"6":[185],"7":[190]},"3":{"8":[200],"3":[170],"4":[175],"5":[180],"6":[185],"7":[190]},"4":{"8":[200],"3":[170],"4":[175],"5":[180],"6":[185],"7":[190]},"5":{"8":[200],"3":[170],"4":[175],"5":[180],"6":[185],"7":[190]},"6":{"8":[200],"3":[170],"4":[175],"5":[180],"6":[185],"7":[190]},"7":{"8":[200],"4":[175],"5":[180],"6":[185],"7":[190]},"20":{"8":[200],"3":[170],"4":[175],"5":[180],"6":[185],"7":[190]},"10":{"8":[200],"3":[170],"4":[175],"5":[180],"6":[185],"7":[190]},"21":{"8":[200],"3":[170],"4":[175],"5":[180],"6":[185],"7":[190]}},"3":{"11":{"8":[185],"5":[170],"6":[175],"7":[180]},"12":{"8":[200],"3":[175],"4":[180],"5":[185],"6":[190],"7":[195]},"13":{"8":[185],"5":[170],"6":[175],"7":[180]},"14":{"8":[185],"5":[170],"6":[175],"7":[180]},"15":{"8":[185],"5":[170],"6":[175],"7":[180]},"16":{"8":[185],"5":[170],"6":[175],"7":[180]},"17":{"8":[185],"5":[170],"6":[175],"7":[180]},"18":{"8":[185],"5":[170],"6":[175],"7":[180]},"19":{"8":[185],"5":[170],"6":[175],"7":[180]},"1":{"8":[200],"3":[175],"4":[180],"5":[185],"6":[190],"7":[195]},"2":{"8":[200],"3":[175],"4":[180],"5":[185],"6":[190],"7":[195]},"3":{"8":[200],"3":[175],"4":[180],"5":[185],"6":[190],"7":[195]},"4":{"8":[200],"3":[175],"4":[180],"5":[185],"6":[190],"7":[195]},"5":{"8":[200],"3":[175],"4":[180],"5":[185],"6":[190],"7":[195]},"6":{"8":[200],"3":[175],"4":[180],"5":[185],"6":[190],"7":[195]},"7":{"8":[200],"3":[175],"4":[180],"5":[185],"6":[190],"7":[195]},"20":{"8":[200],"3":[175],"4":[180],"5":[185],"6":[190],"7":[195]},"10":{"8":[200],"3":[175],"4":[180],"5":[185],"6":[190],"7":[195]},"21":{"8":[200],"3":[175],"4":[180],"5":[185],"6":[190],"7":[195]}},"4":{"11":{"8":[190],"5":[175],"6":[180],"7":[185]},"12":{"8":[200],"3":[180],"4":[185],"5":[190],"6":[195],"7":[200]},"13":{"8":[190],"5":[175],"6":[180],"7":[185]},"14":{"8":[190],"5":[175],"6":[180],"7":[185]},"15":{"8":[190],"5":[175],"6":[180],"7":[185]},"16":{"8":[190],"5":[175],"6":[180],"7":[185]},"17":{"8":[190],"5":[175],"6":[180],"7":[185]},"18":{"8":[190],"5":[175],"6":[180],"7":[185]},"19":{"8":[190],"5":[175],"6":[180],"7":[185]},"1":{"8":[200],"3":[180],"4":[185],"5":[190],"6":[195],"7":[200]},"2":{"8":[200],"3":[180],"4":[185],"5":[190],"6":[195],"7":[200]},"3":{"8":[200],"3":[180],"4":[185],"5":[190],"6":[195],"7":[200]},"4":{"8":[200],"3":[180],"4":[185],"5":[190],"6":[195],"7":[200]},"5":{"8":[200],"3":[180],"4":[185],"5":[190],"6":[195],"7":[200]},"6":{"8":[200],"3":[180],"4":[185],"5":[190],"6":[195],"7":[200]},"7":{"8":[200],"3":[180],"4":[185],"5":[190],"6":[195],"7":[200]},"20":{"8":[200],"3":[180],"4":[185],"5":[190],"6":[195],"7":[200]},"10":{"8":[200],"3":[180],"4":[185],"5":[190],"6":[195],"7":[200]},"21":{"8":[200],"3":[180],"4":[185],"5":[190],"6":[195],"7":[200]}}},"13":300000,"24":3000,"14":5000,"25":50,"15":5000,"26":0,"16":500,"27":0,"17":300,"18":5,"19":5,"2":0,"3":100,"group_id":1,"6":1,"8":20,"20":2,"21":300000},"rid":7}
             * 
             * */
            #endregion

            /*
            SortedDictionary<string, object> result = JsonDictionary.Deserialize(JsonSource);
            SortedDictionary<string, object> body = (SortedDictionary<string, object>)result["body"];
            Console.WriteLine(body["13"].ToString());
            */

            SortedDictionary<string, object> result = JsonDictionary.Deserialize(JsonSource);
            SortedDictionary<string, object> body = (SortedDictionary<string, object>)result["body"];

            string PayoutLoseID = TradeSettings.TradeSettingsDictionary["payout_lose"].ToString();
            string PayoutParityID = TradeSettings.TradeSettingsDictionary["payout_parity"].ToString();


            DebugLog.WriteLine("PayoutLose: " + body[PayoutLoseID].ToString());
            DebugLog.WriteLine("PayoutParity: " + body[PayoutParityID].ToString());

            TradeSettings.PayoutLose = Convert.ToInt32(body[PayoutLoseID].ToString());
            TradeSettings.PayoutParity = Convert.ToInt32(body[PayoutParityID].ToString());

            SortedDictionary<string, object> settings = (SortedDictionary<string, object>)body["settings"];

            string OptionTypes_CallPut_ID = TradeSettings.OptionTypesDictionary["call_put"].ToString();
            SortedDictionary<string, object> CallPutSettings = (SortedDictionary<string, object>)settings[OptionTypes_CallPut_ID];

            foreach (KeyValuePair<string, object> asset in CallPutSettings)
            {
                try
                {
                    int AssetID = Convert.ToInt32(asset.Key);
                    Symbol symbol = TradeSettings.GetSymbolByAssetID(AssetID);

                    SortedDictionary<int, int> PayoutWinByTimeframeID_New = new SortedDictionary<int, int>();


                    SortedDictionary<string, object> assetTf = (SortedDictionary<string, object>)asset.Value;

                    foreach (KeyValuePair<string, object> tf in assetTf)
                    {

                        int TimeframeID = Convert.ToInt32(tf.Key);
                        SortedDictionary<string, object> pd = (SortedDictionary<string, object>)tf.Value;
                        int PayoutWin = Convert.ToInt32(pd["0"].ToString());

                        if (PayoutWinByTimeframeID_New.ContainsKey(TimeframeID))
                        {
                            PayoutWinByTimeframeID_New[TimeframeID] = PayoutWin;
                        }
                        else
                        {
                            PayoutWinByTimeframeID_New.Add(TimeframeID, PayoutWin);
                        }


                        DebugLog.WriteLine("PayoutWin (" + symbol.Name + " - " + TradeSettings.GetTimeframeByID(TimeframeID).LifeTimeMinutes + " min): " + PayoutWin);


                    }

                    symbol.PayoutWinByTimeframeID = PayoutWinByTimeframeID_New;
                   
                }

                catch (Exception e) { }

            }




            return;

        }



        void SetDictionary(dynamic Data)
        {
            #region json_example
            /*
             * 
             * {"type":"RESPONSE","action":"dictionary","body":{"assets":[{"decimals":5,"name":"EURUSD","id":1},{"decimals":5,"name":"GBPUSD","id":2},{"decimals":5,"name":"USDCHF","id":3},{"decimals":3,"name":"USDJPY","id":4},{"decimals":5,"name":"USDCAD","id":5},{"decimals":5,"name":"AUDUSD","id":6},{"decimals":5,"name":"NZDUSD","id":7},{"decimals":3,"name":"EURJPY","id":10},{"decimals":5,"name":"EURGBP","id":11},{"decimals":3,"name":"GBPJPY","id":12},{"decimals":5,"name":"EURCAD","id":13},{"decimals":5,"name":"EURAUD","id":14},{"decimals":3,"name":"AUDJPY","id":15},{"decimals":5,"name":"GBPAUD","id":16},{"decimals":5,"name":"GBPCAD","id":17},{"decimals":5,"name":"GBPCHF","id":18},{"decimals":3,"name":"NZDJPY","id":19},{"decimals":3,"name":"XAUUSD","id":20},{"decimals":3,"name":"XAGUSD","id":21}],"currencies":[{"name":"EUR","id":978},{"name":"RUR","id":643},{"name":"USD","id":840},{"name":"GLD","id":10959}],"time_frames":[{"history_size":0,"period":60,"bar_size":1,"name":"1M","id":1},{"history_size":0,"period":300,"bar_size":5,"name":"5M","id":2},{"history_size":0,"period":900,"bar_size":15,"name":"15M","id":3},{"history_size":0,"period":1800,"bar_size":30,"name":"30M","id":4},{"history_size":0,"period":5,"bar_size":1,"name":"5S","id":100},{"history_size":0,"period":3600,"bar_size":60,"name":"1H","id":5},{"history_size":0,"period":7200,"bar_size":60,"name":"2H","id":6},{"history_size":0,"period":14400,"bar_size":300,"name":"4H","id":7},{"history_size":0,"period":86400,"bar_size":1800,"name":"1D","id":8}],"option_types":[{"id":1,"name":"call_put"},{"id":2,"name":"touch"},{"id":3,"name":"range"},{"id":4,"name":"spread"}],"operation_types":[{"name":"deposit","id":1},{"name":"withdrawal","id":2},{"name":"buy","id":3},{"name":"expiry","id":4},{"name":"compensation","id":7}],"directions":[{"name":"call","id":1},{"name":"put","id":2},{"name":"in","id":3},{"name":"out","id":4}],"option_states":[{"name":"approved","id":2},{"name":"started","id":3},{"name":"closed","id":6}],"option_expiry_reasons":[{"name":"time","id":1},{"name":"touch","id":2},{"name":"early","id":3}],"option_results":[{"name":"win","id":1},{"name":"loss","id":2},{"name":"parity","id":3}],"trade_settings":[{"name":"payout_win","id":1},{"name":"payout_lose","id":2},{"name":"payout_parity","id":3},{"name":"enable","id":6},{"name":"enable_permanent","id":7},{"name":"max_bet_643","id":13},{"name":"max_bet_840","id":14},{"name":"max_bet_978","id":15},{"name":"max_bet_10959","id":16},{"name":"min_bet_643","id":17},{"name":"min_bet_840","id":18},{"name":"min_bet_978","id":19},{"name":"min_bet_10959","id":20},{"name":"max_amount_active_options_643","id":21},{"name":"max_amount_active_options_840","id":22},{"name":"max_amount_active_options_978","id":23},{"name":"max_amount_active_options_10959","id":24},{"name":"early_win_percent","id":25},{"name":"early_loss_percent","id":26},{"name":"early_parity_percent","id":27}]},"rid":4}
             * 
             * 
             * 
             * */
            #endregion


            DebugLog.WriteLine("Setting dictionary");

            #region settings
            try
            {

                foreach (dynamic element in Data.body.option_types)
                {
                    try
                    {

                        //DebugLog.WriteLine("Element name: " + element["name"]);

                        if (!TradeSettings.OptionTypesDictionary.ContainsKey((string)element["name"]))
                        {
                            TradeSettings.OptionTypesDictionary.Add((string)element["name"], (int)element["id"]);
                        }
                        else
                        {
                            TradeSettings.OptionTypesDictionary[(string)element["name"]] = (int)element["id"];
                        }
                        
                    }
                    catch (Exception e) { DebugLog.WriteLine("Exception: " + e.Message); }

                }

                foreach (dynamic element in Data.body.operation_types)
                {
                    try
                    {
                        if (!TradeSettings.OperationTypesDictionary.ContainsKey((string)element["name"]))
                        {
                            TradeSettings.OperationTypesDictionary.Add((string)element["name"], (int)element["id"]);
                        }
                        else
                        {
                            TradeSettings.OperationTypesDictionary[(string)element["name"]] = (int)element["id"];
                        }
                    }
                    catch (Exception) { }

                }

                foreach (dynamic element in Data.body.directions)
                {
                    try
                    {
                        if (!TradeSettings.DirectionsDictionary.ContainsKey((string)element["name"]))
                        {
                            TradeSettings.DirectionsDictionary.Add((string)element["name"], (int)element["id"]);
                        }
                        else
                        {
                            TradeSettings.DirectionsDictionary[(string)element["name"]] = (int)element["id"];
                        }
                    }
                    catch (Exception) { }

                }

                foreach (dynamic element in Data.body.option_states)
                {
                    try
                    {
                        if (!TradeSettings.OptionStatesDictionary.ContainsKey((string)element["name"]))
                        {
                            TradeSettings.OptionStatesDictionary.Add((string)element["name"], (int)element["id"]);
                        }
                        else
                        {
                            TradeSettings.OptionStatesDictionary[(string)element["name"]] = (int)element["id"];
                        }
                    }
                    catch (Exception) { }

                }

                foreach (dynamic element in Data.body.option_expiry_reasons)
                {
                    try
                    {
                        if (!TradeSettings.OptionExpiryReasonsDictionary.ContainsKey((string)element["name"]))
                        {
                            TradeSettings.OptionExpiryReasonsDictionary.Add((string)element["name"], (int)element["id"]);
                        }
                        else
                        {
                            TradeSettings.OptionExpiryReasonsDictionary[(string)element["name"]] = (int)element["id"];
                        }
                    }
                    catch (Exception) { }

                }

                foreach (dynamic element in Data.body.option_results)
                {
                    try
                    {
                        if (!TradeSettings.OptionResultsDictionary.ContainsKey((string)element["name"]))
                        {
                            TradeSettings.OptionResultsDictionary.Add((string)element["name"], (int)element["id"]);
                        }
                        else
                        {
                            TradeSettings.OptionResultsDictionary[(string)element["name"]] = (int)element["id"];
                        }
                    }
                    catch (Exception) { }

                }

                foreach (dynamic element in Data.body.trade_settings)
                {
                    try
                    {
                        if (!TradeSettings.TradeSettingsDictionary.ContainsKey((string)element["name"]))
                        {
                            TradeSettings.TradeSettingsDictionary.Add((string)element["name"], (int)element["id"]);
                        }
                        else
                        {
                            TradeSettings.TradeSettingsDictionary[(string)element["name"]] = (int)element["id"];
                        }
                    }
                    catch (Exception) { }

                }



            }
            catch (Exception e) { DebugLog.WriteLine("SetDictiondary Error: " + e.Message); }
            #endregion


            #region symbols

            try
            {
                foreach (dynamic element in Data.body.assets)
                {

                    try
                    {

                        Symbol s = new Symbol((string)element["name"], (int)element["id"], (int)element["decimals"]);
                        TradeSettings.Symbols.Add(s);

                    }
                    catch (Exception e) { }

                }

            }
            catch (Exception e)
            {

            }


            #endregion


            #region timeframes

            try
            {
                foreach (dynamic element in Data.body.time_frames)
                {

                    try
                    {

                        int period = (int)element["period"];
                        int minutes = (int)period / 60;
                        int id = (int)element["id"];

                        Timeframe tf = new Timeframe(minutes, id);
                        TradeSettings.Timeframes.Add(tf);
                    }
                    catch (Exception e) { }

                }

            }
            catch (Exception e)
            {

            }

            #endregion




            return;




        }





        void OnNewPositionResponse(dynamic Data)
        {
            try
            {
                NewPositionOpenPrice = Convert.ToDouble(Data.body.open_rate.value);
                NewPositionID = (int)Data.body.id;

            }
            catch (Exception e) 
            {
                NewPositionError = true;
                NewPositionErrorMessage = e.Message; 
            }

            NewPositionResponseReceived = true;

        }


        void SetAccountInfo(dynamic Data)
        {
            try
            {
                _Balance = Convert.ToDouble(Data.body.equity);
                DebugLog.WriteLine("Account Balance: " + Balance);
            }
            catch (Exception e) { }

        }





        void CheckWSAuth(dynamic Data)
        {

                TradeGroupID = 0;
                CurrencyID = 0;
                AccountID = 0;

                try
                {
                    TradeGroupID = (int)Data.body.trade_group_id;
                    CurrencyID = (int)Data.body.currency_id;
                    AccountID = (int)Data.body.account_id;

                    Connected = true;
                    Error = false;
                }
                catch (Exception ex)
                {
                    DebugLog.WriteLine("CheckWSAuth error: " + ex.Message);
                    Connected = false;
                    Error = true;
                }

                if (TradeGroupID <= 0 || CurrencyID <= 0 || AccountID <= 0)
                {
                    DebugLog.WriteLine("CheckWSAuth error: No TradeGroupID, CurrencyID or AccountID");
                    Connected = false;
                    Error = true;
                }

        }








        bool GetToken()
        {



            string URL = "https://my.alpari.ru/ru/platforms/binarytrader/";
            string responseBody;

            responseBody = WebClient.Request(URL);

            try
            {
                string sep1 = "data-token=\"";

                //string sep1 = "data-csrf=\"";

                string sep2 = "\"";

                string[] splitted1 = responseBody.Split(new[] { sep1 }, StringSplitOptions.None);
                string toSplit2 = splitted1[1];
                string[] splitted2 = toSplit2.Split(new[] { sep2 }, StringSplitOptions.None);

                Token = splitted2[0];
                DebugLog.WriteLine("Token: " + Token);

            }
            catch (Exception e)
            {
                DebugLog.WriteLine("GetToken error 2: " + e.Message);
                return false;
            }

            if (Token.Length < 10)
            {
                return false;
            }

            return true;
        }


        bool LoggedIn
        {
            get
            {
                string URL = "https://my.alpari.ru/ru/";
                string responseBody;

                responseBody = WebClient.Request(URL);

                if (responseBody.Contains("logout"))
                {
                    DebugLog.WriteLine("Logged in");
                    return true;
                }

                DebugLog.WriteLine("Not logged in");
                return false;
            }
        }



        string DownloadCaptchaImage()
        {
            // Метод проверяет, нужно ли ввести капчу, и если да, то загружает картинку и возвращает локальный путь к картике. Если не нужно, то возвращает пустую строку

            string path = "captcha.png";
            string URL = "https://www.alpari.ru/ru/login/";
            string responseBody = WebClient.Request(URL);
            

            


            if (responseBody.Contains("Введите цифры с картинки:"))
            {
                DebugLog.WriteLine("Need to download and recognize captcha before login");

                URL = "https://www.alpari.ru/ru/login/captcha/";


                DebugLog.WriteLine("Downloading...");
                DebugLog.WriteLine("URL:\t" + URL);
                DebugLog.WriteLine("Save to:\t" + path);

                WebClient.Request(URL, null, path);
                


            }
            else
            {
                DebugLog.WriteLine("No captcha needed");
                return null;

            }




            return path;

        }


        string RecognizeCaptcha(string ImagePath)
        {

            DebugLog.WriteLine("Starting recognizing captcha...");
            DebugLog.WriteLine("Image file: " + ImagePath);


            string result = null;

            AntiCaptcha anticap = new AntiCaptcha(AntiCaptchaKey);

            anticap.CheckDelay = 10000;
            anticap.CheckRetryCount = 20;
            anticap.SlotRetry = 5;
            anticap.SlotRetryDelay = 800;


            try
            {

                result = anticap.GetAnswer(ImagePath);

                if (result == null)
                {
                    DebugLog.WriteLine("AntiCaptcha: no answer");
                    return null;
                }

                



            }
            catch (Exception e)
            {
                DebugLog.WriteLine("AntiCaptcha error: " + e.Message);
                return null;
            }

            DebugLog.WriteLine("Captcha code is: " + result);


            return result;
        }




        bool LoginToWebsite()
        {

            if (LoggedIn)
            {
                return true;
            }

            DebugLog.WriteLine("Starting the web login process");

            string captchaPath = DownloadCaptchaImage();
            string captchaCode = string.Empty;

            if (!string.IsNullOrEmpty(captchaPath))
            {
                captchaCode = RecognizeCaptcha(captchaPath);
            }



            string URL = "https://www.alpari.ru/ru/login/";
            
            SortedDictionary<string, string> PostData = new SortedDictionary<string, string>();
            PostData.Add("successUrl", "https://my.alpari.ru/ru/");
            PostData.Add("login", Username);
            PostData.Add("password", Password);
            PostData.Add("send", "");


            if (!string.IsNullOrEmpty(captchaCode))
            {
                PostData.Add("imgcaptcha", captchaCode);
            }


            string responseBody = WebClient.Request(URL,PostData);


            if (!LoggedIn) 
            {
                DebugLog.WriteLine("FAILED TO LOG IN. Here is the response from the server: ");
                DebugLog.WriteLine("------------------------------------------------------------");
                DebugLog.WriteLine(responseBody);
                DebugLog.WriteLine("------------------------------------------------------------");


                return false; 
            }



            DebugLog.WriteLine("Logged to alpari");



            return true;

        }


        public BinaryPosition SendPosition(BinaryPosition pos)
        {

            return SendPosition(pos.Symbol, pos.Type, pos.Amount, pos.OpenPrice, pos.LifeTimeMinutes, pos.OriginalID);

        }


        public BinaryPosition SendPosition(string Symbol, string PosType, double Amount, double Price, int LifeTimeMinutes, string OriginalID = null)
        {

            if (NewPositionInProgress)
            {
                DebugLog.WriteLine("SendPosition error: Another position opening in progress");
                return null;
            }

            NewPositionInProgress = true;

            NewPositionError = false;
            NewPositionErrorMessage = string.Empty;
            NewPositionOpenPrice = 0;
            NewPositionResponseReceived = false;
            NewPositionID = 0;

            int WinPercent = -1;
            int LossPercent = -1;
            int ParityPercent = -1;

            Timeframe tf = null;
            Symbol s = null;

            try
            {

                tf = TradeSettings.GetTimeframeByMinutes(LifeTimeMinutes);
                s = TradeSettings.GetSymbolByName(Symbol);

                WinPercent = s.PayoutWinByTimeframeID[tf.TimeframeID];
                LossPercent = TradeSettings.PayoutLose;
                ParityPercent = TradeSettings.PayoutParity;

                

            }
            catch (Exception e)
            {
                DebugLog.WriteLine("SendPosition error: position is not supported | " + e.Message);
                NewPositionInProgress = false;
                return null;

            }

            int ts = DateTimeToTimestamp(DateTime.UtcNow);
            string client_lts = ts.ToString() + "000";
            string rate_lts = ts.ToString() + "000";


            if (s.AssetID <= 0 || GetDirectionID(PosType) <= 0 || tf.TimeframeID <= 0)
            {
                DebugLog.WriteLine("SendPosition error: position is not supported");
                NewPositionInProgress = false;
                return null;
            }



            string Request = "{\"type\":\"REQUEST\",\"action\":\"option/buy\",\"body\":{\"option_type_id\":\"1\",\"direction_id\":" + GetDirectionID(PosType) + ",\"asset_id\":\"" + s.AssetID + "\",\"amount\":" + Amount + ",\"time_frame_id\":\"" + tf.TimeframeID + "\",\"win_percent\":" + WinPercent + ",\"loss_percent\":" + LossPercent + ",\"parity_percent\":" + ParityPercent + ",\"client_lts\":" + client_lts + ",\"rate_lts\":" + rate_lts + ",\"rate_value\":" + Price + ",\"forecast1\":" + Price + "},\"rid\":" + RequestID + "}";


            bool res = SendRequest(Request);

            while (!NewPositionResponseReceived)
            {
                System.Threading.Thread.Sleep(1000);
            }

            
            BinaryPosition pos = new BinaryPosition();

            pos.OpenTime = DateTime.UtcNow;
            pos.LifeTimeMinutes = LifeTimeMinutes;
            pos.OriginalID = OriginalID;
            pos.Symbol = Symbol;
            pos.Type = PosType;
            pos.AlpariID = NewPositionID;
            pos.OpenPrice = NewPositionOpenPrice;

            pos.PayoutWin = WinPercent;
            pos.PayoutLose = LossPercent;
            pos.PayoutParity = ParityPercent;


            if (NewPositionError)
            {
                DebugLog.WriteLine("Error opening position with OriginalID " + OriginalID + ": " + NewPositionErrorMessage);
                pos.Status = "Error";

            }
            else
            {
                DebugLog.WriteLine("New position opened");
                pos.Status = "Open";
                OpenPositions.Add(pos);
            }

            NewPositionID = 0;
            NewPositionOpenPrice = 0;

            

            NewPositionInProgress = false;
            return pos;


        }



        int GetDirectionID(string PosType)
        {
            int id = 0;

            switch (PosType)
            {
                case "Long": id = TradeSettings.DirectionsDictionary["call"]; break;
                case "Short": id = TradeSettings.DirectionsDictionary["put"]; break;

            }

            return id;

        }


        bool SendRequest(string Request)
        {

            WaitConnection();
            ws.Send(Request);

            System.Threading.Thread.Sleep(5000);

            return true;
        }


        DateTime TimestampToDateTime(int Timestamp)
        {

            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(Timestamp);

            return dtDateTime;

        }

        int DateTimeToTimestamp(DateTime dt)
        {
            return (int)(dt - new DateTime(1970, 1, 1)).TotalSeconds;

        }







        public void ReadAndSendPositions()
        {
            string Folder = RequestsToSendFolder;

            try
            {
                string[] files = Directory.GetFiles(Folder);

                foreach (string filename in files)
                {

                    try
                    {

                        BinaryPosition NewPos = BinaryPosition.Import(filename);
                        BinaryPosition OpenPos = SendPosition(NewPos);
                        BinaryPosition.Export(OpenPos, RequestResultsFolder + "/" + OpenPos.OriginalID);

                    }
                    catch (Exception)
                    {

                        try
                        {
                            File.Delete(filename);
                        }
                        catch (Exception) { }

                        continue;

                    }


                }




            }
            catch (Exception) { }




        }



        string NormalizeJson(string src, string addLetter = "a")
        {

            int i = 0;
            while (i <= 9)
            {
                src = src.Replace("\"" + i, "\"" + addLetter + i);
                i++;
            }

            return src;
        }





    }









}
