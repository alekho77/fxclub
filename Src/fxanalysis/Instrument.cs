using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using FxMath;

namespace fxanalysis
{
    class Instrument : ICommand
    {
        public bool Execute(IList<string> cmd_params)
        {
            if (cmd_params.Count == 1)
            {
                Profitability(cmd_params[0]);
                return true;
            }
            return false;
        }
        struct statistic_base
        {
            public long avg_max;
            public long avg_wait;
            public long avg_stop;
            public int count;
            public void Add(int max, int wait, int stop)
            {
                avg_max += max;
                avg_wait += wait;
                avg_stop += stop;
                count++;
            }
            public void SumUp()
            {
                if (count > 0)
                {
                    avg_max /= count;
                    avg_wait /= count;
                    avg_stop /= count;
                }
            }
            public float Density(int fullcount) { return (float)count / (float)fullcount; }
        }
        struct statistic_prof
        {
            public statistic_base profit;
            public statistic_base loss;
            public void Add(statistic_prof_cur cur)
            {
                if (cur.profit != null) profit.Add(cur.profit.max, cur.profit.period, cur.profit.stop);
                if (cur.loss != null) loss.Add(cur.loss.max, cur.loss.period, 0);
            }
            public void SumUp()
            {
                profit.SumUp();
                loss.SumUp();
            }
        }
        class statistic
        {
            public statistic()
            {
                buy.profit.avg_max = 0;
                buy.profit.avg_wait = 0;
                buy.profit.avg_stop = 0;
                buy.profit.count = 0;
                buy.loss.avg_max = 0;
                buy.loss.avg_wait = 0;
                buy.loss.avg_stop = 0;
                buy.loss.count = 0;
                sell.profit.avg_max = 0;
                sell.profit.avg_wait = 0;
                sell.profit.avg_stop = 0;
                sell.profit.count = 0;
                sell.loss.avg_max = 0;
                sell.loss.avg_wait = 0;
                sell.loss.avg_stop = 0;
                sell.loss.count = 0;
            }
            public statistic_prof buy;
            public statistic_prof sell;
            public void Add(statistic_cur cur)
            {
                buy.Add(cur.buy);
                sell.Add(cur.sell);
            }
            public void SumUp()
            {
                buy.SumUp();
                sell.SumUp();
            }
        }
        class statistic_base_cur
        {
            public statistic_base_cur(int m, int p, int s)
            {
                max = m;
                period = p;
                stop = s;
            }
            public int max; // максимальный profit/loss на текущем интервале в pips/day
            public int period; // текущий период, соотвествующий максимальному profit/loss
            public int stop; // stoploss, соответсующий максимальному profit на текущем интервале (loss этот параметр не имеет смысла)
        }
        class statistic_prof_cur
        {
            public statistic_base_cur profit = null;
            public statistic_base_cur loss = null;
            public void Update(int delta, int period, int stop)
            {
                int ppd = (delta * (int)Periods.d) / period; // pips per day
                if (delta > 0)
                {
                    if (profit == null)
                    {
                        profit = new statistic_base_cur(ppd, period, stop);
                    } 
                    else
                    {
                        if (ppd > profit.max)
                        {
                            profit.max = ppd;
                            profit.period = period;
                            profit.stop = stop;
                        }
                    }
                }
                else
                {
                    if (loss == null)
                    {
                        loss = new statistic_base_cur(ppd, period, 0);
                    } 
                    else
                    {
                        if (ppd < loss.max)
                        {
                            loss.max = ppd;
                            loss.period = period;
                        }
                    }
                }
            }
        }
        class statistic_cur
        {
            public statistic_prof_cur buy = new statistic_prof_cur();
            public statistic_prof_cur sell = new statistic_prof_cur();
        }
        void Profitability(string binfile)
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

            // Расчет и запись данных по максимальному profit/loss в ед.времени в пунктах
            string instrument_file = pair.ToLower() + ".instrument.dat";
            using (StreamWriter dat = new StreamWriter(Utils.CorrectFilePath(instrument_file), false, Encoding.ASCII))
            {
                dat.WriteLine("# Range of {0} from {1} to {2}", pair, first_date, last_date);
                dat.WriteLine("# Probable maximum profit and loss as a function of deals frequency");
                dat.WriteLine("# AMP/AML   - average maximum profit and loss in pips per day");
                dat.WriteLine("# AWPP/AWLP - average waiting profit and loss period in minutes");
                dat.WriteLine("# PD/LD     - profit and loss density [0..1]");
                dat.WriteLine("# APSL      - average StopLoss on way to maximum profit in pips");
                dat.WriteLine("#                              BUY                          |                         SELL");
                dat.WriteLine("# WP(1) AMP(2) AWPP(3)  PD(4) APSL(5) AML(6) AWLP(7)  LD(8)   AMP(9) AWPP(10)  PD(11) APSL(12) AML(13) AWLP(14)  LD(15)");
                int timeout = Utils.PeriodToMinutes(Periods.M1);
                int count = quotes.Length - timeout;
                float mpips = Linear.Pow(10, pip); // множитель для перевода дельты котировки в пункты
                Dictionary<Periods, statistic> statistics = new Dictionary<Periods, statistic>();
                List<Periods> intervals = new List<Periods>();
                foreach (Periods p in Enum.GetValues(typeof(Periods)))
                {
                    if (Utils.PeriodToMinutes(p) <= timeout)
                    {
                        statistics.Add(p, new statistic());
                        intervals.Add(p);
                    }
                }
                intervals.Sort((x, y) => x == y ? 0 : (Utils.PeriodToMinutes(x) < Utils.PeriodToMinutes(y) ? -1 : 1));
                Console.WriteLine(" Probable maximum profit and loss as a function of deals frequency");
                Console.Write(" Processing: {0,6:#00.0%}", 0.0);
                for (int i = 0; i < count; i++)
                {
                    int buy_cur_sl = 0, sell_cur_sl = 0; // current stoploss
                    int cur_interval = 0; // индекс текущего интервала в сортированном по возрастанию списке
                    statistic_cur cur_stat = new statistic_cur();
                    for (int j = 1; j <= timeout; j++)
                    {
                        int buy = (int)((quotes[i + j].low - quotes[i].high) * mpips); // buy delta in pips
                        int sell = (int)((quotes[i].low - quotes[i + j].high) * mpips); // sell delta in pips
                        buy_cur_sl = Math.Max(-buy, buy_cur_sl);
                        sell_cur_sl = Math.Max(-sell, sell_cur_sl);
                        if (j > Utils.PeriodToMinutes(intervals[cur_interval]))
                        {
                            // переходим в следующий интервал, добавляем статистику по текущему интервалу
                            statistics[intervals[cur_interval]].Add(cur_stat);
                            cur_stat = new statistic_cur(); // сброс текущей статистики
                            cur_interval++;
                        }
                        cur_stat.buy.Update(buy, j, buy_cur_sl);
                        cur_stat.sell.Update(sell, j, sell_cur_sl);
                    } // for (int j = 1; j <= timeout; j++)
                    statistics[intervals[cur_interval]].Add(cur_stat);
                    if ((i + 1) % 3571 == 0 || (i + 1) == count)
                    {
                        Console.Write("\b\b\b\b\b\b{0,6:#00.0%}", (double)(i + 1) / (double)count);
                    }
                } // for (int i = 0; i < count; i++)
                Console.WriteLine();

                foreach (KeyValuePair<Periods,statistic> s in statistics.OrderBy(s => Utils.PeriodToMinutes(s.Key)))
                {
                    s.Value.SumUp();
                    dat.WriteLine("{0,7} {1,6:0} {2,7:0} {3,6:0.0000} {4,7:0} {5,6:0} {6,7:0} {7,6:0.0000}   {8,6:0}  {9,7:0}  {10,6:0.0000}  {11,7:0}  {12,6:0}  {13,7:0}  {14,6:0.0000}",
                        Utils.PeriodToMinutes(s.Key),
                        s.Value.buy.profit.avg_max, s.Value.buy.profit.avg_wait, s.Value.buy.profit.Density(count), s.Value.buy.profit.avg_stop,
                        s.Value.buy.loss.avg_max,   s.Value.buy.loss.avg_wait,   s.Value.buy.loss.Density(count),
                        s.Value.sell.profit.avg_max, s.Value.sell.profit.avg_wait, s.Value.sell.profit.Density(count), s.Value.sell.profit.avg_stop,
                        s.Value.sell.loss.avg_max,   s.Value.sell.loss.avg_wait,   s.Value.sell.loss.Density(count));
                }
            } // using StreamWriter dat
        }
    }
}
