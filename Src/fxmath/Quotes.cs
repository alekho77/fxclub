using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FxMath
{
    public struct Quote
    {
        public int volume; // -1 для интерполированных значений
        public DateTime time;
        public float open, high, low, close;

        public static string FloatFormat(int pip)
        {
            return pip == 2 ? ",6:##0.00" : ",6:0.0000";
        }
    }

    public enum Periods : short
    {
        // Приближенные значения в днях
        Y = -262,
        M6 = -6 * 22,
        M3 = -3 * 22,
        M1 = -22,
        // Точные значения в минутах
        w = 5 * 24 * 60,
        d = 24 * 60,
        h8 = 8 * 60,
        h4 = 4 * 60,
        h1 = 60,
        m30 = 30,
        m15 = 15,
        m5 = 5,
        m = 1
    }

    namespace Orders
    {
        public enum PositionType
        {
            BUY, SELL
        }
        public struct statistic
        {
            public struct position
            {
                public float delta; // разница между открытием и закрытием соотвествующей операции (buy/sell), для TakeProfit >= 0, для StopLoss <= 0, для Timeout <>= 0
                public int time;  // время, когда сработал сработал TakeProfit/StopLoss (разница индексов в массиве котировок между точками открытия и закрытия)
            }
            public struct statistic_base
            {
                public int count;
                private double delta;
                private double wait;
                private int[] wdistrib;
                public bool Triggered
                {
                    get { return count > 0; }
                }
                public static statistic_base operator +(statistic_base left, statistic_base right)
                {
                    statistic_base stat;
                    stat.delta = left.delta + right.delta;
                    stat.wait = left.wait + right.wait;
                    stat.count = left.count + right.count;
                    if (left.wdistrib != null && right.wdistrib != null && left.wdistrib.Length == right.wdistrib.Length)
                    {
//                         stat.wdistrib = new int[left.wdistrib.Length];
                        stat.wdistrib = (int[])left.wdistrib.Clone();
                        for (int i = 0; i < stat.wdistrib.Length; i++)
                        {
                            stat.wdistrib[i] += /*left.wdistrib[i] +*/ right.wdistrib[i];
                        }
                    } 
                    else
                    {
                        stat.wdistrib = null;
                    }
                    return stat;
                }
                public static statistic_base operator +(statistic_base left, position right)
                {
                    statistic_base stat;
                    stat.delta = left.delta + right.delta;
                    stat.wait = left.wait + right.time;
                    stat.count = left.count + 1;
                    if (left.wdistrib != null)
                    {
                        stat.wdistrib = (int[])left.wdistrib.Clone();
                        stat.wdistrib[right.time - 1]++;
                    } 
                    else
                    {
                        stat.wdistrib = null;
                    }
                    return stat;
                }
                public double Delta { get { return count > 0 ? delta / count : 0; } }
                public double Wait { get { return count > 0 ? wait / count : 0; } }
                public double Density(int fullcount) { return (double)count / (double)fullcount; }
                public int[] Distrib(Periods p)
                {
                    if (wdistrib != null)
                    {
                        int s = Utils.PeriodToMinutes(p);
                        if (s > 1)
                        {
                            // Уплотняем статистическое распределение
                            int[] distrib = new int[(wdistrib.Length + s - 1) / s];
                            for (int i = 0; i < distrib.Length; i++)
                            {
                                for (int j = 0; j < s; j++)
                                {
                                    distrib[i] += wdistrib[i * s + j];
                                }
                            }
                            return distrib;
                        }
                        else
                        {
                            return (int[])wdistrib.Clone();
                        }
                    }
                    return null;
                }
                public statistic_base(int timeout)
                {
                    this.delta = 0;
                    this.wait = 0;
                    this.count = 0;
                    this.wdistrib = timeout > 0 ? new int[timeout] : null;
                }
                public statistic_base(double delta, double wait, int count)
                {
                    this.delta = delta;
                    this.wait = wait;
                    this.count = count;
                    this.wdistrib = null;
                }
            }
            public statistic_base profit;
            public statistic_base loss;
            public statistic_base timeout;
            public int wcount; // количество окон
            public statistic(statistic_base profit, statistic_base loss, statistic_base timeout, int wcount)
            {
                this.profit = profit;
                this.loss = loss;
                this.timeout = timeout;
                this.wcount = wcount;
            }
            public statistic(int timeout)
            {
                this.profit = new statistic_base(timeout);
                this.loss = new statistic_base(0);
                this.timeout = new statistic_base(0);
                this.wcount = 0;
            }
            public int Count
            {
                get { return profit.count + loss.count + timeout.count; }
            }
            public double Window { get { return wcount > 0 ? (double)(profit.count) / (double)wcount : 0; } }
            public double WinFreq { get { return wcount > 0 ? (double)Count / (double)wcount : 0; } }
            public double Blackout { get { return WinFreq - profit.Wait; } }
            public bool Add(position pos, int timeout, float takeprofit, float stoploss)
            {
                if (pos.time == timeout)
                {
                    this.timeout += pos;
                }
                else if (pos.delta >= takeprofit)
                {
                    this.profit += pos;
                    return true;
                }
                else if (pos.delta <= -stoploss)
                {
                    this.loss += pos;
                }
                else
                {
                    throw new ApplicationException("Статистика по позициям не соотвествует заданным TakeProfit и StopLoss");
                }
                return false;
            }
            public static statistic operator +(statistic left, statistic right)
            {
                statistic stat;
                stat.profit = left.profit + right.profit;
                stat.loss = left.loss + right.loss;
                stat.timeout = left.timeout + right.timeout;
                stat.wcount = 0;
                return stat;
            }
        }

        public interface IScanner : IDisposable
        {
            /// <summary>
            /// Вычисляет статистику использования конкретного ордера
            /// </summary>
            /// <param name="quotes">Массив минутных котировок</param>
            /// <param name="timeout">Время в минутах в течении которого производиться поиск TakeProfit или StopLoss</param>
            /// <param name="takeprofit">Значение дельты (в ед.котировки!) для TakeProfit</param>
            /// <param name="stoploss">Значение дельты (в ед.котировки!) для StopLoss</param>
            /// <returns>Возвращает завершенную статистику</returns>
            statistic Scan(Quote[] quotes, int timeout, float takeprofit, float stoploss);
            string Name { get; }
        }

        public struct scan_limit
        {
            public int start, end, step;
            static public scan_limit Default
            {
                get
                {
                    scan_limit s;
                    s.start = 0;
                    s.end = int.MaxValue;
                    s.step = 1;
                    return s;
                }
            }
        }
        public class scan_limits
        {
            private scan_limits()
            {
                profit = scan_limit.Default;
                loss = scan_limit.Default;
            }
            public scan_limit profit;
            public scan_limit loss;
            public static scan_limits InitFromString(string str)
            {
                scan_limits limits = new scan_limits();
                string[] fields = str.Split('/');
                if (fields.Length == 6)
                {
                    if (   (fields[0] == "-" || int.TryParse(fields[0], out limits.profit.start))
                        && (fields[1] == "-" || int.TryParse(fields[1], out limits.profit.end))
                        && (fields[2] == "-" || int.TryParse(fields[2], out limits.profit.step))
                        && (fields[3] == "-" || int.TryParse(fields[3], out limits.loss.start))
                        && (fields[4] == "-" || int.TryParse(fields[4], out limits.loss.end))
                        && (fields[5] == "-" || int.TryParse(fields[5], out limits.loss.step))
                        )
                    {
                        if (limits.profit.end >= limits.profit.start && limits.profit.step > 0
                            && limits.loss.end >= limits.loss.start && limits.loss.step > 0)
                        {
                            return limits;
                        }
                    }
                }
                return null;
            }
        }
    }
}
