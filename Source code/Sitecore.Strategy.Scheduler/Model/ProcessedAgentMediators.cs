using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sitecore.Strategy.Scheduler.Model
{
    /// <summary>
    /// List of agent mediators that have been proceeded (executed).
    /// </summary>
    /// <seealso cref="System.Collections.Generic.List{Sitecore.Strategy.Scheduler.Model.IAgentMediator}" />
    /// <seealso cref="Sitecore.Strategy.Scheduler.Model.IProcessedAgentMediators" />
    public class ProcessedAgentMediators : List<IAgentMediator>, IProcessedAgentMediators
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessedAgentMediators"/> class.
        /// </summary>
        /// <param name="maxSize">The maximum size.</param>
        public ProcessedAgentMediators(int maxSize) : base(capacity: maxSize)
        {
        }

        /// <summary>
        /// Removes the specified agent mediator.
        /// </summary>
        /// <param name="agentMediator">The agent mediator.</param>
        public new void Remove(IAgentMediator agentMediator)
        {
            base.Remove(agentMediator);
        }
    }
}