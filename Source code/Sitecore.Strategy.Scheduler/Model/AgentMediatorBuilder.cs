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

            public IAgentMediator Build()
            {
                Assert.IsNotNullOrEmpty(_agentMediator.Name, "AgentName");
                Assert.IsNotNull(_agentMediator.Agent, "Agent");
                Assert.IsNotNullOrEmpty(_agentMediator._executeMethod, "ExecuteMethod");
                Assert.IsNotNull(_agentMediator.Recurrence, "Recurrence");

                if (_agentMediator.IsRecurrenceInterval
                && _agentMediator.Recurrence.Interval.Ticks == 0)
                {
                    return new DisabledAgentMediator(_agentMediator.Name);
                }
                else
                {
                    return _agentMediator;
                }
            }

            public Builder SetAgentName(string name)
            {
                _agentMediator._agentName = name;
                return this;
            }


            public Builder SetAgent(object instance)
            {
                _agentMediator._agentInstance = instance;
                return this;
            }

            public Builder SetExecuteMethod(string methodName)
            {
                _agentMediator._executeMethod = methodName;
                return this;
            }


            public Builder SetRecurrence(Recurrence recurrence)
            {
                _agentMediator.Recurrence = recurrence;
                return this;
            }

            public Builder SetRecurrence(string recurrenceStrPattern)
            {

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

                _agentMediator.Recurrence = new Recurrence(recurrenceStrPattern);

                return this;
            }


            public Builder SetLastRunTime(DateTime lastRunTime)
            {
                _agentMediator.LastRunTime = lastRunTime;
                return this;
            }

            public Builder SetExecutionPriority(int priority)
            {
                _agentMediator.ExecutionPriority = priority;
                return this;
            }


        }
    }
}