using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.Configuration;
using Sitecore.Pipelines;
using Sitecore.Strategy.Scheduler.Model;

namespace Sitecore.Strategy.Scheduler.Pipelines
{
    /// <summary>
    /// Scheduler agent execution pipeline args, 
    /// used to determine agents to execute
    /// as well as agents executed.
    /// </summary>
    /// <seealso cref="Sitecore.Pipelines.PipelineArgs" />
    /// <seealso cref="Sitecore.Strategy.Scheduler.Pipelines.ISchedulerArgs" />
    public class SchedulerArgs : PipelineArgs, ISchedulerArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SchedulerArgs"/> class.
        /// </summary>
        public SchedulerArgs()
        {
            int numberOfAgents = Factory.GetConfigNodes("scheduling/agent").Count;

            this.AgentMediators = new OrderedAgentMediators(maxSize: numberOfAgents);

            this.ProcessedAgentMediators = new ProcessedAgentMediators(maxSize: numberOfAgents);
        }

        /// <summary>
        /// Gets or sets the ordered set of agent mediators.
        /// </summary>
        /// <value>
        /// The agent mediators.
        /// </value>
        public IAgentMediatorsHeap AgentMediators { get; set; }

        /// <summary>
        /// Gets or sets the processed agent mediators that called the Execute() method for each agent.
        /// </summary>
        /// <value>
        /// The processed agent mediators.
        /// </value>
        public IProcessedAgentMediators ProcessedAgentMediators { get; set; }

        /// <summary>
        /// Gets or sets the duration after executing all scheduled agents.
        /// </summary>
        /// <value>
        /// The duration of the sleep.
        /// </value>
        public TimeSpan SleepDuration { get; set; }
    }
}