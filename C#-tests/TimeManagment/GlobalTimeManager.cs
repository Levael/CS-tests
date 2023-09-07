using System.Diagnostics;
using System.Threading;
using CustomOptimization;

namespace GlobalTimeManagment
{
    public class GlobalTimeManager
    {
        #region PUBLIC FIELDS
        #endregion PUBLIC FIELDS


        #region PRIVATE FIELDS

        private readonly int _tickStepMs;
        private long _ticksPassed;
        private bool _doRunTicker;
        private readonly double _stopWatchFrequencyPerMs;
        private readonly int _spinWait;
        private Stopwatch _stopWatch;
        private Queue<List<Action>> _executionQueue;
        private List<(Action action, int intervalMs)> _executeInCycleList;
        private Thread _constantTicker;
        private List<long> _timeStamps;

        #endregion PRIVATE FIELDS


        #region CONSTRUCTOR/ DESTRUCTOR

        /// <summary>
        /// Constructor. Sets "windows time resolution" to minimum value.
        /// Also calls for "WarmUp" function to compile and load everything inside it into cash (for faster execution in the future)
        /// </summary>
        public GlobalTimeManager()
        {
            _tickStepMs = 1;
            _ticksPassed = 0;
            _stopWatchFrequencyPerMs = Stopwatch.Frequency / 1000.0;
            _spinWait = (int)(_tickStepMs * 1_000);
            _executeInCycleList = new();
            _executionQueue = new();
            _stopWatch = new();
            _timeStamps = new();           // Definite size (as arrayOfActions input) for every tick (tick == input function call)

            Optimization.TimeBeginPeriod(1);
            //WarmUp();
            //RunTicker();
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
            _ticksPassed = 0;

            _constantTicker = new Thread(RunTicker);
            _constantTicker.Priority = ThreadPriority.Highest;
            _constantTicker.Start();
        }

        public void StopGlobalTicker()
        {
            StopTicker();
            _constantTicker.Join();
        }

        #endregion PUBLIC METHODS


        #region PRIVATE METHODS

        private void RunTicker()
        {
            Console.WriteLine("Ticker started");

            _doRunTicker = true;
            _stopWatch.Start();

            while (_doRunTicker)
            {
                ExecuteEveryTick();
                BusyWaitUntilNextTick();    // wait till next tick (<= "tickStep" ms)
            }

            _stopWatch.Stop();

            Console.WriteLine("Ticker stoped");
            //AnalyzeAndPrintData();
        }

        private void StopTicker()
        {
            _doRunTicker = false;   // will stop on the next tick
        }

        private void ExecuteEveryTick()
        {
            _ticksPassed++;             // should be incremented BEFORE "BusyWaitUntilNextTick"

            ExecuteCycleFunctions();
            ExecuteQueueFunctions();
        }

        private void BusyWaitUntilNextTick()
        {
            double timeElapsedMs;
            double timeShouldElapseMs;

            while (true)
            {
                timeElapsedMs = _stopWatch.ElapsedTicks / _stopWatchFrequencyPerMs;
                timeShouldElapseMs = _ticksPassed * _tickStepMs;

                if (timeElapsedMs >= timeShouldElapseMs) break;

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
            _executeInCycleList.Add((action, intervalMs));
        }

        public async Task AddToExecutionQueue(List<Action> actionsList, int delayMs = 0)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(delayMs)); 
            _executionQueue.Enqueue(actionsList);
        }

        private void ExecuteCycleFunctions()
        {
            foreach(var pare in _executeInCycleList)
            {
                if (_ticksPassed % pare.intervalMs == 0)    // e.g. if its time has come
                {
                    RecordTimeStamp();
                    pare.action();
                }
            }
        }
        private void ExecuteQueueFunctions()
        {
            foreach (var list in _executionQueue)
            {
                foreach (var action in list)
                {
                    RecordTimeStamp();
                    action();
                }
            }
        }

        private void RecordTimeStamp()
        {
            _timeStamps.Add(_stopWatch.ElapsedTicks);
        }


        // ========================================= TEMP ==================================================================

        /*private List<double> GetAllDelays()
        {
            var delays = new List<double>();

            for (int i = 1; i < _timeStamps.Count; i++)
            {
                var actualMsPassed = CalculateDelayInMs(_timeStamps[i - 1], _timeStamps[i], _stopWatchFrequencyPerMs);
                delays.Add(actualMsPassed);

                //Console.WriteLine(actualMsPassed);
            }

            return delays;
        }

        private double CalculateDelayInMs(long previousTimeStamp, long lastTimeStamp, double frequencyMs)
        {
            return (lastTimeStamp - previousTimeStamp) / frequencyMs;
        }

        public void AnalyzeAndPrintData()
        {
            var _delaysBetweenTicksMs = GetAllDelays();

            var _divergentDelaysCounter = CalculateNumberOfDivergentDelays(_delaysBetweenTicksMs);

            var _totalTimeBySumOfDelaysMs = CalculateTotalTimePassedMs(_delaysBetweenTicksMs);
            //var _totalTimeByDateTimeNowMs = (_trialStopTime - _trialStartTime).TotalMilliseconds;
            var _totalTimeByStopWatchMs = _stopWatch.ElapsedMilliseconds;
            var _ticksNumber = _timeStamps.Count;

            Console.WriteLine("===============================================================");
            //Console.WriteLine($"Total time By TimeNow:\t\t {_totalTimeByDateTimeNowMs} / {_ticksNumber * _tickStepMs}");
            Console.WriteLine($"Total time by StopWatch:\t {_totalTimeByStopWatchMs} / {_ticksNumber * _tickStepMs}");
            Console.WriteLine($"Total time by SumOfDelays:\t {_totalTimeBySumOfDelaysMs} / {_ticksNumber * _tickStepMs - 1}");
            Console.WriteLine($"Number of divergent delays:\t {_divergentDelaysCounter} / {_ticksNumber * _tickStepMs - 1}");
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
                if (Math.Abs(delay - _tickStepMs) > _tickStepMs * (5 / 100.0))
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

