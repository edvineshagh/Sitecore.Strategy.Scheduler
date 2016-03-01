using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sitecore.Strategy.Scheduler.Model
{
    public class ProcessedAgentMediators : List<IAgentMediator>, IAgentMediators
    {
        public ProcessedAgentMediators(int maxSize) : base(capacity: maxSize)
        {
        }

        public new void Remove(IAgentMediator agentMediator)
        {
            base.Remove(agentMediator);
        }
    }
}