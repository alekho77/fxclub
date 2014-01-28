using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FxMath;
using System.IO;
using FxMath.Orders;

namespace fxanalysis
{
    class Positions : ICommand
    {
        public bool Execute(IList<string> cmd_params)
        {
            if (cmd_params.Count == 5)
            {
                Periods p = Periods.m;
                if (Utils.StrToEnum(cmd_params[2], ref p))
                {
                    Operation op = null;
                    if (cmd_params[0].ToLower() == "buy") op = this.Buy;
                    else if (cmd_params[0].ToLower() == "sell") op = this.Sell;
                    if (op != null)
                    {
                        Profitability(cmd_params[1], p, int.Parse(cmd_params[3]), int.Parse(cmd_params[4]), op);
                        return true;
                    }
                }
            }
            return false;
        }
        delegate float Operation(Quote open, Quote close);
        statistic.position SingleScan(Quote[] quotes, int index, int timeout, float takeprofit, float stoploss, Operation op)
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
        float Buy(Quote open, Quote close)
        {
            // TODO: Неплохо бы еще учесть spread.
            return close.low - open.high;
        }
        float Sell(Quote open, Quote close)
        {
            // TODO: Неплохо бы еще учесть spread.
            return open.low - close.high;
        }
        void Profitability(string binfile, Periods waittime, int tp, int sl, Operation op)
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
            Console.WriteLine(" Probable profit and loss on position");

            statistic stat = new statistic(timeout);
            string dat_file = string.Format("{0}.{1}.{2,3:000}-{3,3:000}.{4}.pos.dat", pair.ToLower(), waitname, tp, sl, op.Method.Name.ToLower());
            using (StreamWriter dat = new StreamWriter(Utils.CorrectFilePath(dat_file), false, Encoding.ASCII))
            {
                dat.AutoFlush = true;
                dat.WriteLine("# Range of {0} from {1} to {2}", pair, first_date, last_date);
                dat.WriteLine("# Profit and loss on {2}-position by order [{0,3:000}-{1,3:000}]", tp, sl, op.Method.Name.ToUpper());
                dat.WriteLine("# Wait time is '{0}'", waitname);
                int count = quotes.Length - timeout;
                dat.WriteLine("# Index    - index of open position");
                dat.WriteLine("# Time     - time of open position");
                dat.WriteLine("# Delta    - profit for buy/sell position in pips");
                dat.WriteLine("# ModDelta - modified profit for buy/sell position in pips per day");
                dat.WriteLine("# Wait     - period wait for takeprofit or stoploss");
                dat.WriteLine("# Index(1)          Time(2) Delta(3) ModDelta(4) Wait(5)");

                float takeprofit = tp / mpips;
                float stoploss = sl / mpips;
                bool inwindow = false;
                Console.Write(" Processed: {0,6:#00.0%}", 0.0);
                for (int i = 0; i < count; i++)
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
                    dat.WriteLine("{0,10} {1:dd.MM.yyyy-HH:mm} {2,8:0.0} {3,11:0.0} {4,7}", i, quotes[i].time, pos.delta * mpips, pos.delta * mpips / pos.time * (float)Periods.d, pos.time);
                    if ((i + 1) % 3571 == 0 || (i + 1) == count)
                    {
                        Console.Write("\b\b\b\b\b\b{0,6:#00.0%}", (double)(i + 1) / (double)count);
                    }
                }
                if (inwindow)
                {
                    stat.wcount++;
                    inwindow = false;
                }
                // записываем общую статистику
                dat.WriteLine("#");
                dat.WriteLine("# Common order statistic by {0}:", op.Method.Name.ToUpper());
                dat.WriteLine("# MAP/MAL   - modified average profit/loss in pips per day");
                dat.WriteLine("# AP/AL/AT  - average profit, loss and timeout in pips");
                dat.WriteLine("# AWPP/AWPL - profit and loss average waiting period in minutes");
                dat.WriteLine("# DP/DL/DT  - profit, loss and timeout density [0..1]");
                dat.WriteLine("# WIN       - average profit window in minutes");
                dat.WriteLine("#");
                dat.WriteLine("# MAP(3)   MAL(4) AP(5) AL(6) AT(7) AWPP(8) AWPL(9)     DP(10)      DL(11)    DT(12) WIN(13)");
                dat.WriteLine("# {0,6:###0.0} {1,8:####0.0} {2,5:0} {3,5:0} {4,5:0} {5,7:0.0} {6,7:0.0} {7,10:0.00000000} {8,10:0.00000000} {9,10:0.00000000} {10,7:0.0}",
                    stat.profit.Triggered ? stat.profit.Delta * mpips / (stat.profit.Wait + 0.5 / stat.profit.Density(count)) * (double)Periods.d : 0,
                    stat.loss.Triggered ? stat.loss.Delta * mpips / (stat.loss.Wait + 0.5 / stat.loss.Density(count)) * (double)Periods.d : 0,
                    stat.profit.Delta * mpips, stat.loss.Delta * mpips, stat.timeout.Delta * mpips,
                    stat.profit.Wait, stat.loss.Wait,
                    stat.profit.Density(count), stat.loss.Density(count), stat.timeout.Density(count),
                    stat.Window);

            }
            int[] awpp_distrib = stat.profit.Distrib(Periods.h1);
            string dis_file = string.Format("{0}.{1}.{2,3:000}-{3,3:000}.{4}.awpp-dis.dat", pair.ToLower(), waitname, tp, sl, op.Method.Name.ToLower());
            using (StreamWriter dat = new StreamWriter(Utils.CorrectFilePath(dis_file), false, Encoding.ASCII))
            {
                dat.WriteLine("# Distribution of AWPP by hour");
                dat.WriteLine("# Index(1) Count(2)");
                for (int i = 0; i < awpp_distrib.Length; i++)
                {
                    dat.WriteLine("{0} {1}", i, awpp_distrib[i]);
                }
            }
        }
    }
}
