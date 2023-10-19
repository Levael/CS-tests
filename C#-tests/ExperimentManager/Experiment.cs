using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CustomExtensions;
using GlobalTimeManagment;

namespace ExperimentManager
{
    public class Experiment
    {
        #region PUBLIC FIELDS
        #endregion PUBLIC FIELDS


        #region PRIVATE FIELDS

        // time related variables
        private DateTime _experimentStartTime;
        private DateTime _experimentEndTime;
        private TimeSpan _experimentDuration;

        // input data related variables
        private string      _researcherName;         // the person who conducts the experiment (the one behind the computer)
        private string      _participantName;        // the person on whom the experiment is being conducted (is the one who is sitting in the moog)
        private Parameters  _parameters;

        // output data related variables
        private List<ResponseFromParticipant> _participantResponses;

        // different variables
        private GlobalTimeManager _GTM;             // reference to TimeManagment
        private List<Trial> _trials;                // every trial of current experiment (may be added "on the go")

        #endregion PRIVATE FIELDS


        public Experiment(GlobalTimeManager GTM)
        {
            _GTM = GTM;

            _trials = new();
            _participantResponses = new();
        }

        public void StartExperiment()
        {
            _experimentStartTime = DateTime.Now;

            // ...
        }

        public void FinishExperiment()
        {
            _experimentEndTime = DateTime.Now;

            // ...
        }

        public void TestRun(int forward, int wait, int backward)
        {
            int numberOfParallelThings = 2;                                 // 2 = moog + oculus
            var countdown = new CountdownEvent(numberOfParallelThings);

            List<Action>[] FirstListOfAllSets = GenerateSinglePart(sequenceLength: forward, countdown, isMoog: true, isOculus: true);     // functions for forward movement
            List<Action>[] SecondListOfAllSets = GenerateSinglePart(sequenceLength: backward, countdown, isMoog: true, isOculus: false);   // functions for backward movement

            countdown.Reset(2); // 2 = moog + oculus
            _GTM.AddFunctionsForRangeTicksToExecutionQueue(FirstListOfAllSets);     // movement forward
            countdown.Wait();                                                       // wait for end of forward movement
            countdown.Reset(1); // 1 = moog
            Thread.Sleep(wait);                                                     // waiting for answer
            _GTM.AddFunctionsForRangeTicksToExecutionQueue(SecondListOfAllSets);    // movement backward
            countdown.Wait();                                                       // wait for end of backward movement

            Console.WriteLine("done");
        }

        private void AfterSectionFinished(CountdownEvent countdown)
        {
            countdown.Signal();
        }

        private List<Action>[] GenerateSinglePart(int sequenceLength, CountdownEvent countdown, bool isMoog, bool isOculus)
        {
            List<Action>[] allLists = new List<Action>[sequenceLength];

            for (int i = 0; i < allLists.Length; i++)
            {
                List<Action> actionsList = new();
                if (isMoog)                     actionsList.Add(() => { Console.WriteLine("Moog"); });
                if (isOculus && i % 11 == 0)    actionsList.Add(() => { Console.WriteLine("Oculus"); });

                if (i == allLists.Length - 1)   // last item
                {
                    actionsList.Add(() => {
                        if (isMoog)     AfterSectionFinished(countdown);    // for moog
                        if (isOculus)   AfterSectionFinished(countdown);    // for oculus
                    });
                }

                allLists[i] = actionsList;
            }

            return allLists;
        }
    }
}
