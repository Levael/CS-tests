using System.Runtime.CompilerServices;

namespace GlobalTimeManagment
{
    public class GlobalTimeManager
    {
        //TODO: thread priority + paralell

        /// <summary>
        /// Constructor. Calls for "WarmUp" function to compile and load evething inside into cash (for faster execution in the future)
        /// </summary>
        public GlobalTimeManager()
        {
            WinAPIs.TimeFunctions.TimeBeginPeriod(1);

            WarmUp();
        }

        ~GlobalTimeManager()
        {
            WinAPIs.TimeFunctions.TimeEndPeriod(1);
        }

        public void StartTrialTimeManager(string runningMode = "regular")
        {
            var singleSegmentTimeManager = new SingleSegmentTimeManager(runningMode: runningMode, commandsQueueLength: 500);
            var singleSegmentTimeManager2 = new SingleSegmentTimeManager(runningMode: runningMode, commandsQueueLength: 1500);

            singleSegmentTimeManager.StartTheSegment();
            Thread.Sleep(1000);
            singleSegmentTimeManager2.StartTheSegment();



            singleSegmentTimeManager.AnalyzeTrialTimeData();
            singleSegmentTimeManager.PrintToConsoleAnalyzedTrialTimeData();
            //singleSegmentTimeManager.ExportDataToTxtFile();

            singleSegmentTimeManager2.AnalyzeTrialTimeData();
            singleSegmentTimeManager2.PrintToConsoleAnalyzedTrialTimeData();
            //singleSegmentTimeManager2.ExportDataToTxtFile();
        }

        /// <summary>
        /// Atually just runs "StartTrialTimeManager" function, but with only 2 repetitions (to make sure every function was called)
        /// and reduce time delay when running the fucntion for the very first time
        /// </summary>
        private void WarmUp()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            StartTrialTimeManager(runningMode: "warmup");
            Console.ResetColor();

            //WarmUpMethods(typeof(SingleSegmentTimeManager));
        }

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

