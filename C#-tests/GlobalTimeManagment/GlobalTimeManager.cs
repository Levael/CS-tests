namespace GlobalTimeManagment
{
    public static class GlobalTimeManager
    {
        public static void StartTrialTimeManager()
        {
            var singleSegmentTimeManager = new SingleSegmentTimeManager();

            singleSegmentTimeManager.StartTheTrial();


            singleSegmentTimeManager.AnalyzeTrialTimeData();
            singleSegmentTimeManager.PrintToConsoleAnalyzedTrialTimeData();
            singleSegmentTimeManager.ExportDataToTxtFile();
        }
    }
}

