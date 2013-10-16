using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

using DbLoging;
using GenericMethods;
using ntec_query_collector;

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

            //#################################################################################################################
            // CONFIGs
            string configPath = "config.xml";

            /* ntec sites */
            ArrayList siteTitles = Generic.ReadXml(configPath, "title");
            ArrayList dbPaths = Generic.ReadXml(configPath, "db_path");
            ArrayList defaultXmlFolder = Generic.ReadXml(configPath, "default_xml_folder");

            LoadntecDbData(30, dbPaths, defaultXmlFolder, siteTitles);

            // xml schema ctrl
            bool xmlSchema = false; try { xmlSchema = Generic.ReadXml(configPath, "xml_schema")[0].ToString().Equals("true", StringComparison.CurrentCultureIgnoreCase); } catch {}

            // timeout
            int maxWaitTime = 15; try { maxWaitTime = Convert.ToInt32(Generic.ReadXml(configPath, "timeout")[0].ToString()); } catch { }

            // max threads
            int maxThreads = 4; try { maxThreads = Convert.ToInt32(Generic.ReadXml(configPath, "max_parallel_queries")[0].ToString()); } catch { }

            //#################################################################################################################

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
            if (!query.ContainsKey(key))
                query.Add(key, value);
            else
                query[key] = value;

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
                    Console.WriteLine("PAGE: " + query["page_title"] + " FRAME: " + query["frame_title"] + " STATUS: " + query["status"]);
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
                                                FrameTitle = query["frame_title"],
                                                PageTitle = query["page_title"],
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

        /// <summary>
        /// Get data from site database
        /// </summary>
        private static void LoadntecDbData(int scheduleInterval, ArrayList dbPathList, ArrayList xmlFolderPathList, ArrayList siteNameList)
        {
            if (dbPathList.Count != xmlFolderPathList.Count || dbPathList.Count != siteNameList.Count) return;

            for (int i = 0; i < dbPathList.Count; i++)
            {
                string siteName = siteNameList[i].ToString();
                string dbPath = dbPathList[i].ToString();
                string xmlFolderPath = xmlFolderPathList[i].ToString();

                Collector collector = new Collector(scheduleInterval, dbPath, xmlFolderPath);

                List<Dictionary<string, string>> dbQuery = collector.GetQueries();

                for (int k = 0; k < dbQuery.Count; k++)
                {
                    dbQuery[k] = AddKeyStatus(dbQuery[k], "status", "");
                    dbQuery[k] = AddKeyStatus(dbQuery[k], "site_name", siteName);
                    dbQuery[k] = AddKeyStatus(dbQuery[k], "exec_time", "");
                }

                if (dbQuery.Count > 0) _queries.AddRange(dbQuery);
            }
        }
    }
}
