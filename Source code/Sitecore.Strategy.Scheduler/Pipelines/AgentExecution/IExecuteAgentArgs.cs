using Sitecore.Strategy.Scheduler.Model;

namespace Sitecore.Strategy.Scheduler.Pipelines.AgentExecution
{
    /// <summary>
    /// Pipeline argument for managing agent execution state.
    /// </summary>
    public interface IExecuteAgentArgs
    {
        /// <summary>
        /// Gets or sets the agent that shall be executed.
        /// </summary>
        /// <value>
        /// The agent instance.
        /// </value>
        IAgentMediator Agent { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether agent should be executed.
        /// </summary>
        /// <value>
        /// If set to <c>true</c> the processor pipeline should proceed with agent 
        /// execution; otherwise, it should abort the execution pipeline.
        /// </value>
        bool CanExecute { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether agent is executed.
        /// </summary>
        /// <value>
        /// <c>true</c> if the IAgentMediator called Execute method on the Agent instance.
        /// </value>
        bool IsExecuted { get; set; }
    }
}