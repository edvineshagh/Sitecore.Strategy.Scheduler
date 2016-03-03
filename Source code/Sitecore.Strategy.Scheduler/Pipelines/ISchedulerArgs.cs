using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.Strategy.Scheduler.Model;

namespace Sitecore.Strategy.Scheduler.Pipelines
{
    /// <summary>
    /// Scheduler agent execution pipeline args, 
    /// used to determine agents to execute
    /// as well as agents executed.
    /// </summary>
    public interface ISchedulerArgs
    {
        /// <summary>
        /// Gets or sets the ordered set of agent mediators based on
        /// <see cref="IAgetMediator.GetNextRunTime"/> and 
        /// <see cref="IAgetMediator.ExecutionPriority"/>.
        /// </summary>
        /// <value>
        /// The agent mediators.
        /// </value>
        IAgentMediatorsHeap AgentMediators { get; set;  }

        /// <summary>
        /// Gets or sets the processed agent mediators that called the Execute() method for each agent.
        /// </summary>
        /// <value>
        /// The processed agent mediators.
        /// </value>
        IProcessedAgentMediators ProcessedAgentMediators { get; set;  }

        /// <summary>
        /// Gets or sets the duration after executing all scheduled agents.
        /// </summary>
        /// <value>
        /// The duration of the sleep.
        /// </value>
        TimeSpan SleepDuration { get; set; }
    }
}