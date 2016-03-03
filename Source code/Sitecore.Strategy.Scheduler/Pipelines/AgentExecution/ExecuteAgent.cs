using Sitecore.Diagnostics;
using Sitecore.Pipelines;
using Sitecore.Shell.Applications.ContentEditor;

namespace Sitecore.Strategy.Scheduler.Pipelines.AgentExecution
{
    public class ExecuteAgent
    {
        /// <summary>
        /// Execute agent if permitted; otherwise, abort the pipeline.
        /// </summary>
        /// <param name="executeAgentArgs">The execute agent arguments.</param>
        public void Process(IExecuteAgentArgs executeAgentArgs)
        {
            Assert.ArgumentNotNull(executeAgentArgs, "executeAgentArgs");
            Assert.ArgumentNotNull(executeAgentArgs.Agent, "executeAgentArgs");
            Assert.IsTrue(executeAgentArgs is PipelineArgs, "exectuedAgentArgs is Not PipelineArgs");

            Log.Info(
                string.Format("Scheduler - Begin execute agent: {0}."
                    , executeAgentArgs.Agent.AgentName)
                , this);
            

            executeAgentArgs.IsExecuted = false;

            if (executeAgentArgs.CanExecute)
            {
                Profiler.StartOperation(string.Format("Scheduler - agent {0}.", executeAgentArgs.Agent.AgentName));
                executeAgentArgs.Agent.Execute();
                Profiler.EndOperation();

                executeAgentArgs.IsExecuted = true;

                Log.Info(
                        string.Format("Scheduler - End execute agent: {0}.  Queue next runtime for: {1}."
                            , executeAgentArgs.Agent.AgentName
                            , DateUtil.ToServerTime(executeAgentArgs.Agent.GetNextRunTime()).ToString("yyyy-MM-dd HH:mm:ss")
                        ) 
                    , this)
                ;
            }
            else
            {
                Log.Info(
                    string.Format("Scheduler - Abort execution pipeline for agent: {0}."
                    , executeAgentArgs.Agent.AgentName), this);

                (executeAgentArgs as PipelineArgs).AbortPipeline();

            }
        }
    }
}