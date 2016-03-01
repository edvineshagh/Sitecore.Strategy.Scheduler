using System;
using System.Linq;
using Sitecore.Diagnostics;
using Sitecore.Diagnostics.PerformanceCounters;
using Sitecore.Strategy.Scheduler.Model;
using Sitecore.Strategy.Scheduler.Model.NullAgent;

namespace Sitecore.Strategy.Scheduler.Pipelines.SchedulerInitialization
{
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
            // re-add them because the NextRunTime may change when LastRunTime changes
            // therefore, we need to rebuild the Heap. 


            var agentMediators = new OrderedAgentMediators(schedulerArgs.AgentMediators.Count);

            while (schedulerArgs.AgentMediators.Count > 0)
            {
                var agentMediator = schedulerArgs.AgentMediators.Pop();

                if (agentMediator == null || agentMediator is NullAgentMediator) continue;

                try
                {
                    var record = agentHistory.GetById(agentMediator.Name);
                    if (record != null)
                    {
                        agentMediator.SetLastRunTime(record.LastRunTime);
                    }
                    agentMediators.Add(agentMediator);

                }
                catch
                {
                    Log.Error(string.Format("Scheduler - Unable to determine agent type for {0}."
                               , agentMediator.Name)
                        , this);

                    agentMediator.SetNextRunTime(DateTime.UtcNow);
                }
            }

            schedulerArgs.AgentMediators = agentMediators;

  
        }
    }
}