using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Pipelines;
using Sitecore.Strategy.Scheduler.Model;
using Sitecore.Strategy.Scheduler.Pipelines;
using Sitecore.Strategy.Scheduler.Pipelines.AgentExecution;

namespace Sitecore.Strategy.Scheduler
{
    /// <summary>
    /// Scheduler factory responsible for scheduling entities.
    /// </summary>
    public interface ISchedulerFactory
    {

        /// <summary>
        /// Create a new instance of scheduler pipeline, which is the starting point of the scheduler.
        /// </summary>
        /// <returns></returns>
        CorePipeline   NewSchedulerPipeline();

        /// <summary>
        /// Create an instance of object used to persist into repository.
        /// </summary>
        /// <returns></returns>
        AgentExecutionRecord NewAgentExecutionRepositoryRecord();

        /// <summary>
        /// Create an instance of PipelineArgs for <see cref="NewSchedulerPipeline"/>.
        /// </summary>
        /// <returns></returns>
        ISchedulerArgs NewSchedulerPipelineArgs();

        /// <summary>
        /// Create a new pipeline instance for executing single agent.
        /// </summary>
        /// <returns></returns>
        CorePipeline      NewAgentExecuteAgentPipeline();

        /// <summary>
        /// Creates an instance of PipelineArgs for <see cref="NewAgentExecuteAgentPipeline"/>.
        /// </summary>
        /// <returns></returns>
        IExecuteAgentArgs NewAgentExecuteAgentPipelineArgs();

        /// <summary>
        /// Create a new pipeline instance that is responsible for executing all agents via infinite loop thread.
        /// </summary>
        /// <returns></returns>
        CorePipeline NewThreadWorkerLoopPipeline();

        /// <summary>
        /// Create an instance on execution repository, which is responsible for 
        /// retrieving/storing last execution times for all agents.  
        /// </summary>
        /// <returns></returns>
        IAgentExecutionRepository NewExecutionRepository();
        
        /// <summary>
        /// Create an instance of IAgentMediator for the agent specified by the configuration path.
        /// The agent name property of the AgentMediator may not match of that from configuration file, 
        /// because this is guaranteed to be unique.  
        /// </summary>
        /// <param name="agentNode">xPath to agent within in Sitecore configuration file</param>
        /// <returns>an instance of IAgentMediator with respective runtime schedule.</returns>
        IAgentMediator NewAgentMediator(System.Xml.XmlNode agentNode);
    }
}
