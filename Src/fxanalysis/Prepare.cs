using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FxTestDb;
using FxMath;
using System.IO;

namespace fxanalysis
{
    using IDataRow = IDictionary<string, object>;

    class Prepare : ICommand
    {
        public bool Execute(IList<string> cmd_params)
        {
            if (cmd_params.Count == 2)
            {
                Preparing(Utils.CorrectFilePath(cmd_params[0]), cmd_params[1].ToUpper());
                return true;
            }
            return false;
        }

        private void Preparing(string databasefile, string pair)
        {
            // читаем из БД котировки и заполняем промежутки линейно интерполированными значениями
            // подключение к БД
            using (Database db = new Database(databasefile))
            {
                // запросили id пары
                int? pid = db.GetPairID(pair);
                if (!pid.HasValue)
                {
                    throw new ApplicationException("Forex pair " + pair + " not found");
                }
                // запросили общую статистику по паре
                IDataRow pair_stat = db.GetPairStatistic(pid.Value);
                if (pair_stat == null)
                {
                    throw new ApplicationException("No data for pair " + pair);
                }
                // запросим описание пары
                IDataRow pair_data = db.GetPair(pid.Value);
                if (pair_data == null)
                {
                    throw new ApplicationException("No pair description for " + pair);
                }
                string qfmt = Quote.FloatFormat(Convert.ToInt16(pair_data["PIP"]));
                DateTime first_date = Convert.ToDateTime(pair_stat["FIRST"]);
                DateTime last_date = Convert.ToDateTime(pair_stat["LAST"]);
                // расчет требуемого числа минутных котировок
                uint required_count = 0;
                for (DateTime t = first_date.AddMinutes(-1); t < last_date.AddSeconds(-30); t = t.AddMinutes(1))
                {
                    if (t.DayOfWeek == DayOfWeek.Saturday || t.DayOfWeek == DayOfWeek.Sunday)
                    {
                        continue;
                    }
                    required_count++;
                }
                // Выполняем заполнение минутных разрывов (интерполяция)
                string log_file = Utils.CorrectFilePath(pair.ToLower() + ".int.log");
                string bin_file = Utils.CorrectFilePath(pair.ToLower() + ".int.bin");
                using (StreamWriter log = new StreamWriter(log_file, false, Encoding.UTF8))
                {
                    log.WriteLine("Range of {0} from {1} to {2}", pair, first_date, last_date);
                    log.WriteLine("Count of quotes: total required {0}, exists {1}, missing {2}", required_count, pair_stat["COUNT"], required_count - Convert.ToInt32(pair_stat["COUNT"]));
                    log.WriteLine("Avg: {0" + qfmt + "}, {1" + qfmt + "}, {2" + qfmt + "}, {3" + qfmt + "}, {4}", pair_stat["OPENAVG"], pair_stat["HIGHAVG"], pair_stat["LOWAVG"], pair_stat["CLOSEAVG"], pair_stat["VOLAVG"]);
                    log.WriteLine("Min: {0" + qfmt + "}, {1" + qfmt + "}, {2" + qfmt + "}, {3" + qfmt + "}, {4}", pair_stat["OPENMIN"], pair_stat["HIGHMIN"], pair_stat["LOWMIN"], pair_stat["CLOSEMIN"], pair_stat["VOLMIN"]);
                    log.WriteLine("Max: {0" + qfmt + "}, {1" + qfmt + "}, {2" + qfmt + "}, {3" + qfmt + "}, {4}", pair_stat["OPENMAX"], pair_stat["HIGHMAX"], pair_stat["LOWMAX"], pair_stat["CLOSEMAX"], pair_stat["VOLMAX"]);
                    log.WriteLine("Count of null-volume {0}", pair_stat["NULLVOL"]);
                    using (BinaryWriter binfile = new BinaryWriter(File.Open(bin_file, FileMode.Create, FileAccess.Write, FileShare.Read)))
                    {
                        BinFile file = new BinFile(binfile);
                        file.WriteHeader(pair, Periods.m, Convert.ToInt16(pair_data["PIP"]), required_count, first_date, last_date);
                        uint count = 0;
                        int exists_count = 0;
                        int missing_count = 0;
                        uint last_count = 0;
                        IDataRow last_candle = null;
                        List<DateTime> gap = null;
                        Console.WriteLine(" Range of {0} from {1} to {2}", pair, first_date, last_date);
                        Console.WriteLine(" Count of quotes: total required {0}, exists {1}, missing {2}", required_count, pair_stat["COUNT"], required_count - Convert.ToInt32(pair_stat["COUNT"]));
                        Console.Write(" Interpolating: {0,6:#00.0%}", 0.0);
                        for (DateTime t = first_date; t < last_date.AddSeconds(30); t = t.AddMinutes(1))
                        {
                            IDataRow candle = db.GetCandle(pid.Value, t);
                            if (t.AddMinutes(-1).DayOfWeek == DayOfWeek.Saturday || t.AddMinutes(-1).DayOfWeek == DayOfWeek.Sunday)
                            {
                                if (candle != null)
                                {
                                    throw new ApplicationException("unexpected data");
                                }
                                continue;
                            }
                            if (candle != null)
                            {
                                if (gap != null)
                                {
                                    // был разрыв, нужно произвести интерполяцию
                                    log.WriteLine("********** GAP {0} minutes between {1} and {2}", gap.Count, Convert.ToDateTime(last_candle["QTIME"]), Convert.ToDateTime(candle["QTIME"]));
                                    IPolynomial inter = new LinearInterpolation();
                                    float[] x = new float[2] { 0, (gap.Count + 1) };
                                    string[] qnames = { "OPENRATE", "HIGHRATE", "LOWRATE", "CLOSERATE" };
                                    Dictionary<string, float[]> coef = new Dictionary<string, float[]>();
                                    foreach (string qname in qnames)
                                    {
                                        float[] y = new float[2] { Convert.ToSingle(last_candle[qname]), Convert.ToSingle(candle[qname]) };
                                        coef[qname] = inter.Transform(x, y);
                                    }
                                    // Выводим в лог весь разрыв и записываем результат в массив
                                    log.WriteLine("{0,8} {1} {2" + qfmt + "} {3" + qfmt + "} {4" + qfmt + "} {5" + qfmt + "}", last_count, Convert.ToDateTime(last_candle["QTIME"]).ToString("yyyy-MM-dd,HH:mm"), Convert.ToSingle(last_candle["OPENRATE"]), Convert.ToSingle(last_candle["HIGHRATE"]), Convert.ToSingle(last_candle["LOWRATE"]), Convert.ToSingle(last_candle["CLOSERATE"]));
                                    for (int i = 0; i < gap.Count; i++)
                                    {
                                        Quote q;
                                        q.volume = -1;
                                        q.time = gap[i];
                                        q.open = Convert.ToSingle(inter.Inverse(coef["OPENRATE"], i + 1));
                                        q.high = Convert.ToSingle(inter.Inverse(coef["HIGHRATE"], i + 1));
                                        q.low = Convert.ToSingle(inter.Inverse(coef["LOWRATE"], i + 1));
                                        q.close = Convert.ToSingle(inter.Inverse(coef["CLOSERATE"], i + 1));
                                        uint c = last_count + (uint)(i + 1);
                                        file.WriteQuote(c, q);
                                        log.WriteLine("{0,8} {1} {2" + qfmt + "} {3" + qfmt + "} {4" + qfmt + "} {5" + qfmt + "}", c, q.time.ToString("yyyy-MM-dd,HH:mm"), q.open, q.high, q.low, q.close);
                                    }
                                    log.WriteLine("{0,8} {1} {2" + qfmt + "} {3" + qfmt + "} {4" + qfmt + "} {5" + qfmt + "}", count, Convert.ToDateTime(candle["QTIME"]).ToString("yyyy-MM-dd,HH:mm"), Convert.ToSingle(candle["OPENRATE"]), Convert.ToSingle(candle["HIGHRATE"]), Convert.ToSingle(candle["LOWRATE"]), Convert.ToSingle(candle["CLOSERATE"]));
                                }
                                file.WriteQuote(count, Convert.ToInt32(candle["VOLUME"]), Convert.ToDateTime(candle["QTIME"]),
                                                Convert.ToSingle(candle["OPENRATE"]),
                                                Convert.ToSingle(candle["HIGHRATE"]),
                                                Convert.ToSingle(candle["LOWRATE"]),
                                                Convert.ToSingle(candle["CLOSERATE"]));
                                exists_count++;
                                gap = null;
                                last_candle = candle;
                                last_count = count;
                            }
                            else
                            {
                                if (gap == null)
                                    gap = new List<DateTime>();
                                gap.Add(t);
                                missing_count++;
                            }

                            count++;
                            if (count % 3571 == 0 || count == required_count)
                            {
                                Console.Write("\b\b\b\b\b\b{0,6:#00.0%}", (double)count / (double)required_count);
                            }
                        }
                        Console.WriteLine();
                        if (count != required_count || exists_count != Convert.ToInt32(pair_stat["COUNT"]) || missing_count != (required_count - Convert.ToInt32(pair_stat["COUNT"])))
                        {
                            log.WriteLine("*** Count of quotes: total required {0}, exists {1}, missing {2}", count, exists_count, missing_count);
                            throw new ApplicationException("discrepancy data");
                        }
                    } // using BinaryWriter bin
                } // using StreamWriter log
            } // using Database db
        }
    }
}
