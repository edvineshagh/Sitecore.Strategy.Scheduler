using System;
using Sitecore.Diagnostics;

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
            
            foreach (var agentMediatorSet in schedulerArgs.AgentMediators)
            {
                var executionTime = agentMediatorSet.Key;
                var agentMediatorList = agentMediatorSet.Value;

                foreach (var agentMediatorPriority in agentMediatorList)
                {
                    var priority = agentMediatorPriority.Key;
                    var agentMediator = agentMediatorPriority.Value;

                    try
                    {
                        agentMediator.NextRunTime = agentHistory.GetLastRuntime(agentMediator.Name);
                    }
                    catch
                    {
                        Log.Error(
                            string.Format("Scheduler - Unable to determine agent type for {0}"
                                         ,  agentMediator.Name)
                          , this );

                        agentMediator.NextRunTime = DateTime.UtcNow;
                    }
                }
                
            }
        }
    }
}