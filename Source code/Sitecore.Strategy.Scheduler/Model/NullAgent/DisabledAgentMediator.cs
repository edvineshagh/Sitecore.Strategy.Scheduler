using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sitecore.Strategy.Scheduler.Model.NullAgent
{
    /// <summary>
    /// Employ a null pattern for disabled agents
    /// </summary>
    public class DisabledAgentMediator : NullAgentMediator, IAgentMediator
    {
        public DisabledAgentMediator(string agentName) : base(agentName)
        {   
        }
    }
}