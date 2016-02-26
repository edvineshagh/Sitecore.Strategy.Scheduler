using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.Pipelines;
using Sitecore.Strategy.Scheduler.Model;

namespace Sitecore.Strategy.Scheduler.Pipelines
{
    public class SchedulerArgs : PipelineArgs, ISchedulerArgs
    {
        public SchedulerArgs()
        {
            this.AgentMediators = new AgentPriorityList();
            this.ProcessedAgentMediators = new HashSet<IAgentMediator>();
        }

        public AgentPriorityList AgentMediators { get; private set; }

        public HashSet<IAgentMediator> ProcessedAgentMediators { get; private set; }
    }
}