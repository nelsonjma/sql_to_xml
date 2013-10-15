using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace DbLoging
{
    class Loging
    {
        protected DbLog _log;
        protected string _dbPath;

        public Loging(string dbPath)
        {
            _log = null;
            _dbPath = dbPath;
        }

        public void ConnectToDataBase()
        {
            try
            {
                if (_dbPath == string.Empty) return;
                
                if (_log == null)
                    _log = new DbLog(new SQLiteConnection(@"Data Source=" + _dbPath + ";Version=3;"));
            }
            catch (Exception ex)
            {
                throw new Exception("Error connection to database: " + ex.Message);
            }
        }

        public void WriteLogs(IEnumerable<Log> logs)
        {
            try
            {
                if (_log == null) return;

                _log.Log.InsertAllOnSubmit(logs);

                _log.Log.Context.SubmitChanges();
            }
            catch (Exception ex)
            {
                throw new Exception("Error inserting data: " + ex.Message);
            }
            
        }

    }
}
