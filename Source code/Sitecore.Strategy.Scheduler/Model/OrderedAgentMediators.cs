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
    /// Order agent mediators based on next execution priority
    /// </summary>
    public class OrderedAgentMediators : PriorityQueue<IAgentMediator>, IAgentMediatorsHeap
    {
        public OrderedAgentMediators(int maxSize) 
        {
            base.Initialize(maxSize);
        }


        public int Count { get { return Size(); } }


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


        public new IAgentMediator Add(IAgentMediator agentMediator)
        {
            if (agentMediator == null 
            || agentMediator is NullAgentMediator)
            {
                return null;
            }
            return base.Add(agentMediator);
        }

        
        public IEnumerable<IAgentExecutionRecord> GetItems()
        {
            for (int i = 0; i < Count ; i++)
            {
                if (heap[i] != SentinelObject)
                {

                    IAgentExecutionRecord rec = FactoryInstance.Current.NewAgentExecutionRepositoryRecord();

                    
                    rec.AgentName = heap[i].Name;
                    rec.AgentType = heap[i].Agent== null ? null : heap[i].Agent.GetType();
                    rec.LastRunTime = heap[i].GetLastRunTime();
                    rec.NextRunTime = heap[i].GetNextRunTime();
                   
                    yield return rec;
                }
            }
        }




    }
}