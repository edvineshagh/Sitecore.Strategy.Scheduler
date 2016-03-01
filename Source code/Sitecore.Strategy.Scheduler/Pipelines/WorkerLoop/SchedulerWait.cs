using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.Shell.Applications.ContentEditor;
using Sitecore.Strategy.Scheduler.Pipelines.SchedulerInitialization;
using DateTime = System.DateTime;

namespace Sitecore.Strategy.Scheduler.Pipelines.WorkerLoop
{
    public class SchedulerWait
    {
        public void Process(ISchedulerArgs schedulerArgs)
        {
            Assert.ArgumentNotNull(schedulerArgs, "schedulerArgs");
            Assert.ArgumentNotNull(schedulerArgs.AgentMediators, "schedulerArgs.AgentMediators");
            Assert.ArgumentNotNull(schedulerArgs.ProcessedAgentMediators, "schedulerArgs.ProcessedAgentMediators");


            var sleepInterval = DateUtil.ParseTimeSpan(Factory.GetString("scheduling/frequency", false),
           TimeSpan.FromMinutes(1.0));



            var nextAgentTimeSpan = schedulerArgs.AgentMediators == null || schedulerArgs.AgentMediators.Count == 0
                ? sleepInterval
                : schedulerArgs.AgentMediators.Top().GetNextRunTime() - DateTime.UtcNow;


            // Wait until configured scheduling/frequency if next agent execution is less than
            // configured amount; otherwise, sleep until next agent execution time.
            var sleepDuration = nextAgentTimeSpan > sleepInterval ? nextAgentTimeSpan : sleepInterval;


            Log.Info(string.Format("Scheduler - Sleeping; Re-run in: {0}, at: {1}", sleepDuration,
                (DateUtil.ToServerTime(DateTime.UtcNow) + sleepDuration).ToString("yyyy-MM-dd HH:mm:ss")), this);


            Thread.Sleep(sleepDuration);
        }
    }
}