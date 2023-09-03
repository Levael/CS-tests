using GlobalTimeManagment;


class MainProgram
{
    static void Main()
    {
        var GTM = new GlobalTimeManager();

        
        GTM.StartTrialTimeManager();
        Thread.Sleep(3000);
        GTM.StartTrialTimeManager();

        /*Thread GlobalTimeManagerThread = new Thread(GlobalTimeManager.StartGlobalTimeManager);
        GlobalTimeManagerThread.Priority = ThreadPriority.Highest;

        GlobalTimeManagerThread.Start();

        GlobalTimeManagerThread.Join();*/

        Console.WriteLine("\nPress any key to stop...");
        Console.ReadKey();
    }
}
