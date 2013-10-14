using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;

namespace sql_to_xml
{
    class Program
    {
        /// query information
        private static List<Dictionary<string, string>> _queries;

        private static readonly DateTime StartTime = DateTime.Now;

        static void Main(string[] args)
        {
            Start:

            _queries = new List<Dictionary<string, string>>();

            _queries.Add(new Dictionary<string, string> { { "title", "a" }, { "sql", "b" }, { "conn", "c" }, { "xml_file", "d" } });
            _queries.Add(new Dictionary<string, string> { { "title", "b" }, { "sql", "f" }, { "conn", "g" }, { "xml_file", "h" } });
            _queries.Add(new Dictionary<string, string> { { "title", "c" }, { "sql", "j" }, { "conn", "l" }, { "xml_file", "m" } });
            _queries.Add(new Dictionary<string, string> { { "title", "d" }, { "sql", "b" }, { "conn", "c" }, { "xml_file", "d" } });
            _queries.Add(new Dictionary<string, string> { { "title", "e" }, { "sql", "f" }, { "conn", "g" }, { "xml_file", "h" } });
            _queries.Add(new Dictionary<string, string> { { "title", "f" }, { "sql", "j" }, { "conn", "l" }, { "xml_file", "m" } });
            _queries.Add(new Dictionary<string, string> { { "title", "g" }, { "sql", "b" }, { "conn", "c" }, { "xml_file", "d" } });
            _queries.Add(new Dictionary<string, string> { { "title", "h" }, { "sql", "f" }, { "conn", "g" }, { "xml_file", "h" } });
            _queries.Add(new Dictionary<string, string> { { "title", "i" }, { "sql", "j" }, { "conn", "l" }, { "xml_file", "m" } });

            // use xml schema or not
            bool xmlSchema = true;

            // max wait time
            int maxWaitTime = 15;

            // add status to query list
            for (int i = 0; i < _queries.Count; i++)
            {
                _queries[i] = AddKeyStatus(_queries[i], "status", "");
                _queries[i] = AddKeyStatus(_queries[i], "site_name", "");
            }

            // launch the threads
            int maxThreads = 4; 
            // 3 threads are 0.1.2 we need to remve the 3 do that its not 0.1.2.3
            maxThreads = maxThreads - 1;
            List<Thread> threads = new List<Thread>();

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

            Console.WriteLine("Job ended");
            Thread.Sleep(5000);

            goto Start;

            Console.Read();
        }

        /// <summary>
        /// Run The Thread Controler, it runs in a thread and launch another thread that will run the query
        /// </summary>
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
    }
}
