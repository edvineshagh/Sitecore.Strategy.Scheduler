using System;
using System.Collections.Generic;

namespace Sitecore.Strategy.Scheduler.Model
{
    /// <summary>
    /// More than one agent may possibly be scheduled at a given time interval.
    /// This class keeps track of all agents that must be executed per each interval.
    /// </summary>
    public class AgentPriorityList : SortedDictionary<DateTime, SortedAgentMediators>
    {
    }



}