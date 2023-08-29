namespace GlobalTimeManagment
{
    public static class GlobalTimeManager
    {
        public static void StartTrialTimeManager()
        {
            var trialTimeManager = new TrialTimeManager();

            trialTimeManager.StartTheTrial();


            trialTimeManager.AnalyzeTrialTimeData();
            trialTimeManager.PrintToConlsoleAnalyzedTrialTimeData();
        }
    }
}

