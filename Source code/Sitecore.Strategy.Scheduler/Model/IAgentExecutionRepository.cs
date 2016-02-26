using System;

namespace Sitecore.Strategy.Scheduler.Model
{
    /// <summary>
    /// (Re)Store Agent execution times from external storage
    /// </summary>
    public interface IAgentExecutionRepository
    {
        /// <summary>
        /// Save agents last runtime into external storage.
        /// </summary>
        /// <param name="priorityAgents">Agents</param>
        void FlushLastAgentRunTimes(AgentPriorityList priorityAgents);

        /// <summary>
        /// Retrieve last runtime for the specified agent.
        /// If the agent is not found, then defaultLastRuntime is returned.
        /// </summary>
        /// <param name="agentName">Agent name as defined in the config setting.</param>
        /// <returns></returns>
        DateTime GetLastRuntime(string agentName);
    }
}