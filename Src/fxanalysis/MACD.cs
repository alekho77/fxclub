using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FxMath;
using FxMath.Orders;
using System.IO;

namespace fxanalysis
{
    class MACD : ICommand
    {
        public bool Execute(IList<string> cmd_params)
        {
            if (cmd_params.Count == 6)
            {
                /*
                    [0] - тип позиции buy/sell");
                    [1] - путь к файлу с подготовленными данными");
                    [2] - максимальный период ожидания (Y, M6, M3, M1, w, d, h8, h4, h1, m30, m15, m5, m)");
                    [3] - значение TakeProfit ордера");
                    [4] - значение StopLoss ордера");
                    [5] - период наколения истории (Y, M6, M3, M1, w, d, h8, h4, h1, m30, m15, m5, m)");
                */
                Periods p = Periods.m;
                if (Utils.StrToEnum(cmd_params[2], ref p))
                {
                    if (cmd_params[0].ToLower() == "buy") Scanner = Order.CreateScanner("simple", PositionType.BUY);
                    else if (cmd_params[0].ToLower() == "sell") Scanner = Order.CreateScanner("simple", PositionType.SELL);
                    if (Scanner != null)
                    {
                        Periods h = Periods.M1;
                        if (Utils.StrToEnum(cmd_params[5], ref h))
                        {
                            Profitability(cmd_params[1], p, int.Parse(cmd_params[3]), int.Parse(cmd_params[4]), h);
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        void Profitability(string binfile, Periods waittime, int tp, int sl, Periods h)
        {
            string pair;
            short pip;
            DateTime first_date, last_date;
            Periods bintype;
            Quote[] quotes = BinFile.ReadBinFile(binfile, out pair, out pip, out bintype, out first_date, out last_date);
            if (bintype != Periods.m)
            {
                throw new ApplicationException("Данные в исходном файле осреднены по " + Enum.GetName(typeof(Periods), bintype));
            }

            string waitname = Enum.GetName(typeof(Periods), waittime);
            int timeout = Utils.PeriodToMinutes(waittime);
            int histime = Utils.PeriodToMinutes(h);
            float mpips = Linear.Pow(10, pip); // множитель для перевода дельты котировки в пункты
            float takeprofit = tp / mpips;
            float stoploss = sl / mpips;
            Console.WriteLine(" Probable profit and loss for MACD");
            Quote[] history = new Quote[histime];
            for (int i = 0; i < histime; i++)
            {
                history[i] = quotes[i];
            }
            int count = histime - timeout;
            statistic stat = Scanner.Scan(history, timeout, takeprofit, stoploss);
            if (stat.Count != count)
            {
                throw new ApplicationException(string.Format("Ошибка в расчетах статистики!"));
            }
            Console.WriteLine(" Common order statistic [{0}, {1}]:", tp, sl);
            Console.WriteLine(" Np/Mp     = {0}/{1}", stat.profit.count, stat.wcount);
            Console.WriteLine(" MAP/MAL   = {0:0.0}/{1:0.0} pips per day",
                stat.profit.Triggered ? stat.profit.Delta * mpips / (stat.profit.Wait + 0.5 / stat.profit.Density(count)) * (double)Periods.d : 0,
                stat.loss.Triggered ? stat.loss.Delta * mpips / (stat.loss.Wait + 0.5 / stat.loss.Density(count)) * (double)Periods.d : 0);
            Console.WriteLine(" AP/AL/AT  = {0:0}/{1:0}/{2:0} pips", stat.profit.Delta * mpips, stat.loss.Delta * mpips, stat.timeout.Delta * mpips);
            Console.WriteLine(" AWPP/AWPL = {0:0.0}/{1:0.0} minutes", stat.profit.Wait, stat.loss.Wait);
            Console.WriteLine(" DP/DL/DT  = {0}/{1}/{2} density", stat.profit.Density(count), stat.loss.Density(count), stat.timeout.Density(count));
            Console.WriteLine(" WIN/tp    = {0:0.0}/{1:0.0} minutes", stat.Window, (double)count / (double)stat.wcount);

            // Уплотнение статистического распределения интервалов AWPP до часа
            int[] awpp_distrib = stat.profit.Distrib(Periods.h1);
            string dat_file = string.Format("{0}.{1}.{2,3:000}-{3,3:000}.{4}.macd.dat", pair.ToLower(), waitname, tp, sl, Scanner.Name.ToLower());
            using (StreamWriter dat = new StreamWriter(Utils.CorrectFilePath(dat_file), false, Encoding.ASCII))
            {
                dat.WriteLine("# Distribution of AWPP by hour");
                dat.WriteLine("# Index(1) Count(2)");
                for (int i = 0; i < awpp_distrib.Length; i++)
                {
                    dat.WriteLine("{0} {1}", i, awpp_distrib[i]);
                }
            }
        }
        IScanner Scanner = null;
//         Quote[] History;
//         bool Process(Quote[] quotes, IScanner scan, int idx)
//         {
//             
//             return false;
//         }
    }
}
