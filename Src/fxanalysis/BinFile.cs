using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using FxMath;

namespace fxanalysis
{
    class BinFile
    {
        public BinFile(BinaryWriter binfile)
        {
            file = binfile;
        }
        public void WriteHeader(string pair, Periods p, short pip, uint count, DateTime first, DateTime last)
        {
            file.Write(pair); // Это займет 7 байт
            file.Write((byte)(0xAA)); // это просто выровнит до 8-ми
            file.Write((short)p);
            file.Write(pip);
            file.Write(count);
            file.Write(first.ToBinary());
            file.Write(last.ToBinary());
        }
        public void WriteQuote(uint index, Quote q)
        {
            // формат Open-High-Low-Close
            file.Write(index);
            file.Write(q.volume);
            file.Write(q.time.ToBinary());
            file.Write(q.open);
            file.Write(q.high);
            file.Write(q.low);
            file.Write(q.close);
        }
        public void WriteQuote(uint index, int volume, DateTime time, float open, float high, float low, float close)
        {
            Quote q;
            q.volume = volume;
            q.time = time;
            q.open = open;
            q.high = high;
            q.low = low;
            q.close = close;
            WriteQuote(index, q);
        }
        public static Quote[] ReadBinFile(string binfile, out string pair, out short pip, out Periods avgtype, out DateTime first_date, out DateTime last_date)
        {
            Quote[] quotes = null;
            // Чтение данных
            using (BinaryReader bin = new BinaryReader(File.Open(Utils.CorrectFilePath(binfile), FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                pair = bin.ReadString();
                bin.ReadByte(); // пропускаем заполнитель 0xAA
                avgtype = (Periods)bin.ReadInt16();
                pip = bin.ReadInt16();
                string qfmt = Quote.FloatFormat(pip);
                uint count = bin.ReadUInt32();
                first_date = DateTime.FromBinary(bin.ReadInt64());
                last_date = DateTime.FromBinary(bin.ReadInt64());
                Console.WriteLine(" Range of {0} from {1} to {2}", pair, first_date, last_date);
                Console.WriteLine(" Count of quotes: {0}", count);
                quotes = new Quote[count];
                Console.Write(" Reading: {0,6:#00.0%}", 0.0);
                for (int i = 0; i < count; i++)
                {
                    if (bin.ReadUInt32() != i)
                    {
                        throw new ApplicationException("Ошибка в данных бинарного файла - неправильный индекс");
                    }
                    quotes[i].volume = bin.ReadInt32();
                    quotes[i].time = DateTime.FromBinary(bin.ReadInt64());
                    quotes[i].open = bin.ReadSingle();
                    quotes[i].high = bin.ReadSingle();
                    quotes[i].low = bin.ReadSingle();
                    quotes[i].close = bin.ReadSingle();
                    if ((i + 1) % 3571 == 0 || (i + 1) == count)
                    {
                        Console.Write("\b\b\b\b\b\b{0,6:#00.0%}", (double)(i + 1) / (double)count);
                    }
                } // for (int i = 0; i < count; i++)\
                Console.WriteLine();
            } // using BinaryReader srcbin
            return quotes;
        }
        private BinaryWriter file;
    }
}
