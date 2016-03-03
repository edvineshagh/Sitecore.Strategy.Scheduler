using Sitecore.Diagnostics;
using Sitecore.Strategy.Scheduler.Model.NullAgent;
using Sitecore.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sitecore.Strategy.Scheduler.Model
{
    public partial class AgentMediator
    {
        /// <summary>
        /// Builder pattern to aid in creation of AgentMediator.
        /// </summary>
        public class Builder
        {
            private readonly AgentMediator _agentMediator;

            public Builder()
            {
                _agentMediator = new AgentMediator();

            }

            /// <summary>
            /// Builds an instance of AgentMediator.
            /// </summary>
            /// <returns></returns>
            public IAgentMediator Build()
            {
                Assert.IsNotNullOrEmpty(_agentMediator.AgentName, "AgentName");
                Assert.IsNotNull(_agentMediator.Agent, "Agent");
                Assert.IsNotNullOrEmpty(_agentMediator._executeMethod, "ExecuteMethod");
                Assert.IsNotNull(_agentMediator.Recurrence, "Recurrence");

                if (_agentMediator.IsRecurrenceInterval
                && _agentMediator.Recurrence.Interval.Ticks == 0)
                {
                    return new DisabledAgentMediator(_agentMediator.AgentName);
                }
                else
                {
                    return _agentMediator;
                }
            }

            /// <summary>
            /// Sets the name of the agent.
            /// </summary>
            /// <param name="name">The name.</param>
            /// <returns>Builder</returns>
            public Builder SetAgentName(string name)
            {
                _agentMediator._agentName = name;
                return this;
            }

            /// <summary>
            /// Sets the agent instance.
            /// </summary>
            /// <param name="instance">The instance.</param>
            /// <returns>Builder</returns>
            public Builder SetAgent(object instance)
            {
                Assert.ArgumentNotNull(instance, "Agnet instance");
                _agentMediator._agentInstance = instance;
                return this;
            }

            /// <summary>
            /// Sets the execute method name that will be used to invoke the agent.
            /// </summary>
            /// <param name="methodName">Name of the method.</param>
            /// <returns>Builder</returns>
            public Builder SetExecuteMethod(string methodName)
            {
                Assert.ArgumentNotNullOrEmpty(methodName,"methodName");
                _agentMediator._executeMethod = methodName;
                return this;
            }

            /// <summary>
            /// Sets the execution recurrence schedule.
            /// </summary>
            /// <param name="recurrence">The recurrence.</param>
            /// <returns>Builder.</returns>
            public Builder SetRecurrence(Recurrence recurrence)
            {
                Assert.ArgumentNotNull(recurrence, "recurrence");
                _agentMediator.Recurrence = recurrence;
                return this;
            }

            /// <summary>
            /// Sets the execution recurrence schedule.
            /// </summary>
            /// <param name="recurrenceStrPattern">The recurrence string pattern.</param>
            /// <returns></returns>
            public Builder SetRecurrence(string recurrenceStrPattern)
            {
                Assert.ArgumentNotNullOrEmpty(recurrenceStrPattern, "recurrence");

                // Utilize Sitecore.Tasks.Recurrence string pattern for interval
                // So, if we only get the interval string, we will convert it 
                // to expected recurrence pattern

                int pipeCount = recurrenceStrPattern.Split(new char[] { '|' }).Length - 1;

                if (pipeCount < 3)
                {

                    recurrenceStrPattern =
                        DateUtil.IsoNowDate + "|" +
                        (new string('|', 2 - pipeCount)) + recurrenceStrPattern;
                }


                // the @ is used to denote specific time of day; as appose to interval
                _agentMediator.IsRecurrenceInterval = !recurrenceStrPattern.Contains("@");


                if (!_agentMediator.IsRecurrenceInterval)
                {
                    /* recall the pattern is startDateTime|endDateTime|DaysOfWeek|SleepIntervalOrShceduleTime
                     * and we are interested in the last value within the pipe (i.e. the SleepIntervalOrShceduleTime)
                     */

                    var pipeIndex = recurrenceStrPattern.LastIndexOf('|');

                    // convert time to Utc for comparison and remove the @ symbol

                    var timeVal = recurrenceStrPattern.Remove(0, pipeIndex + 1).Replace("@", string.Empty);

                    var newDateTime =
                        DateUtil.ToUniversalTime(
                            DateUtil.ParseDateTime(
                                string.Format("{0}T{1}", DateTime.UtcNow.ToString("yyyy-MM-dd"), timeVal)
                                + (recurrenceStrPattern.IndexOf("Z", StringComparison.OrdinalIgnoreCase) > 0 ? "Z" : string.Empty)
                                , DateTime.MaxValue
                            )
                        );

                    var newTime = newDateTime.ToString("HHmmss");

                    recurrenceStrPattern = recurrenceStrPattern.Substring(0, pipeIndex + 1) + newTime;

                }

                return SetRecurrence(new Recurrence(recurrenceStrPattern));
            }

            /// <summary>
            /// Sets the agent last run time.
            /// </summary>
            /// <param name="lastRunTime">The last run time.</param>
            /// <returns>Builder.</returns>
            public Builder SetLastRunTime(DateTime lastRunTime)
            {
                _agentMediator.SetLastRunTime(lastRunTime);
                return this;
            }

            /// <summary>
            /// Sets the execution priority used for when multiple agents are scheduled to run at the same time.  
            /// Execution priority is numeric order; therefore, lower numbers have a higher priority.
            /// </summary>
            /// <param name="priority">The priority.</param>
            /// <returns>Builder.</returns>
            public Builder SetExecutionPriority(int priority)
            {
                _agentMediator.ExecutionPriority = priority;
                return this;
            }


        }
    }
}