using System;
using Sitecore.Tasks;

namespace Sitecore.Strategy.Scheduler.Model
{
    /// <summary>
    /// Contains information about agent scheduling, as well as the ability to execute the agent.
    /// </summary>
    public interface IAgentMediator
    {
        /// <summary>
        /// This property is used by the caller to determine agent execution order.
        /// </summary>
        int ExecutionPriority { get; set; }

        /// <summary>
        /// Last time that the agent was executed.
        /// </summary>
        void SetLastRunTime(DateTime value);

        /// <summary>
        /// Last time that the agent was executed.
        /// </summary>
        DateTime GetLastRunTime();

        /// <summary>
        /// Next time that the agent is scheduled to execute.
        /// </summary>
        void SetNextRunTime(DateTime value);

        /// <summary>
        /// Set the next time that the agent is scheduled to execute.
        /// </summary>
        DateTime GetNextRunTime();

        /// <summary>
        /// Recurrence schedule.
        /// </summary>
        Recurrence Recurrence { get; }

        /// <summary>
        /// If false, then the interval portion of Recurrence is 
        /// interpreted as time of day; otherwise, it is treated as timespan interval.
        /// </summary>
        bool IsRecurrenceInterval { get; }

        /// <summary>
        /// Returns true if agent should be executed; otherwise, false is returned.
        /// </summary>
        bool IsDue { get; }

        /// <summary>
        /// Agent name.
        /// </summary>
        string AgentName { get; }

        /// <summary>
        /// Instance of the agent to be executed.
        /// </summary>
        object Agent { get; }

        /// <summary>
        /// Method that is responsible for invocation of the agent.
        /// </summary>
        void Execute();

        
    }
}