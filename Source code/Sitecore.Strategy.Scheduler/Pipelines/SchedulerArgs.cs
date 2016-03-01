using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.Configuration;
using Sitecore.Pipelines;
using Sitecore.Strategy.Scheduler.Model;

namespace Sitecore.Strategy.Scheduler.Pipelines
{
    public class SchedulerArgs : PipelineArgs, ISchedulerArgs
    {
        public SchedulerArgs()
        {
            int numberOfAgents = Factory.GetConfigNodes("scheduling/agent").Count;

            this.AgentMediators = new OrderedAgentMediators(maxSize: numberOfAgents);

            this.ProcessedAgentMediators = new ProcessedAgentMediators(maxSize: numberOfAgents);
        }

        public IAgentMediatorsHeap AgentMediators { get; set; }

        public IAgentMediators ProcessedAgentMediators { get; set; }
    }
}