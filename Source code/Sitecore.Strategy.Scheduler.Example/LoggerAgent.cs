namespace Sitecore.Strategy.Scheduler.Example
{
    public class LoggerAgent
    {
        private readonly string _messageToLog ;

        private int _counter = 0;

        public LoggerAgent(string messageToLog)
        {
            _messageToLog = messageToLog;
        }

        public void Run()
        {
            Sitecore.Diagnostics.Log.Info(string.Format("{0}{1}",_messageToLog, ++_counter), this);
        }
    }
}