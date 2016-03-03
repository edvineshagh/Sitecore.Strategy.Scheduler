using System;
using System.Linq;
using Sitecore.Diagnostics;
using Sitecore.Diagnostics.PerformanceCounters;
using Sitecore.Strategy.Scheduler.Model;
using Sitecore.Strategy.Scheduler.Model.NullAgent;

namespace Sitecore.Strategy.Scheduler.Pipelines.SchedulerInitialization
{
    /// <summary>
    /// Processor responsible for loading agent execution times from repository.
    /// This is needed because worker process may recycle; thus, we need to 
    /// keep track of last execution time if we want to preserve agent scheduled
    /// execution time.
    /// </summary>
    public class GetAgentLastRunTimes
    {
        public void Process(ISchedulerArgs schedulerArgs)
        {
            Assert.ArgumentNotNull(schedulerArgs, "schedulerArgs");
            Assert.ArgumentNotNull(schedulerArgs.AgentMediators, "schedulerArgs.AgentMediators");

            Log.Info("Scheduler - Get agent last run times from repository."
                , this);

            var agentHistory  = FactoryInstance.Current.NewExecutionRepository();


            // Remove all agents from heap to reload LastRunTime and 
            // re-add them because the NextRunTime will change when LastRunTime 
            // is updated; therefore, we need to rebuild the Heap. 


            var agentMediators = new OrderedAgentMediators(schedulerArgs.AgentMediators.Count);

            while (schedulerArgs.AgentMediators.Count > 0)
            {

                var agentMediator = schedulerArgs.AgentMediators.Pop();

                if (agentMediator == null || agentMediator is NullAgentMediator) continue;

                try
                {
                    var record = agentHistory.GetById(agentMediator.AgentName);
                    if (record != null)
                    {
                        agentMediator.SetLastRunTime(record.LastRunTime);
                    }
                    agentMediators.Add(agentMediator);

                }
                catch
                {
                    Log.Error(string.Format("Scheduler - Unable to determine agent type for {0}."
                               , agentMediator.AgentName)
                        , this);

                    agentMediator.SetNextRunTime(DateTime.UtcNow);
                }
            }

            schedulerArgs.AgentMediators = agentMediators;

  
        }
    }
}