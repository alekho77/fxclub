using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using FxMath;

namespace fxanalysis
{
    class IndexedQuote
    {
        public IndexedQuote(uint idx, Quote q, Periods at)
        {
            index = idx;
            quote = q;
            if (quote.volume < 0) quote.volume = 0;
            quote.time = RoundQuoteTime(q.time, at);
            switch (at)
            {
                case Periods.Y:
                    qtfmt = "yyyy";
                    break;
                case Periods.M6:
                case Periods.M3:
                case Periods.M1:
                    qtfmt = "yyyy-MM";
                    break;
                case Periods.w:
                case Periods.d:
                    qtfmt = "yyyy-MM-dd";
                    break;
                default:
                    qtfmt = "yyyy-MM-dd,HH:mm";
                    break;
            }
            N = 1;
        }
        public void Add(Quote q)
        {
            // добавление данных
            quote.volume += q.volume;
            quote.high = Math.Max(quote.high, q.high);
            quote.low = Math.Min(quote.low, q.low);
            quote.close = q.close;
        }
        public void Avg(Quote q)
        {
            // добавление данных
            if (q.volume > 0) quote.volume += q.volume;
            quote.open += q.open;
            quote.high += q.high;
            quote.low += q.low;
            quote.close += q.close;
            N++;
        }
        public string ToString(string qfmt)
        {
            Quote q = AvgQuote;
            return string.Format("{0,8} {1,16} {2" + qfmt + "} {3" + qfmt + "} {4" + qfmt + "} {5" + qfmt + "} {6,8}", index, q.time.ToString(qtfmt), q.open, q.high, q.low, q.close, q.volume);
        }
        public readonly uint index;
        public Quote AvgQuote
        {
            get
            {
                Quote q;
                q.volume = quote.volume;
                q.time = quote.time;
                q.open = quote.open / N;
                q.high = quote.high / N;
                q.low = quote.low / N;
                q.close = quote.close / N;
                return q;
            }
        }

        public static DateTime RoundQuoteTime(DateTime qtime, Periods at)
        {
            qtime = qtime.AddMinutes(-1);
            switch (at)
            {
                case Periods.Y:
                    return new DateTime(qtime.Year, 1, 1);
                case Periods.M6:
                    return new DateTime(qtime.Year, ((qtime.Month - 1) / 6) * 6 + 1, 1);
                case Periods.M3:
                    return new DateTime(qtime.Year, ((qtime.Month - 1) / 3) * 3 + 1, 1);
                case Periods.M1:
                    return new DateTime(qtime.Year, qtime.Month, 1);
                case Periods.w:
                    qtime = qtime.AddDays(1 - (qtime.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)qtime.DayOfWeek)); // переход на начало недели
                    return new DateTime(qtime.Year, qtime.Month, qtime.Day);
                case Periods.d:
                    return new DateTime(qtime.Year, qtime.Month, qtime.Day);
                case Periods.h8:
                    return new DateTime(qtime.Year, qtime.Month, qtime.Day, (qtime.Hour / 8) * 8, 0, 0);
                case Periods.h4:
                    return new DateTime(qtime.Year, qtime.Month, qtime.Day, (qtime.Hour / 4) * 4, 0, 0);
                case Periods.h1:
                    return new DateTime(qtime.Year, qtime.Month, qtime.Day, qtime.Hour, 0, 0);
                case Periods.m30:
                    return new DateTime(qtime.Year, qtime.Month, qtime.Day, qtime.Hour, (qtime.Minute / 30) * 30, 0);
                case Periods.m15:
                    return new DateTime(qtime.Year, qtime.Month, qtime.Day, qtime.Hour, (qtime.Minute / 15) * 15, 0);
                case Periods.m5:
                    return new DateTime(qtime.Year, qtime.Month, qtime.Day, qtime.Hour, (qtime.Minute / 5) * 5, 0);
                case Periods.m:
                    return qtime;
            }
            throw new ApplicationException("Unexpected average type " + Enum.GetName(typeof(Periods), at));
        }
        
        private Quote quote;
        private string qtfmt;
        private int N;
    }

    class Average : ICommand
    {
        public bool Execute(IList<string> cmd_params)
        {
            if (cmd_params.Count == 2)
            {
                Periods at = Periods.m;
                if (Utils.StrToEnum(cmd_params[1], ref at))
                {
                    Averaging(Utils.CorrectFilePath(cmd_params[0]), at);
                    return true;
                }
            }
            return false;
        }
        private void Averaging(string srcbinfile, Periods avgtype)
        {
            string pair;
            short pip;
            DateTime first_date, last_date;
            Periods srctype;
            Quote[] quotes = BinFile.ReadBinFile(srcbinfile, out pair, out pip, out srctype, out first_date, out last_date);
            if (srctype != Periods.m)
            {
                throw new ApplicationException("Данные в исходном файле уже осреднены по " + Enum.GetName(typeof(Periods), srctype));
            }

            // Расчет периодов
            uint index_count; // расчет числа периодов, которое получиться после осреднения
            string candles_type = Enum.GetName(typeof(Periods), avgtype);
            string qfmt = Quote.FloatFormat(pip);
            Console.WriteLine(" Range of {0} from {1} to {2} by {3}", pair, first_date, last_date, candles_type);
            Console.WriteLine(" Count of quotes: {0}", quotes.Length);
            // Расчет и запись свечей, заданного типа
            string candles_file = Utils.CorrectFilePath(pair.ToLower() + "." + candles_type + ".candles.dat");
            using (StreamWriter dat = new StreamWriter(candles_file, false, Encoding.ASCII))
            {
                dat.WriteLine("# Range of {0} from {1} to {2} by {3}", pair, first_date, last_date, candles_type);
                dat.WriteLine("# Index-{0,-3}(1)    Time(2) Open(3)High(4)Low(5)Close(6) Volume(7)", candles_type);
                uint index = 0;
                IndexedQuote candle = null;
                DateTime cur_time = IndexedQuote.RoundQuoteTime(first_date, avgtype);
                Console.Write(" Accumulating: {0,6:#00.0%}", 0.0);
                for (int i = 0; i < quotes.Length; i++)
                {
                    if (DateTime.Equals(cur_time, IndexedQuote.RoundQuoteTime(quotes[i].time, avgtype)))
                    {
                        if (quotes[i].volume >= 0)
                        {
                            // для свечей берем только действительные значения
                            if (candle == null)
                            {
                                candle = new IndexedQuote(index, quotes[i], avgtype);
                            }
                            else
                            {
                                // добавление данных в текущий период
                                candle.Add(quotes[i]);
                            }
                        }
                    }
                    else
                    {
                        // новый период
                        cur_time = IndexedQuote.RoundQuoteTime(quotes[i].time, avgtype);
                        index++;
                        // записываем накопленные данные
                        if (candle != null)
                        {
                            dat.WriteLine(candle.ToString(qfmt));
                            if (quotes[i].volume >= 0)
                            {
                                // если есть данные, то инициализируем новое накопление
                                candle = new IndexedQuote(index, quotes[i], avgtype);
                            }
                            else
                            {
                                candle = null; // иначе сбрасываем
                            }
                        }
                    }
                    if ((i + 1) % 3571 == 0 || (i + 1) == quotes.Length)
                    {
                        Console.Write("\b\b\b\b\b\b{0,6:#00.0%}", (double)(i + 1) / (double)quotes.Length);
                    }
                } // for (int i = 0; i < count; i++)
                if (candle != null)
                {
                    dat.WriteLine(candle.ToString(qfmt));
                }
                index_count = index + 1;
                Console.WriteLine();
            } // using StreamWriter dat

            // Осреднение и запись котировок, заданного типа
            string avgbinfile = Utils.CorrectFilePath(pair.ToLower() + "." + candles_type + ".bin");
            using (BinaryWriter binfile = new BinaryWriter(File.Open(avgbinfile, FileMode.Create, FileAccess.Write, FileShare.Read)))
            {
                BinFile file = new BinFile(binfile);
                file.WriteHeader(pair, avgtype, pip, index_count, first_date, last_date);
                uint index = 0;
                IndexedQuote avgquote = null;
                DateTime cur_time = IndexedQuote.RoundQuoteTime(first_date, avgtype);
                Console.Write(" Averaging: {0,6:#00.0%}", 0.0);
                for (int i = 0; i < quotes.Length; i++)
                {
                    if (DateTime.Equals(cur_time, IndexedQuote.RoundQuoteTime(quotes[i].time, avgtype)))
                    {
                        if (avgquote == null)
                        {
                            avgquote = new IndexedQuote(index, quotes[i], avgtype);
                        }
                        else
                        {
                            avgquote.Avg(quotes[i]);
                        }
                    }
                    else
                    {
                        // записываем накопленные данные (формат Open-High-Low-Close)
                        file.WriteQuote(avgquote.index, avgquote.AvgQuote);
                        // новый период
                        cur_time = IndexedQuote.RoundQuoteTime(quotes[i].time, avgtype);
                        index++;
                        avgquote = new IndexedQuote(index, quotes[i], avgtype);
                    }
                    if ((i + 1) % 3571 == 0 || (i + 1) == quotes.Length)
                    {
                        Console.Write("\b\b\b\b\b\b{0,6:#00.0%}", (double)(i + 1) / (double)quotes.Length);
                    }
                } // for (int i = 0; i < count; i++)
                Console.WriteLine();
                // дописываем данные
                file.WriteQuote(avgquote.index, avgquote.AvgQuote);
                if (avgquote.index != index || (index + 1) != index_count)
                {
                    throw new ApplicationException("Ошибка в программе: неверный расчет индексов после осреднения");
                }
            } //using BinaryWriter bin
        }
    }
}
