using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;
using System.Data.Common;
using FxTestDb;

namespace csvload
{
    class Program
    {
        static void Main(string[] args)
        {
            /*
             * Параметры командной строки:
             * >csvload database csvfile pair
             * database - путь к файлу с БД
             * csvfile  - путь к файлу с минутными котировками в формате CSV: YYYY.MM.DD,hh:mm,open,max,min,close,volume
             *            Разделитель десятичной дроби в open, max, min, close - точка '.'
             * pair     - название котировки: EURUSD, GBPUSD, USDCHF, USDJPY и т.д.
            */

            if (args.Length != 3)
            {
                Console.WriteLine(" Утилита для заливки минутных котировок в тестовую БД.\r\n");
                Console.WriteLine(" Пример использования:");
                Console.WriteLine(" csvload database csvfile pair\r\n");
                Console.WriteLine(" database - путь к файлу с БД");
                Console.WriteLine(" csvfile  - путь к файлу с минутными котировками в формате CSV: YYYY.MM.DD,hh:mm,open,max,min,close,volume");
                Console.WriteLine("            разделитель десятичной дроби в open, max, min, close - точка '.'");
                Console.WriteLine(" pair     - название котировки: EURUSD, GBPUSD, USDCHF, USDJPY и т.д.");
                return;
            }

            // корректировка относительного пути
            string ExePath = Environment.CurrentDirectory + "\\";
            string databasefile = args[0];
            if (!databasefile.Contains("\\"))
                databasefile = ExePath + databasefile;
            string csvfile = args[1];
            if (!csvfile.Contains("\\"))
                csvfile = ExePath + csvfile;

            try
            {
                // подключение к БД
                using (Database db = new Database(databasefile))
                {
                    int? idpair = db.GetPairID(args[2].ToUpper());
                    if (idpair == null)
                    {
                        throw new ApplicationException("Неизвестная валютная пара (" + args[2] + ").");
                    }
                    
                    using (DataLoader loader = db.BeginDataLoad())
                    {
                        // открываем файл с данными
                        CultureInfo dt_provider = CultureInfo.InvariantCulture;
                        NumberFormatInfo num_provider = new NumberFormatInfo();
                        num_provider.NumberDecimalSeparator = ".";
                        using (StreamReader stream = OpenFxArhiveFile(csvfile))
                        {
                            long fpos = 0; // позиция в файле
                            int line_count = 0; // текущая строка

                            // Подготавливаем прогресс-бар
                            FileInfo finfo = new FileInfo(csvfile);
                            Console.WriteLine(" File size: {0:N0} bytes", finfo.Length);
                            string pbarstr = "";
                            pbarstr = pbarstr.PadRight(50, '-');
                            const string pbarfmt = " Line: {0:D8} {1} {2}%";
                            Console.Write(pbarfmt, line_count, pbarstr, 0);

                            // основной цикл по-строчной разбоки файла
                            //FbTransaction trans = Connection.BeginTransaction();
                            string str;
                            while ((str = stream.ReadLine()) != null)
                            {
                                fpos += str.Length + 2; // +2 это символы '\r' и '\n' 
                                if (++line_count % 3571 /*60493*/ == 0) // здесь используется простое число, чтобы не было заметно шага
                                {
                                    int p = (int)((fpos * 100) / finfo.Length);
                                    pbarstr = "";
                                    pbarstr = pbarstr.PadRight(p / 2, '#');
                                    pbarstr = pbarstr.PadRight(50, '-');
                                    Console.Write("\r" + pbarfmt, line_count, pbarstr, p);
                                }

                                // разобрали по столбцам: date/time/open/high/low/close/volume
                                string[] columns = str.Split(new char[] { ',' });
                                if (columns.Length != 6 && columns.Length != 7)
                                {
                                    throw new ApplicationException("В строке " + line_count.ToString() + " указано " + columns.Length.ToString() + " параметров.");
                                }

                                // парсим дату и время
                                DateTime qtime;
                                try
                                {
                                    qtime = DateTime.ParseExact(columns[0] + " " + columns[1], "yyyy.MM.dd HH:mm", dt_provider);
                                }
                                catch (System.Exception ex)
                                {
                                    throw new ApplicationException("В строке " + line_count.ToString() + " не удалось разобрать дату и время '"
                                                                    + columns[0] + " " + columns[1] + "' - " + ex.Message, ex);
                                }

                                // парсим значения котировок
                                double open, high, low, close;
                                try
                                {
                                    open = Convert.ToDouble(columns[2], num_provider);
                                    high = Convert.ToDouble(columns[3], num_provider);
                                    low = Convert.ToDouble(columns[4], num_provider);
                                    close = Convert.ToDouble(columns[5], num_provider);
                                }
                                catch (System.Exception ex)
                                {
                                    throw new ApplicationException("В строке " + line_count.ToString() + " не удалось разобрать значения котировок '"
                                                                    + columns[2] + ", " + columns[3] + ", " + columns[4] + ", " + columns[5] + "' - " + ex.Message, ex);
                                }

                                // парсим объем сделок
                                int? volume = null;
                                if (columns.Length == 7)
                                {
                                    try
                                    {
                                        volume = Convert.ToInt32(columns[6]);
                                        volume = volume > 0 ? volume : null;
                                    }
                                    catch (System.Exception ex)
                                    {
                                        throw new ApplicationException("В строке " + line_count.ToString() + " не удалось разобрать значение объема сделок '"
                                                                        + columns[6] + "' - " + ex.Message, ex);
                                    }
                                }

                                loader.Push(idpair.Value, qtime, open, high, low, close, volume);
                            }
                            pbarstr = "";
                            pbarstr = pbarstr.PadRight(50, '#');
                            Console.WriteLine("\r" + pbarfmt, line_count, pbarstr, 100);
                        } // using StreamReader
                    } // using DataLoader
                } // using Database
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

            //Console.WriteLine("\r\n Для завершения программы нажмите клавишу Enter.");
            //Console.Read();
        }

        static StreamReader OpenFxArhiveFile(string filename)
        {
            if (!File.Exists(filename))
            {
                throw new ApplicationException("Файл с архивом котировок не найден (" + filename + ").");
            }
            return new StreamReader(filename, Encoding.GetEncoding("windows-1251"), true);
        }
    }
}
