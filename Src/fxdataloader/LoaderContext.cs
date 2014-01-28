using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FxComApiTrader;

namespace fxdataloader
{
    public enum FxStatus
    {
        Offline = FxConnectionStatus.cs_Offline,
        Online = FxConnectionStatus.cs_Online,
        Connecting = FxConnectionStatus.cs_Connecting,
        LoadingData = FxConnectionStatus.cs_LoadingData,
        WaitingForConnection = FxConnectionStatus.cs_WaitingForConnection
    }

    public struct FxInstrument
    {
        public string id;
        public string name;
        public string type;
        public double buy, sell;
    }

    public struct Candle
    {
        public DateTime time;
        public double open, close;
        public double high, low;
    }

    public delegate void FxErrorEventHandler(string msg);
    public delegate void FxStatusChangeEventHandler(FxStatus Status);
    public delegate void FxPairMessageEventHandler(FxInstrument Inst, string operation);

    public class FxErrorException : ApplicationException
    {
        public FxErrorException(IFxError fx_error)
            : base(fx_error.Code.ToString() + ": " + fx_error.Message)
        {
        }
    }

    class LoaderContext : ApplicationContext
    {
        public static readonly string ExePath = Application.ExecutablePath.Substring(0, Application.ExecutablePath.LastIndexOf('\\') + 1);
        //public static readonly string DbFileName = ExePath + "fxdata.fdb";

        private static LoaderContext FContext;
        public static LoaderContext Context
        {
            get
            {
                // Если нет синглтона, то создаем его
                if (FContext == null) FContext = new LoaderContext();
                return FContext;
            }
        }
        
        //public static DatabaseUtilDb Database { get { return Context.FDatabase; } }
        //private DatabaseUtilDb FDatabase;

        private IFxTraderApi FxApi;
        public static string APIVersion { get { return "ICTS ApiTrader v" + Context.FxApi.Version; } }
        public static FxStatus APIStatus { get { return (FxStatus)Context.FxApi.Status; } }

        public event FxErrorEventHandler OnFxErrorEvent;
        public event FxStatusChangeEventHandler OnFxStatusChangeEvent;
        public event FxPairMessageEventHandler OnFxPairMessageEvent;

        public static void Logon(string schema, string login, string password, string language)
        {
            IFxError error = Context.FxApi.Logon(schema, login, password, language, "879hozin");
            if (error != null) throw new FxErrorException(error);
        }

        public static void Logout()
        {
            IFxError error = Context.FxApi.Logoff();
            if (error != null) throw new FxErrorException(error);
        }

        public static DateTime FxServerTime
        {
            get 
            {
                DateTime val;
                IFxError error = Context.FxApi.GetServerTime(out val);
                if (error != null) throw new FxErrorException(error);
                return val;
            }
        }

        public static IDictionary<string, int> FxChartIntervals
        {
            get
            {
                IDictionary<string, int> list = new Dictionary<string, int>();
                IFxChartIntervalList intervals = Context.FxApi.ChartIntervals;
                for (int i = 0; i < intervals.Count; i++)
                {
                    IFxChartInterval interval = intervals.get_Interval(i);
                    list[interval.Id] = interval.Duration;
                }
                return list;
            }
        }

        public static IDictionary<string, string> FxPairGroups
        {
            get
            {
                IDictionary<string, string> list = new Dictionary<string, string>();
                IFxPairGroupList groups = Context.FxApi.PairGroups;
                for (int i = 0; i < groups.Count; i++)
                {
                    IFxPairGroup group = groups.get_Group(i);
                    list[group.GroupId] = group.GroupName;
                }
                return list;
            }
        }

        public static IList<FxInstrument> FxPairs
        {
            get
            {
                IList<FxInstrument> list = new List<FxInstrument>();
                IFxPairList pairs = Context.FxApi.Pairs;
                for (int i = 0; i < pairs.Count; i++)
                {
                    IFxPair pair = pairs.get_Pair(i);
                    FxInstrument p;
                    p.id = pair.PairId;
                    p.name = pair.Name;
                    p.type = pair.PairType.ToString();
                    p.buy = pair.BuyRate;
                    p.sell = pair.SellRate;
                    list.Add(p);

                    //FxPairList DependentPairs;
                    //IFxError error = Context.FxApi.UpdatePairSubscribtion(pair.PairId, FxLogic.lg_True, out DependentPairs);
                    //if (error != null)
                    //{
                    //   throw new FxErrorException(error);
                    //}
                }
                return list;
            }
        }

        public static IList<Candle> Test()
        {
            FxCandleList list;
            DateTime from = DateTime.Parse("26.04.2012 18:00:00");
            DateTime to = DateTime.Parse("26.04.2012 19:00:00");
            IFxError error = Context.FxApi.GetCandleHistory("3", "1", 10, from, to, out list);
            if (error == null)
            {
                IList<Candle> candles = new List<Candle>();
                for (int i = 0; i < list.Count; i++)
                {
                    IFxCandle candle = list.get_Candle(i);
                    Candle c;
                    c.time = candle.Time;
                    c.open = candle.Open;
                    c.close = candle.Close;
                    c.high = candle.High;
                    c.low = candle.Low;
                    candles.Add(c);
                }
                return candles;
            }
            return null;
        }

        private LoaderContext()
        {
            // подключаемся к FxCom Trader API
            try
            {
                FxApi = new FxTraderApiClass();
                // подписка на события
                IFxTraderApiEvents_Event api_events = FxApi as IFxTraderApiEvents_Event;
                //api_events.OnAccountMessage += new IFxTraderApiEvents_OnAccountMessageEventHandler();
                api_events.OnError += new IFxTraderApiEvents_OnErrorEventHandler(OnFxError);
                //api_events.OnFeedStatusChange += new IFxTraderApiEvents_OnFeedStatusChangeEventHandler;
                //api_events.OnLogoff += new IFxTraderApiEvents_OnLogoffEventHandler;
                //api_events.OnMarginCallMessage += new IFxTraderApiEvents_OnMarginCallMessageEventHandler;
                //api_events.OnOrderMessage += new IFxTraderApiEvents_OnOrderMessageEventHandler;
                api_events.OnPairMessage += new IFxTraderApiEvents_OnPairMessageEventHandler(OnFxPairMessage);
                //api_events.OnRulesChange += new IFxTraderApiEvents_OnRulesChangeEventHandler;
                api_events.OnStatusChange += new IFxTraderApiEvents_OnStatusChangeEventHandler(OnFxStatusChange);
                //api_events.OnTextMessage += new IFxTraderApiEvents_OnTextMessageEventHandler;
                //api_events.OnTradeMessage += new IFxTraderApiEvents_OnTradeMessageEventHandler;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message, "Неудалось подключиться к FxCom Trader API!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
                return;
            }

            // подписываемся на разные события
            Application.ApplicationExit += new EventHandler(this.OnApplicationExit);
            Application.Idle += new EventHandler(this.OnApplicationIdle);

            //FDatabase = new DatabaseUtilDb(DbFileName);

            // если БД нет, то создаем ее
//             if (!File.Exists(DbFileName))
//             {
//                 if (!FDatabase.Create(Properties.Resources.TestUtilDbSQLScript))
//                 {
//                     Application.Exit();
//                     return;
//                 }
//             }

            // открываем БД
//             if (!FDatabase.Open())
//             {
//                 Application.Exit();
//                 return;
//             }

            MainForm = new FormMain();
        }

        private void OnApplicationIdle(object sender, EventArgs e)
        {
            // Простой
        }

        private void OnApplicationExit(object sender, EventArgs e)
        {
            // Закрытие и сохранение данных
            if (FxApi.Status != FxConnectionStatus.cs_Offline)
            {
                FxApi.Logoff();
            }
            //FDatabase.Close();
        }

        private void OnFxError(FxError Error)
        {
            OnFxErrorEvent(Error.ErrorType.ToString() + " - " + Error.Code.ToString() + ": " + Error.Message);
        }

        private void OnFxStatusChange(FxConnectionStatus Status)
        {
            OnFxStatusChangeEvent((FxStatus)Status);
        }

        private void OnFxPairMessage(FxPair pair, FxMessageType msgtype)
        {
            FxInstrument inst;
            inst.id = pair.PairId;
            inst.name = pair.Name;
            inst.type = pair.PairType.ToString();
            inst.buy = pair.BuyRate;
            inst.sell = pair.SellRate;
            OnFxPairMessageEvent(inst, msgtype.ToString());
        }
    }
}
