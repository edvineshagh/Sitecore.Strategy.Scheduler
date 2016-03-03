using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.Syndication;

namespace Sitecore.Strategy.Scheduler.Model
{
    /// <summary>
    /// List of agent mediators that have been proceeded (executed).
    /// </summary>
    /// <seealso cref="System.Collections.Generic.IEnumerable{Sitecore.Strategy.Scheduler.Model.IAgentMediator}" />
    public interface IProcessedAgentMediators : IEnumerable<IAgentMediator>
    {
        /// <summary>
        /// Adds the specified agent mediator.
        /// </summary>
        /// <param name="agentMediator">The agent mediator.</param>
        void Add(IAgentMediator agentMediator);

        /// <summary>
        /// Removes the specified agent mediator.
        /// </summary>
        /// <param name="agentMediator">The agent mediator.</param>
        void Remove(IAgentMediator agentMediator);

        /// <summary>
        /// Clears this instance.
        /// </summary>
        void Clear();
    }
}
