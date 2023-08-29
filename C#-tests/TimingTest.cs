using System;
using System.Collections.Generic;
using System.Diagnostics;


namespace Timings
{
    /*static public class TimingTest
    {
        private static List<long>       _timeStamps                 = new();
        private static Stopwatch        _stopWatch                  = new();
        private static readonly double  _tickStepMs                 = 1.0;
        private static readonly double  _tickStepErrorBoundsPercent = 25;
        private static readonly int     _loopRepetitions            = 1_000;
        private static readonly int     _spinWait                   = (int)(_tickStepMs * 1_000);           // 1_000 просто подобрал. можно и меньше, но не больше
        private static readonly long    _stopWatchFrequencyPerSec   = Stopwatch.Frequency;                  // Because the Stopwatch frequency depends on the installed hardware and operating system, the Frequency value remains constant while the system is running.
        private static readonly double  _stopWatchFrequencyPerMs    = _stopWatchFrequencyPerSec / 1000.0;   // "/ 1000" to translate second to milliseconds

        // TODO: add queue


        /// <summary>
        /// Entry point from outside. Starts the main function ("Run")
        /// </summary>
        public static void Run ()
        {
            RunMainLoop();
        }

        private static void RunMainLoop()
        {
            ExecuteBeforeLoopStarts();
            StartTheLoop();
            ExecuteAfterLoopEnds();
        }

        private static void ExecuteBeforeLoopStarts()
        {
            WinAPIs.TimeFunctions.TimeBeginPeriod(1);
            _stopWatch.Start();
        }

        /// <summary>
        /// Current realization has one obvious disadvantage: if there is a big lag -- next few tick will be called without any delay
        /// </summary>
        private static void StartTheLoop()
        {
            for (int step_index = 0; step_index < _loopRepetitions; step_index++)
            {
                ExecuteEveryLoopTick();

                // for better visual understanding first declaration of those 2 vars is mostly formal. real logic happend inside "while loop"
                double timeElapsedMs = 0;
                double timeShouldElapseMs = _loopRepetitions * _tickStepMs;

                while (timeElapsedMs < timeShouldElapseMs)
                {
                    // busy-waiting. works way better than just "sleep" function, bu actively "occupies" the processor. would be better to give that time to someone elsemeanwhile
                    Thread.SpinWait(_spinWait);

                    timeElapsedMs = _stopWatch.ElapsedTicks / _stopWatchFrequencyPerMs;
                    timeShouldElapseMs = (step_index + 1) * _tickStepMs;
                }
            }
        }

        private static void ExecuteEveryLoopTick()
        {
            // Here may be logic of adding to queue moog commands one by one for example


            _timeStamps.Add(_stopWatch.ElapsedTicks);
            //Console.WriteLine(_realTotalTimePassedMs);
        }

        private static void ExecuteAfterLoopEnds()
        {
            WinAPIs.TimeFunctions.TimeEndPeriod(1);
            _stopWatch.Stop();
            var data = AnalyzeTimings();
            
            Console.WriteLine($"Inaccurate total time\t: {_stopWatch.ElapsedMilliseconds} ms");
            Console.WriteLine($"Accurate total time\t: {data["actualTotalMsPassed"]} ms.\t Divergent delays count: {data["divergentDelays"]}");
        }

        private static Dictionary<string, double> AnalyzeTimings ()
        {
            double actualTotalMsPassed = 0;
            double divergentDelaysCounter = 0;

            for (int i = 1; i < _loopRepetitions; i++)
            {
                var actualMsPassed = CalculateDelayInMs(_timeStamps[i-1], _timeStamps[i], _stopWatchFrequencyPerSec);
                actualTotalMsPassed += actualMsPassed;

                // 100 = 100%. E.G: if error is bigger than "3%" -> print ('3' is for example only)
                if (Math.Abs(actualMsPassed - _tickStepMs) > (_tickStepMs * (_tickStepErrorBoundsPercent / 100)))
                {
                    divergentDelaysCounter++;
                    //Console.WriteLine(actualMsPassed);
                }

            }

            return new Dictionary<string, double>() {
                { "actualTotalMsPassed", actualTotalMsPassed },
                { "divergentDelays", divergentDelaysCounter }
            };
        }

        private static double CalculateDelayInMs (long previousTimeStamp, long lastTimeStamp, long frequencySec)
        {
            return (double)(lastTimeStamp - previousTimeStamp) / _stopWatchFrequencyPerMs;
        }
    }*/
}
