using System.Collections.Generic;

namespace Sitecore.Strategy.Scheduler.Model
{
    /// <summary>
    /// Agents are sorted based on execution priority
    /// </summary>
    public class SortedAgentMediators : SortedList<int, IAgentMediator>
    {
        private static readonly AgentExecutionPriorityComparer AgentComparer = new AgentExecutionPriorityComparer();

        public SortedAgentMediators()
            : base(AgentComparer)
        {
            // Pass internal comparer so that we don't get 
            // duplicate key exception when adding agent
            // with the same key
        }

        public void Add(IAgentMediator agent)
        {
            this.Add(agent.ExecutionPriority, agent);
        }

        private class AgentExecutionPriorityComparer : IComparer<int>
        {
            public int Compare(int x, int y)
            {
                // When keys are the same, we return 1
                // to avoid duplicate key exception.
                return x == y ? 1 : x - y;
            }
        }
    }
}