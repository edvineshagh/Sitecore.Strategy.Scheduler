using System;
using System.Collections.Generic;
using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.Pipelines;
using Sitecore.Strategy.Scheduler.Model.NullAgent;
using Sitecore.Strategy.Scheduler.Model;

namespace Sitecore.Strategy.Scheduler.Pipelines.SchedulerInitialization
{
    /// <summary>
    /// The processor is responsible for retrieving agents from 
    /// Sitecore configuration path scheduler/agent and building
    /// a list of AgentMediators that shall be used for execution.
    /// </summary>
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
                var agentMediatorList = new List<IAgentMediator>();

                foreach (System.Xml.XmlNode agentNode in Factory.GetConfigNodes("scheduling/agent"))
                {
                    try
                    {
                        IAgentMediator agentMediator = FactoryInstance.Current.NewAgentMediator(agentNode);

                        Assert.IsNotNull(agentMediator, "agentMediator");

                        if (agentMediator is DisabledAgentMediator)
                        {
                            Log.Info(
                                string.Format("Scheduler- Skipping inactive agent: {0}.", agentMediator.AgentName)
                                ,this);
                        }
                        else if (agentMediator is NullAgentMediator)
                        {
                            continue;
                        }
                        else
                        {
                            Log.Info( string.Format("Scheduler - Load agent {0} (interval:{1})."
                                    , agentMediator.AgentName, agentMediator.Recurrence.ToString())
                                , this
                                );

                            agentMediatorList.Add(agentMediator);

                        }
                    }
                    catch (Exception exception)
                    {
                        Log.Error("Scheduler - Error while instantiating agent. Definition: " + agentNode.OuterXml, exception, this);
                    }

                }

                // The priority queue requires initial size when creating the object;
                // thus, we first load all agents into a list (skipping disabled or problem agents), 
                // and then adding them to priority queue.

                schedulerArgs.ProcessedAgentMediators = new ProcessedAgentMediators(agentMediatorList.Count);
                schedulerArgs.AgentMediators = new OrderedAgentMediators(agentMediatorList.Count);
                agentMediatorList.ForEach(agentMediator => schedulerArgs.AgentMediators.Add(agentMediator));
                
                Log.Info("Scheduler - Agents load complete.", this);

            }
            catch (Exception e)
            {
                Log.Error("Scheduler - Error while reading agents.", e, this);
            }
        }

    }
}