using Sitecore.Diagnostics;
using Sitecore.Pipelines;
using Sitecore.Shell.Applications.ContentEditor;

namespace Sitecore.Strategy.Scheduler.Pipelines.AgentExecution
{
    public class ExecuteAgent
    {
        public void Process(IExecuteAgentArgs executeAgentArgs)
        {
            Assert.ArgumentNotNull(executeAgentArgs, "executeAgentArgs");
            Assert.ArgumentNotNull(executeAgentArgs.Agent, "executeAgentArgs");
            Assert.IsTrue(executeAgentArgs is PipelineArgs, "exectuedAgentArgs is Not PipelineArgs");

            Log.Info(
                string.Format("Scheduler - {0} execute agent: {1}."
                    , executeAgentArgs.CanExecute ? "Begin" : "Abort"
                    , executeAgentArgs.Agent.Name)
                , this);
            

            executeAgentArgs.IsExecuted = false;

            if (executeAgentArgs.CanExecute)
            {
                Profiler.StartOperation(string.Format("Scheduler - agent {0}.", executeAgentArgs.Agent.Name));
                executeAgentArgs.Agent.Execute();
                Profiler.EndOperation();

                executeAgentArgs.IsExecuted = true;

                Log.Info(
                        string.Format("Scheduler - End execute agent: {0}.  Queue next runtime for: {1}."
                            , executeAgentArgs.Agent.Name
                            , DateUtil.ToServerTime(executeAgentArgs.Agent.NextRunTime).ToString("yyyy-MM-dd HH:mm:ss")
                        ) 
                    , this)
                ;
            }
            else
            {
                (executeAgentArgs as PipelineArgs).AbortPipeline();

            }
        }
    }
}