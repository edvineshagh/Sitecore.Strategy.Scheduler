using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.Tasks;

namespace Sitecore.Strategy.Scheduler.Model.NullAgent
{
    /// <summary>
    /// Employ null pattern for invalid agents.
    /// </summary>
    public class NullAgentMediator : IAgentMediator
    {
        public class NullAgent { public void Run() {} }

        private readonly Tasks.Recurrence _recurrence;
        private readonly NullAgent _nullAgent;

        public NullAgentMediator() : this(typeof(NullAgentMediator).Name)
        {

        }

        public NullAgentMediator(string agentName)
        {
            this.TypeName = agentName;
            _recurrence = new Recurrence("|||00:00:00");
            _nullAgent = new NullAgent();
        }

        public int ExecutionPriority
        {
            get { return 0; }
            set { }
        }

        public string TypeName  {get; private set;}

        public void SetLastRunTime(DateTime value)
        {
        }

        public DateTime GetLastRunTime()
        {
            return DateTime.MaxValue;
        }

        public void SetNextRunTime(DateTime value)
        {
        }

        public DateTime GetNextRunTime()
        {
            return DateTime.MaxValue;
        }

        public bool IsDue
        {
            get { return false; }
        }

        public string Name
        {
            get { return TypeName; }
            set { /* blank */ }
        }

        public void Execute()
        {
        }

        public object Agent
        {
            get { return _nullAgent; }
        }


        public Tasks.Recurrence Recurrence
        {
            get { return _recurrence; }
        }

        public bool IsRecurrenceInterval
        {
            get { return false; }
        }
    }
}