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
        private readonly double _gtmTickStepMs;             // Almost main parameter: step of ticker in milliseconds. Minimum for Moog -- 1
        private readonly double _gtmMinimumTickStepMs;      // If there is a delay larger than "gtmTickStepMs", than instead of "Sleep(0)" it will sleep minimal value (to prevent instant moves)
        private long            _gtmTicksPassed;            // Counter for custom ticks passed from the "StartGlobalTicker". (gtm = Global Time Manager (because there is another tick -- from stopwatch))
        private bool            _doRunTicker;               // Boolean flag to start/stop "Global Ticker"
        private readonly double _stopWatchFrequencyPerMs;   // By default it's per sec.
        private readonly int    _spinWait;                  // Hard to explain. Long story short -- the only way I found for max accuracy for "Sleep" function (number hand-picked)
        private Stopwatch       _stopWatch;                 // C# built-in ticker. Starts from 0 and ticks "StopWatch.Frequency" times per second. Best for measuring time

        // Main objects
        private ConcurrentQueue<List<Action>>                                           _executionQueue;        // Queue for "event-dependent" functions (like Moog movement or Oculus render)
        private List<(Action action, int intervalMs)>                                   _executeInCycleList;    // List for functions that should be called every X ms. (like devices connections checkers)
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
            _gtmTickStepMs              = 1;
            _gtmMinimumTickStepMs       = 0.5;
            _gtmTicksPassed             = 0;
            _stopWatchFrequencyPerMs    = (double)(Stopwatch.Frequency / 1_000.0);  // /1000 to translate sec to ms
            _spinWait                   = (int)(_gtmTickStepMs * 500);  // 500 just gives better accuracy than 1000 (for example). Lesser number -> better accuracy -> more CPU usage

            _executeInCycleList         = new();
            _executionQueue             = new();
            _stopWatch                  = new();
            _timeStamps                 = new();

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
            var success = _executionQueue.TryDequeue(out List<Action> actionsList);

            // "success" will be "false" if queue is empty, so just exit from function
            if (!success) return;

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
        private void RecordTimeStamp(string text = "incognito")
        {
            _timeStamps.Enqueue((_stopWatch.ElapsedTicks, text));
        }

        /// <summary>
        /// Atually just runs "StartTrialTimeManager" function, but with only 2 repetitions (to make sure every function was called)
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
        }


        // ========================================= TEMP ==================================================================

        public List<double> Debug()
        {
            var delays = new List<double>();
            var array = _timeStamps.ToArray();

            using (StreamWriter sw = new StreamWriter(@"C:\Users\Levael\GitHub\C#-tests\C#-tests\Tests\temp_debug.txt", true))
            {
                var _totalTimeByStopWatchMs = _stopWatch.ElapsedMilliseconds;
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("Total time: " + _totalTimeByStopWatchMs + '\n');

                for (int i = 1; i < array.Length; i++)
                {
                    var actualMsPassed = (array[i].tickOrdinalNumber - array[i - 1].tickOrdinalNumber) / _stopWatchFrequencyPerMs;

                    if (actualMsPassed < 0.5) Console.ForegroundColor = ConsoleColor.Red;
                    if (actualMsPassed >= 0.5 && actualMsPassed <= 0.8) Console.ForegroundColor = ConsoleColor.Yellow;
                    if (actualMsPassed >= 0.8 && actualMsPassed <= 1.2) Console.ForegroundColor = ConsoleColor.Green;
                    if (actualMsPassed >= 1.2) Console.ForegroundColor = ConsoleColor.Magenta;

                    if (Console.ForegroundColor == ConsoleColor.Green) continue;
                    Console.WriteLine(actualMsPassed + " - " + i);

                    Console.ResetColor();

                    //delays.Add(actualMsPassed);

                    // Check for error bigger than 7%
                    if (((actualMsPassed - _gtmTickStepMs) > _gtmTickStepMs * (7 / 100.0)) || (actualMsPassed < _gtmMinimumTickStepMs))
                    {
                        sw.Write($"{i},{actualMsPassed};");
                        //Console.WriteLine(actualMsPassed + " - " + i);
                    }

                }
            }

            return delays;
        }

        #endregion PRIVATE METHODS



        // ====================================================================


        /*public void StartTrialTimeManager()
        {
            var arrayOfListsOfActions1 = new List<Action>[1000];
            var arrayOfListsOfActions2 = new List<Action>[1500];

            var singleSegmentTimeManager = new SingleSegmentTimeManager(arrayOfListsOfActions1);
            var singleSegmentTimeManager2 = new SingleSegmentTimeManager(arrayOfListsOfActions2);

            singleSegmentTimeManager.StartTheSegment();
            Thread.Sleep(1000);
            singleSegmentTimeManager2.StartTheSegment();



            singleSegmentTimeManager.AnalyzeTrialTimeData();
            singleSegmentTimeManager.PrintToConsoleAnalyzedTrialTimeData();
            //singleSegmentTimeManager.ExportDataToTxtFile();

            singleSegmentTimeManager2.AnalyzeTrialTimeData();
            singleSegmentTimeManager2.PrintToConsoleAnalyzedTrialTimeData();
            //singleSegmentTimeManager2.ExportDataToTxtFile();
        }*/

        /*public void AnalyzeAndPrintData()
         {
             var _delaysBetweenTicksMs = Debug();

             var _divergentDelaysCounter = CalculateNumberOfDivergentDelays(_delaysBetweenTicksMs);

             var _totalTimeBySumOfDelaysMs = CalculateTotalTimePassedMs(_delaysBetweenTicksMs);
             //var _totalTimeByDateTimeNowMs = (_trialStopTime - _trialStartTime).TotalMilliseconds;
             var _totalTimeByStopWatchMs = _stopWatch.ElapsedMilliseconds;
             var _ticksNumber = _timeStamps.Count;

             Console.WriteLine("===============================================================");
             //Console.WriteLine($"Total time By TimeNow:\t\t {_totalTimeByDateTimeNowMs} / {_ticksNumber * _gtmTickStepMs}");
             Console.WriteLine($"Total time by StopWatch:\t {_totalTimeByStopWatchMs} / {_ticksNumber * _gtmTickStepMs}");
             Console.WriteLine($"Total time by SumOfDelays:\t {_totalTimeBySumOfDelaysMs} / {_ticksNumber * _gtmTickStepMs - 1}");
             Console.WriteLine($"Number of divergent delays:\t {_divergentDelaysCounter} / {_ticksNumber * _gtmTickStepMs - 1}");
             Console.WriteLine("===============================================================");
         }

         private double CalculateTotalTimePassedMs(List<double> delays)
         {
             double totalTimePassedMs = 0;

             foreach (var delay in delays)
             {
                 totalTimePassedMs += delay;
             }

             return totalTimePassedMs;
         }

         private int CalculateNumberOfDivergentDelays(List<double> delays)
         {
             int numberOfDivergentDelays = 0;
             int numberOfCheckedDelays = 0;

             foreach (var delay in delays)
             {
                 numberOfCheckedDelays++;
                 if (Math.Abs(delay - _gtmTickStepMs) > _gtmTickStepMs * (5 / 100.0))
                 {
                     numberOfDivergentDelays++;
                     Console.WriteLine(delay + " - " + numberOfCheckedDelays);
                 }
             }

             return numberOfDivergentDelays;
         }*/


        //WarmUpMethods(typeof(SingleSegmentTimeManager));
        //[MethodImpl(MethodImplOptions.NoOptimization)]

        // doesn't work, suka
        /*private void WarmUpMethods(Type type)
        {
            var methods = type.GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);

            foreach (var method in methods)
            {
                var handle = method.MethodHandle;
                RuntimeHelpers.PrepareMethod(handle);
                Console.WriteLine($"Method {method.Name} has been prepared.");
            }
        }*/
    }
}

