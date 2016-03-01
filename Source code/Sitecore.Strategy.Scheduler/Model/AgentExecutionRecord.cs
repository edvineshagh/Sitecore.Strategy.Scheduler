using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using Sitecore.Security.Accounts;

namespace Sitecore.Strategy.Scheduler.Model
{

    [Serializable]
    public class AgentExecutionRecord : IAgentExecutionRecord
    {
        public string AgentName { get; set; }
        public Type AgentType { get; set; }
        public DateTime LastRunTime { get; set; }
        public DateTime NextRunTime { get; set; }

    }
}