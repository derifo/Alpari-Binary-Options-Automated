using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlpariBinaryTransmitter
{
    public class TradeSettings
    {
        public List<Symbol> Symbols;
        public List<Timeframe> Timeframes;

        public int PayoutLose;
        public int PayoutParity;



        public SortedDictionary<string,int> OptionTypesDictionary;
        public SortedDictionary<string, int> OperationTypesDictionary;
        public SortedDictionary<string, int> DirectionsDictionary;
        public SortedDictionary<string, int> OptionStatesDictionary;
        public SortedDictionary<string, int> OptionExpiryReasonsDictionary;
        public SortedDictionary<string, int> OptionResultsDictionary;
        public SortedDictionary<string, int> TradeSettingsDictionary;


        public TradeSettings()
        {
            Symbols = new List<Symbol>();
            Timeframes = new List<Timeframe>();

            OptionTypesDictionary = new SortedDictionary<string, int>();
            OperationTypesDictionary = new SortedDictionary<string, int>();
            DirectionsDictionary = new SortedDictionary<string, int>();
            OptionStatesDictionary = new SortedDictionary<string, int>();
            OptionExpiryReasonsDictionary = new SortedDictionary<string, int>();
            OptionResultsDictionary = new SortedDictionary<string, int>();
            TradeSettingsDictionary = new SortedDictionary<string, int>();

        }

        public Symbol GetSymbolByName(string Name)
        {
            Symbol sOut = null;

            foreach (Symbol s in Symbols)
            {
                if (s.Name == Name)
                {
                    sOut = s;
                    break;
                }
            }

            return sOut;
        }

        public Symbol GetSymbolByAssetID(int AssetID)
        {

            Symbol sOut = null;

            foreach (Symbol s in Symbols)
            {
                if (s.AssetID == AssetID)
                {
                    sOut = s;
                    break;
                }
            }

            return sOut;
        }

        public Timeframe GetTimeframeByMinutes(int LifeTimeMinutes)
        {
            Timeframe tfOut = null;

            foreach (Timeframe tf in Timeframes)
            {

                if (tf.LifeTimeMinutes == LifeTimeMinutes)
                {
                    tfOut = tf;
                    break;
                }

            }

            return tfOut;

        }


        public Timeframe GetTimeframeByID(int TimeframeID)
        {
            Timeframe tfOut = null;

            foreach (Timeframe tf in Timeframes)
            {

                if (tf.TimeframeID == TimeframeID)
                {
                    tfOut = tf;
                    break;
                }

            }

            return tfOut;

        }

    }


    public class TradeSettingsDictionary
    {

        public int PayoutWin;
        public int PayoutLose;
        public int PayoutParity;
        public int Enable;
        public int EnablePermanent;

        public int MaxBet643;
        public int MaxBet840;
        public int MaxBet978;
        public int MaxBet10959;

        public int MinBet643;
        public int MinBet840;
        public int MinBet978;
        public int MinBet10959;

        public int MaxAmountActiveOptions643;
        public int MaxAmountActiveOptions840;
        public int MaxAmountActiveOptions978;
        public int MaxAmountActiveOptions10959;

        public int EarlyWinPercent;
        public int EarlyLossPercent;
        public int EarlyParityPercent;



    }



    public class OptionResultsDictionary
    {
        public int Win;
        public int Loss;
        public int Parity;

    }




    public class OptionExpiryReasonsDictionary
    {
        public int Time;
        public int Touch;
        public int Early;

    }



    public class OptionStatesDictionary
    {
        public int Approved;
        public int Started;
        public int Closed;

    }







    public class DirectionsDictionary
    {
        public int Call;
        public int Put;
        public int In;
        public int Out;

    }



    public class OperationTypesDictionary
    {
        public int Deposit;
        public int Withdrawal;
        public int Buy;
        public int Expiry;
        public int Compensation;

    }



    public class OptionTypesDictionary
    {
        public int CallPut;
        public int Touch;
        public int Range;
        public int Spread;

    }






    public class Symbol
    {
        public string Name;
        public int AssetID;
        public int Decimals;


        public SortedDictionary<int, int> PayoutWinByTimeframeID;

        public Symbol(string _Name, int _AssetID, int _Decimals)
        {
            Name = _Name;
            AssetID = _AssetID;
            Decimals = _Decimals;

            PayoutWinByTimeframeID = new SortedDictionary<int, int>();


        }
        
    }

    public class Timeframe
    {
        public int LifeTimeMinutes;
        public int TimeframeID;

        public Timeframe(int _LifeTimeMinutes, int _TimeframeID)
        {
            LifeTimeMinutes = _LifeTimeMinutes;
            TimeframeID = _TimeframeID;
        }

    }






}
