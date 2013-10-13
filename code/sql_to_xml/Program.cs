using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace sql_to_xml
{
    class Program
    {

        private static QueryExecutionCtrl QueryExecCtrl = new QueryExecutionCtrl();

        private static readonly DateTime StartTime = DateTime.Now;

        static void Main(string[] args)
        {
            Start:

            List<Dictionary<string, string>> queries = new List<Dictionary<string, string>>();

            queries.Add(new Dictionary<string, string> { { "title", "a" }, { "sql", "b" }, { "conn", "c" }, { "xml_file", "d" } });
            queries.Add(new Dictionary<string, string> { { "title", "b" }, { "sql", "f" }, { "conn", "g" }, { "xml_file", "h" } });
            queries.Add(new Dictionary<string, string> { { "title", "c" }, { "sql", "j" }, { "conn", "l" }, { "xml_file", "m" } });
            queries.Add(new Dictionary<string, string> { { "title", "d" }, { "sql", "b" }, { "conn", "c" }, { "xml_file", "d" } });
            queries.Add(new Dictionary<string, string> { { "title", "e" }, { "sql", "f" }, { "conn", "g" }, { "xml_file", "h" } });
            queries.Add(new Dictionary<string, string> { { "title", "f" }, { "sql", "j" }, { "conn", "l" }, { "xml_file", "m" } });
            queries.Add(new Dictionary<string, string> { { "title", "g" }, { "sql", "b" }, { "conn", "c" }, { "xml_file", "d" } });
            queries.Add(new Dictionary<string, string> { { "title", "h" }, { "sql", "f" }, { "conn", "g" }, { "xml_file", "h" } });
            queries.Add(new Dictionary<string, string> { { "title", "i" }, { "sql", "j" }, { "conn", "l" }, { "xml_file", "m" } });

            // use xml schema or not
            bool xmlSchema = true;

            // max wait time
            int maxWaitTime = 15;

            // add status to query list
            for (int i = 0; i < queries.Count; i++)
                queries[i] = AddKeyStatus(queries[i]);

            // connect to query thread controller
            QueryExecCtrl.ListQueries = queries;

            // launch the threads
            int maxThreads = 4; 
            // 3 threads are 0.1.2 we need to remve the 3 do that its not 0.1.2.3
            maxThreads = maxThreads - 1;
            List<Thread> threads = new List<Thread>();

            for (int i = 0; i < QueryExecCtrl.ListQueries.Count; i++)
            {
                Dictionary<string, string> query;
                int index;
                int auxMaxWaitTime;
                bool auxXmlSchema;

                Monitor.Enter(QueryExecCtrl);
                try
                {
                    query = QueryExecCtrl.ListQueries[i];
                    index = i;
                    auxMaxWaitTime = maxWaitTime;
                    auxXmlSchema = xmlSchema;
                }
                finally
                {
                    Monitor.Exit(QueryExecCtrl);                    
                }

                Thread th = new Thread(() => ConnectionLaunch(index, auxMaxWaitTime, query, auxXmlSchema));
                th.Start();

                while (th.ThreadState != ThreadState.Running){ Thread.Sleep(1); }

                threads.Add(th);

                CheckIfReadyToNextThread(ref threads, maxThreads);
            }

            CheckIfReadyToNextThread(ref threads, 0);

            // print end message
            PrintScreenMessage();

            Console.WriteLine("Job ended");
            Thread.Sleep(5000);

            goto Start;

            Console.Read();
        }

        private static void ConnectionLaunch(int index, int maxWaitTime, Dictionary<string, string> queryObj, bool writeXmlSchema)
        {
            string conn = queryObj["conn"];
            string sql = queryObj["sql"];
            string file = queryObj["xml_file"];

            double waitTime = 0;

            GetDbData dbData = new GetDbData();

            UpdateStatusMessage(index, "launch query");

            dbData.RunQuery(conn, sql);

            while (dbData.ThreadAlive)
            {
                waitTime = dbData.WaitTime;
                
                if (waitTime > maxWaitTime)
                {
                    dbData.KillThread();
                    UpdateStatusMessage(index, "thread timedout"); 
                    return;
                }

                UpdateStatusMessage(index, "running for: " + waitTime + " seconds");

                Thread.Sleep(100);
            }

            if (dbData.ErrorMsg == string.Empty)
            {
                 UpdateStatusMessage(index, "success: total time " + waitTime + " seconds");
                // generate xml
                //dbData.GenerateXml(file, writeXmlSchema);
            }
            else
                UpdateStatusMessage(index, "Error: " + dbData.ErrorMsg);
        }

        private static void UpdateStatusMessage(int index, string message)
        {
            Monitor.Enter(QueryExecCtrl);
            try
            {
                QueryExecCtrl.ListQueries[index]["status"] = message;    
            }
            finally
            {
                Monitor.Exit(QueryExecCtrl);
            }
        }

        private static Dictionary<string, string> AddKeyStatus(Dictionary<string, string> query)
        {
            if (!query.ContainsKey("status")) query.Add("status", "");

            return query;
        }

        private static void CheckIfReadyToNextThread(ref List<Thread> threads, int maxThreads)
        {
            while (threads.Count > maxThreads)
            {
                Thread.Sleep(500);

                for (int i = 0; i < threads.Count; i++)
                {
                    if (!threads[i].IsAlive)
                        threads.RemoveAt(i);  
                }

                PrintScreenMessage();

                if (threads.Count == 0) return;
            }
        }

        private static void PrintScreenMessage()
        {
            Monitor.Enter(QueryExecCtrl);
            try
            {
                Console.Clear();

                Console.WriteLine("Total Time:" + Math.Round((DateTime.Now - StartTime).TotalSeconds) + " seconds");

                foreach (Dictionary<string, string> query in QueryExecCtrl.ListQueries)
                {
                    Console.WriteLine("title: " + query["title"] + " / " + "status:" + query["status"]);
                }
            }
            finally
            {
                Monitor.Exit(QueryExecCtrl);
            }
        }
    }

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
                
                count++;

                if (count > 10) return;
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
                Random rnd = new Random();


                Thread.Sleep(rnd.Next(1, 20)*1000);

                _dataRetrievedWithSuccess = true;

                return new DataTable();
            }
            catch (Exception ex)
            {
                ErrorMsg = ex.Message;

                return new DataTable();
            }
        }
    }

    /// <summary>
    /// Contains the list of ntec database...
    /// </summary>
    class QueryExecutionCtrl
    {
        public List<Dictionary<string, string>> ListQueries = new List<Dictionary<string, string>>();
    }
}
