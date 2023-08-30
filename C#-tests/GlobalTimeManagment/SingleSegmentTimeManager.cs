﻿using System;
using System.Diagnostics;
using System.Runtime;
using System.IO;

// NOTES:
//
// 1) There is always ~15ms difference between SumOfDelays and TotalTime. No matter how many ticks there were. Why? What are those 15ms?


namespace GlobalTimeManagment
{
    public class SingleSegmentTimeManager
    {
        public Queue<List<Delegate>> trialDelegatesQueue;

        private Stopwatch _stopWatch;
        private DateTime _trialStartTime;
        private DateTime _trialStopTime;
        private long _totalTimeByStopWatchMs;
        private double _totalTimeBySumOfDelays;
        private double _totalTimeByDateTimeNowMs;
        private long[] _timeStamps;     // array instead of List for better performance. maybe will change it later back to List
        private List<double> _delaysBetweenTicks;
        private int _divergentDelaysCounter;
        private readonly double _tickStepMs;
        private readonly double _tickStepErrorBoundsPercent;
        private readonly int _ticksNumber;
        private readonly int _spinWait;
        private readonly double _stopWatchFrequencyPerMs;


        public SingleSegmentTimeManager()
        {
            trialDelegatesQueue = new();


            _tickStepMs = 1.0;
            _tickStepErrorBoundsPercent = 10;
            _ticksNumber = 1000;
            //_ticksNumber = trialDelegatesQueue.Count;
            _spinWait = (int)(_tickStepMs * 1_000);
            _stopWatchFrequencyPerMs = Stopwatch.Frequency / 1000.0;

            _stopWatch = new();
            _timeStamps = new long[_ticksNumber];
            _delaysBetweenTicks = new();
        }

        public void StartTheTrial()
        {
            ExecuteBeforeLoopStarts();
            RunTheLoop();
            ExecuteAfterLoopEnds();
        }


        public void ExecuteBeforeLoopStarts()
        {
            WinAPIs.TimeFunctions.TimeBeginPeriod(1);
            //PauseGarbageCollector();

            // there is no realy need to clear the array. Every time it will be rewritten anyway
            //_timeStamps.Clear();
            //Array.Fill(_timeStamps, -1);

            _stopWatch.Start();
            _trialStartTime = DateTime.Now;
        }

        public void ExecuteAfterLoopEnds()
        {
            _stopWatch.Stop();
            _trialStopTime = DateTime.Now;

            //ResumeGarbageCollector();
            WinAPIs.TimeFunctions.TimeEndPeriod(1);
        }

        public void ExecuteEveryLoopTick(int index)
        {
            _timeStamps[index] = _stopWatch.ElapsedTicks;

            //var commandsForThisTick = trialDelegatesQueue.Peek();
        }


        /// <summary>
        /// first function in a queue will be called immediately. next: after "tickStep" ms
        /// </summary>
        public void RunTheLoop()
        {
            for (int tickIndex = 0; tickIndex < _ticksNumber; tickIndex++)
            {
                RecordTimeStamp(tickIndex);         // adds current stopWatch tick to "_timeStamps"
                ExecuteEveryLoopTick(tickIndex);    // function(s) from queue
                BusyWaitUntilNextTick(tickIndex);   // wait till next tick (<= "tickStep" ms)
            }
        }

        private void BusyWaitUntilNextTick(int tickIndex)
        {
            // for better visual understanding first declaration of those 2 vars is mostly formal. real logic happend inside "while loop"
            double timeElapsedMs = 0;
            double timeShouldElapseMs = _tickStepMs;

            while (timeElapsedMs < timeShouldElapseMs)
            {
                // busy-waiting. works way better than just "sleep" function, bu actively "occupies" the processor. would be better to give that time to someone elsemeanwhile
                Thread.SpinWait(_spinWait);

                timeElapsedMs = _stopWatch.ElapsedTicks / _stopWatchFrequencyPerMs;
                timeShouldElapseMs = (tickIndex + 1) * _tickStepMs;
            }
        }

        private void RecordTimeStamp(int tickIndex)
        {
            _timeStamps[tickIndex] = _stopWatch.ElapsedTicks;
        }
        private void PauseGarbageCollector()
        {
            GC.Collect();                                       // Принудительный сбор мусора перед приостановкой
            GC.WaitForPendingFinalizers();                      // Ожидание завершения финализаторов
            GCSettings.LatencyMode = GCLatencyMode.LowLatency;  // Отключение автоматического сборщика мусора
        }
        private void ResumeGarbageCollector()
        {
            GCSettings.LatencyMode = GCLatencyMode.Interactive;
        }

        public void AnalyzeTrialTimeData()
        {
            _delaysBetweenTicks = GetAllDelays();

            _divergentDelaysCounter = CalculateNumberOfDivergentDelays(_delaysBetweenTicks);

            _totalTimeBySumOfDelays = CalculateTotalTimePassedMs(_delaysBetweenTicks);
            _totalTimeByDateTimeNowMs = (_trialStopTime - _trialStartTime).TotalMilliseconds;
            _totalTimeByStopWatchMs = _stopWatch.ElapsedMilliseconds;
        }

        public void PrintToConsoleAnalyzedTrialTimeData()
        {
            Console.WriteLine($"Total time By TimeNow:\t\t {_totalTimeByDateTimeNowMs} / 1000");
            Console.WriteLine($"Total time by SumOfDelays:\t {_totalTimeBySumOfDelays} / 999");
            Console.WriteLine($"Total time by StopWatch:\t {_totalTimeByStopWatchMs} / 1000");
            Console.WriteLine($"Number of divergent delays:\t {_divergentDelaysCounter} / 999");
        }

        private List<double> GetAllDelays()
        {
            var delays = new List<double>();

            for (int i = 1; i < _timeStamps.Length; i++)
            {
                var actualMsPassed = CalculateDelayInMs(_timeStamps[i - 1], _timeStamps[i], _stopWatchFrequencyPerMs);
                delays.Add(actualMsPassed);
            }

            return delays;
        }

        private double CalculateDelayInMs(long previousTimeStamp, long lastTimeStamp, double frequencyMs)
        {
            return (lastTimeStamp - previousTimeStamp) / frequencyMs;
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
                if (Math.Abs(delay - _tickStepMs) > _tickStepMs * (_tickStepErrorBoundsPercent / 100.0))
                {
                    numberOfDivergentDelays++;


                    /*Console.Write($"Index: {numberOfCheckedDelays}.\t Divergent delay value: ");

                    if (delay > 1) { Console.ForegroundColor = ConsoleColor.Cyan; }
                    else if (delay == 1) { Console.ForegroundColor = ConsoleColor.Green; }
                    else { Console.ForegroundColor = ConsoleColor.Red; }

                    Console.WriteLine(delay);
                    Console.ResetColor();*/
                }
            }

            return numberOfDivergentDelays;
        }

        public void ExportDataToTxtFile ()
        {
            using (StreamWriter sw = new StreamWriter(@"C:\Users\Levael\GitHub\C#-tests\C#-tests\GlobalTimeManagment\temp_debug.txt"))
            {
                foreach (var value in _timeStamps)
                {
                    sw.Write($"{value},{3};");
                }
            }
        }
    }
}