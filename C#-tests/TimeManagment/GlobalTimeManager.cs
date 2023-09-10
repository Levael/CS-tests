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
        private readonly int    _gtmTickStepMs;             // Almost main parameter: step of ticker in milliseconds. Minimum for Moog -- 1
        private long            _gtmTicksPassed;            // Counter for custom ticks passed from the "StartGlobalTicker". (gtm = Global Time Manager (because there is another tick -- from stopwatch))
        private bool            _doRunTicker;               // Boolean flag to start/stop "Global Ticker"
        private readonly double _stopWatchFrequencyPerMs;   // By default it's per sec.
        private readonly int    _spinWait;                  // Hard to explain. Long story short -- the only way I found for max accuracy for "Sleep" function (number hand-picked)
        private Stopwatch       _stopWatch;                 // C# built-in ticker. Starts from 0 and ticks "StopWatch.Frequency" times per second. Best for measuring time

        // Main objects
        private Queue<List<Action>>                                         _executionQueue;        // Queue for "event-dependent" functions (like Moog movement or Oculus render)
        private List<(Action action, int intervalMs)>                       _executeInCycleList;    // List for functions that should be called every X ms. (like devices connections checkers)
        private Thread                                                      _globalTicker;          // The thread that responsible for the entire Global Ticker. Runs with high priority
        private List<(long tickOrdinalNumber, string invokedMethodName)>    _timeStamps;            // List of tuples for most important time stamps: tick number + called function name

        // Lockers (otherwise different threads may write to shared objects at the same time and thereby cause collisions)
        private readonly object _lockForTimeStamps = new object();
        private readonly object _lockForExecutionQueue = new object();
        private readonly object _lockForExecuteInCycleList = new object();

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
            _gtmTicksPassed             = 0;
            _stopWatchFrequencyPerMs    = (double)(Stopwatch.Frequency / 1_000.0);
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
                BusyWaitUntilNextTick();    // wait till next tick (<= "tickStep" ms)
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
            //RecordTimeStamp();

            ExecuteCycleFunctions();
            ExecuteQueueFunctions();
        }

        private void BusyWaitUntilNextTick()
        {
            double stopWatchTicksElapsed;
            double stopWatchTicksShouldElapse = _gtmTicksPassed * _stopWatchFrequencyPerMs * _gtmTickStepMs;

            while (true)
            {
                stopWatchTicksElapsed = _stopWatch.ElapsedTicks;

                if (stopWatchTicksElapsed >= stopWatchTicksShouldElapse) break;

                Thread.SpinWait(_spinWait);
            }
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

        public void AddToCycleExecutionList(Action action, int intervalMs)
        {
            lock (_lockForExecuteInCycleList)
            {
                _executeInCycleList.Add((action, intervalMs));
            }
        }

        public async Task AddToExecutionQueue(List<Action> actionsList, int delayMs = 0)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(delayMs));

            lock (_lockForExecutionQueue)
            {
                _executionQueue.Enqueue(actionsList);
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

        private void ExecuteQueueFunctions()
        {
            foreach (var actionsList in _executionQueue)
            {
                foreach (var action in actionsList)
                {
                    RecordTimeStamp(action);
                    action();
                }
            }
        }

        private void RecordTimeStamp(Action action)
        {
            lock (_lockForTimeStamps)
            {
                //var calledFrom = (action != null) ? nameof(action) : "incognito";
                _timeStamps.Add((_stopWatch.ElapsedTicks, nameof(action)));
            }
        }

        private void RecordTimeStamp(string text = "incognito")
        {
            lock (_lockForTimeStamps)
            {
                _timeStamps.Add((_stopWatch.ElapsedTicks, text));
            }
        }


        // ========================================= TEMP ==================================================================

        public List<double> GetAllDelays()
        {
            var delays = new List<double>();

            for (int i = 1; i < _timeStamps.Count; i++)
            {
                var actualMsPassed = CalculateDelayInMs(_timeStamps[i - 1].tickOrdinalNumber, _timeStamps[i].tickOrdinalNumber, _stopWatchFrequencyPerMs);
                delays.Add(actualMsPassed);

                if ((actualMsPassed - _gtmTickStepMs) > _gtmTickStepMs * (5 / 100.0))
                {
                    Console.WriteLine(actualMsPassed + " - " + i);
                }
            }

            return delays;
        }

        private double CalculateDelayInMs(long previousTimeStamp, long lastTimeStamp, double frequencyMs)
        {
            return (lastTimeStamp - previousTimeStamp) / frequencyMs;
        }

       /*public void AnalyzeAndPrintData()
        {
            var _delaysBetweenTicksMs = GetAllDelays();

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


        #endregion PRIVATE METHODS






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

