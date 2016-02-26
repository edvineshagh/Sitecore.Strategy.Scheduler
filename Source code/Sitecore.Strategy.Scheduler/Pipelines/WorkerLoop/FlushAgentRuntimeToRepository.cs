﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.Configuration;
using Sitecore.Diagnostics;

namespace Sitecore.Strategy.Scheduler.Pipelines.WorkerLoop
{
    public class FlushAgentRuntimeToRepository
    {
        public void Process(ISchedulerArgs schedulerArgs)
        {
            Assert.ArgumentNotNull(schedulerArgs, "schedulerArgs");
            Assert.ArgumentNotNull(schedulerArgs.AgentMediators, "schedulerArgs.AgentMediators");
            Assert.ArgumentNotNull(schedulerArgs.ProcessedAgentMediators, "schedulerArgs.ProcessedAgentMediators");

            Log.Info("Scheduler - flush agent run times to repository.", this);


            var agentHistory = FactoryInstance.Current.NewExecutionRepository();

            agentHistory.FlushLastAgentRunTimes(schedulerArgs.AgentMediators);
        }
    }
}