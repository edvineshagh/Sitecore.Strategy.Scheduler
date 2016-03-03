using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.Pipelines;
using Sitecore.Strategy.Scheduler.Model.NullAgent;
using Sitecore.Strategy.Scheduler.Model;
using Sitecore.Strategy.Scheduler.Pipelines;
using Sitecore.Strategy.Scheduler.Pipelines.AgentExecution;
using Sitecore.Xml;
using Sitecore.Tasks;

namespace Sitecore.Strategy.Scheduler
{


    public class SchedulerFactory : ISchedulerFactory
    {

        protected const string PipelineGroupName = "scheduler"; 
        
        /// <summary>
        /// Used to determine the order in which agents are executed.
        /// </summary>
        protected int ExecutionPriorityCounter;

        /// <summary>
        /// Used to keep track of agent names used, to insure we don't have duplicate names.
        /// </summary>
        protected readonly HashSet<string> AgentNameConflictList;


        public SchedulerFactory()
        {
            AgentNameConflictList  = new HashSet<string>();
        }


        public virtual AgentExecutionRecord NewAgentExecutionRepositoryRecord()
        {
            return new AgentExecutionRecord();
        }

        /// <summary>
        /// Create an instance of PipelineArgs for <see cref="NewSchedulerPipeline" />.
        /// </summary>
        /// <returns></returns>
        public virtual ISchedulerArgs NewSchedulerPipelineArgs()
        {
            var xPath = string.Format("pipelines/group[@groupName='{0}']/pipelines/scheduler.start", 
                PipelineGroupName);
            
            var node = Sitecore.Configuration.Factory.GetConfigNode( xPath);

            var argTypeStr = XmlUtil.GetAttribute("argsType", node);

            Assert.IsNotNullOrEmpty(argTypeStr, "Unable to find argsType property for " + xPath);

            var argType = Type.GetType(argTypeStr);
            
            return Activator.CreateInstance(argType) as ISchedulerArgs;

        }

        /// <summary>
        /// Create a new instance of scheduler pipeline, which is the starting point of the scheduler.
        /// </summary>
        /// <returns></returns>
        public virtual CorePipeline NewSchedulerPipeline()
        {
            return CorePipelineFactory.GetPipeline("scheduler.start", PipelineGroupName);
        }

        /// <summary>
        /// Creates an instance of PipelineArgs for <see cref="NewAgentExecuteAgentPipeline" />.
        /// </summary>
        /// <returns></returns>
        public virtual IExecuteAgentArgs NewAgentExecuteAgentPipelineArgs()
        {

            var xPath = string.Format("pipelines/group[@groupName='{0}']/pipelines/scheduler.executeAgent",
                PipelineGroupName);

            var node = Sitecore.Configuration.Factory.GetConfigNode(xPath);

            var argTypeStr = XmlUtil.GetAttribute("argsType", node);

            Assert.IsNotNullOrEmpty(argTypeStr, "Unable to find argsType property for " + xPath);


            var argType = Type.GetType(argTypeStr);

            return Activator.CreateInstance(argType) as IExecuteAgentArgs;
        }

        /// <summary>
        /// Create a new pipeline instance for executing single agent.
        /// </summary>
        /// <returns></returns>
        public virtual CorePipeline NewAgentExecuteAgentPipeline()
        {
            return CorePipelineFactory.GetPipeline("scheduler.executeAgent", PipelineGroupName);
        }

        /// <summary>
        /// Create a new pipeline instance that is responsible for executing all agents via infinite loop thread.
        /// </summary>
        /// <returns></returns>
        public virtual CorePipeline NewThreadWorkerLoopPipeline()
        {
            return CorePipelineFactory.GetPipeline("scheduler.threadWorkerLoop", PipelineGroupName);
        }

        public virtual IAgentExecutionRepository NewExecutionRepository()
        {
            return
                Sitecore.Configuration.Factory.CreateObject("scheduling/agentExecutionRepository", true) as
                    IAgentExecutionRepository;
        }

        /// <summary>
        /// Create an instance of agent mediator for the agent specified via configuration path. 
        /// If the agent name has already been defined, a numeric suffix is added to uniquify the agent name.
        /// </summary>
        /// <param name="agentNode">xml node to agent with configuration path sitecore/scheduling/agent</param>
        /// <returns></returns>
        public virtual IAgentMediator NewAgentMediator(System.Xml.XmlNode agentNode)
        {
            string agentName = null;

            // Get "name", method", "interval" attributes for each agent node
            object obj2 = Factory.CreateObject(agentNode, true);
            string methodName = StringUtil.GetString(new string[] { XmlUtil.GetAttribute("method", agentNode), "Execute" });

            agentName = StringUtil.GetString(new string[] { XmlUtil.GetAttribute("name", agentNode), obj2.GetType().FullName });
                        
            int executionPrority ;
            if (!int.TryParse(XmlUtil.GetAttribute("executionPriority", agentNode), out executionPrority))
            {
                executionPrority = ExecutionPriorityCounter++;
            }
                        
            // Get unique agent name. 
            // If agent Name exist, then we add a numeric suffix so that
            // Agents with same names are executed based on the order 
            // entered in the configuration file.

            int counter = 0;
            string agentPraposedName;
            do
            {
                agentPraposedName = string.Format("{0}{1}", agentName, (counter++ == 0) ? string.Empty : "." + counter);
            } while (AgentNameConflictList.Contains(agentPraposedName))
                ;
            AgentNameConflictList.Add(agentPraposedName);

			string intervalStrPattern = XmlUtil.GetAttribute("interval", agentNode) ?? string.Empty;

            IAgentMediator agent =
                new AgentMediator.Builder()
                    .SetAgent(obj2)
                    .SetAgentName(agentPraposedName)
                    .SetExecuteMethod(methodName)

                    /* we set to default DateTime.MinValue, as appose to DateTime.UtcNow
                     * because for schedules that are once a week, may get next runtime 
                     * that might be one week later; but, we want it to run immediately.
                     * The GetAgentLastRunTimes pipeline, should override this when possible
                     */
                    .SetLastRunTime(DateTime.MinValue)

                    .SetRecurrence(intervalStrPattern)
                    .SetExecutionPriority(executionPrority)
                    .Build();

                return agent;

        }

    }
}