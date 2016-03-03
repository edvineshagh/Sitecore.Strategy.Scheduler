using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sitecore.Strategy.Scheduler
{
    /// <summary>
    /// Singleton instance of the scheduler factory set via Sitecore configuration file.
    /// </summary>
    public class FactoryInstance
    {
        private static readonly object _lock = new object();
        private static ISchedulerFactory _instance;

        public static ISchedulerFactory Current
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance =
                                Sitecore.Configuration.Factory.CreateObject("scheduling/factory", true) as
                                    ISchedulerFactory;
                        }
                    }
                }

                return _instance;
            }
        }
    }

}