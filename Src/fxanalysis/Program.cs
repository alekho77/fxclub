using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using FxMath;
using System.IO;
using System.Globalization;
using System.Threading;

namespace fxanalysis
{
    public interface ICommand
    {
        bool Execute(IList<string> cmd_params);
    }

    class Program
    {
        static void Main(string[] args)
        {
            /*
             * Параметры командной строки:
             * >fxanalysis command options
             *
            */
            Console.WriteLine();
            Console.WriteLine(" Утилита для статистического анализа котировок.");
            Console.WriteLine();

            CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            ci.NumberFormat.NumberDecimalSeparator = ".";
            Thread.CurrentThread.CurrentCulture = ci;

            // Разбор командной строки
            List<string> options = new List<string>();
            string cmdname = null;
            List<string> cmd_params = new List<string>();
            for (int i = 0; i < args.Length; i++)
            {
                if (cmdname == null)
                {
                    if (args[i][0] == '-')
                    {
                        options.Add(args[i].ToLower());
                    }
                    else
                    {
                        cmdname = args[i].ToLower();
                    }
                } 
                else
                {
                    cmd_params.Add(args[i]);
                }
            }

            try
            {
                ICommand cmd = null;
                switch (cmdname)
                {
                    case "help": cmd = new Man(); break;
                    case "prepare": cmd = new Prepare(); break;
                    case "average": cmd = new Average(); break;
                    case "spec": cmd = new Spectrum(); break;
                    case "instrument": cmd = new Instrument(); break;
                    case "order": cmd = new Order(); break;
                    case "test": cmd = new Test(); break;
                    case "gputest": cmd = new GPUTest(); break;
                    case "position": cmd = new Positions(); break;
                    case "macd": cmd = new MACD(); break;
                } // switch (cmdname)
                if (cmd != null)
                {
                    if (!cmd.Execute(cmd_params))
                    {
                        Man.Show(cmdname);
                    }
                } 
                else
                {
                    Man.Show(null);
                }
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

            if (!options.Exists(opt => opt == "-nopause"))
            {
                // Ожидаем завершения работы от пользователя
                Console.WriteLine("\r\n Для завершения программы нажмите клавишу Enter.");
                Console.Read();
            }
        }
    }
}
