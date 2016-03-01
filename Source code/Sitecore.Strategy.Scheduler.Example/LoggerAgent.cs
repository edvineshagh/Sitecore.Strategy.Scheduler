using System.Threading;

namespace Sitecore.Strategy.Scheduler.Example
{
    public class LoggerAgent
    {
        private readonly string _messageToLog ;
        private readonly int _sleepDurationInSeconds;

        private int _counter = 0;

        public LoggerAgent(string messageToLog) : this(messageToLog, 0)
        {
        }

        public LoggerAgent(string messageToLog, string sleepDurationInSeconds)
        {
            int.TryParse(sleepDurationInSeconds, out _sleepDurationInSeconds);
            _messageToLog = messageToLog;

        }

        public LoggerAgent(string messageToLog, int sleepDurationInSeconds)
        {
            _messageToLog = messageToLog;
            _sleepDurationInSeconds = sleepDurationInSeconds;
        }

        public void Run()
        {

            Sitecore.Diagnostics.Log.Info(string.Format("{0}{1} - LoggerAgent sleep duration: {2} seconds"
                ,_messageToLog, ++_counter, _sleepDurationInSeconds), this);

            if (_sleepDurationInSeconds > 0)
            {
                Thread.Sleep(_sleepDurationInSeconds);
            }
        }
    }
}