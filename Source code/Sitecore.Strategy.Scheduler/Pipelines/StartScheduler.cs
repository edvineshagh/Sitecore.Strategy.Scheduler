using System.Threading;
using System.Web;
using System.Web.Hosting;
using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.Diagnostics.PerformanceCounters;
using Sitecore.Pipelines;

namespace Sitecore.Strategy.Scheduler.Pipelines
{
    public class StartScheduler
    {
        private static readonly object _lock = new object();
        private static Thread _workerThread;

        public void Process(PipelineArgs args)
        {
            var sleepInterval = DateUtil.ParseTimeSpan(Factory.GetString("scheduling/frequency", false));

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

            ////////////////////////////////
            // Initialize scheduler pipeline
            //
            var initArgPipline = FactoryInstance.Current.NewSchedulerPipeline();
            var schedulerArgs = FactoryInstance.Current.NewSchedulerPipelineArgs() as PipelineArgs;

            initArgPipline.Run(schedulerArgs);


            ////////////////////////////////
            // Run scheduler pipeline
            var workerLoopPipeline = FactoryInstance.Current.NewThreadWorkerLoopPipeline();

            while (HostingEnvironment.ShutdownReason == ApplicationShutdownReason.None)
            {
                workerLoopPipeline.Run(schedulerArgs);
            }

            _workerThread = null;
            SystemCount.ThreadingBackgroundThreadsStarted.Decrement();
        }
    }
}