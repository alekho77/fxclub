using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FirebirdSql.Data.FirebirdClient;
using FirebirdSql.Data.Isql;
using System.Data;

namespace FxTestDb
{
    using IDataRow = IDictionary<string, object>;

    public class DataLoader : IDisposable
    {
        public DataLoader(FbConnection con)
        {
            Connection = con;
            Transaction = Connection.BeginTransaction();
            Count = 0;
            //StartTime = DateTime.Now;
            //TotalCount = 0;
        }

        public void Dispose()
        {
            Transaction.Commit();
            Transaction.Dispose();
        }

        public void Push(int pid, DateTime qtime, double openrate, double highrate, double lowrate, double closerate, int? volume)
        {
            // добавляем базу отпечатков
            using (FbCommand cmd = new FbCommand("insert into candles (pid, qtime, openrate, highrate, lowrate, closerate, volume) values(@pid, @qtime, @openrate, @highrate, @lowrate, @closerate, @volume)", Connection, Transaction))
            {
                cmd.Parameters.Add("@pid", pid);
                cmd.Parameters.Add("@qtime", qtime);
                cmd.Parameters.Add("@openrate", openrate);
                cmd.Parameters.Add("@highrate", highrate);
                cmd.Parameters.Add("@lowrate", lowrate);
                cmd.Parameters.Add("@closerate", closerate);
                cmd.Parameters.Add("@volume", volume);
                cmd.ExecuteNonQuery();
            }
            //TotalCount++;
            if (++Count == CommintCache)
            {
                Transaction.CommitRetaining();
                Count = 0;
                //if (TotalCount >= 100000)
                //{
                //    TimeSpan dt = DateTime.Now - StartTime;
                //    throw new ApplicationException("Время загрузки " + TotalCount.ToString() + " элементов: " + dt.ToString());
                //}
            }
        }

        private FbConnection Connection;
        private FbTransaction Transaction;
        private const int CommintCache = 1000;
        private int Count;
        //private DateTime StartTime;
        //private int TotalCount;
    }

    public class Database : IDisposable
    {
        public Database(string dbfilename)
        {
            FbConnectionStringBuilder ConnectionStr = new FbConnectionStringBuilder();
            ConnectionStr.DataSource = "localhost";
            ConnectionStr.Database = dbfilename;
            ConnectionStr.ServerType = FbServerType.Default;
            ConnectionStr.Charset = "UNICODE_FSS";
            ConnectionStr.UserID = "SYSDBA";
            ConnectionStr.Password = "masterkey";

            Connection = new FbConnection(ConnectionStr.ToString());
            Connection.Open();
        }

        public void Dispose()
        {
            Connection.Close();
            Connection.Dispose();
        }

        public int? GetPairID(string pairname)
        {
            if (pairname.Length == 6)
            {
                string base_code = pairname.Substring(0, 3);
                string quote_code = pairname.Substring(3, 3);
                using (FbCommand cmd = new FbCommand("select pid from pairs where base_cid=(select cid from currencies where code=@base_code) and quote_cid=(select cid from currencies where code=@quote_code)", Connection))
                {
                    cmd.Parameters.Add("@base_code", base_code);
                    cmd.Parameters.Add("@quote_code", quote_code);
                    using (IDataReader data = cmd.ExecuteReader())
                    {
                        if (data.Read())
                        {
                            return Convert.ToInt32(data["PID"]);
                        }
                    }
                }
            }
            return null;
        }

        public DataLoader BeginDataLoad()
        {
            return new DataLoader(Connection);
        }

        public IList<IDataRow> SelectPairs()
        {
            using (FbCommand cmd = new FbCommand("select pairs.*, (cb.code || cq.code) as name from pairs join currencies cb on cb.cid=pairs.base_cid join currencies cq on cq.cid=pairs.quote_cid", Connection))
            {
                using (IDataReader data = cmd.ExecuteReader())
                {
                    return TranslateSelect(data);
                }
            }
        }

        public IDataRow GetPair(int pid)
        {
            using (FbCommand cmd = new FbCommand("select pairs.*, (cb.code || cq.code) as name from pairs "+
                                                    "join currencies cb on cb.cid=pairs.base_cid "+
                                                    "join currencies cq on cq.cid=pairs.quote_cid "+
                                                 "where pairs.pid=@pid", Connection))
            {
                cmd.Parameters.Add("@pid", pid);
                using (IDataReader data = cmd.ExecuteReader())
                {
                    if (data.Read())
                    {
                        return TranslateRow(data);
                    }
                }
            }
            return null;
        }

        public IDataRow GetPairStatistic(int pid)
        {
            using (FbCommand cmd = new FbCommand("select "+
                                                    "min(qtime) as first, max(qtime) as last, count(*), "+
                                                    "avg(openrate) as openavg, min(openrate) as openmin, max(openrate) as openmax, "+
                                                    "avg(highrate) as highavg, min(highrate) as highmin, max(highrate) as highmax, "+
                                                    "avg(lowrate) as lowavg, min(lowrate) as lowmin, max(lowrate) as lowmax, "+
                                                    "avg(closerate) as closeavg, min(closerate) as closemin, max(closerate) as closemax, "+
                                                    "avg(volume) as volavg, min(volume) as volmin, max(volume) as volmax, "+
                                                    "(select count(c.volume) from candles c where c.pid=@pid and c.volume is null) as nullvol "+
                                                "from candles where pid=@pid", Connection))
            {
                cmd.Parameters.Add("@pid", pid);
                using (IDataReader data = cmd.ExecuteReader())
                {
                    if (data.Read() && !(data["FIRST"] is DBNull))
                    {
                        return TranslateRow(data);
                    }
                }
            }
            return null;
        }

        public bool CheckCandle(int pid, DateTime qtime)
        {
            qtime = qtime.AddMinutes(-1);
            if (CheckCache == null || (CheckCacheTime.Year != qtime.Year) || (CheckCacheTime.Month != qtime.Month) || (CheckCacheTime.Day != qtime.Day))
            {
                CheckCache = new bool[24, 60];
                CheckCacheTime = qtime;
                DateTime from = new DateTime(qtime.Year, qtime.Month, qtime.Day, 0, 0, 30); // котировка замеряется по окончании периода
                DateTime to = from.AddDays(1);
                using (FbCommand cmd = new FbCommand("select qtime from candles where pid=@pid and (qtime between @from and @to)", Connection))
                {
                    cmd.Parameters.Add("@pid", pid);
                    cmd.Parameters.Add("@from", from);
                    cmd.Parameters.Add("@to", to);
                    using (IDataReader data = cmd.ExecuteReader())
                    {
                        while (data.Read())
                        {
                            DateTime this_qtime = Convert.ToDateTime(data["QTIME"]).AddMinutes(-1); // котировка замерялась по окончании периода
                            if (CheckCache[this_qtime.Hour, this_qtime.Minute])
                            {
                                throw new ApplicationException("Котировка уже была '" + this_qtime.ToString() + "'");
                            }
                            CheckCache[this_qtime.Hour, this_qtime.Minute] = true;
                        }
                    }
                }
            }
            return CheckCache[qtime.Hour, qtime.Minute];
        }

        public bool? CheckHoliday(DateTime day)
        {
            using (FbCommand cmd = new FbCommand("select dayoff from holidays where mm=@mm and dd=@dd and yyyy=@yyyy", Connection))
            {
                cmd.Parameters.Add("@mm", day.Month);
                cmd.Parameters.Add("@dd", day.Day);
                cmd.Parameters.Add("@yyyy", day.Year);
                using (IDataReader data = cmd.ExecuteReader())
                {
                    if (data.Read())
                    {
                        return Convert.ToInt32(data["DAYOFF"]) == 1;
                    }
                }
            }
            using (FbCommand cmd = new FbCommand("select dayoff from holidays where mm=@mm and dd=@dd and yyyy is null", Connection))
            {
                cmd.Parameters.Add("@mm", day.Month);
                cmd.Parameters.Add("@dd", day.Day);
                using (IDataReader data = cmd.ExecuteReader())
                {
                    if (data.Read())
                    {
                        return Convert.ToInt32(data["DAYOFF"]) == 1;
                    }
                }
            }
            return null;
        }

        public IDataRow GetCandle(int pid, DateTime qtime)
        {
            qtime = qtime.AddMinutes(-1);
            if (CandleCache == null || (CandleCacheTime.Year != qtime.Year) || (CandleCacheTime.Month != qtime.Month) || (CandleCacheTime.Day != qtime.Day))
            {
                CandleCache = new IDataRow[24, 60];
                CandleCacheTime = qtime;
                DateTime from = new DateTime(qtime.Year, qtime.Month, qtime.Day, 0, 0, 30); // котировка замеряется по окончании периода
                DateTime to = from.AddDays(1);
                using (FbCommand cmd = new FbCommand("select * from candles where pid=@pid and (qtime between @from and @to)", Connection))
                {
                    cmd.Parameters.Add("@pid", pid);
                    cmd.Parameters.Add("@from", from);
                    cmd.Parameters.Add("@to", to);
                    using (IDataReader data = cmd.ExecuteReader())
                    {
                        while (data.Read())
                        {
                            DateTime this_qtime = Convert.ToDateTime(data["QTIME"]).AddMinutes(-1); // котировка замерялась по окончании периода
                            if (CandleCache[this_qtime.Hour, this_qtime.Minute] != null)
                            {
                                throw new ApplicationException("Котировка уже была '" + this_qtime.ToString() + "'");
                            }
                            CandleCache[this_qtime.Hour, this_qtime.Minute] = TranslateRow(data);
                        }
                    }
                }
            }
            return CandleCache[qtime.Hour, qtime.Minute];
        }

        /// <summary>
        /// Разбирает одну запись из выборки БД по столбцам в ассоативный список.
        /// </summary>
        /// <param name="data">Текущая запись из БД</param>
        /// <returns>Ассоативный список "столбец = значение"</returns>
        private IDataRow TranslateRow(IDataRecord data)
        {
            IDataRow row = new Dictionary<string, object>();
            for (int col = 0; col < data.FieldCount; col++)
            {
                row.Add(data.GetName(col), data[col] is DBNull ? null : data[col]);
            }
            return row;
        }

        /// <summary>
        /// Разбирает выборку из БД в список по строкам.
        /// </summary>
        /// <param name="data">Адаптер выборки из БД</param>
        /// <returns>Список ассоативных списков</returns>
        private IList<IDataRow> TranslateSelect(IDataReader data)
        {
            IList<IDataRow> list = new List<IDataRow>();
            while (data.Read())
            {
                list.Add(TranslateRow(data));
            }
            return list;
        }

        private FbConnection Connection;
        private bool[,] CheckCache = null;
        private DateTime CheckCacheTime;
        private IDataRow[,] CandleCache = null;
        private DateTime CandleCacheTime;
    }
}
