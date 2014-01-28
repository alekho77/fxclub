using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FxMath
{
    public static class Utils
    {
        public static bool StrToEnum<E>(string str, ref E value)
        {
            foreach (E val in Enum.GetValues(typeof(E)))
            {
                if (str.ToLower() == Enum.GetName(typeof(E), val).ToLower())
                {
                    value = val;
                    return true;
                }
            }
            return false;
        }
        public static int PeriodToMinutes(Periods p)
        {
            return p < 0 ? -(int)p * (int)Periods.d : (int)p;
        }
        public static string CorrectFilePath(string filename)
        {
            if (!filename.Contains("\\"))
                return ExePath + filename;
            return filename;
        }
        public static string SpanToStr(TimeSpan time)
        {
            // размер вывода 12 символов
            return string.Format("{0,3}:{1:00}:{2:00}:{3:00}", time.Days, time.Hours, time.Minutes, time.Seconds);
        }
        private static string ExePath = Environment.CurrentDirectory + "\\";
    }
}
