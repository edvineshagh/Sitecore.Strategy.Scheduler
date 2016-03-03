using System.Threading;

namespace Sitecore.Strategy.Scheduler.Example
{
    /// <summary>
    /// Sample loger agent that is used to test scheduling activities.
    /// </summary>
    public class LoggerAgent
    {
        private readonly string _messageToLog ;
        private readonly int _sleepDurationInSeconds;

        private int _counter = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggerAgent"/> class.
        /// </summary>
        /// <param name="messageToLog">The message to log.</param>
        public LoggerAgent(string messageToLog) : this(messageToLog, 0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggerAgent"/> class.
        /// </summary>
        /// <param name="messageToLog">The message to log.</param>
        /// <param name="sleepDurationInSeconds">The sleep duration in seconds.</param>
        public LoggerAgent(string messageToLog, string sleepDurationInSeconds)
        {
            int.TryParse(sleepDurationInSeconds, out _sleepDurationInSeconds);
            _messageToLog = messageToLog;

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggerAgent"/> class.
        /// </summary>
        /// <param name="messageToLog">The message to log.</param>
        /// <param name="sleepDurationInSeconds">The sleep duration in seconds.</param>
        public LoggerAgent(string messageToLog, int sleepDurationInSeconds)
        {
            _messageToLog = messageToLog;
            _sleepDurationInSeconds = sleepDurationInSeconds;
        }

        /// <summary>
        /// Runs this instance of the agent.
        /// </summary>
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