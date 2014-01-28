using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using FxMath;
using System.Threading;
using FxMath.Orders;
using FxCuda;

namespace fxanalysis
{
    internal class Order : ICommand
    {
        public bool Execute(IList<string> cmd_params)
        {
            if (cmd_params.Count == 5)
            {
                Periods p = Periods.m;
                scan_limits limit_buy = scan_limits.InitFromString(cmd_params[3]);
                scan_limits limit_sell = scan_limits.InitFromString(cmd_params[4]);
                if (Utils.StrToEnum(cmd_params[1], ref p) && (limit_buy != null || limit_sell != null))
                {
                    Profitability(cmd_params[0], p, cmd_params[2].ToLower(), limit_buy, limit_sell);
                    return true;
                }
            }
            return false;
        }
        abstract class Scanner : IScanner
        {
            public abstract string Name { get; }
            public abstract statistic Scan(Quote[] quotes, int timeout, float takeprofit, float stoploss);
            public void Dispose() { }
            protected delegate float Operation(Quote open, Quote close);
            protected abstract statistic Scanning(Quote[] quotes, int timeout, float takeprofit, float stoploss, Operation op);
            protected statistic.position SingleScan(Quote[] quotes, int index, int timeout, float takeprofit, float stoploss, Operation op)
            {
                statistic.position pos;
                for (int i = 1; i < timeout; i++)
                {
                    float delta = op(quotes[index], quotes[index + i]);
                    if (delta >= takeprofit || delta <= -stoploss)
                    {
                        pos.delta = delta;
                        pos.time = i;
                        return pos;
                    }
                }
                pos.delta = op(quotes[index], quotes[index + timeout]);
                pos.time = timeout;
                return pos;
            }
            protected float Buy(Quote open, Quote close)
            {
                // TODO: Неплохо бы еще учесть spread.
                return close.low - open.high;
            }
            protected float Sell(Quote open, Quote close)
            {
                // TODO: Неплохо бы еще учесть spread.
                return open.low - close.high;
            }
        }
        abstract class SimpleScanner : Scanner
        {
            protected override statistic Scanning(Quote[] quotes, int timeout, float takeprofit, float stoploss, Operation op)
            {
                statistic stat = new statistic(timeout);
                bool inwindow = false;
                for (int i = 0; i < (quotes.Length - timeout); i++)
                {
                    statistic.position pos = SingleScan(quotes, i, timeout, takeprofit, stoploss, op);
                    if (stat.Add(pos, timeout, takeprofit, stoploss))
                    {
                        inwindow = true;
                    }
                    else
                    {
                        if (inwindow)
                        {
                            stat.wcount++;
                            inwindow = false;
                        }
                    }
                }
                if (inwindow)
                {
                    stat.wcount++;
                    inwindow = false;
                }
                return stat;
            }
        }
        class SimpleScannerBuy : SimpleScanner
        {
            public override string Name { get { return "BUY"; } }
            public override statistic Scan(Quote[] quotes, int timeout, float takeprofit, float stoploss)
            {
                return Scanning(quotes, timeout, takeprofit, stoploss, this.Buy);
            }
        }
        class SimpleScannerSell : SimpleScanner
        {
            public override string Name { get { return "SELL"; } }
            public override statistic Scan(Quote[] quotes, int timeout, float takeprofit, float stoploss)
            {
                return Scanning(quotes, timeout, takeprofit, stoploss, this.Sell);
            }
        }
        abstract class MultiScanner : Scanner
        {
            protected override statistic Scanning(Quote[] quotes, int timeout, float takeprofit, float stoploss, Operation op)
            {
                // Дробим исходные данные по числу процессоров и запускаем потоки
                Thread[] threads = new Thread[Environment.ProcessorCount];
                stats = new statistic[threads.Length];
                int count = quotes.Length - timeout;
                wstat = new bool[count];
                for (int i = 0; i < threads.Length; i++)
                {
                    int start = (int)(((long)i * (long)count) / (long)threads.Length);
                    int end = (int)(((long)(i + 1) * (long)count) / (long)threads.Length) - 1;
                    threads[i] = new Thread(this.Run);
                    threads[i].Name = string.Format("Scanning Thread {0}", i);
                    threads[i].Priority = ThreadPriority.Highest;
                    object[] paramlist = new object[] { quotes, start, end, timeout, takeprofit, stoploss, op, i };
                    threads[i].Start(paramlist);
                }

                // Ждем пока все потоки отработают
                while(!IsDone(threads))
                {
                    Thread.Sleep(1);
                }

                // Объединяем статистики от разных потоков
                statistic stat = new statistic(timeout);
                for (int i = 0; i < threads.Length; i++)
                {
                    stat += stats[i];
                }

                // Вычисляем число окон.
                bool inwindow = false;
                for (int i = 0; i < count; i++)
                {
                    if (wstat[i])
                    {
                        inwindow = true;
                    }
                    else
                    {
                        if (inwindow)
                        {
                            stat.wcount++;
                            inwindow = false;
                        }
                    }
                }
                if (inwindow)
                {
                    stat.wcount++;
                    inwindow = false;
                }

                return stat;
            }
            private void Run(object obj)
            {
                object[] paramlist = (object[])obj;
                Quote[] quotes = (Quote[])paramlist[0];
                int start = (int)paramlist[1];
                int end = (int)paramlist[2];
                int timeout = (int)paramlist[3];
                float takeprofit = (float)paramlist[4];
                float stoploss = (float)paramlist[5];
                Operation op = (Operation)paramlist[6];
                int index = (int)paramlist[7];
                RangeScan(quotes, start, end, timeout, takeprofit, stoploss, op, ref stats[index]);
            }
            private void RangeScan(Quote[] quotes, int start, int end, int timeout, float takeprofit, float stoploss, Operation op, ref statistic stat)
            {
                stat = new statistic(timeout);
                for (int i = start; i <= end; i++)
                {
                    statistic.position pos = SingleScan(quotes, i, timeout, takeprofit, stoploss, op);
                    wstat[i] = stat.Add(pos, timeout, takeprofit, stoploss);
                }
            }
            private bool IsDone(Thread[] threads)
            {
                for (int i = 0; i < threads.Length; i++)
                {
                    if (threads[i].ThreadState != ThreadState.Stopped) return false;
                }
                return true;
            }
            private statistic[] stats;
            private bool[] wstat;
        }
        class MultiScannerBuy : MultiScanner
        {
            public override string Name { get { return "BUY"; } }
            public override statistic Scan(Quote[] quotes, int timeout, float takeprofit, float stoploss)
            {
                return Scanning(quotes, timeout, takeprofit, stoploss, this.Buy);
            }
        }
        class MultiScannerSell : MultiScanner
        {
            public override string Name { get { return "SELL"; } }
            public override statistic Scan(Quote[] quotes, int timeout, float takeprofit, float stoploss)
            {
                return Scanning(quotes, timeout, takeprofit, stoploss, this.Sell);
            }
        }
        void Profitability(string binfile, Periods waittime, string scanner, scan_limits limit_buy, scan_limits limit_sell)
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
            float mpips = Linear.Pow(10, pip); // множитель для перевода дельты котировки в пункты
            Console.WriteLine(" Probable average profit and loss on orders as a function of deals the takeprofit and stoploss");

            // Расчет BUY и запись данных по profit/loss в ед.времени в пунктах
            if (limit_buy != null)
            {
                string buy_order_file = pair.ToLower() + "." + waitname + ".buy.order.dat";
                using (StreamWriter dat = new StreamWriter(Utils.CorrectFilePath(buy_order_file), false, Encoding.ASCII))
                {
                    dat.AutoFlush = true;
                    dat.WriteLine("# Range of {0} from {1} to {2}", pair, first_date, last_date);
                    dat.WriteLine("# Probable average profit and loss on BUY-orders as a function of deals the takeprofit and stoploss");
                    dat.WriteLine("# Wait time is '{0}'", waitname);
                    using (IScanner scan = CreateScanner(scanner, PositionType.BUY))
                    {
                        OrderProfit(quotes, timeout, dat, scan, mpips, limit_buy);
                    }
                }
            }

            // Расчет SELL и запись данных по profit/loss в ед.времени в пунктах
            if (limit_sell != null)
            {
                string sell_order_file = pair.ToLower() + "." + waitname + ".sell.order.dat";
                using (StreamWriter dat = new StreamWriter(Utils.CorrectFilePath(sell_order_file), false, Encoding.ASCII))
                {
                    dat.AutoFlush = true;
                    dat.WriteLine("# Range of {0} from {1} to {2}", pair, first_date, last_date);
                    dat.WriteLine("# Probable average profit and loss on SELL-orders as a function of deals the takeprofit and stoploss");
                    dat.WriteLine("# Wait time is '{0}'", waitname);
                    using (IScanner scan = CreateScanner(scanner, PositionType.SELL))
                    {
                        OrderProfit(quotes, timeout, dat, scan, mpips, limit_sell);
                    }
                }
            }
        }
        internal static IScanner CreateScanner(string scanner_type, PositionType pos)
        {
            switch (scanner_type)
            {
                case "simple":
                    switch (pos)
                    {
                        case PositionType.BUY: return new SimpleScannerBuy();
                        case PositionType.SELL: return new SimpleScannerSell();
                    }
                    break;
                case "multi":
                    switch (pos)
                    {
                        case PositionType.BUY: return new MultiScannerBuy();
                        case PositionType.SELL: return new MultiScannerSell();
                    }
                    break;
                case "cuda":
                    switch (pos)
                    {
                        case PositionType.BUY: return new CudaOrderScannerBuy();
                        case PositionType.SELL: return new CudaOrderScannerSell();
                    }
                    break;
            }
            throw new ApplicationException(string.Format("Неизвестный тип сканера '{0}'", scanner_type));
        }
        void OrderProfit(Quote[] quotes, int timeout, StreamWriter dat, IScanner scan, float mpips, scan_limits limits)
        {
            int count = quotes.Length - timeout;

            dat.WriteLine("# Limits of takeprofit and stoploss are [{0}, {1}]",
                limits.profit.end == scan_limit.Default.end ? "-" : limits.profit.end.ToString(),
                limits.loss.end == scan_limit.Default.end ? "-" : limits.loss.end.ToString());
            dat.WriteLine("# TP/SL     - takeprofit/stoploss in pips");
            dat.WriteLine("# MAP/MAL   - modified average profit/loss in pips per day");
            dat.WriteLine("# AP/AL/AT  - average profit, loss and timeout in pips");
            dat.WriteLine("# AWPP/AWPL - profit and loss average waiting period in minutes");
            dat.WriteLine("# DP/DL/DT  - profit, loss and timeout density [0..1]");
            dat.WriteLine("# WIN       - average profit window in minutes");
            dat.WriteLine("# WCOUNT    - count of profit windows");
            dat.WriteLine("# WFREQ     - average period bitween windows in minutes");
            dat.WriteLine("# BOUT      - blackout, gap bitween takeprofit event and next window in minutes");
            dat.WriteLine("#                                          {0}", scan.Name);
            dat.WriteLine("# TP(1) SL(2) MAP(3)   MAL(4) AP(5) AL(6) AT(7) AWPP(8) AWPL(9)     DP(10)      DL(11)    DT(12) WIN(13) WCOUNT(14) WFREQ(15) BOUT(16)");

            string pstrf = "{0,4:0000}x{1,4:0000} {2}-{3} {4,6:##0.0%}";
            string pstr_tmp = string.Format(pstrf, 0, 0, Utils.SpanToStr(TimeSpan.FromTicks(0)), Utils.SpanToStr(TimeSpan.FromTicks(0)), 0.0);
            pstrf = pstrf.PadLeft(pstrf.Length + pstr_tmp.Length, '\b');
            Console.Write(" Determining {0} profitability: {1}", scan.Name, pstr_tmp);
            DateTime start = DateTime.Now;
            for (int l = limits.loss.start; l <= limits.loss.end; l += limits.loss.step)
            {
                statistic last_stat = new statistic(timeout);
                for (int p = limits.profit.start; p <= limits.profit.end; p += limits.profit.step)
                {
                    statistic stat = last_stat = scan.Scan(quotes, timeout, p / mpips, l / mpips);
                    if (stat.Count != count)
                    {
                        throw new ApplicationException(string.Format("Ошибка в расчетах статистики [{0}, {1}]!", p, l));
                    }
                    dat.WriteLine("{0,7} {1,5} {2,6:###0.0} {3,8:####0.0} {4,5:0} {5,5:0} {6,5:0} {7,7:0.0} {8,7:0.0} {9,10:0.00000000} {10,10:0.00000000} {11,10:0.00000000} {12,7:0.0} {13,10} {14,9:0.0} {15,8:0.0}", p, l,
                        stat.profit.Triggered ? stat.profit.Delta * mpips / (stat.profit.Wait + 0.5 / stat.profit.Density(count)) * (double)Periods.d : 0,
                        stat.loss.Triggered ? stat.loss.Delta * mpips / (stat.loss.Wait + 0.5 / stat.loss.Density(count)) * (double)Periods.d : 0,
                        stat.profit.Delta * mpips, stat.loss.Delta * mpips, stat.timeout.Delta * mpips,
                        stat.profit.Wait, stat.loss.Wait,
                        stat.profit.Density(count), stat.loss.Density(count), stat.timeout.Density(count),
                        stat.Window, stat.wcount, stat.WinFreq, stat.Blackout);
                    double completed = 0.0;
                    if (limits.profit.end != scan_limit.Default.end && limits.loss.end != scan_limit.Default.end)
                    {
                        completed = (double)((limits.profit.end - limits.profit.start + 1) * (l - limits.loss.start) + (p - limits.profit.start + 1)) / (double)((limits.loss.end - limits.loss.start + 1) * (limits.profit.end - limits.profit.start + 1));
                    }
                    TimeSpan elapsed = DateTime.Now - start;
                    TimeSpan remaining = TimeSpan.FromTicks(0);
                    if (completed > 0.0)
                    {
                        remaining = TimeSpan.FromTicks((long)((double)elapsed.Ticks / completed));
                    }
                    Console.Write(pstrf, l, p, Utils.SpanToStr(elapsed), Utils.SpanToStr(remaining), completed);
                    if (!stat.profit.Triggered) break;
                }
                dat.WriteLine();
                if (!last_stat.profit.Triggered && !last_stat.loss.Triggered) break;
            }
            Console.WriteLine();
        }
    }
}
