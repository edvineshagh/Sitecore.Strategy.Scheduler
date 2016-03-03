using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Sitecore.Pipelines.HealthMonitor;
using Sitecore.Strategy.Scheduler.Model.NullAgent;
using Lucene.Net.Util;
using System.Collections;

namespace Sitecore.Strategy.Scheduler.Model
{
    /// <summary>
    /// Ordered agent mediators that are based on the NextRunTime and ExecutionPriority.
    /// </summary>
    public class OrderedAgentMediators : PriorityQueue<IAgentMediator>, IAgentMediatorsHeap
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrderedAgentMediators"/> class.
        /// </summary>
        /// <param name="maxSize">The maximum number of elements allowed in the list.</param>
        public OrderedAgentMediators(int maxSize) 
        {
            base.Initialize(maxSize);
        }

        /// <summary>
        /// Returns number of elements in the priority queue.
        /// </summary>
        public int Count { get { return Size(); } }


        /// <summary>
        /// Lesses the than.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <returns></returns>
        public override bool LessThan(IAgentMediator a, IAgentMediator b)
        {
            if (a == null)
            {
                return true;
            }
            else if (b == null)
            {
                return false;
            }

            var timeDiff = a.GetNextRunTime() - b.GetNextRunTime();

            if (a.GetNextRunTime() <= b.GetNextRunTime())
            {
                return true;
            }
            else if (a.GetNextRunTime() > b.GetNextRunTime())
            {
                return false;
            }

            return a.ExecutionPriority < b.ExecutionPriority;
        }

        /// <summary>
        /// Add agent mediator to the heap.  
        /// If agentMediator is null, then it is not added.
        /// </summary>
        /// <param name="agentMediator">Instance to add to the priority queue.</param>
        /// <returns>The entity that was just added.</returns>
        public new IAgentMediator Add(IAgentMediator agentMediator)
        {
            if (agentMediator == null 
            || agentMediator is NullAgentMediator)
            {
                return null;
            }
            return base.Add(agentMediator);
        }

        /// <summary>
        /// Retrieve items from heap as immutable objects;
        /// so, that the internals of the heap are not effected.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IAgentExecutionRecord> GetItems()
        {
            for (int i = 0; i < Count ; i++)
            {
                if (heap[i] != SentinelObject)
                {

                    IAgentExecutionRecord rec = FactoryInstance.Current.NewAgentExecutionRepositoryRecord();

                    
                    rec.AgentName = heap[i].AgentName;
                    rec.AgentType = heap[i].Agent== null ? null : heap[i].Agent.GetType();
                    rec.LastRunTime = heap[i].GetLastRunTime();
                    rec.NextRunTime = heap[i].GetNextRunTime();
                   
                    yield return rec;
                }
            }
        }
    }
}