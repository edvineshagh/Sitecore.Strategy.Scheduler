using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.Syndication;

namespace Sitecore.Strategy.Scheduler.Model
{
    public interface IAgentMediators : IEnumerable<IAgentMediator>
    {
        void Add(IAgentMediator agentMediator);
        void Remove(IAgentMediator agentMediator);
        void Clear();
    }
}
