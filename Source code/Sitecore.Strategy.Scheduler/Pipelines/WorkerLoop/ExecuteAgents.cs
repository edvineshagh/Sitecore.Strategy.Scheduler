using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.Diagnostics;
using Sitecore.Pipelines;
using System.Web.Hosting;
using Sitecore.Strategy.Scheduler.Model;

namespace Sitecore.Strategy.Scheduler.Pipelines.WorkerLoop
{
    /// <summary>
    /// The processor is responsible for looping through every AgentMediator
    /// whose NextRunTime is due and calling the execute pipeline
    /// and then updating execution list for next time.
    /// </summary>
    public class ExecuteAgents
    {
        public void Process(ISchedulerArgs schedulerArgs)
        {

            Assert.ArgumentNotNull(schedulerArgs, "schedulerArgs");
            Assert.ArgumentNotNull(schedulerArgs.AgentMediators, "schedulerArgs.AgentMediators");
            Assert.ArgumentNotNull(schedulerArgs.ProcessedAgentMediators, "schedulerArgs.ProcessedAgentMediators");

            Log.Info("Scheduler - Execute all agents.", this);


            var executeAgentPipeline = FactoryInstance.Current.NewAgentExecuteAgentPipeline();

            if (schedulerArgs.AgentMediators.Count == 0)
            {
                return;
            }


            // process all agents that are due until this point

            var now = DateTime.UtcNow;

            IAgentMediator topAgentMediator;

            while ((topAgentMediator=schedulerArgs.AgentMediators.Top()).GetNextRunTime() < now
                 && HostingEnvironment.ShutdownReason == ApplicationShutdownReason.None)
            {

                
                try
                {
                    var agentArgs = FactoryInstance.Current.NewAgentExecuteAgentPipelineArgs();
                    agentArgs.Agent = topAgentMediator;
                    agentArgs.IsExecuted = false;

                    executeAgentPipeline.Run(agentArgs as PipelineArgs);

                    schedulerArgs.AgentMediators.UpdateTop();

                }
                catch (Exception exception)
                {
                    Log.Error("Scheduler - Exception in agent: " + topAgentMediator.AgentName, exception, this);
                }
                finally
                {
                    schedulerArgs.ProcessedAgentMediators.Add(topAgentMediator);
                }

            }
        }

    }
}