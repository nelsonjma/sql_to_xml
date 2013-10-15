using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using DbLoging;

namespace sql_to_xml
{
    class Program
    {
        /// query information
        private static List<Dictionary<string, string>> _queries;

        private static readonly DateTime StartTime = DateTime.Now;

        static void Main(string[] args)
        {
            Console.WriteLine("Read Config File");

            _queries = new List<Dictionary<string, string>>();

            _queries.Add(new Dictionary<string, string> { { "title", "a" }, { "sql", "selecta 'a' as hello" }, { "conn", @"Data Source=C:\Users\xyon\Desktop\Codigo\demo_northwind.db;Version=3;" }, { "xml_file", @"C:\temp\demo_a.xml" } });
            _queries.Add(new Dictionary<string, string> { { "title", "b" }, { "sql", "select 'b' as hello" }, { "conn", @"Data Source=C:\Users\xyon\Desktop\Codigo\demo_northwind.db;Version=3;" }, { "xml_file", @"C:\temp\demo_b.xml" } });
            _queries.Add(new Dictionary<string, string> { { "title", "c" }, { "sql", "select 'c' as hello" }, { "conn", @"Data Source=C:\Users\xyon\Desktop\Codigo\demo_northwind.db;Version=3;" }, { "xml_file", @"C:\temp\demo_c.xml" } });
            _queries.Add(new Dictionary<string, string> { { "title", "d" }, { "sql", "select 'd' as hello" }, { "conn", @"Data Source=C:\Users\xyon\Desktop\Codigo\demo_northwind.db;Version=3;" }, { "xml_file", @"C:\temp\demo_d.xml" } });
            _queries.Add(new Dictionary<string, string> { { "title", "e" }, { "sql", "select 'e' as hello" }, { "conn", @"Data Source=C:\Users\xyon\Desktop\Codigo\demo_northwind.db;Version=3;" }, { "xml_file", @"C:\temp\demo_e.xml" } });
            _queries.Add(new Dictionary<string, string> { { "title", "f" }, { "sql", "select 'f' as hello" }, { "conn", @"Data Source=C:\Users\xyon\Desktop\Codigo\demo_northwind.db;Version=3;" }, { "xml_file", @"C:\temp\demo_f.xml" } });
            _queries.Add(new Dictionary<string, string> { { "title", "g" }, { "sql", "select 'g' as hello" }, { "conn", @"Data Source=C:\Users\xyon\Desktop\Codigo\demo_northwind.db;Version=3;" }, { "xml_file", @"" } });
            _queries.Add(new Dictionary<string, string> { { "title", "h" }, { "sql", "select 'h' as hello" }, { "conn", @"Data Source=C:\Users\xyon\Desktop\Codigo\demo_northwind.db;Version=3;" }, { "xml_file", @"C:\temp\demo_h.xml" } });
            _queries.Add(new Dictionary<string, string> { { "title", "i" }, { "sql", "select 'i' as hello" }, { "conn", @"Data Source=C:\Users\xyon\Desktop\Codigo\demo_northwind.db;Version=3;" }, { "xml_file", @"C:\temp\demo_i.xml" } });

            // use xml schema or not
            bool xmlSchema = true;

            // max wait time
            int maxWaitTime = 15;

            // add status to query list
            for (int i = 0; i < _queries.Count; i++)
            {
                _queries[i] = AddKeyStatus(_queries[i], "status", "");
                _queries[i] = AddKeyStatus(_queries[i], "site_name", "");
                _queries[i] = AddKeyStatus(_queries[i], "exec_time", "");
            }

            // launch the threads
            int maxThreads = 4; 
            // 3 threads are 0.1.2 we need to remve the 3 do that its not 0.1.2.3
            maxThreads = maxThreads - 1;
            List<Thread> threads = new List<Thread>();

            Console.WriteLine("Start loading threads");

            for (int i = 0; i < _queries.Count; i++)
            {
                Dictionary<string, string> query;
                int index;
                int auxMaxWaitTime;
                bool auxXmlSchema;

                Monitor.Enter(_queries);
                try
                {
                    query = _queries[i];
                    index = i;
                    auxMaxWaitTime = maxWaitTime;
                    auxXmlSchema = xmlSchema;
                }
                finally
                {
                    Monitor.Exit(_queries);                    
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

            Console.WriteLine("Writing log");

            WriteToLogScreenMessage();

            Console.WriteLine("Job ended");
            Thread.Sleep(5000);

            Console.Read();
        }

        /// <summary>
        /// Run The Thread Controler, it runs in a thread and launch another thread that will run the query
        /// </summary>
        private static void ConnectionLaunch(int index, int maxWaitTime, Dictionary<string, string> queryObj, bool writeXmlSchema)
        {
            double waitTime = 0;

            try
            {
                string conn = queryObj["conn"];
                string sql = queryObj["sql"];
                string file = queryObj["xml_file"];

                GetDbData dbData = new GetDbData();

                UpdateStatusMessage(index, "launch query");

                dbData.RunQuery(conn, sql);

                while (dbData.ThreadAlive)
                {
                    waitTime = dbData.WaitTime;

                    if (waitTime > maxWaitTime)
                    {
                        dbData.KillThread();

                        UpdateExecutionTime(index, waitTime);
                        UpdateStatusMessage(index, "thread timedout");

                        return;
                    }

                    UpdateExecutionTime(index, waitTime);
                    UpdateStatusMessage(index, "running for: " + waitTime + " seconds");

                    Thread.Sleep(100);
                }

                if (dbData.ErrorMsg == string.Empty)
                {
                    UpdateStatusMessage(index, "success: total time " + waitTime + " seconds");

                    // generate xml
                    dbData.GenerateXml(file, writeXmlSchema);
                }
                else
                {
                    UpdateStatusMessage(index, "Error: " + dbData.ErrorMsg);
                }

                // last time in the thread
                UpdateExecutionTime(index, waitTime); 
            }
            catch (Exception ex)
            {
                UpdateExecutionTime(index, waitTime); 
                UpdateStatusMessage(index, "Error: " + ex.Message);
            }
        }

        /// <summary>
        /// Updates message of a thread
        /// </summary>
        private static void UpdateStatusMessage(int index, string message)
        {
            Monitor.Enter(_queries);
            try
            {
                _queries[index]["status"] = message;    
            }
            finally
            {
                Monitor.Exit(_queries);
            }
        }

        /// <summary>
        /// Store the execution time of the query
        /// </summary>
        private static void UpdateExecutionTime(int index, double execTime)
        {
            _queries[index]["exec_time"] = execTime.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Adds a key to dictionary
        /// </summary>
        private static Dictionary<string, string> AddKeyStatus(Dictionary<string, string> query, string key, string value)
        {
            if (!query.ContainsKey(key)) query.Add(key, value);

            return query;
        }

        /// <summary>
        /// Thread start/end control
        /// </summary>
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

        /// <summary>
        /// Send query status to console
        /// </summary>
        private static void PrintScreenMessage()
        {
            Monitor.Enter(_queries);
            try
            {
                Console.Clear();

                Console.WriteLine("Total Time:" + Math.Round((DateTime.Now - StartTime).TotalSeconds) + " seconds");

                foreach (Dictionary<string, string> query in _queries)
                {
                    Console.WriteLine("title: " + query["title"] + " / " + "status:" + query["status"]);
                }
            }
            finally
            {
                Monitor.Exit(_queries);
            }
        }

        private static void WriteToLogScreenMessage()
        {
            List<Log> logs = new List<Log>();

            try
            {
                foreach (Dictionary<string, string> query in _queries)
                {
                    Log log = new Log
                                            {
                                                Conn = query["conn"],
                                                Created = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"),
                                                ExecTime = query["exec_time"],
                                                SQL = query["sql"],
                                                SiteName = query["site_name"],
                                                Status = query["status"],
                                                Title = query["title"],
                                                XMLFile = query["xml_file"]
                                            };
                    logs.Add(log);
                }

                Loging writeLog = new Loging("sql_to_xml_log.db");
                writeLog.ConnectToDataBase();
                writeLog.WriteLogs(logs);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
