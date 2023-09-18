using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using CustomOptimization;

namespace GlobalTimeManagment
{
    public class GlobalTimeManager
    {
        #region PUBLIC FIELDS
        // there is no public fields
        #endregion PUBLIC FIELDS


        #region PRIVATE FIELDS

        // Time related parameters
        private readonly double _gtmTickStepMs;                 // Almost main parameter: step of ticker in milliseconds. Minimum for Moog -- 1
        private readonly double _gtmMinimumTickStepMs;          // If there is a delay larger than "gtmTickStepMs", than instead of "Sleep(0)" it will sleep minimal value (to prevent instant moves)
        private long            _gtmTicksPassed;                // Counter for custom ticks passed from the "StartGlobalTicker". (gtm = Global Time Manager (because there is another tick -- from stopwatch))
        private bool            _doRunTicker;                   // Boolean flag to start/stop "Global Ticker"
        private readonly double _stopWatchFrequencyPerMs;       // By default it's per sec.
        private readonly int    _spinWait;                      // Hard to explain. Long story short -- the only way I found for max accuracy for "Sleep" function (number hand-picked)
        private Stopwatch       _stopWatch;                     // C# built-in ticker. Starts from 0 and ticks "StopWatch.Frequency" times per second. Best for measuring time
        private readonly double _tickPermissibleErrorPercent;   // Parameter that in analytics counts the number of ticks that took longer by the specified percentage

        // Main objects
        private ConcurrentQueue<List<Action>>                                           _executionQueue;        // Queue for "event-dependent" functions (like Moog movement or Oculus render)
        private List<(Action action, int intervalMs)>                                   _executeInCycleList;    // List of tuples for functions that should be called every X ms. (like devices connections checkers)
        private Thread                                                                  _globalTicker;          // The thread that responsible for the entire Global Ticker. Runs with high priority
        private ConcurrentQueue<(long tickOrdinalNumber, string invokedMethodName)>     _timeStamps;            // Thread-safe structure of tuples for most important time stamps: tick number + called function name

        // Different stuff
        private Dictionary<string, string> _gtmKeyWords;

        #endregion PRIVATE FIELDS


        #region CONSTRUCTOR / DESTRUCTOR

        /// <summary>
        /// Constructor. Sets "windows time resolution" to minimum value.
        /// Also calls for "WarmUp" function to precompile and preload everything inside it into cash (for faster execution in the future)
        /// </summary>
        public GlobalTimeManager()
        {
            _gtmTickStepMs                  = 1;
            _gtmMinimumTickStepMs           = 0.5;
            _gtmTicksPassed                 = 0;
            _tickPermissibleErrorPercent    = 7;
            _stopWatchFrequencyPerMs        = (double)(Stopwatch.Frequency / 1_000.0);  // /1000 to translate sec to ms
            _spinWait                       = (int)(_gtmTickStepMs * 500);  // 500 just gives better accuracy than 1000 (for example). Lesser number -> better accuracy -> more CPU usage

            _executeInCycleList             = new();
            _executionQueue                 = new();
            _stopWatch                      = new();
            _timeStamps                     = new();

            _gtmKeyWords = new() {
                {"gtmStarted", "Start_of_global_ticker"},
                {"gtmStoped", "End_of_global_ticker"}
            };

            Optimization.TimeBeginPeriod(1);
            //WarmUp();
            StartGlobalTicker();
        }

        /// <summary>
        /// Destructor. Resets "windows time resolution" to its previous value
        /// </summary>
        ~GlobalTimeManager()
        {
            Optimization.TimeEndPeriod(1);
        }

        #endregion CONSTRUCTOR/ DESTRUCTOR


        #region PUBLIC METHODS

        public void StartGlobalTicker()
        {
            _gtmTicksPassed = 0;

            _globalTicker = new Thread(RunTicker);
            _globalTicker.Priority = ThreadPriority.AboveNormal;
            _globalTicker.Start();
        }

        public void StopGlobalTicker()
        {
            StopTicker();
            _globalTicker.Join();
        }

        public void AddToCycleExecutionList(Action action, int intervalMs)
        {
            _executeInCycleList.Add((action, intervalMs));
        }

        /// <summary>
        /// Sync version of method
        /// </summary>
        public void AddToExecutionQueue(List<Action> actionsList)
        {
            _executionQueue.Enqueue(actionsList);
        }

        /// <summary>
        /// Async version of method. Waits "delayMs" and only after that adds new item to the queue
        /// </summary>
        public async Task AddToExecutionQueue(List<Action> actionsList, int delayMs)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(delayMs));

            _executionQueue.Enqueue(actionsList);
        }

        #endregion PUBLIC METHODS


        #region PRIVATE METHODS

        private void RunTicker()
        {
            _doRunTicker = true;

            _stopWatch.Start();
            RecordTimeStamp(_gtmKeyWords["gtmStarted"]);

            while (_doRunTicker)
            {
                ExecuteEveryTick();
                BusyWaitUntilNextTick();
            }

            RecordTimeStamp(_gtmKeyWords["gtmStoped"]);
            _stopWatch.Stop();
        }

        private void StopTicker()
        {
            _doRunTicker = false;   // will stop on the next tick
        }

        private void ExecuteEveryTick()
        {
            _gtmTicksPassed++;             // should be incremented BEFORE "BusyWaitUntilNextTick"
            RecordTimeStamp();

            ExecuteCycleFunctions();
            ExecuteNextFunctionInQueue();
        }

        /// <summary>
        /// The time when next tick should happed is predeterminated and happens every "tickStep" ms.
        /// But if there is a lag and it took too many time -- next tick(s) won't be 0ms, but "gtmMinimumTickStep" ms.
        /// This is done for safety in order to avoid sudden jerks.
        /// (especially critical for the robot, although there will be additional checks)
        /// </summary>
        private void BusyWaitUntilNextTick()
        {
            double stopWatchTicksElapsed = _stopWatch.ElapsedTicks;
            double stopWatchTicksShouldElapse = _gtmTicksPassed * _stopWatchFrequencyPerMs * _gtmTickStepMs;
            double stopWatchMinimumTicksShouldElapse = stopWatchTicksElapsed + (_stopWatchFrequencyPerMs * _gtmMinimumTickStepMs);

            bool readyForNextTick = false;
            bool passedMinamalTicksNumber = false;

            while (true)
            {
                stopWatchTicksElapsed = _stopWatch.ElapsedTicks;
                readyForNextTick = stopWatchTicksElapsed >= stopWatchTicksShouldElapse;
                passedMinamalTicksNumber = stopWatchTicksElapsed >= stopWatchMinimumTicksShouldElapse;

                if (readyForNextTick && passedMinamalTicksNumber) break;

                Thread.SpinWait(_spinWait);
            }
        }

        private void ExecuteCycleFunctions()
        {
            foreach(var pare in _executeInCycleList)
            {
                if (_gtmTicksPassed % pare.intervalMs == 0)    // e.g. if its time has come
                {
                    //RecordTimeStamp();
                    pare.action();
                }
            }
        }

        private void ExecuteNextFunctionInQueue()
        {
            var dequeuedSuccessfully = _executionQueue.TryDequeue(out List<Action> actionsList);

            // "success" will be "false" if queue is empty, so just exit from function
            if (!dequeuedSuccessfully) return;

            foreach (var action in actionsList)
            {
                RecordTimeStamp(action);
                action();
            }
        }

        /// <summary>
        /// For record what function was called
        /// </summary>
        private void RecordTimeStamp(Action action)
        {
            _timeStamps.Enqueue((_stopWatch.ElapsedTicks, nameof(action)));
        }

        /// <summary>
        /// For record of custom text
        /// </summary>
        private void RecordTimeStamp(string text = "noname")
        {
            _timeStamps.Enqueue((_stopWatch.ElapsedTicks, text));
        }

        public void Debug(bool doWriteToConsole = false)
        {
            var delays                  = new List<double>();
            var timeStamps              = _timeStamps.ToArray();
            var totalTimeByStopWatchMs  = _stopWatch.ElapsedMilliseconds;

            var projectRootPath         = @"C:\Users\Levael\GitHub\C#-tests\C#-tests\";
            var relativePath            = @"Tests\Debug_log.txt";
            var fullPath                = Path.Combine(projectRootPath, relativePath);

            if (!File.Exists(fullPath))
            {
                Console.WriteLine("File is not found");
                return;
            }

            if (doWriteToConsole)
            {
                // Prints to console total passed time (by stopwatch)
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("Total time: " + totalTimeByStopWatchMs + '\n');
                Console.ResetColor();
            }

            // Opening stream to output file
            using (var streamWriter = new StreamWriter(fullPath, true))
            {
                // start from 1 because delay is calculated between 2 timestamps (current - previous)
                for (int i = 1; i < timeStamps.Length; i++)
                {
                    var actualMsPassed = (timeStamps[i].tickOrdinalNumber - timeStamps[i - 1].tickOrdinalNumber) / _stopWatchFrequencyPerMs;
                    //delays.Add(actualMsPassed);

                    var permissibleDelta    = (_gtmTickStepMs * (_tickPermissibleErrorPercent / 100.0));
                    var unacceptablyFast    = (actualMsPassed < _gtmMinimumTickStepMs);
                    var fasterThanDesired   = (actualMsPassed >= _gtmMinimumTickStepMs) && (actualMsPassed < (_gtmTickStepMs - permissibleDelta));
                    var desiredResult       = (actualMsPassed >= (_gtmTickStepMs - permissibleDelta)) && (actualMsPassed <= (_gtmTickStepMs + permissibleDelta));
                    var slowerThanDesired   = (actualMsPassed > (_gtmTickStepMs + permissibleDelta));
                    
                    if (!desiredResult)
                    {
                        // Write divergent tick to file
                        streamWriter.Write($"{i},{actualMsPassed};");


                        if (doWriteToConsole)
                        {
                            // Choose suitable color for console message
                            if (unacceptablyFast)   Console.ForegroundColor = ConsoleColor.Red;
                            if (slowerThanDesired)  Console.ForegroundColor = ConsoleColor.Yellow;
                            if (desiredResult)      Console.ForegroundColor = ConsoleColor.Green;   // meaningless in this context, but let it be
                            if (fasterThanDesired)  Console.ForegroundColor = ConsoleColor.Magenta;

                            // Print divergent tick (only 4 decimal places)
                            Console.WriteLine($"{actualMsPassed:F4} - {i}");
                            Console.ResetColor();
                        }
                    }
                }
            }
        }

        #endregion PRIVATE METHODS

        #region LEGACY COMMENTS

        /*/// <summary>
        /// Actually just runs "StartTrialTimeManager" function, but with only 2 repetitions (to make sure every function was called)
        /// and reduce time delay when running the fucntion for the very first time
        /// </summary>
        private void WarmUp()
        {
            Console.ForegroundColor = ConsoleColor.Red;

            /// warmup for "SingleSegmentTimeManager" class
            var warmUpSegmentTimeManager = new SingleSegmentTimeManager();
            warmUpSegmentTimeManager.StartTheSegment();
            //warmUpSegmentTimeManager.AnalyzeTrialTimeData();
            //warmUpSegmentTimeManager.PrintToConsoleAnalyzedTrialTimeData();

            Console.ResetColor();
        }*/

        #endregion LEGACY COMMENTS
    }
}

