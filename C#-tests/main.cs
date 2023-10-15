using ExperimentManager;
using GlobalTimeManagment;

/* TODO:
*   1) Add field to GUI: "reseacher notes about experiment"
*   2) Add to global notes that the release version should be precompiled for better performance (AOT compilation: "dotnet publish -r win-x64 -c Release")
*   3) Update GTM documentation
*/


class MainProgram
{
    static void Main()
    {
        InitClosingHandlers();

        var GTM = new GlobalTimeManager();
        var testExperiment = new Experiment(GTM);

        GTM.StartGlobalTicker();

        Thread.Sleep(3000); // obj init takes time, so wait for 3sec 
        testExperiment.TestRun(forward: 1000, wait: 2000, backward: 2500);
        Thread.Sleep(5000); // obj init takes time, so wait for 3sec 
        testExperiment.TestRun(forward: 1000, wait: 2000, backward: 2500);

        //Thread.Sleep(100);    // milliseconds * seconds * minutes * hours    /// every hour
        GTM.StopGlobalTicker();
        GTM.Debug(doWriteToConsole: false);




        /*Console.WriteLine("\nPress any key to stop...");
        Console.ReadKey();*/
    }

    private static void InitClosingHandlers()
    {
        // After exit from program
        AppDomain.CurrentDomain.ProcessExit += (s, e) =>
        {
            //Console.WriteLine("Program is exiting");
        };

        // After crash of program
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            //Console.WriteLine("Unhandled exception occurred");
        };
    }
}
