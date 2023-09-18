using GlobalTimeManagment;


class MainProgram
{
    static void Main()
    {
        var GTM = new GlobalTimeManager();

        Thread.Sleep(1000);    // milliseconds * seconds * minutes * hours    /// every hour  * 60 * 60
        GTM.StopGlobalTicker();

        GTM.Debug(doWriteToConsole: true);


        /*Console.WriteLine("\nPress any key to stop...");
        Console.ReadKey();*/
    }
}
