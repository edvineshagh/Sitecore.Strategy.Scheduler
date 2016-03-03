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
        private Dictionary<string, IAgentExecutionRecord> _inMemExecutionRecords;


        // When appending to file, we need to find to location of the last write operation, 
        // so that we can reload last execution run times.  By storing the last offset
        // in the beginning of the file, we can read the header and seek to last write offset.
        // The following string provides a human readable prefix for the header offset.
        private const string FlushString = "Last flush offset: ";


        /// <summary>
        /// Keep track of agent execution due to worker process recycling.
        /// </summary>
        /// <param name="logFile">Logfile to flush into or retrieve from for last executed time</param>
        /// <param name="append">Append to log file if true; otherwise, overwrite file</param>
        /// <param name="defaultLastRunTime">Default dateTime to use when agent record not found</param>
        public AgentExecutionRepository(string logFile, bool append, DateTime defaultLastRunTime)
        {
            try
            {
                _append = append;
                _defaultLastRuntime = defaultLastRunTime;
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
        /// Initializes a new instance of the <see cref="AgentExecutionRepository"/> class.
        /// </summary>
        /// <param name="logFile">The log file.</param>
        /// <param name="append">if set to <c>true</c> [append].</param>
        public AgentExecutionRepository(string logFile, bool append) : this(logFile, append, DateTime.MinValue)
        {
        }


        /// <summary>
        /// Retrieve last runtime record for the specified agent.
        /// </summary>
        /// <param name="agentName">Agent name as defined in the config setting. if Agent name is omitted, then full assembly type maybe used</param>
        /// <returns>null if agent is not found; otherwise, respective record is returned.</returns>
        public virtual IAgentExecutionRecord GetById(string agentName)
        {
            return InMemExecutionRecords.ContainsKey(agentName) ? InMemExecutionRecords[agentName] : null;
        }

        /// <summary>
        /// Add agent execution record to repository.
        /// </summary>
        /// <param name="record">record to persist</param>
        public virtual void Add(IAgentExecutionRecord record)
        {
            Assert.ArgumentNotNull(record, "record");
            Assert.ArgumentNotNull(record.AgentType, "record.AgentType");
            Assert.ArgumentNotNullOrEmpty(record.AgentName, "record.AgentName");
            InMemExecutionRecords[record.AgentName] = record;
        }

        /// <summary>
        /// Returns all the execution records from the repository.
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<IAgentExecutionRecord> GetExecutionRecords()
        {
            return InMemExecutionRecords == null
                ? null
                : InMemExecutionRecords.Values;
        }

        /// <summary>
        /// Save agents last runtime into external storage.
        /// </summary>
        public virtual void Save()
        {
            if (_inMemExecutionRecords == null )
            {
                Sitecore.Diagnostics.Log.Error("Scheduler - No agent execution records available to save.", this);
            }
            else if (_logFile == null)
            {
                Sitecore.Diagnostics.Log.Error("Scheduler - Skipping agent execution record save.  See earlier exception.", this);
            }

            try
            {
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
                    foreach (var record in _inMemExecutionRecords.Values)
                    {
                        if (record != null)
                        {
                            writer.WriteLine(Serialize(record));
                        }
                    }

                }
            }
            catch (Exception e)
            {
               Sitecore.Diagnostics.Log.Error(
                   string.Format("Scheduler - Agent execution repository unable to write to {0}.", _logFile), e, this);
            }

        }


        /// <summary>
        /// Replace macro string with respective configuration settings 
        /// and return full file path.  For example, replace $(DataFolder) with respective value from configuration file.
        /// </summary>
        /// <param name="logPath">File path to parse</param>
        /// <returns>Full file path</returns>
        protected virtual string ParsePath(string logPath)
        {

            var regEx = new Regex(@"\$\(([^)]+)\)");

            foreach (Match match in regEx.Matches(logPath))
            {
                var token = match.Groups[0].Value;
                var param = match.Groups[1].Value;

                logPath = logPath.Replace(token,
                    Sitecore.Configuration.Settings.GetSetting(param, token)
                    );
            }

            logPath = Path.GetFullPath(logPath);

            return logPath;
        }


        /// <summary>
        /// Serialize a record for persisting into a file record.
        /// </summary>
        /// <param name="record">Record to serialize</param>
        /// <returns>A string representing the serialized data</returns>
        protected virtual string Serialize(IAgentExecutionRecord record)
        {
            return record == null ? string.Empty : string.Format("{0}\t{1}\t{2}\t{3}"
                , DateUtil.ToIsoDate(DateUtil.ToServerTime(record.LastRunTime), includeTicks: false, convertToUTC: false)
                , DateUtil.ToIsoDate(DateUtil.ToServerTime(record.NextRunTime), includeTicks: false, convertToUTC: false)
                , record.AgentName
                , record.AgentType == null ? string.Empty : record.AgentType.AssemblyQualifiedName);
        }


        /// <summary>
        /// DeSerialize string into respective record.
        /// </summary>
        /// <param name="recordStr">A record that would represent IAgentExecutionRecord</param>
        /// <returns>An instance of IAgentExecution record if the recordStr format is correct; otherwise, null is returned.</returns>
        protected virtual IAgentExecutionRecord DeSerialize(string recordStr)
        {

            string lastRunDateTime, nextRunDateTime, name, typeName;
            string[] arr = recordStr.Split(new char[] { '\t' });

            // Skip lines that don't match tab delimited record format.
            if (arr.Length != 4
                || !DateUtil.IsIsoDate((lastRunDateTime = arr[0]))
                || !DateUtil.IsIsoDate((nextRunDateTime = arr[1]))
                || string.IsNullOrEmpty((name = arr[2]))
                || string.IsNullOrEmpty((typeName = arr[3])))
            {
                return null;
            }

            var record = FactoryInstance.Current.NewAgentExecutionRepositoryRecord();


            record.AgentName = name;

            record.LastRunTime = DateUtil.ToUniversalTime(DateUtil.ParseDateTime(lastRunDateTime, _defaultLastRuntime));

            record.NextRunTime = DateUtil.ToUniversalTime(DateUtil.ParseDateTime(nextRunDateTime, _defaultLastRuntime));

            record.AgentType = Type.GetType(typeName);

            return record;
        }


        /// <summary>
        /// Last execution runtime for each agent is stored via Agent name hash.
        /// Agent name is guaranteed to be unique when the 
        /// SchedulerFactory.NewAgentMediator() creates the agent.
        /// </summary>
        protected Dictionary<string, IAgentExecutionRecord> InMemExecutionRecords
        {
            get
            {
                if (_inMemExecutionRecords == null && _logFile != null)
                {
                    try
                    {
                        _inMemExecutionRecords = LoadLastAgentsRunTime();
                        Sitecore.Diagnostics.Log.Info(
                            string.Format("Scheduler - Agent execution repository log {0} is Loaded.", _logFile), this);
                    }
                    catch (Exception e)
                    {
                        _inMemExecutionRecords = new Dictionary<string, IAgentExecutionRecord>();

                        Sitecore.Diagnostics.Log.Error(
                            string.Format("Scheduler - Agent execution repository log path {0} is inaccessible.", _logFile), e, this);

                        _logFile = null;

                    }
                }
                return _inMemExecutionRecords;
            }
        }


        /// <summary>
        /// Returns the header string with a fixed size.  
        /// This helps in implementing a random access solution.
        /// </summary>
        /// <param name="offset">The offset of store for next record.</param>
        /// <returns>Fixed size string</returns>
        protected virtual string GetHeaderString(long offset)
        {
            var offsetHeader = string.Format("# {0}{1,20}", FlushString, offset);
            const string recordHeading = "# LastRunTime\tNextRunTime\tAgentName\tAgentType";

            return string.Format("{0}\n{1}", offsetHeader, recordHeading);

        }


        /// <summary>
        /// Read the header of the file and obtain the last flush offset.
        /// </summary>
        /// <returns>Last record set offset</returns>
        protected long GetLastFlushOffset()
        {
            long lastOffset = 0;
            if (!File.Exists(_logFile))
            {
                return 0;
            }

            using (TextReader reader = new StreamReader(_logFile))
            {
                string line = reader.ReadLine();
                int flushIndex;
                if (line != null && (flushIndex = line.LastIndexOf(FlushString, System.StringComparison.Ordinal)) > 0)
                {
                    lastOffset = long.TryParse(line.Substring(flushIndex + FlushString.Length), out lastOffset) ? lastOffset : 0;
                }
            }
            return lastOffset;
        }


        /// <summary>
        /// Retrieve last agent run times from repository.
        /// </summary>
        /// <returns>A dictionary of agent run times that is keyed by agentName. /></returns>
        protected virtual Dictionary<string, IAgentExecutionRecord> LoadLastAgentsRunTime()
        {
            var lastRunTimes = new Dictionary<string, IAgentExecutionRecord>();

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
                            var record = DeSerialize(line);


                            if (record == null
                                || string.IsNullOrWhiteSpace(record.AgentName)
                                || record.AgentType == null)
                            {
                                Log.Error(
                                    string.Format("Scheduler - Unable to load agent last runtime record: {0}." 
                                    , line), this);
                            }
                            else
                            {
                                lastRunTimes[record.AgentName] = record;
                            }

                        }
                    }
                }
            }
            catch(Exception e)
            {

                Sitecore.Diagnostics.Log.Error(
                     string.Format(
                        "Scheduler - Agent execution repository is unable to read last agent run time of {0}."
                        , _logFile)
                    , e, this);
            }

            return lastRunTimes;
        }


    }
}