using System;
using System.Data;
using System.Threading;
using DbConnection;

namespace GenericMethods
{
    class GetDbData
    {
        private readonly DateTime _nowTime;
        private Thread _thread;
        private DataTable _dataTable;

        private bool _dataRetrievedWithSuccess;

        public string ErrorMsg { get; set; }

        public GetDbData()
        {
            _nowTime = DateTime.Now;

            // return message
            ErrorMsg = string.Empty;

            _dataRetrievedWithSuccess = false;
        }

        /* PUBLIC */
        /// <summary>
        /// Launch the thread that will get the data
        /// </summary>
        public void RunQuery(string conn, string sql)
        {
            _thread = new Thread(() => { _dataTable = GetData(conn, sql); });
            _thread.Start();

            int count = 0;
            while (_thread.ThreadState != ThreadState.Running)
            {
                Thread.Sleep(10); 
                
                count++; if (count > 10) return;
            }
        }

        /// <summary>
        /// Check If thread did not end is job
        /// </summary>
        public bool ThreadAlive
        {
            get { return _thread.IsAlive; }
        }

        public void KillThread()
        {
            _thread.Abort();
        }

        /// <summary>
        /// Get current wait time of the running query
        /// </summary>
        public double WaitTime
        {
            get { return Math.Round((DateTime.Now - _nowTime).TotalSeconds); }
        }

        /// <summary>
        /// Will generate xml data if dara return with sucess
        /// </summary>
        public void GenerateXml(string file, bool writeXmlSchema)
        {
            if (!_dataRetrievedWithSuccess) return;

            if (writeXmlSchema)
                _dataTable.WriteXml(file, XmlWriteMode.WriteSchema);
            else
                _dataTable.WriteXml(file);
        }

        /* PRIVATE */
        /// <summary>
        /// Run Query and return a datatable
        /// </summary>
        private DataTable GetData(string conn, string sql)
        {
            try
            {
                RunQuery runQuery = new RunQuery();

                DataTable dt = runQuery.GetData(conn, sql);

                _dataRetrievedWithSuccess = true;

                return dt;
            }
            catch (Exception ex)
            {
                ErrorMsg = ex.Message;

                return new DataTable();
            }
        }
    }
}
