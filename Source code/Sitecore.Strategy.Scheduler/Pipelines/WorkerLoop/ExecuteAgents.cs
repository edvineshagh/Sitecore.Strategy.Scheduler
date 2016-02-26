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
    public class ExecuteAgents
    {
        public void Process(ISchedulerArgs schedulerArgs)
        {

            Assert.ArgumentNotNull(schedulerArgs, "schedulerArgs");
            Assert.ArgumentNotNull(schedulerArgs.AgentMediators, "schedulerArgs.AgentMediators");
            Assert.ArgumentNotNull(schedulerArgs.ProcessedAgentMediators, "schedulerArgs.ProcessedAgentMediators");

            Log.Info("Scheduler - Execute all agents.", this);

            var executedKeyList = new List<DateTime>();


            var executeAgentPipeline = FactoryInstance.Current.NewAgentExecuteAgentPipeline();

            foreach (KeyValuePair<DateTime, SortedAgentMediators> agentsKeyValue in schedulerArgs.AgentMediators)
            {
                // Execute based on sorted list
                if (agentsKeyValue.Key > DateTime.UtcNow 
                || HostingEnvironment.ShutdownReason != ApplicationShutdownReason.None)
                {
                    return;
                }

                executedKeyList.Add(agentsKeyValue.Key);

                var sortedAgents = agentsKeyValue.Value;

                foreach (var agentMediator in sortedAgents.Values)
                {

                    try
                    {
                        var agentArgs = FactoryInstance.Current.NewAgentExecuteAgentPipelineArgs();
                        agentArgs.Agent = agentMediator;
                        agentArgs.IsExecuted = false;

                        executeAgentPipeline.Run(agentArgs as PipelineArgs);

                    }
                    catch (Exception exception)
                    {
                        Log.Error("Scheduler - Exception in agent: " + agentMediator.Name, exception, this);
                    }
                    finally
                    {
                        schedulerArgs.ProcessedAgentMediators.Add(agentMediator);
                    }

                }
            }
        }

    }
}