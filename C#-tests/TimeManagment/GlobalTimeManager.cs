using System.Collections.Concurrent;
using System.Diagnostics;
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
        private ConcurrentQueue<(long stopwatchTickOrdinalNumber, string invokedMethodName)>     _timeStamps;   // Thread-safe structure of tuples for most important time stamps: tick number + called function name

        // Different stuff
        private Dictionary<string, string> _gtmKeyWords;

        #endregion PRIVATE FIELDS


        #region CONSTRUCTOR / DESTRUCTOR

        /// <summary>
        /// Constructor. Sets "windows time resolution" to minimum value.
        /// Also calls for "WarmUp" function to precompile and preload everything inside it into cache (for faster execution in the future)
        /// </summary>
        public GlobalTimeManager()
        {
            _gtmTickStepMs                  = 1;
            _gtmMinimumTickStepMs           = 0.5;
            _gtmTicksPassed                 = 0;
            _tickPermissibleErrorPercent    = 10;
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
        public void AddFunctionsForSingleTickToExecutionQueue(List<Action> actionsList)
        {
            _executionQueue.Enqueue(actionsList);
        }

        /// <summary>
        /// Async version of method. Waits "delayMs" and only after that adds new item to the queue
        /// </summary>
        public async Task AddFunctionsForSingleTickToExecutionQueue(List<Action> actionsList, int delayMs)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(delayMs));

            _executionQueue.Enqueue(actionsList);
        }

        /// <summary>
        /// Adds a bunch of 
        /// </summary>
        public void AddFunctionsForRangeTicksToExecutionQueue(List<Action>[] actionsLists)
        {
            foreach (var actionsList in actionsLists)
            {
                _executionQueue.Enqueue(actionsList);
            }
        }

        /// <summary>
        /// Returns sorted array of tuples with all timeStamps (tick, funcName)
        /// </summary>
        public (long stopwatchTickOrdinalNumber, string invokedMethodName)[] GetTimeLine()
        {
            return  _timeStamps
                    .ToArray()
                    .OrderBy(entry => entry.stopwatchTickOrdinalNumber)
                    .ToArray();

            /*
                The "OrderBy" method in LINQ returns an object of type "IOrderedEnumerable<T>", which is a lazily-evaluated sequence.
                This means that the sorting will actually not take place until you start iterating through the elements of this sequence.
                When you call "ToArray()" on an "IOrderedEnumerable<T>", the sorting is actually performed, and the results are stored in a new array.

                So, the first call to "ToArray()" converts the "ConcurrentQueue<(long Ticks, string Description)>" to a regular array so that it can be sorted using "OrderBy".
                The second call to "ToArray()" is needed to convert the sorted IOrderedEnumerable<T> sequence back into an array, which is then returned by the GetTimeLine method.
            */
        }

        /// <summary>
        /// Big function but it's temporary and for development purposes only
        /// </summary>
        public void Debug(bool doWriteToConsole = false)
        {
            var delays = new List<double>();
            var timeStamps = _timeStamps.ToArray();
            var totalTimeByStopWatchMs = _stopWatch.ElapsedMilliseconds;

            /*foreach (var timeStamp in timeStamps)
            {
                Console.WriteLine(timeStamp);
            }*/

            Console.WriteLine($"Items in timeStamps: {timeStamps.Length}");
            Console.WriteLine($"Items in executionQueue: {_executionQueue.Count()}");


            var projectRootPath = @"C:\Users\Levael\GitHub\C#-tests\C#-tests\";
            var relativePath = @"Tests\Debug_log.txt";
            var fullPath = Path.Combine(projectRootPath, relativePath);

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
                var permissibleErrorDelta = (_gtmTickStepMs * (_tickPermissibleErrorPercent / 100.0));

                // start from 1 because delay is calculated between 2 timestamps (current - previous)
                for (int i = 1; i < timeStamps.Length; i++)
                {
                    var actualMsPassed = (timeStamps[i].stopwatchTickOrdinalNumber - timeStamps[i - 1].stopwatchTickOrdinalNumber) / _stopWatchFrequencyPerMs;
                    //delays.Add(actualMsPassed);

                    var unacceptablyFast = (actualMsPassed < _gtmMinimumTickStepMs);
                    var fasterThanDesired = (actualMsPassed >= _gtmMinimumTickStepMs) && (actualMsPassed < (_gtmTickStepMs - permissibleErrorDelta));
                    var desiredResult = (actualMsPassed >= (_gtmTickStepMs - permissibleErrorDelta)) && (actualMsPassed <= (_gtmTickStepMs + permissibleErrorDelta));
                    var slowerThanDesired = (actualMsPassed > (_gtmTickStepMs + permissibleErrorDelta));


                    if (!desiredResult)
                    {
                        if (unacceptablyFast || slowerThanDesired)
                        {
                            // Write divergent tick to file
                            streamWriter.Write($"{i},{actualMsPassed};");
                        }

                        if (doWriteToConsole)
                        {
                            // Choose suitable color for console message
                            if (unacceptablyFast) Console.ForegroundColor = ConsoleColor.Red;
                            if (slowerThanDesired) Console.ForegroundColor = ConsoleColor.Magenta;
                            if (desiredResult) Console.ForegroundColor = ConsoleColor.Green;   // meaningless in this context, but let it be
                            if (fasterThanDesired) Console.ForegroundColor = ConsoleColor.Yellow;

                            // Print divergent tick (only 4 decimal places)
                            Console.WriteLine($"{actualMsPassed:F4} - {i}");
                            Console.ResetColor();
                        }
                    }
                }
            }
        }

        /*public void Debug(bool doWriteToConsole = false, bool doWriteToFile = false)
        {
            var timeLine = GetTimeLine();
            var programLifeTimeMs = timeLine[timeLine.Length - 1].stopwatchTickOrdinalNumber / _stopWatchFrequencyPerMs;

            Console.WriteLine($"programLifeTime: {programLifeTimeMs} ms");


            *//*var projectRootPath = @"C:\Users\Levael\GitHub\C#-tests\C#-tests\";
            var relativePath = @"Tests\Debug_log.txt";
            var fullPath = Path.Combine(projectRootPath, relativePath);

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
                var permissibleErrorDelta = (_gtmTickStepMs * (_tickPermissibleErrorPercent / 100.0));

                // start from 1 because delay is calculated between 2 timestamps (current - previous)
                for (int i = 1; i < timeStamps.Length; i++)
                {
                    var actualMsPassed = (timeStamps[i].tickOrdinalNumber - timeStamps[i - 1].tickOrdinalNumber) / _stopWatchFrequencyPerMs;
                    //delays.Add(actualMsPassed);

                    var unacceptablyFast = (actualMsPassed < _gtmMinimumTickStepMs);
                    var fasterThanDesired = (actualMsPassed >= _gtmMinimumTickStepMs) && (actualMsPassed < (_gtmTickStepMs - permissibleErrorDelta));
                    var desiredResult = (actualMsPassed >= (_gtmTickStepMs - permissibleErrorDelta)) && (actualMsPassed <= (_gtmTickStepMs + permissibleErrorDelta));
                    var slowerThanDesired = (actualMsPassed > (_gtmTickStepMs + permissibleErrorDelta));


                    if (!desiredResult)
                    {
                        if (unacceptablyFast || slowerThanDesired)
                        {
                            // Write divergent tick to file
                            streamWriter.Write($"{i},{actualMsPassed};");
                        }

                        if (doWriteToConsole)
                        {
                            // Choose suitable color for console message
                            if (unacceptablyFast) Console.ForegroundColor = ConsoleColor.Red;
                            if (slowerThanDesired) Console.ForegroundColor = ConsoleColor.Magenta;
                            if (desiredResult) Console.ForegroundColor = ConsoleColor.Green;   // meaningless in this context, but let it be
                            if (fasterThanDesired) Console.ForegroundColor = ConsoleColor.Yellow;

                            // Print divergent tick (only 4 decimal places)
                            Console.WriteLine($"{actualMsPassed:F4} - {i}");
                            Console.ResetColor();
                        }
                    }
                }*//*
        }*/

        #endregion PUBLIC METHODS


        #region PRIVATE METHODS

        private void RunTicker()
        {
            // the opposite flag value will be set from outside by calling "StopTicker()" and will actually stop the ticker only on the next tick
            _doRunTicker = true;

            _stopWatch.Start();
            RecordTimeStamp(_gtmKeyWords["gtmStarted"]);

            // 99.99% of all this thread time will be spent in this loop
            while (_doRunTicker)
            {
                ExecuteEveryTick();
                BusyWaitUntilNextTick();
            }

            RecordTimeStamp(_gtmKeyWords["gtmStoped"]);
            RecordTimeStamp(_gtmKeyWords["gtmStoped"]); // Temp. Just for test (2 times to see on the chart where is the very end)
            _stopWatch.Stop();
        }

        private void StopTicker()
        {
            _doRunTicker = false;   // will stop on the next tick
        }

        /// <summary>
        /// The main function of whole class
        /// </summary>
        private void ExecuteEveryTick()
        {
            _gtmTicksPassed++;              // should be incremented BEFORE "BusyWaitUntilNextTick"
            RecordTimeStamp();              // temporary, for debug only. Delete later

            ExecuteCycleFunctions();        // Checks if there is anything in "CycleQueue" to be executed       (functions to be called every X ms)
            ExecuteNextFunctionInQueue();   // Checks if there is anything in "ExecutionQueue" to be executed   (functions to be called right after it was added)
            Console.WriteLine("-----");
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
            // check at tick 0

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

            // "dequeuedSuccessfully" will be "false" if queue is empty, so just exit from function
            if (!dequeuedSuccessfully) return;

            foreach (var action in actionsList)
            {
                //RecordTimeStamp(action);  // temp. release mode -- should be uncommented
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


        #endregion PRIVATE METHODS


        #region LEGACY COMMENTS

        /*/// <summary>
        /// Actually just runs "StartTrialTimeManager" function, but with only 2 repetitions (to make sure every function was called)
        /// and reduce time delay when running the function for the very first time
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

