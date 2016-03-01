using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using Sitecore.Diagnostics;
using Sitecore.Pipelines;

namespace Sitecore.Strategy.Scheduler.Pipelines.WorkerLoop
{
    public class InitializeExecutionList
    {
        public void Process(ISchedulerArgs schedulerArgs)
        {
            Assert.ArgumentNotNull(schedulerArgs, "schedulerArgs");
            Assert.IsTrue(schedulerArgs is PipelineArgs, "schedulerArgs is Not PipelineArgs");

            Log.Info("Scheduler - Clear processed agents list.", this);

            if (HostingEnvironment.ShutdownReason != ApplicationShutdownReason.None)
            {
                (schedulerArgs as PipelineArgs).AbortPipeline();
            }
            else
            {

                schedulerArgs.ProcessedAgentMediators.Clear();
            }
            
        }
    }
}