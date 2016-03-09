# Sitecore Scheduler #
This contribution adds pipeline processes to scheduling, and extends scheduling options from this:

	<agent interval="days.hh:mm:ss" .../>
to this:

	<agent interval="startTimeStamp|endTimeStamp|DaysOfWeek|durationOrTime" ... />  

### Table of contents ###

1. [Motivation](#Motivation)
2. [Installation](#Installation)
3. [Usage](#Usage)
4. [Example](#Example)
5. [Concerns](#Concerns)

# <a name="Motivation"></a> 1. Motivation #
This contribution aims to address existing limitations of Sitecore scheduler, which include:

1. Unable to schedule activities to run at a specific time.  The  recommended workarounds are [not very elegant](https://sdn.sitecore.net/Forum/ShowPost.aspx?PostID=29010 "Publishing site at specific time").  Some of the suggestions include:
 
	1.1. Create an agent with short sleep duration and manage the execution time yourself.

	1.2. Use windows scheduler

	1.3. Create Database maintenance task

	1.4. [Scheduling Database Agent to run Sitecore command](http://sitecore-community.github.io/docs/documentation/Sitecore%20Fundamentals/Asynchronous%20Tasks/#database_agent "Database Agent").  

	1.4. Create Windows services

	
2. Agents defined for the scheduler do not have "finer" reoccurring interval; for example, currently there is no way to run task every other weekday at 10:00 pm.  It is worth noting that there is a creative way to run a task by using a database command and [over write scheduler.LastRun in finally block](https://sitecorebasics.wordpress.com/2015/09/17/one-more-way-to-run-sitecore-scheduled-task-at-the-same-time-every-day/ "run a task at specific time").

3. Agent execution times are not persisted; thus, agents are re-executed during worker process recycling.

Additionally, this contribution can lead to better system utilization because:

1. Not every agent is evaluate for execution.

	The existing scheduler loops through every agent and call respective Scheduler+Agent.IsDue() method to determine if the agent execute method should be called.
	This contribution keeps track of agents next runtime in a heap (sorted list) and only calls agents whose runtime do not exceed the current timestamp.

2. Scheduler sleeps for longer duration if possible.

	The existing scheduler uses fixed-time-step sleep duration as specified by the configuration file setting `/sitecore/scheduling/frequency`.  Therefore, the scheduler thread wakes-up to re-evaluate agents 'IsDue() method even if there are no agents that need to be executed.  In contrast, this implementation uses variable-time-step duration, which is frequently seen in game and software simulation engines. With this approach, the scheduler keeps track of agents' next runtime and sleeps until the first agent that needs to be executed.  If the duration until the next agent runtime is less than the scheduler configuration frequency, then the configuration frequency is used for the sleep time to insure that the system is not overloaded by continual running back-to-back scheduled agents; thereby, perserving existing sheduler behavior.

3.  Control execution sequence 

	Not all task agents are scheduled to run asynchronously; thus, you may wish control the execution order when more than one agent runs within the same time interval.


# <a name="Installation"></a> 2. Installation #
Install the distribution package from Sitecore Desktop Start->Development Tools->Installation Wizard.  There is no need to alter existing agent intervals. You can make use of the enhanced functionality, as needed, by simply changing your interval to desired schedule.


# <a name="Usage"></a> 3. Usage #
Below is an example of an agent declaration within a configuration [patch file](http://sitecore-community.github.io/docs/documentation/Sitecore%20Fundamentals/Patch%20Files/ "Sitecore configuration patch files"):

	<agent name="..."
           type="..." 
           method="..."
		   executionPriority="..." 
           interval="..." />

## `name` attribute ##
The `name` attribute is a unique agent identifier.  If omitted, then the `type` is used as the agent identifier.  If multiple agents are defined with the same name, then a sequential numeric value is  appended to the name to insure uniqueness.  This name is visible in the `/Data/logs/SchedulerAgentsLastRun.log` file, which is used to keep track of last execution run time in the event the web worker process is recycled.  The log file path is configured in `Sitecore.Strategy.Schedulre.config` file under `scheduling/agentExecutionRepository` node.

## `type` and `method` attribute ##
The `type` attribute is the agent type that shall be used by the scheduler mediator, and the `method` specifies the method name that mediator shall invoke.  These two attributes are required.

## `executionPriority` attribute ##
The `executionPriority` is an optional attribute that handles the execution order of agents.  The `executionPriority` is an integer value that is assigned to agents based on their ordinal position (starting from 0).  If multiple agents are scheduled to run at a specific time, then they are executed in the order specified by this property.  The execution order can be changed by either changing the order in which agents are defined within the configuration file or by explicitly specifying the execution order.  Agents are executed from smallest to the largest execution priority value. Negative values for this property are permitted. 

## `interval` attribute ##
As mentioned, the interval is extended from this:

	<agent interval="days.hh:mm:ss" .../>
to:

	<agent interval="startTimeStamp|endTimeStamp|DaysOfWeek|durationOrTime" ... />  

 The required interval attribute contains either sleep duration between agent executions or the execution time of the day.  You may recognize the new format from Sitecore content tree item `/sitecore/system/Tasks/Schedules/*` scheduler field.  Internally, the module uses Sitecore.Tasks.Recurrence to parse the interval string pattern, which is a pipe delimited values for: `startTimeStamp|endTimeStamp|DaysOfWeek|durationOrTime`.  Below are examples, of agents running every 15 minutes:

	<agent interval="00:15:00" />

	<agent interval="|||00:15:00" />

	<agent interval="now|||00:15:00" />

	<agent interval="2000-12-31T23:59:59Z||0|00:15:00" ... />

The ***startTimeStamp*** and ***endTimeStamp*** are [iso time](https://en.wikipedia.org/wiki/ISO_8601 "ISO 8601") format, which can make use of constant strings `today`, `now`, `tomorrow`, `yesterday`.  For example, you can *disable an agent* by setting the end time stamp to `yesterday` instead of changing the sleep duration to zero.   Note that the start time of the last entry from the above example contains the optional **Z** suffix for start time, which designates zero Utc offset; otherwise, the time is checked against the local server time. If the value for start and end time stamps are excluded (e.g. "|||00:15:00") then they are set to `DateTime.MinValue` and `DateTime.MaxValue` respectively.

The start time stamp plays an important role in scheduling, as it is used to determine the next agent execution time.  For example, if you want to execute the agent every hour at 7 minutes past the hour then you would put your initial start time to be 7 minutes past the hour. The following example demonstrates such functionality:

	<agent interval="0001-01-01T00:07:00Z|||01:00:00" ... /> 

The ***DaysOfWeek*** (after 2nd pipe) value represents days of the week, which is compared against the Utc day if the start time uses Utc **Z** suffix designation (like above example); otherwise, the day of the week is evaluated based on server time. 
The days of the week is a numeric value, where `Everyday=0`, `Sun=1`, `Mon=2`, `Tue=4`, `Wed=8`, `Thu=16`, `Fri=32`, `Sat=64`.  If this value is omitted, then it is defaulted to `0` to denote every day. You can sum these numbers to get the desired days.  For example, to run a publisher agent on weekends (sat=64 + sun=1 = 65) at exactly 1:00 `am`, we would use the following format:

	<agent interval="||65|@01:00:00" ... />

The last entry of the interval attribute, ***DurationOrTime***, is either agent sleep duration or scheduled time of the day for execution.  If this field is prefixed by the symbol **@** (like the above example), then it is interpreted as a specific execution  time; otherwise, it is interpreted as an interval, which is a .NET [TimeSpan](https://msdn.microsoft.com/en-us/library/ee372286(v=vs.110).aspx "TimeSpan"). Also note that the above example excludes the start time stamp; therefore, the day of the week comparison is done against local server time.

Below example will execute agents at 4 `am` because the `@` prefix is included:

	<agent interval="@4:00:00" .../>
	<agent interval="|||@04:00:00" .../>
	<agent interval="now||0|@04:00:00" .../>  

where as the following examples will execute agents every 4 hours because the `@` symbol is omitted:

	<agent interval="4:00:00" .../>
	<agent interval="|||04:00:00" .../>
	<agent interval="now||0|04:00:00" .../>  


# <a name="Example"></a> 4. Example #
###1. Configuration path file example ###

	<configuration xmlns:patch="http://www.sitecore.net/xmlconfig/">
	  <sitecore>
	    <scheduling>
		  <!-- Simple agent 
		  -->
	       <agent name ="AA11"
	             type="Sitecore.Strategy.Scheduler.Example.LoggerAgent, Sitecore.Strategy.Scheduler.Example"
	             method="Run"
	             interval="00:00:20">
	             <param desc="messageToLog">====AAAA===== Test Log </param>
	            <param desc="sleep duration for test logging agent">0</param>
	      </agent>
	      
	      <!-- this agent runs first because of the executionPriority attribute-->
	      <agent name="BB22"
	             type="Sitecore.Strategy.Scheduler.Example.LoggerAgent, Sitecore.Strategy.Scheduler.Example" 
	             method="Run" 
	             interval="00:00:30"
	             executionPriority="-1">
	              <param desc="messageToLog">====BBBB===== Test Log </param>
	              <param desc="sleep duration for test logging agent">0</param>
	      </agent>
	
	      <!-- run agent every 15 minutes pass the hour -->
	      <agent name="CC33"
	             type="Sitecore.Strategy.Scheduler.Example.LoggerAgent, Sitecore.Strategy.Scheduler.Example"
	             method="Run"
	             interval="0001-01-01T00:00:15Z|||00:01:00"> 
	              <param desc="messageToLog">====CCCC===== Test Log </param>
	              <param desc="sleep duration for test logging agent">0</param>
	      </agent>
	
	      <!-- run agent on Friday at 2:00 pm local server time where
	           Everyday=0, Sun=1, Mon=2, Tue=4, Wed=8, Thu=16, Fri=32, Sat=64
	      -->
	      <agent name="DD44"
	         type="Sitecore.Strategy.Scheduler.Example.LoggerAgent, Sitecore.Strategy.Scheduler.Example"
	         method="Run"
	         interval="||0|@14:00:00">
	        <param desc="messageToLog">====DDDD===== Test Log </param>
	        <param desc="sleep duration for test logging agent">0</param>
	      </agent>

	    </scheduling>
	  </sitecore>
	</configuration>

### 2. Agent processor example ###

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

# <a name="Concerns"></a> 5. Concerns #
* This extension has not been load/stress tested, nor has it been exercised in production environment.  

* In theory, the extension can perform better because the sleep duration is no longer fixed; rather, the scheduler thread sleeps until the next agent execution is needed.  The sleep duration within configuration setting `scheduling/frequency` is still utilized if the next execution duration is less than configured frequency.  

* There is additional processing overhead to be aware of:

	* Because this extension is extensible via pipelines a degree of overhead is incurred.  As experience has shown, the value of extensible implementation of Sitecore outweighs nominal overhead of pipelines.

	* To accommodate variable size sleep duration, a [heap](https://en.wikipedia.org/wiki/Heap_(data_structure)) is utilized.  After agents are executed, they are removed and re-added to the heap data structure to preserve the updated execution order.  The heap takes Log(n) to add or remove an item to the queue; therefore, maximum of `2 n Log(n)` iterations is needed if **all** agents are being rescheduled.
	
	*  After processing scheduled agent, all agent are iterated through in order to flush and preserve last execution time into repository in the event the worker process is recycled.
	
	Sitecore 8.1 has approximately 30 agents out-of-box; therefore, the equation `2n Log(n)` shows that there will be 300 additional cycles in comparision to the original scheduler implementation.  Unless `scheduling/frequency` is consitantly greater than sleep duration  between scheduled agents, the extra processing time is gained back when the scheduler thread sleeps longer.
	



