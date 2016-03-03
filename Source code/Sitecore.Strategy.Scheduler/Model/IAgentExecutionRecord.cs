using System;

namespace Sitecore.Strategy.Scheduler.Model
{
    /// <summary>
    /// Provides information about scheduling agent past/future execution times.
    /// </summary>
    public interface IAgentExecutionRecord
    {
        /// <summary>
        /// Gets or sets the name of the agent.
        /// </summary>
        /// <value>
        /// The name of the agent.
        /// </value>
        string AgentName { get; set; }

        /// <summary>
        /// Gets or sets the type of the agent, which is found 
        /// configuration path scheduling/agents.
        /// </summary>
        /// <value>
        /// The type of the agent.
        /// </value>
        Type AgentType { get; set; }

        /// <summary>
        /// Gets or sets the last run time for the agent.
        /// </summary>
        /// <value>
        /// The last run time.
        /// </value>
        DateTime LastRunTime { get; set; }

        /// <summary>
        /// Gets or sets the next run time for the agent.
        /// </summary>
        /// <value>
        /// The next run time.
        /// </value>
        DateTime NextRunTime { get; set; }
    }
}
