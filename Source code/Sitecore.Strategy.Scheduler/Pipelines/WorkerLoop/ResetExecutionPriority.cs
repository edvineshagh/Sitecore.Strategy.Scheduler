using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using Sitecore.Diagnostics;
using Sitecore.Strategy.Scheduler.Model;

namespace Sitecore.Strategy.Scheduler.Pipelines.WorkerLoop
{
    public class ResetExecutionPriority
    {
        public void Process(ISchedulerArgs schedulerArgs)
        {
            Assert.ArgumentNotNull(schedulerArgs, "schedulerArgs");
            Assert.ArgumentNotNull(schedulerArgs.AgentMediators, "schedulerArgs.AgentMediators");
            Assert.ArgumentNotNull(schedulerArgs.ProcessedAgentMediators, "schedulerArgs.ProcessedAgentMediators");

            Log.Info("Scheduler - Sort agent execution order.", this);


            if (HostingEnvironment.ShutdownReason != ApplicationShutdownReason.None)
            {
                return;
            }

            // Remove executed agents and re-add them to the heap so that
            // they are added to the correct location of the sorted list.
            //
            var processedAgents = new List<IAgentMediator>();

            var processedTimeSet = new List<DateTime>();


            // Identify items to remove from priority list so that we can re-add them in the correct place
            //
            foreach (KeyValuePair<DateTime, SortedAgentMediators> agentsKeyValue in schedulerArgs.AgentMediators)
            {
                DateTime executionWindow = agentsKeyValue.Key;
                SortedAgentMediators sortedAgentMediators = agentsKeyValue.Value;


                if (sortedAgentMediators.Count == 0
                || sortedAgentMediators.First().Value == null)
                {
                    // Ignore blank entries
                    processedTimeSet.Add(executionWindow);
                }

                // assume everything in the same time window is executed
                else if (schedulerArgs.ProcessedAgentMediators.Contains(sortedAgentMediators.First().Value))
                {
                    var sortedAgents = schedulerArgs.AgentMediators[executionWindow].Values;
                    processedAgents.AddRange(sortedAgents);
                    processedTimeSet.Add(executionWindow);
                }

                // Stop looping through the remaining elements
                // because we have already identified all agent mediators
                // that need to be re-added.
                else if (processedAgents.Count == processedTimeSet.Count)
                {
                    
                    break;
                }         
 
            }

            // Remove processed agent mediators
            processedTimeSet.ForEach( timeSlot => schedulerArgs.AgentMediators.Remove(timeSlot));


            // ReAdd agents so that they are inserted in the correct position
            foreach (var agent in processedAgents)
            {
                if (!schedulerArgs.AgentMediators.ContainsKey(agent.NextRunTime))
                {
                    schedulerArgs.AgentMediators.Add(agent.NextRunTime, new SortedAgentMediators());
                }
                schedulerArgs.AgentMediators[agent.NextRunTime].Add(agent);
            }

        }
    }
}