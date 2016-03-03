using System;
using Sitecore.Diagnostics;
using Sitecore.Jobs;
using Sitecore.SecurityModel;
using Sitecore.Strategy.Scheduler.Model.NullAgent;
using Sitecore.Tasks;
using DateTime = System.DateTime;

namespace Sitecore.Strategy.Scheduler.Model
{
    /// <summary>
    /// Contains information about agent scheduling, 
    /// as well as the ability to execute the agent.
    /// </summary>
    public partial class AgentMediator : IAgentMediator
    {

        private string _executeMethod;
        private string _agentName;
        private object _agentInstance;
        private Recurrence _recurrence;
        private bool _isUtcTimeSpecified;
        private bool _recalcNextRunTime = true;
        private DateTime _lastRunTime, _nextRunTime;


        /// <summary>
        /// Prevents a default instance of the <see cref="AgentMediator"/> class from being created.
        /// Use AgentMediator.Builder to create an instance of this object since it take many parameters.
        /// </summary>
        private AgentMediator()
        {
        }

        /// <summary>
        /// This property is used by the caller to determine agent execution order.
        /// </summary>
        public virtual int ExecutionPriority { get; set; }

        /// <summary>
        /// If false, then the interval portion of Recurrence is
        /// interpreted as time of day; otherwise, it is treated as timespan interval.
        /// </summary>
        public virtual bool IsRecurrenceInterval { get; private set; }  // as appose to specific time of day*/  


        /// <summary>
        /// Recurrence schedule.
        /// </summary>
        public virtual Recurrence Recurrence
        {
            get { return _recurrence; }

            private set
            {
                _recurrence = value;

                _isUtcTimeSpecified = _recurrence != null
                    && _recurrence.ToString().IndexOf("Z", StringComparison.OrdinalIgnoreCase) > 0;
            }
        }

        /// <summary>
        /// Instance of the agent to be executed.
        /// </summary>
        public Object Agent { get { return _agentInstance; } }

        /// <summary>
        /// Method that is responsible for invocation of the agent.
        /// </summary>
        public virtual void Execute()
        {
            try
            {
                JobOptions options = new JobOptions(this.AgentName, "schedule", "scheduler", this._agentInstance, this._executeMethod)
                {
                    SiteName = "scheduler",
                    ContextUser = (DomainManager.GetDomain("sitecore") ?? DomainManager.GetDefaultDomain()).GetAnonymousUser()
                };
                JobManager.Start(options).WaitHandle.WaitOne();
            }
            finally
            {
                this.SetLastRunTime(DateTime.UtcNow);
                _recalcNextRunTime = true;
            }
        }


        /// <summary>
        /// Last time that the agent was executed.  Updating this method will affect the result of <see cref="GetNextRunTime()"/>.
        /// </summary>
        /// <param name="value"></param>
        public virtual void SetLastRunTime(DateTime value)
        {
            _lastRunTime = value;
            _recalcNextRunTime = true;
        }

        /// <summary>
        /// Last time that the agent was executed.
        /// </summary>
        /// <returns></returns>
        public virtual DateTime GetLastRunTime()
        {
            return _lastRunTime;
        }

        /// <summary>
        /// Set the next time that the agent is scheduled to execute.
        /// </summary>
        /// <param name="value"></param>
        public virtual void SetNextRunTime(DateTime value)
        {
            _nextRunTime = value;
            _recalcNextRunTime = false;
        }

        /// <summary>
        /// Get next time that the agent is scheduled to execute.
        /// </summary>
        /// <returns></returns>
        public virtual DateTime GetNextRunTime()
        {
            if (_recalcNextRunTime)
            {
                _recalcNextRunTime = false;
                var now = DateTime.UtcNow;
                var oneDay = new TimeSpan(days: 1, hours: 0, minutes: 0, seconds: 0);

                // The agent interval in the configuration file the symbol '@' to denote
                // execution for a specific time; therefor, IsRecurrenceInterval is false.
                if (!IsRecurrenceInterval)
                {
                    // Execution is expected to be daily at a specific time; where the 
                    // frequency is determined by day of the week-day.  Therefore, 
                    // the next runtime should not be more than a week since last execution.
                    
                    var executionDay = this.GetLastRunTime() - GetLastRunTime().TimeOfDay;
                    _nextRunTime = executionDay + this.Recurrence.Interval;

                    const int twoWeeks = 14;
                    int additionalDays = 0;

                    while ( _nextRunTime < this.GetLastRunTime() ||
                            !IsValidDay(_nextRunTime) ||
                            additionalDays > twoWeeks)
                    {
                        _nextRunTime += oneDay;
                        additionalDays++;
                    }

                    if (additionalDays > twoWeeks)
                    {
                        Log.Error("Scheduler - Disabling agent. Infinite loop detected for agent next runtime - " + this.AgentName,
                            this);
                        _nextRunTime = DateTime.MaxValue;
                    }
                }
                else if (this.Recurrence.Interval.Ticks > 0)
                {
                    var executeCount = (this.GetLastRunTime() - this.Recurrence.StartDate).Ticks
                                       /this.Recurrence.Interval.Ticks;

                    var nextRunTimespan = new TimeSpan((1 + executeCount)*this.Recurrence.Interval.Ticks);

                    _nextRunTime = this.Recurrence.StartDate.AddTicks(nextRunTimespan.Ticks);
                }
                else
                {
                    // Zero interval ticks means that the agent is disabled
                    Log.Info(string.Format("Scheduler - Skipping disabled agent {0}", AgentName), this);
                    _nextRunTime = DateTime.MaxValue;
                }
            }
            return _nextRunTime;
        }

        /// <summary>
        /// Determines whether is valid day of the week for [the specified date time].
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns></returns>
        protected virtual bool IsValidDay(DateTime dateTime)
        {
            // Recall that Recurrence.IsValidDay checks against the Utc DateTime
            // So we need to make that adjustment if user wants to test 
            // against local server timestamp

            var timeDifference = 
                  _isUtcTimeSpecified 
                ? new TimeSpan() 
                : dateTime - DateUtil.ToServerTime(dateTime);

            return this.Recurrence.IsValidDay(dateTime - timeDifference);
        }

        /// <summary>
        /// Returns true if agent should be executed; otherwise, false is returned.
        /// </summary>
        public virtual bool IsDue
        {
            get
            {
                var now = DateTime.UtcNow;

                bool isValidDayOfWeek = IsValidDay(now);

                bool isDue = this.Recurrence.InRange(now)   /* between start and end date in UTC */
                            && isValidDayOfWeek
                            && now > this.GetNextRunTime();

                return isDue;
            }
        }

        /// <summary>
        /// Agent name.
        /// </summary>
        public virtual string AgentName
        {
            get
            {
                return this._agentName;
            }
        }


    }

}