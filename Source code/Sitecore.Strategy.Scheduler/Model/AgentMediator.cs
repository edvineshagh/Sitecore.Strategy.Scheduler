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
     
        private AgentMediator()
        {
        }

        public int ExecutionPriority { get; set; }

        public bool IsRecurrenceInterval { get; private set; }  // as appose to specific time of day*/  

        public Recurrence Recurrence
        {
            get { return _recurrence; }

            private set
            {
                _recurrence = value;

                _isUtcTimeSpecified = _recurrence != null
                    && _recurrence.ToString().IndexOf("Z", StringComparison.OrdinalIgnoreCase) > 0;
            }
        }

        public Object Agent { get { return _agentInstance; } }

        public void Execute()
        {
            try
            {
                JobOptions options = new JobOptions(this.Name, "schedule", "scheduler", this._agentInstance, this._executeMethod)
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


        private DateTime _lastRunTime;

        public void SetLastRunTime(DateTime value)
        {
            _lastRunTime = value;
            _recalcNextRunTime = true;
        }

        public DateTime GetLastRunTime()
        {
            return _lastRunTime;
        }

        private DateTime _nextRunTime;

        public void SetNextRunTime(DateTime value)
        {
            _nextRunTime = value;
            _recalcNextRunTime = false;
        }

        public DateTime GetNextRunTime()
        {
            if (_recalcNextRunTime)
            {
                _recalcNextRunTime = false;
                var now = DateTime.UtcNow;
                var oneDay = new TimeSpan(days: 1, hours: 0, minutes: 0, seconds: 0);

                if (!IsRecurrenceInterval)
                {
                    _nextRunTime = this.GetLastRunTime() - GetLastRunTime().TimeOfDay + this.Recurrence.Interval;

                    const int twoWeeks = 14;
                    int additionalDays = 0;
                    while ( //_nextRunTime < now ||
                        _nextRunTime < this.GetLastRunTime() ||
                        !IsValidDay(_nextRunTime) ||
                        additionalDays > twoWeeks)
                    {
                        _nextRunTime += oneDay;
                        additionalDays++;
                    }

                    if (additionalDays > twoWeeks)
                    {
                        Log.Error("Scheduler - Disabling agent. Infinite loop detected for agent next runtime - " + this.Name,
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
                    _nextRunTime = DateTime.MaxValue;
                }
            }
            return _nextRunTime;
        }

        private bool IsValidDay(DateTime dateTime)
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

        public bool IsDue
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

        public string Name
        {
            get
            {
                return this._agentName;
            }
        }


    }

}