using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace fxanalysis
{
    class Man : ICommand
    {
        public bool Execute(IList<string> cmd_params)
        {
            if (cmd_params.Count == 1)
            {
                Show(cmd_params[0]);
            }
            else
            {
                Show("help");
            }
            return true;
        }
        public static void Show(string command)
        {
            if (command == null)
            {
                Console.WriteLine(" Пример использования:");
                Console.WriteLine(" fxanalysis [options] command [params]");
                Console.WriteLine(" options  - опции работы утилиты:");
                Console.WriteLine(" -nopause - завершение работы программы без ожидание нажатия клавиши пользователем");
                Console.WriteLine();
                Console.WriteLine(" command  - одна из команд {help, prepare, average, spec, order, test, gputest}");
                Console.WriteLine(" params   - соотвествующие параметры команды");
                Console.WriteLine();
                Show("help");
            }
            else
            {
                switch (command)
                {
                    case "help":
                        Console.WriteLine(" Получение справки:");
                        Console.WriteLine(" fxanalysis [options] help [command]");
                        Console.WriteLine(" options    - опции работы утилиты (см. справку к 'fxanalysis')");
                        Console.WriteLine(" [command]  - одна из возможных команд или пусто для получения данной справки");
                        break;
                    case "prepare":
                        Console.WriteLine(" Подготовка данных:");
                        Console.WriteLine(" fxanalysis [options] prepare database pair");
                        Console.WriteLine(" options  - опции работы утилиты (см. справку к 'fxanalysis')");
                        Console.WriteLine(" database - путь к файлу с БД");
                        Console.WriteLine(" pair     - название котировки: EURUSD, GBPUSD, USDCHF, USDJPY и т.д.");
                        Console.WriteLine(" Результат будет сохранен в файл <pair>.int.bin");
                        break;
                    case "average":
                        Console.WriteLine(" Осреднение данных:");
                        Console.WriteLine(" fxanalysis [options] average file.bin AT");
                        Console.WriteLine(" options  - опции работы утилиты (см. справку к 'fxanalysis')");
                        Console.WriteLine(" file.bin - файл с подготовленными данными (<pair>.int.bin)");
                        Console.WriteLine(" AT       - тип осреднения (Y, M6, M3, M1, w, d, h8, h4, h1, m30, m15, m5, m)");
                        Console.WriteLine(" Результат будет сохранен в файлы <pair>.<AT>.bin и <pair>.<AT>.candles.dat");
                        break;
                    case "spec":
                        Console.WriteLine(" Спектральный анализ:");
                        Console.WriteLine(" fxanalysis [options] spec apow threads file.bin winfunc");
                        Console.WriteLine(" options  - опции работы утилиты (см. справку к 'fxanalysis')");
                        Console.WriteLine(" apow     - степень аппроксимирующего полинома [0..3]");
                        Console.WriteLine(" threads  - число потоков для вычислений на CPU или слово 'CUDA'");
                        Console.WriteLine(" file.bin - путь к файлу с подготовленными данными");
                        Console.WriteLine(" winfunc  - оконная функция (rect, hamming, blackman)");
                        break;
                    case "instrument":
                        Console.WriteLine(" Доходность инструмента от частоты совершения сделок:");
                        Console.WriteLine(" fxanalysis [options] instrument pair.int.bin");
                        Console.WriteLine(" options      - опции работы утилиты (см. справку к 'fxanalysis')");
                        Console.WriteLine(" pair.int.bin - путь к файлу с подготовленными данными");
                        Console.WriteLine(" Результат будет сохранен в файл <pair>.instrument.dat");
                        break;
                    case "order":
                        Console.WriteLine(" Статистика по ордерам:");
                        Console.WriteLine(" fxanalysis [options] order pair.int.bin timeout scan a/b/c/d/e/f a/b/c/d/e/f");
                        Console.WriteLine(" options      - опции работы утилиты (см. справку к 'fxanalysis')");
                        Console.WriteLine(" pair.int.bin - путь к файлу с подготовленными данными");
                        Console.WriteLine(" timeout      - максимальный период ожидания (Y, M6, M3, M1, w, d, h8, h4, h1, m30, m15, m5, m)");
                        Console.WriteLine(" scan         - тип сканера (single, multi, cuda)");
                        Console.WriteLine(" a/b/c/d/e/f  - диапазон сканирования, первый для BUY, второй для SELL");
                        Console.WriteLine("                a - start profit, b - end profit, c - step profit ('-' - поумолчанию)");
                        Console.WriteLine("                d - start loss, e - end loss, f - step loss ('-' - поумолчанию)");
                        Console.WriteLine("                возможно указание слова skip для пропуска анализа BUY или SELL");
                        Console.WriteLine(" Результат будет сохранен в файл <pair>.<timeout>.order.dat");
                        break;
                    case "position":
                        Console.WriteLine(" Статистика по позициям конкретного ордера:");
                        Console.WriteLine(" fxanalysis [options] position type pair.int.bin timeout takeprofit stoploss");
                        Console.WriteLine(" options      - опции работы утилиты (см. справку к 'fxanalysis')");
                        Console.WriteLine(" type         - тип позиции buy/sell");
                        Console.WriteLine(" pair.int.bin - путь к файлу с подготовленными данными");
                        Console.WriteLine(" timeout      - максимальный период ожидания (Y, M6, M3, M1, w, d, h8, h4, h1, m30, m15, m5, m)");
                        Console.WriteLine(" takeprofit   - значение TakeProfit ордера");
                        Console.WriteLine(" stoploss     - значение StopLoss ордера");
                        Console.WriteLine(" Результат будет сохранен в файл <pair>.<timeout>.position.dat");
                        break;
                    case "macd":
                        Console.WriteLine(" Статистика по алгоритму MACD:");
                        Console.WriteLine(" fxanalysis [options] macd type pair.int.bin timeout takeprofit stoploss history");
                        Console.WriteLine(" options      - опции работы утилиты (см. справку к 'fxanalysis')");
                        Console.WriteLine(" type         - тип позиции buy/sell");
                        Console.WriteLine(" pair.int.bin - путь к файлу с подготовленными данными");
                        Console.WriteLine(" timeout      - максимальный период ожидания (Y, M6, M3, M1, w, d, h8, h4, h1, m30, m15, m5, m)");
                        Console.WriteLine(" takeprofit   - значение TakeProfit ордера");
                        Console.WriteLine(" stoploss     - значение StopLoss ордера");
                        Console.WriteLine(" history      - период наколения истории (Y, M6, M3, M1, w, d, h8, h4, h1, m30, m15, m5, m)");
//                         Console.WriteLine(" Результат будет сохранен в файл <pair>.<timeout>.position.dat");
                        break;
                    case "test":
                        Console.WriteLine(" Запуск некоторого теста:");
                        Console.WriteLine(" fxanalysis [options] test [some data]");
                        Console.WriteLine(" options  - опция работы утилиты (см. справку к 'fxanalysis')");
                        break;
                    case "gputest":
                        Console.WriteLine(" Вывод информации о CUDA:");
                        Console.WriteLine(" fxanalysis [options] gputest");
                        Console.WriteLine(" options  - опция работы утилиты (см. справку к 'fxanalysis')");
                        break;
                    default:
                        Show(null);
                        break;
                }
            }
        }
    }
}
