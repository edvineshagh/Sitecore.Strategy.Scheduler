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

    /// <summary>
    /// The processor is responsible for setting the sleep duration 
    /// before re-evaluating agents for execution.  The sleep duration 
    /// is the larger value between scheduler/frequency or 
    /// the agent with closest NextRunTime.
    /// </summary>
    public class SchedulerWait
    {
        public void Process(ISchedulerArgs schedulerArgs)
        {
            Assert.ArgumentNotNull(schedulerArgs, "schedulerArgs");
            Assert.ArgumentNotNull(schedulerArgs.AgentMediators, "schedulerArgs.AgentMediators");
            Assert.ArgumentNotNull(schedulerArgs.ProcessedAgentMediators, "schedulerArgs.ProcessedAgentMediators");


            try
            {

                schedulerArgs.SleepDuration = DateUtil.ParseTimeSpan(Factory.GetString("scheduling/frequency", assert: false),
                           TimeSpan.FromMinutes(1.0)); 
                
                var nextAgentTimeSpan = schedulerArgs.AgentMediators == null || schedulerArgs.AgentMediators.Count == 0
                    ? schedulerArgs.SleepDuration
                    : schedulerArgs.AgentMediators.Top().GetNextRunTime() - DateTime.UtcNow;


                // Wait until configured scheduling/frequency if next agent execution is less than
                // configured amount; otherwise, sleep until next agent execution time.

                if (nextAgentTimeSpan > schedulerArgs.SleepDuration)
                {
                    schedulerArgs.SleepDuration = nextAgentTimeSpan;
                }

            }
            catch (Exception e)
            {
                Log.Error(string.Format("Scheduler - Unable to determine best sleep duration between agent executions."), e, this);
            }
            finally
            {
                Log.Info(string.Format("Scheduler - Sleeping; Re-run in: {0}, at: {1}", schedulerArgs.SleepDuration,
                    (DateUtil.ToServerTime(DateTime.UtcNow) + schedulerArgs.SleepDuration).ToString("yyyy-MM-dd HH:mm:ss")), this);

                Thread.Sleep(schedulerArgs.SleepDuration);
            }
        }
    }
}