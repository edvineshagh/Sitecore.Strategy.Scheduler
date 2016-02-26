using Sitecore.Strategy.Scheduler.Model;

namespace Sitecore.Strategy.Scheduler.Pipelines.AgentExecution
{
    public interface IExecuteAgentArgs
    {
        IAgentMediator Agent { get; set; }
        bool CanExecute { get; set; }
        bool IsExecuted { get; set; }
    }
}