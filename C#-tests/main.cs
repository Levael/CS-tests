using GlobalTimeManagment;


class MainProgram
{
    static void Main()
    {
        var GTM = new GlobalTimeManager();

        Thread.Sleep(new TimeSpan(hours: 1, minutes: 0, seconds: 0));
        GTM.StopGlobalTicker();

        GTM.Debug();


        /*Console.WriteLine("\nPress any key to stop...");
        Console.ReadKey();*/
    }
}
