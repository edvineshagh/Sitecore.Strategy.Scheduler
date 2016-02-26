using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.Strategy.Scheduler.Model;

namespace Sitecore.Strategy.Scheduler.Pipelines
{
    public interface ISchedulerArgs
    {
        AgentPriorityList AgentMediators { get; }
        HashSet<IAgentMediator> ProcessedAgentMediators { get; }
    }
}