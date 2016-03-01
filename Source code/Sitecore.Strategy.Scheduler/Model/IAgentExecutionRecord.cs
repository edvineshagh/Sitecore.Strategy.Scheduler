using System;

namespace Sitecore.Strategy.Scheduler.Model
{
    public interface IAgentExecutionRecord
    {
        string AgentName { get; set; }
        Type AgentType { get; set; }
        DateTime LastRunTime { get; set; }
        DateTime NextRunTime { get; set; }
    }
}
