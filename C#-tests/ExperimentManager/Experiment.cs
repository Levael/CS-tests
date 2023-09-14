using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

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
        private string _researcherName;         // the person who conducts the experiment (the one behind the computer)
        private string _participantName;        // the person on whom the experiment is being conducted (is the one who is sitting in the moog)
        private Parameters _parameters;

        // output data related variables
        private List<ResponseFromParticipant> _participantResponses;

        // different variables
        private List<Trial> _trials;                                    // every trial of current experiment (may be added "on the go")

        #endregion PRIVATE FIELDS


        public Experiment()
        {
            _trials = new();
            _participantResponses = new();
        }

        /*private void ExecuteBeforeExperiment()
        {
            _experimentStartTime = DateTime.Now;
        }

        private void ExecuteAfterExperiment()
        {
            _experimentEndTime = DateTime.Now;
        }*/

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
    }
}
