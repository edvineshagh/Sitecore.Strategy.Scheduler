using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using Sitecore.Security.Accounts;

namespace Sitecore.Strategy.Scheduler.Model
{

    /// <summary>
    /// Provides information about scheduling agent past/future execution times.
    /// </summary>
    /// <seealso cref="Sitecore.Strategy.Scheduler.Model.IAgentExecutionRecord" />
    [Serializable]
    public class AgentExecutionRecord : IAgentExecutionRecord
    {
        /// <summary>
        /// Gets or sets the name of the agent.
        /// </summary>
        /// <value>
        /// The name of the agent.
        /// </value>
        public string AgentName { get; set; }

        /// <summary>
        /// Gets or sets the type of the agent, which is found
        /// configuration path scheduling/agents.
        /// </summary>
        /// <value>
        /// The type of the agent.
        /// </value>
        public Type AgentType { get; set; }

        /// <summary>
        /// Gets or sets the last run time for the agent.
        /// </summary>
        /// <value>
        /// The last run time.
        /// </value>
        public DateTime LastRunTime { get; set; }

        /// <summary>
        /// Gets or sets the next run time for the agent.
        /// </summary>
        /// <value>
        /// The next run time.
        /// </value>
        public DateTime NextRunTime { get; set; }

    }
}