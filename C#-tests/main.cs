using GlobalTimeManagment;


class MainProgram
{
    static void Main()
    {

        GlobalTimeManager.StartTrialTimeManager();

        /*Thread GlobalTimeManagerThread = new Thread(GlobalTimeManager.StartGlobalTimeManager);
        GlobalTimeManagerThread.Priority = ThreadPriority.Highest;

        GlobalTimeManagerThread.Start();

        GlobalTimeManagerThread.Join();*/

        Console.WriteLine("\nPress any key to stop...");
        Console.ReadKey();
    }
}
