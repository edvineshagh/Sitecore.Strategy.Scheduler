using System;
using System.Threading;
using System.Web;
using System.Web.Hosting;
using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.Diagnostics.PerformanceCounters;
using Sitecore.Pipelines;

namespace Sitecore.Strategy.Scheduler.Pipelines
{

    /// <summary>
    /// Entry point for scheduler processes.
    /// </summary>
    public class StartScheduler
    {
        private static readonly object _lock = new object();
        private static Thread _workerThread;
        

        public void Process(PipelineArgs args)
        {

            //////////////////////////////////
            // Detect if scheduler is disabled
            //
            var sleepInterval = DateUtil.ParseTimeSpan(Factory.GetString("scheduling/frequency", false),
                TimeSpan.FromMinutes(1.0));

            if (sleepInterval.Ticks <= 0)
            {
                Log.Info("Scheduler - Scheduling is disabled (interval is 00:00:00)", this);
                return;
            }


            lock (_lock)
            {
                
                if (_workerThread != null)
                {
                    Log.Info("Scheduler - Already started.", this);
                    return;
                }

                ///////////////////////////////////
                // Start the worker thread pipeline
                //

                _workerThread = new Thread(StartScheduler.WorkLoop);
                _workerThread.Start();
                SystemCount.ThreadingBackgroundThreadsStarted.Increment(1L);
                Log.Info("Scheduler - Worker thread started", this);
            }

        }

        /// <summary>
        /// Work horse for running the scheduling jobs
        /// </summary>
        private static void WorkLoop()
        {

            var defaultSleepInterval = DateUtil.ParseTimeSpan(Factory.GetString("scheduling/frequency", false),
                TimeSpan.FromMinutes(1.0));

            ////////////////////////////////
            // Initialize scheduler pipeline
            //
            var initArgPipline = FactoryInstance.Current.NewSchedulerPipeline();
            var schedulerArgs = FactoryInstance.Current.NewSchedulerPipelineArgs() ;

            initArgPipline.Run(schedulerArgs as PipelineArgs);

            ////////////////////////////////
            // Run scheduler pipeline
            var workerLoopPipeline = FactoryInstance.Current.NewThreadWorkerLoopPipeline();

            while (HostingEnvironment.ShutdownReason == ApplicationShutdownReason.None)
            {
                try
                {
                    schedulerArgs.SleepDuration = TimeSpan.Zero; // set default

                    workerLoopPipeline.Run(schedulerArgs as PipelineArgs);

                    Assert.IsTrue(schedulerArgs.SleepDuration.Ticks > 0, 
                        string.Format("Invalid sleep duration ({0}) after last execution.", 
                        schedulerArgs.SleepDuration));                   
                    
                }
                catch (Exception e)
                {
                    Sitecore.Diagnostics.Log.Error("Unhanded exception occurred while executing scheduler agents.", 
                        e, typeof(StartScheduler));

                    // if we get here, then the SchedulerWait processor may gave never been
                    // called; so, we force a sleep time here.
                    Thread.Sleep(defaultSleepInterval);
                }
            }

            _workerThread = null;
            SystemCount.ThreadingBackgroundThreadsStarted.Decrement();
        }
    }
}