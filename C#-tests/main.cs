using GlobalTimeManagment;


class MainProgram
{
    static void Main()
    {
        Thread.Sleep(1000);
        GlobalTimeManager.StartTrialTimeManager();
        Thread.Sleep(1000);
        GlobalTimeManager.StartTrialTimeManager();
        // ААААААААААААА, да что с тобой не так?!

        /*Thread GlobalTimeManagerThread = new Thread(GlobalTimeManager.StartGlobalTimeManager);
        GlobalTimeManagerThread.Priority = ThreadPriority.Highest;

        GlobalTimeManagerThread.Start();

        GlobalTimeManagerThread.Join();*/

        Console.WriteLine("\nPress any key to stop...");
        Console.ReadKey();
    }
}
