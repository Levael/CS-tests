using System.Runtime.CompilerServices;
using APIs;

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
            TimeFunctions.TimeBeginPeriod(1);

            WarmUp();
        }

        ~GlobalTimeManager()
        {
            TimeFunctions.TimeEndPeriod(1);
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
            warmUpSegmentTimeManager.AnalyzeTrialTimeData();
            warmUpSegmentTimeManager.PrintToConsoleAnalyzedTrialTimeData();

            Console.ResetColor();
        }




        public void StartTrialTimeManager()
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
        }


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

