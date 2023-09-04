using System.Diagnostics;
using APIs; 


namespace GlobalTimeManagment
{
    /// <summary>
    /// Class for time managment of single segment -- as oneway movement of Moog without stops, or an oculus animation
    /// </summary>
    public class SingleSegmentTimeManager
    {
        //public Queue<List<Delegate>> trialDelegatesQueue;

        private Stopwatch       _stopWatch;                     // C# built-in ticker. Starts from 0 and ticks "StopWatch.Frequency" times per second. Best for measuring time
        private DateTime        _trialStartTime;                // Full time stamp to know when segment started
        private DateTime        _trialStopTime;                 // Full time stamp to know when segment stoped
        private long            _totalTimeByStopWatchMs;        // The duration in ms of the segment according to StopWatch
        private double          _totalTimeBySumOfDelaysMs;      // The duration in ms of the segment according to sum of delays between ticks (calculated with StopWatch)
        private double          _totalTimeByDateTimeNowMs;      // The duration in ms of the segment according to DateTime (stop - start)
        private long[]          _timeStamps;                    // Array of time stamps of every "ExecuteEveryLoopTick" function call (StopWatch ticks passed from start)
        private double[]        _delaysBetweenTicksMs;          // Array of delays between all ticks (length = number of ticks - 1)
        private int             _divergentDelaysCounter;        // Just a counter of delays whose duration is "tickPermissibleErrorPercent" longer than the ideal (tickStepMs)
        private readonly double _tickStepMs;                    // Almost main parameter: step of ticker in milliseconds. Minimum for Moog -- 1
        private readonly double _tickPermissibleErrorPercent;   // Parameter that in analytics counts the number of ticks that took longer by the specified percentage
        private readonly int    _ticksNumber;                   // Length of array with functions needed to execute every "tickStep" ms
        private readonly int    _spinWait;                      // Hard to explain. Long story short -- the only way I found for max accuracy for "Sleep" function (number hand-picked)
        private readonly double _stopWatchFrequencyPerMs;       // Value in ticks per ms (by default it's in ticks per sec). Needed for delay calculations and such stuff


        public SingleSegmentTimeManager(List<Action>[]? arrayOfListsOfActions = null)
        {
            // If "queueOfListsOfActions" is null, then it's warmup run and no need to spend time for more than 2 iterations of main loop
            if (arrayOfListsOfActions == null)  _ticksNumber = 2;
            else                                _ticksNumber = arrayOfListsOfActions.Length;


            //trialDelegatesQueue = new();
            _tickStepMs                     = 1.0;                              // 1ms is standart. Best for Moog
            _tickPermissibleErrorPercent    = 5;                                // 5% is just high precision for such task (IMHO)
            _spinWait                       = (int)(_tickStepMs * 1_000);       // 1000 is small enough not to exceed 1 mc, and at the same time it is large enough not to call the function too many times
            _stopWatchFrequencyPerMs        = Stopwatch.Frequency / 1000.0;     // 1000 here is number of ms in sec
            _stopWatch                      = new();                            // StopWatch initialisation. One for the whole class
            _timeStamps                     = new long[_ticksNumber];           // Definite size (as arrayOfActions input) for every tick (tick == input function call)
            _delaysBetweenTicksMs           = new double[_ticksNumber - 1];     // _ticksNumber - 1 because there are one fewer gaps than ticks

        }

        public void StartTheSegment()
        {
            ExecuteBeforeLoopStarts();
            RunTheLoop();
            ExecuteAfterLoopEnds();
        }


        public void ExecuteBeforeLoopStarts()
        {
            Optimization.PauseGarbageCollector();

            _trialStartTime = DateTime.Now;
            _stopWatch.Start();
        }

        public void ExecuteAfterLoopEnds()
        {
            _stopWatch.Stop();
            _trialStopTime = DateTime.Now;

            Optimization.ResumeGarbageCollector();
        }

        public void ExecuteEveryLoopTick(int index)
        {
            RecordTimeStamp(index);         // adds current stopWatch tick to "_timeStamps"

            //var commandsForThisTick = trialDelegatesQueue.Peek();
        }


        /// <summary>
        /// first function in a queue will be called immediately. next: after "tickStep" ms
        /// </summary>
        public void RunTheLoop()
        {
            for (int tickIndex = 0; tickIndex < _ticksNumber; tickIndex++)
            {
                ExecuteEveryLoopTick(tickIndex);    // function(s) from queue
                BusyWaitUntilNextTick(tickIndex);   // wait till next tick (<= "tickStep" ms)
            }
        }

        
        /// <summary>
        /// Busy-waiting. works way better than just "sleep" function, but actively "occupies" the processor. would be better to give that time to someone else meanwhile
        /// </summary>
        /// <param name="tickIndex">Points on current index in queue</param>
        private void BusyWaitUntilNextTick(int tickIndex)
        {
            // for better visual understanding first declaration of those 2 vars is mostly formal. real logic happend inside "while loop"
            double timeElapsedMs = 0;
            double timeShouldElapseMs = _tickStepMs;

            while (timeElapsedMs < timeShouldElapseMs)
            {
                Thread.SpinWait(_spinWait);

                timeElapsedMs = _stopWatch.ElapsedTicks / _stopWatchFrequencyPerMs;
                timeShouldElapseMs = (tickIndex + 1) * _tickStepMs;
            }
        }

        private void RecordTimeStamp(int tickIndex)
        {
            _timeStamps[tickIndex] = _stopWatch.ElapsedTicks;
        }

        public void AnalyzeTrialTimeData()
        {
            _delaysBetweenTicksMs = GetAllDelays();

            _divergentDelaysCounter = CalculateNumberOfDivergentDelays(_delaysBetweenTicksMs);

            _totalTimeBySumOfDelaysMs = CalculateTotalTimePassedMs(_delaysBetweenTicksMs);
            _totalTimeByDateTimeNowMs = (_trialStopTime - _trialStartTime).TotalMilliseconds;
            _totalTimeByStopWatchMs = _stopWatch.ElapsedMilliseconds;
        }

        public void PrintToConsoleAnalyzedTrialTimeData()
        {
            Console.WriteLine("===============================================================");
            Console.WriteLine($"Total time By TimeNow:\t\t {_totalTimeByDateTimeNowMs} / {_ticksNumber * _tickStepMs}");
            Console.WriteLine($"Total time by StopWatch:\t {_totalTimeByStopWatchMs} / {_ticksNumber * _tickStepMs}");
            Console.WriteLine($"Total time by SumOfDelays:\t {_totalTimeBySumOfDelaysMs} / {_ticksNumber * _tickStepMs - 1}");
            Console.WriteLine($"Number of divergent delays:\t {_divergentDelaysCounter} / {_ticksNumber * _tickStepMs - 1}");
            Console.WriteLine("===============================================================");
        }

        private double[] GetAllDelays()
        {
            var delays = new double[_timeStamps.Length - 1];

            for (int i = 1; i < _timeStamps.Length; i++)
            {
                var actualMsPassed = CalculateDelayInMs(_timeStamps[i - 1], _timeStamps[i], _stopWatchFrequencyPerMs);
                delays[i-1] = actualMsPassed;
            }

            return delays;
        }

        private double CalculateDelayInMs(long previousTimeStamp, long lastTimeStamp, double frequencyMs)
        {
            return (lastTimeStamp - previousTimeStamp) / frequencyMs;
        }

        private double CalculateTotalTimePassedMs(double[] delays)
        {
            double totalTimePassedMs = 0;

            foreach (var delay in delays)
            {
                totalTimePassedMs += delay;
            }

            return totalTimePassedMs;
        }

        private int CalculateNumberOfDivergentDelays(double[] delays)
        {
            int numberOfDivergentDelays = 0;
            int numberOfCheckedDelays = 0;

            foreach (var delay in delays)
            {
                numberOfCheckedDelays++;
                if (Math.Abs(delay - _tickStepMs) > _tickStepMs * (_tickPermissibleErrorPercent / 100.0))
                {
                    numberOfDivergentDelays++;
                }
            }

            return numberOfDivergentDelays;
        }

        public void ExportDataToTxtFile ()
        {
            using (StreamWriter sw = new StreamWriter(@"C:\Users\Levael\GitHub\C#-tests\C#-tests\GlobalTimeManagment\temp_debug.txt", true))
            {
                foreach (var value in _timeStamps)
                {
                    sw.Write($"{value},{3};");
                }
            }
        }
    }
}
