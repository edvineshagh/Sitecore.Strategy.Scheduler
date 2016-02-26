using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.Diagnostics;
using Sitecore.Pipelines;

namespace Sitecore.Strategy.Scheduler.Pipelines.AgentExecution
{
    public class CanExecuteAgent
    {
        public void Process(IExecuteAgentArgs executeAgentArgs)
        {
            Assert.ArgumentNotNull(executeAgentArgs, "executeAgentArgs");
            Assert.ArgumentNotNull(executeAgentArgs.Agent, "executeAgentArgs");
            Assert.IsTrue(executeAgentArgs is PipelineArgs, "exectuedAgentArgs is Not PipelineArgs");

            Log.Info(string.Format("Scheduler - Can execute agent: {0}."
                , executeAgentArgs.Agent.Name), this);

            executeAgentArgs.CanExecute = executeAgentArgs.Agent.IsDue;

        }
    }
}