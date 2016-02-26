using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.Pipelines;
using Sitecore.Strategy.Scheduler.Model;

namespace Sitecore.Strategy.Scheduler.Pipelines.AgentExecution
{
    public class ExecuteAgentArgs : PipelineArgs, IExecuteAgentArgs
    {
        public IAgentMediator Agent { get; set; }
        public bool CanExecute { get; set; }
        public bool IsExecuted { get; set; }
    }
}