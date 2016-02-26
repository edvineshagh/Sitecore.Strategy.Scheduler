using System;
using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.Pipelines;
using Sitecore.Strategy.Scheduler.Model.NullAgent;
using Sitecore.Strategy.Scheduler.Model;

namespace Sitecore.Strategy.Scheduler.Pipelines.SchedulerInitialization
{
    public class LoadAgents
    {
        public void Process(ISchedulerArgs schedulerArgs)
        {
            Assert.ArgumentNotNull(schedulerArgs, "schedulerArgs");
            Assert.ArgumentNotNull(schedulerArgs.AgentMediators, "schedulerArgs.AgentMediators");
            Assert.ArgumentNotNull(schedulerArgs.ProcessedAgentMediators, "schedulerArgs.ProcessedAgentMediators");
            
            Log.Info("Scheduler - Load agents for execution.", this);

            try
            {

                foreach (System.Xml.XmlNode agentNode in Factory.GetConfigNodes("scheduling/agent"))
                {
                    try
                    {
                        IAgentMediator agentMediator = FactoryInstance.Current.NewAgentMediator(agentNode);

                        Assert.IsNotNull(agentMediator, "agentMediator");

                        if (agentMediator is DisabledAgentMediator)
                        {
                            Log.Info(
                                string.Format("Scheduler- Skipping inactive agent: {0}.", agentMediator.Name)
                                ,this);
                        }
                        else if (agentMediator is NullAgentMediator)
                        {
                            continue;
                        }
                        else
                        {
                            Log.Info( string.Format("Scheduler - Load agent {0} (interval:{1})."
                                    , agentMediator.Name, agentMediator.Recurrence.ToString())
                                , this
                                );

                            if (!schedulerArgs.AgentMediators.ContainsKey(agentMediator.NextRunTime))
                            {
                                schedulerArgs.AgentMediators.Add(agentMediator.NextRunTime, new SortedAgentMediators());
                            }
                            schedulerArgs.AgentMediators[agentMediator.NextRunTime].Add(agentMediator);
                        }
                    }
                    catch (Exception exception)
                    {
                        Log.Error("Scheduler - Error while instantiating agent. Definition: " + agentNode.OuterXml, exception, this);
                    }
                }
                Log.Info("Scheduler - Agents load complete.", this);

            }
            catch (Exception e)
            {
                Log.Error("Scheduler - Error while reading agents.", e, this);
            }
        }

    }
}