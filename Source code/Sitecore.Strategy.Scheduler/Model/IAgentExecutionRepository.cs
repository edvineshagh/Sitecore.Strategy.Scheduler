using System;
using System.Collections.Generic;

namespace Sitecore.Strategy.Scheduler.Model
{
    /// <summary>
    /// (Re)Store Agent execution times from external storage
    /// </summary>
    public interface IAgentExecutionRepository
    {
        /// <summary>
        /// Gets retrieves last agent execution record.
        /// </summary>
        /// <param name="agentName">Unique agent name use to lookup record item</param>
        /// <returns>If no record is found, then null is returned.  Otherwise, respective record is returned.</returns>
        IAgentExecutionRecord GetById(string agentName);

        /// <summary>
        /// Returns all the execution records from the repository.
        /// </summary>
        /// <returns></returns>
        IEnumerable<IAgentExecutionRecord> GetExecutionRecords();

        /// <summary>
        /// Add agent execution record to repository.
        /// </summary>
        /// <param name="record">record to persist</param>
        void Add(IAgentExecutionRecord record);

        /// <summary>
        /// Flush record into repository.
        /// </summary>
        void Save();

    }
}