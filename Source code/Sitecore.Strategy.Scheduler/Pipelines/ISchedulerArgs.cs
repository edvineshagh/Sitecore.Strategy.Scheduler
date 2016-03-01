using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.Strategy.Scheduler.Model;

namespace Sitecore.Strategy.Scheduler.Pipelines
{
    public interface ISchedulerArgs
    {
        IAgentMediatorsHeap AgentMediators { get; set;  }
        IAgentMediators ProcessedAgentMediators { get; set;  }
    }
}