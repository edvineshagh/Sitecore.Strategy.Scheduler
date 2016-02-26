using System;
using System.Collections.Generic;
using System.IO;
using Sitecore.Diagnostics;
using Sitecore.Strategy.Scheduler.Pipelines;
using System.Text.RegularExpressions;

namespace Sitecore.Strategy.Scheduler.Model
{
    /// <summary>
    /// (Re)Store Agent execution times from external storage.
    /// </summary>
    public class AgentExecutionRepository : IAgentExecutionRepository
    {
        private string _logFile;
        private readonly bool _append;
        private readonly DateTime _defaultLastRuntime;
        private Dictionary<string, DateTime> _lastRunTimes;


        // When appending to file, we need to find to location of the last write operation, 
        // so that we can reload last execution run times.  By storing the last offset
        // in the beginning of the file, we can read the header and seek to last write offset.
        // The following string provides a human readable prefix for the header offset.
        private const string FlushString = "Last flush offset: ";


        /// <summary>
        /// Keep track of agent execution due to worker process recycling.
        /// </summary>
        /// <param name="logFile">Logfile to flush into or retrieve from for last executed time</param>
        public AgentExecutionRepository(string logFile, bool append)
        {
            try
            {
                _append = append;
                _defaultLastRuntime = DateTime.UtcNow;
                _logFile = ParsePath( logFile );
            }
            catch (Exception e)
            {
                _logFile = null;
                Sitecore.Diagnostics.Log.Error(
                    string.Format("Scheduler - Agent execution repository log path {0} is inaccessible.", logFile), e, this);
            }
            
            
            
        }

        /// <summary>
        /// Replace macro string with respective configuration settings 
        /// and return full file path.  For example, replace $(DataFolder) with respective value from configuration file.
        /// </summary>
        /// <param name="logPath">File path to parse</param>
        /// <returns>Full file path</returns>
        private string ParsePath(string logPath)
        {

            var regEx = new Regex(@"\$\(([^)]+)\)");

            foreach (Match match in regEx.Matches(logPath))
            {
                var token = match.Groups[0].Value;
                var param = match.Groups[1].Value;

                logPath = logPath.Replace(token, 
                    Sitecore.Configuration.Settings.GetSetting(param,token)
                    );
            }
            
            logPath = Path.GetFullPath(logPath);

            return logPath;
        }


        /// <summary>
        /// Last execution runtime for each agent is stored via Agent name hash,
        /// because same agent type maybe used more than once.  So, we use the
        /// agent name, which is guaranteed to be unique when the 
        /// SchedulerFactory.NewAgentMediator() creates the agent.
        /// </summary>
        private Dictionary<string, DateTime> LastRunTimes
        {
            get
            {
                if (_lastRunTimes == null && _logFile !=null)
                {
                    try {
                        _lastRunTimes = LoadLastAgentsRunTime();
                        Sitecore.Diagnostics.Log.Info(
                            string.Format("Scheduler - Agent execution repository log {0} is Loaded.", _logFile), this);
                    }
                    catch (Exception e)
                    {
                        _lastRunTimes = new Dictionary<string, DateTime>();

                        Sitecore.Diagnostics.Log.Error(
                            string.Format("Scheduler - Agent execution repository log path {0} is inaccessible.", _logFile), e, this);

                        _logFile = null;
                    
                    }
                }
                return _lastRunTimes;
            }
        }

        /// <summary>
        /// Save agents last runtime into external storage.
        /// </summary>
        /// <param name="priorityAgents">Agents</param>
        
        public void FlushLastAgentRunTimes(AgentPriorityList priorityAgents)
        {
            if (priorityAgents == null || _logFile==null) return;

            try
            {
                // Because the execution time is being passed in, we will replace
                // lastRunTimes with the new values at the end of this module.
                var lastRunTimes = new Dictionary<string, DateTime>();

                long lastOffset= _append ? GetLastFlushOffset() : 0;

                var isAppendToFile = lastOffset > 0;

                // Code Analysis suppression reason see: http://stackoverflow.com/questions/3831676/ca2202-how-to-solve-this-case
                // [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]

                using (var fileStream = new FileStream(_logFile, isAppendToFile ? FileMode.OpenOrCreate : FileMode.Create))
                using (TextWriter writer = new StreamWriter(fileStream))
                {

                    // Write/Update header string to the file
                    //
                    if (isAppendToFile)
                    {
                        
                        fileStream.Seek(0, SeekOrigin.End);
                        string header1 = GetHeaderString(fileStream.Position);
                        fileStream.Seek(0, SeekOrigin.Begin);
                        writer.WriteLine(header1);
                        writer.Flush();
                        fileStream.Seek(0, SeekOrigin.End);

                    }
                    else
                    {
                        string templateHeaderStr = GetHeaderString(0);
                        var headerLen = writer.Encoding.GetBytes(templateHeaderStr + "\n").Length;
                        var header1 = GetHeaderString(headerLen);
                        writer.WriteLine(header1);
                    }

                    string header2 = string.Format("## {0} - {1} ", DateUtil.ToServerTime(DateTime.UtcNow), typeof(AgentMediator).FullName);

                    writer.WriteLine(header2);

                    
                    // Write execution times into file
                    //
                    foreach (var sortedAgentsByName in priorityAgents.Values)
                    {
                        foreach (var agentMediator in sortedAgentsByName.Values)
                        {
                            var agentType = agentMediator.Agent.GetType().AssemblyQualifiedName;

                            var record = string.Format("{0}\t{1}\t{2}",
                                DateUtil.ToIsoDate(DateUtil.ToServerTime(agentMediator.LastRunTime), includeTicks: false, convertToUTC: false),
                                agentMediator.Name, agentType
                                );

                            writer.WriteLine(record);

                            lastRunTimes[agentMediator.Name] = agentMediator.LastRunTime;
                        }
                    }

                    _lastRunTimes = lastRunTimes;
                }
            }
            catch (Exception e)
            {
               Sitecore.Diagnostics.Log.Error(
                   string.Format("Scheduler - Agent execution repository unable to write to {0}.  Turning off history flush.", _logFile), e, this);
            }

        }

        /// <summary>
        /// Returns the header string with a fixed size.  
        /// This helps in implementing a random access solution.
        /// </summary>
        /// <param name="offset">The offset of store for next record.</param>
        /// <returns>Fixed size string</returns>
        private string GetHeaderString(long offset)
        {
            return string.Format("# {0}{1,20}", FlushString, offset);
        }

        /// <summary>
        /// Read the header of the file and obtain the last flush offset.
        /// </summary>
        /// <returns>Last record set offset</returns>
        private long GetLastFlushOffset()
        {
            long lastOffset = 0;
            if (!File.Exists(_logFile))
            {
                return 0;
            }

            //using (TextReader reader = new StreamReader(_logFile))
            using (TextReader reader = File.OpenText(_logFile))
            {
                string line = reader.ReadLine();
                int flushIndex;
                if ((flushIndex = line.LastIndexOf(FlushString)) > 0)
                {
                    lastOffset = long.TryParse(line.Substring(flushIndex + FlushString.Length), out lastOffset) ? lastOffset : 0;
                }
            }
            return lastOffset;
        }

        /// <summary>
        /// Retrieve last runtime for the specified agent.
        /// If the agent is not found, then defaultLastRuntime is returned
        /// </summary>
        /// <param name="agentName">Agent name as defined in the config setting.</param>
        /// <returns></returns>
        public DateTime GetLastRuntime(string agentName)
        {
            var index = agentName;
            var lastRunTime = LastRunTimes.ContainsKey(index) ? LastRunTimes[index] : _defaultLastRuntime;
            return lastRunTime;
        }


        /// <summary>
        /// Retrieve last agent run times from repository
        /// </summary>
        /// <returns>A dictionary agent runtimes that is keyed by agentName /></returns>
        private Dictionary<string, DateTime> LoadLastAgentsRunTime()
        {
            var lastRunTimes = new Dictionary<string, DateTime>();

            if (_logFile == null || !File.Exists(_logFile))
            {

                Sitecore.Diagnostics.Log.Warn(
                    "Scheduler - Agent execution repository Log missing.  No previous agent runtime loaded"
                    , this);

                return lastRunTimes;
            }

            long lastOffset = GetLastFlushOffset();
            if (lastOffset == 0)
            {
                return lastRunTimes;
            }

            bool isHeaderFound=false;
            try
            {
                //using (TextReader reader = new StreamReader(_logFile))
                using (TextReader reader = File.OpenText(_logFile))
                {
                    (reader as StreamReader).BaseStream.Seek(lastOffset, SeekOrigin.Begin);

                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        // Find starting position
                        bool isHeader = line.StartsWith("##");
                        if (isHeader)
                        {
                            // We just got to the beginning of next flush.  
                            // User may have manually updated the file for last runtime.
                            // So we will abort further reading and assume the runtimes are loaded
                            if (isHeaderFound)
                            {
                                return lastRunTimes;
                            }
                            else
                            {
                                // Skip first header
                                isHeaderFound = true;
                                continue;
                            }
                        }
                        
                        // Keep reading a line until a header is found.
                        if (!isHeaderFound)
                        {
                            continue;
                        }
                        else
                        {
                            string dateTime, name, typeName;
                            string[] arr = line.Split(new char[] {'\t'});

                            // Skip lines that don't match tab delimited record format.
                            if (arr.Length != 3
                                || !DateUtil.IsIsoDate((dateTime = arr[0]))
                                || string.IsNullOrEmpty((name = arr[1]))
                                || string.IsNullOrEmpty((typeName = arr[2])))
                            {
                                continue;
                            }


                            lastRunTimes[name] = DateUtil.ToUniversalTime( DateUtil.ParseDateTime(dateTime, _defaultLastRuntime));
                        }
                    }
                }
            }
            catch(Exception e)
            {

                Sitecore.Diagnostics.Log.Error(
                     string.Format(
                        "Scheduler - Agent execution repository is unable to read last agent run time of {0}"
                        , _logFile)
                    , e, this);
            }

            return lastRunTimes;
        }
    }
}