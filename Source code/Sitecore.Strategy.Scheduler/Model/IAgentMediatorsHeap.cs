using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.Pipelines.LoggingIn;

namespace Sitecore.Strategy.Scheduler.Model
{
    /// <summary>
    /// Used to keep track of agents to executes via priority queue
    /// </summary>
    public interface IAgentMediatorsHeap 
    {
        /// <summary>
        /// Add agent mediator to the heap
        /// </summary>
        /// <param name="agentMediator"></param>
        /// <returns></returns>
        IAgentMediator Add(IAgentMediator agentMediator);

        /// <summary>
        /// Pop the top element from the heap
        /// </summary>
        /// <returns></returns>
        IAgentMediator Pop();
        
        /// <summary>
        /// Peek at the top element on the heap
        /// </summary>
        /// <returns></returns>
        IAgentMediator Top();

        /// <summary>
        /// Update the top element when it is changed
        /// </summary>
        /// <returns></returns>
        IAgentMediator UpdateTop();

        /// <summary>
        /// Retrieve items from heap as immutable objects; 
        /// so, that the internals of the heap are not effected.
        /// </summary>
        /// <returns></returns>
        IEnumerable<IAgentExecutionRecord> GetItems();

        /// <summary>
        /// Clears contents of the heap
        /// </summary>
        void Clear();

        /// <summary>
        /// Returns the size of the heap
        /// </summary>
        /// <value></value>
        int Count { get; }
    }
}
