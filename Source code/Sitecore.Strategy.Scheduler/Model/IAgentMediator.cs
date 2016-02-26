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
        DateTime LastRunTime { get; set; }

        /// <summary>
        /// Next time that the agent is scheduler to execute.
        /// </summary>
        DateTime NextRunTime { get; set; }

        /// <summary>
        /// Recurrence scheduling mediator
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
        string Name { get; }

        /// <summary>
        /// Instance of the agent to be executed.
        /// </summary>
        object Agent { get; }

        /// <summary>
        /// Method that is responsible for invokation of the agent.
        /// </summary>
        void Execute();

        
    }
}