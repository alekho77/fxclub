using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using FxTestDb;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace qmap
{
    using IDataRow = IDictionary<string, object>;

    class Interval : IComparable<Interval>
    {
        public Interval(DateTime b)
        {
            Start = b;
            End = b.AddMinutes(1);
        }
        public int CompareTo(Interval other)
        {
            TimeSpan this_span = End - Start;
            TimeSpan other_span = other.End - other.Start;
            return other_span.CompareTo(this_span); // обратная сортировка
        }

        public DateTime Start;
        public DateTime End;
    }

    class Program
    {
        static void Main(string[] args)
        {
            /*
             * Параметры командной строки:
             * >qmap database
             * database  - путь к файлу с БД
            */

            if (args.Length != 1)
            {
                Console.WriteLine(" Утилита для получения графического представления о заливке котировок в тестовую БД.\r\n");
                Console.WriteLine(" Пример использования:");
                Console.WriteLine(" qmap database\r\n");
                Console.WriteLine(" database  - путь к файлу с БД");
                return;
            }

            // корректировка относительного пути
            string ExePath = Environment.CurrentDirectory + "\\";
            string databasefile = args[0];
            if (!databasefile.Contains("\\"))
                databasefile = ExePath + databasefile;

            try
            {
                // подключение к БД
                using (Database db = new Database(databasefile))
                {
                    Console.WriteLine();
                    // Пробегаемся по всем парам
                    foreach (IDataRow pair in db.SelectPairs())
                    {
                        int pid = Convert.ToInt32(pair["PID"]);
                        IDataRow pair_stat = db.GetPairStatistic(pid);
                        if (pair_stat != null)
                        {
                            DateTime first_date = Convert.ToDateTime(pair_stat["FIRST"]);
                            DateTime last_date = Convert.ToDateTime(pair_stat["LAST"]);
                            string stat_file = pair["NAME"].ToString().ToLower() + ".stat";
                            using (StreamWriter stat = new StreamWriter(ExePath + stat_file, false, Encoding.UTF8))
                            {
                                stat.WriteLine("Range of {0} from {1} to {2}", pair["NAME"], first_date, last_date);
                                stat.WriteLine("Count of quotes {0}", pair_stat["COUNT"]);
                                stat.WriteLine("Avg: {0:0.0000}, {1:0.0000}, {2:0.0000}, {3:0.0000}, {4}", pair_stat["OPENAVG"], pair_stat["HIGHAVG"], pair_stat["LOWAVG"], pair_stat["CLOSEAVG"], pair_stat["VOLAVG"]);
                                stat.WriteLine("Min: {0:0.0000}, {1:0.0000}, {2:0.0000}, {3:0.0000}, {4}", pair_stat["OPENMIN"], pair_stat["HIGHMIN"], pair_stat["LOWMIN"], pair_stat["CLOSEMIN"], pair_stat["VOLMIN"]);
                                stat.WriteLine("Max: {0:0.0000}, {1:0.0000}, {2:0.0000}, {3:0.0000}, {4}", pair_stat["OPENMAX"], pair_stat["HIGHMAX"], pair_stat["LOWMAX"], pair_stat["CLOSEMAX"], pair_stat["VOLMAX"]);
                                stat.WriteLine("Count of null-volume {0}", pair_stat["NULLVOL"]);
                            }
                            int from_year = first_date.Year;
                            int to_year = last_date.Year;
                            Console.Write(" {0}: {1}-{2} ({3:D8})... {4:00.0%}", pair["NAME"], from_year, to_year, pair_stat["COUNT"], 0.0);
                            int width = 7 * 24 * 60; // ширина картинки = чило минут в неделе
                            // посчитываем высоту картинки по числу недель во всех годах выбранного интервала
                            int height = 0;
                            const int hweek = 5;
                            for (int year = from_year; year <= to_year; year++)
                            {
                                height += hweek * WeekInYear(year);
                            }
                            // создаем картинку
                            using (Bitmap bmp = new Bitmap(width, height))
                            {
                                int y = 0;
                                Interval qbreak = null;
                                List<Interval> qbreaks = new List<Interval>();
                                Interval qextra = null;
                                List<Interval> qextras = new List<Interval>();
                                // пробегаемся по годам
                                for (int year = from_year; year <= to_year; year++)
                                {
                                    // заливаем дни недели до первого дня года белым
                                    DateTime first_day = new DateTime(year, 1, 1);
                                    int x = 24 * 60 * (first_day.DayOfWeek == DayOfWeek.Sunday ? 6 : (int)first_day.DayOfWeek - 1);
                                    for (int i = 0; i < x;  i++)
                                        for (int j = 0; j < hweek; j++)
                                            bmp.SetPixel(i, y + j, Color.White);
                                    // пробегаемся по месяцам
                                    for (int m = 1; m <= 12; m++)
                                    {
                                        // пробегаемся по дням
                                        for (int d = 1; d <= DateTime.DaysInMonth(year, m); d++)
                                        {
                                            DateTime this_day = new DateTime(year, m, d);
                                            bool isholiday = CheckHoliday(this_day, db);
                                            // проверяем и соотвественно красим
                                            for (int hh = 0; hh < 24; hh++)
                                                for (int mm = 0; mm < 60; mm++, x++)
                                                {
                                                    DateTime qtime = new DateTime(year, m, d, hh, mm, 00);
                                                    Color color_exists = Color.Black;
                                                    Color color_absence = Color.Red;
                                                    bool free_time = qtime.TimeOfDay.CompareTo(new TimeSpan(22, 00, 00)) > 0 ?
                                                        (qtime.DayOfWeek == DayOfWeek.Friday || qtime.DayOfWeek == DayOfWeek.Saturday || qtime.DayOfWeek == DayOfWeek.Sunday)
                                                        : isholiday;
                                                    if (qtime > last_date || free_time)
                                                    {
                                                        color_exists = Color.Blue;
                                                        color_absence = Color.Gray;
                                                        free_time = true;
                                                    }
                                                    bool qexists = db.CheckCandle(pid, qtime);
                                                    // Отмечаем, если надо, разрыв
                                                    if (qexists || free_time)
                                                    {
                                                        if (qbreak != null)
                                                        {
                                                            qbreaks.Add(qbreak);
                                                            qbreak = null;
                                                        }
                                                    } 
                                                    else
                                                    {
                                                        if (qbreak == null)
                                                        {
                                                            qbreak = new Interval(qtime);
                                                        } 
                                                        else
                                                        {
                                                            qbreak.End = qtime.AddMinutes(1);
                                                        }
                                                    }
                                                    // Отмечаем, если надо, доп. данные
                                                    if (!qexists || !free_time)
                                                    {
                                                        if (qextra != null)
                                                        {
                                                            qextras.Add(qextra);
                                                            qextra = null;
                                                        }
                                                    } 
                                                    else
                                                    {
                                                        if (qextra == null)
                                                        {
                                                            qextra = new Interval(qtime);
                                                        } 
                                                        else
                                                        {
                                                            qextra.End = qtime.AddMinutes(1);
                                                        }
                                                    }
                                                    // Собственно рисуем интервал
                                                    Color col = qexists ? color_exists : color_absence;
                                                    for (int j = 0; j < hweek; j++)
                                                        bmp.SetPixel(x, y + j, col);
                                                }
                                            if (x >= width)
                                            {
                                                x = 0;
                                                y += hweek;
                                                Console.Write("\b\b\b\b\b{0:00.0%}", (double)y / (double)height);
                                            }
                                        }
                                    }
                                    if (x > 0)
                                    {
                                        // заливаем остаток последней недели года белым
                                        for (; x < width; x++)
                                            for (int j = 0; j < hweek; j++)
                                                bmp.SetPixel(x, y + j, Color.White);
                                        y += hweek; // новый год начинаем с новой строки
                                    }
                                    Console.Write("\b\b\b\b\b{0:00.0%}", (double)y / (double)height);
                                }
                                // сохраняем картинку
                                string image_file = pair["NAME"].ToString().ToLower() + ".png";
                                bmp.Save(ExePath + image_file, ImageFormat.Png);
                                Console.Write("\b\b\b\b\b\b{0}...", image_file);
                                // сортируем и сохраняем разрывы
                                qbreaks.Sort();
                                string log_file = pair["NAME"].ToString().ToLower() + ".log";
                                using (StreamWriter log = new StreamWriter(ExePath + log_file, false, Encoding.UTF8))
                                {
                                    foreach (Interval b in qbreaks)
                                    {
                                        TimeSpan span = b.End - b.Start;
                                        log.WriteLine("{0}; {1}; {2}", span, b.Start.ToString("dd.MM.yyyy HH:mm"), b.End.ToString("dd.MM.yyyy HH:mm"));
                                    }
                                }
                                Console.Write("\b\b\b {0}...", log_file);
                                // сортируем и сохраняем доп. данные
                                qextras.Sort();
                                string ext_file = pair["NAME"].ToString().ToLower() + ".ext";
                                using (StreamWriter ext = new StreamWriter(ExePath + ext_file, false, Encoding.UTF8))
                                {
                                    foreach (Interval e in qextras)
                                    {
                                        TimeSpan span = e.End - e.Start;
                                        ext.WriteLine("{0}; {1}; {2}", span, e.Start.ToString("dd.MM.yyyy HH:mm"), e.End.ToString("dd.MM.yyyy HH:mm"));
                                    }
                                }
                                Console.Write("\b\b\b {0} OK", ext_file);
                            } // using Bitmap bmp
                            Console.WriteLine();
                        } 
                        else
                        {
                            Console.WriteLine(" {0}: {1,20}... skipped", pair["NAME"], "No data ");
                        }
                    } // foreach IDataRow pair
                } // using Database db
            }
            catch (DbException dbex)
            {
                Console.WriteLine("\r\n Ошибка при работе с БД: " + dbex.Message);
            }
            catch (ApplicationException appex)
            {
                Console.WriteLine("\r\n Программная ошибка: " + appex.Message);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("\r\n Ошибка: " + ex.Message);
            }
            finally
            {
            }
            
            Console.WriteLine("\r\n Для завершения программы нажмите клавишу Enter.");
            Console.Read();
        }

        static int WeekInYear(int year)
        {
            int weeks = 0;
            DateTime first_monday = new DateTime(year, 1, 1);
            if (first_monday.DayOfWeek != DayOfWeek.Monday)
            {
                weeks++;
                int day_shift = (8 - (int)first_monday.DayOfWeek) % 7;
                first_monday = first_monday.AddDays(day_shift);
            }
            DateTime last_sunday = new DateTime(year, 12, 31);
            if (last_sunday.DayOfWeek != DayOfWeek.Sunday)
            {
                weeks++;
                int day_shift = -(int)last_sunday.DayOfWeek;
                last_sunday = last_sunday.AddDays(day_shift);
            }
            weeks += ((last_sunday - first_monday).Days + 1) / 7;
            return weeks;
        }

        static bool CheckHoliday(DateTime day, Database db)
        {
            bool? exholiday = db.CheckHoliday(day);
            if (exholiday == null)
            {
                return (day.DayOfWeek == DayOfWeek.Saturday || day.DayOfWeek == DayOfWeek.Sunday);
            }
            return exholiday.Value;
        }
    }
}
