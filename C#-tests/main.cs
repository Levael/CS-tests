using GlobalTimeManagment;

/* TODO:
*   1) Add field to GUI: "reseacher notes about experiment"
*/


class MainProgram
{
    static void Main()
    {
        var GTM = new GlobalTimeManager();

        Thread.Sleep(1000 * 60 * 60);    // milliseconds * seconds * minutes * hours    /// every hour
        GTM.StopGlobalTicker();

        GTM.Debug(doWriteToConsole: false);


        /*Console.WriteLine("\nPress any key to stop...");
        Console.ReadKey();*/
    }
}
