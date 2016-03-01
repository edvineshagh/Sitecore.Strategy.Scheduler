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

        private const string PipelineGroupName = "scheduler"; 
        
        /// <summary>
        /// Used to determine the order in which agents are executed.
        /// </summary>
        private int _executionPriorityCounter;

        /// <summary>
        /// Used to keep track of agent names used, to insure we don't have duplicate names.
        /// </summary>
        private readonly HashSet<string> _agentNameConflictList;


        public SchedulerFactory()
        {
            _agentNameConflictList  = new HashSet<string>();
        }


        public AgentExecutionRecord NewAgentExecutionRepositoryRecord()
        {
            return new AgentExecutionRecord();
        }

        public ISchedulerArgs NewSchedulerPipelineArgs()
        {
            var xPath = string.Format("pipelines/group[@groupName='{0}']/pipelines/scheduler.start", 
                PipelineGroupName);
            
            var node = Sitecore.Configuration.Factory.GetConfigNode( xPath);

            var argTypeStr = XmlUtil.GetAttribute("argsType", node);

            Assert.IsNotNullOrEmpty(argTypeStr, "Unable to find argsType property for " + xPath);

            var argType = Type.GetType(argTypeStr);
            
            return Activator.CreateInstance(argType) as ISchedulerArgs;

        }

        public CorePipeline NewSchedulerPipeline()
        {
            return CorePipelineFactory.GetPipeline("scheduler.start", PipelineGroupName);
        }

        public IExecuteAgentArgs NewAgentExecuteAgentPipelineArgs()
        {

            var xPath = string.Format("pipelines/group[@groupName='{0}']/pipelines/scheduler.executeAgent",
                PipelineGroupName);

            var node = Sitecore.Configuration.Factory.GetConfigNode(xPath);

            var argTypeStr = XmlUtil.GetAttribute("argsType", node);

            Assert.IsNotNullOrEmpty(argTypeStr, "Unable to find argsType property for " + xPath);


            var argType = Type.GetType(argTypeStr);

            return Activator.CreateInstance(argType) as IExecuteAgentArgs;
        }

        public CorePipeline NewAgentExecuteAgentPipeline()
        {
            return CorePipelineFactory.GetPipeline("scheduler.executeAgent", PipelineGroupName);
        }

        public CorePipeline NewThreadWorkerLoopPipeline()
        {
            return CorePipelineFactory.GetPipeline("scheduler.threadWorkerLoop", PipelineGroupName);
        }

        public IAgentExecutionRepository NewExecutionRepository()
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
        public IAgentMediator NewAgentMediator(System.Xml.XmlNode agentNode)
        {
            string agentName = null;

            // Get "name", method", "interval" attributes for each agent node
            object obj2 = Factory.CreateObject(agentNode, true);
            string methodName = StringUtil.GetString(new string[] { XmlUtil.GetAttribute("method", agentNode), "Execute" });

            agentName = StringUtil.GetString(new string[] { XmlUtil.GetAttribute("name", agentNode), obj2.GetType().FullName });
                        
            int executionPrority ;
            if (!int.TryParse(XmlUtil.GetAttribute("executionPriority", agentNode), out executionPrority))
            {
                executionPrority = _executionPriorityCounter++;
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
            } while (_agentNameConflictList.Contains(agentPraposedName))
                ;
            _agentNameConflictList.Add(agentPraposedName);

			string intervalStrPattern = XmlUtil.GetAttribute("interval", agentNode) ?? string.Empty;

            IAgentMediator agent =
                new AgentMediator.Builder()
                    .SetAgent(obj2)
                    .SetAgentName(agentPraposedName)
                    .SetExecuteMethod(methodName)

                    /* we set to default DateTime.MinValue, as appose to DateTime.UtcNow
                     * because for schedules that are once a week our next runtime might
                     * be one week later; but, we want it to run immediately.
                     * The GetAgentLastRunTimes, should override this when possible
                     */
                    .SetLastRunTime(DateTime.MinValue)

                    .SetRecurrence(intervalStrPattern)
                    .SetExecutionPriority(executionPrority)
                    .Build();

                return agent;

        }

    }
}