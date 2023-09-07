

namespace MainClasses
{
    public class Trial
    {
        #region PUBLIC FIELDS
        #endregion PUBLIC FIELDS

        #region PRIVATE FIELDS

        // time related variables
        private DateTime _trialStartTime;
        private DateTime _trialEndTime;
        private TimeSpan _trialDuration;

        // parameters related variables
        private List<StimulusType>  _stimulusTypes;
        private bool                _doTrackEyes;
        private bool _doPlaySoundForResponceReception;
        private bool _doPlaySoundForResponceCorrectness;
        private bool _doPlaySoundForTrialStart;
        private bool _doPlaySoundForTrialEnd;
        private bool _doPlaySoundForExceedingWaitingTime;

        // input data related variables
        private double[] _vestibularCommands;
        private double[] _visualCommands;
        private double[] _audioCommands;

        // output data related variables
        private ResponseFromParticipant _participantResponseShouldBe;
        private ResponseFromParticipant _participantActualResponse;
        private bool                    _receivedResponseIsCorrect;
        private List<double>            _eyesTrackedData;               // TODO: change later from "double" to something else

        private double[] _vestibularFeedback;
        private double[] _visualFeedback;
        private double[] _audioFeedback;

        // different variables
        private bool _trialIsValid;


        #endregion PRIVATE FIELDS

        public Trial() {
        
        }
    }
}