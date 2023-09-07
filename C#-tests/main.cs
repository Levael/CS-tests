using GlobalTimeManagment;
using MainClasses;


class MainProgram
{
    static void Main()
    {
        var GTM = new GlobalTimeManager();
        GTM.StartGlobalTicker();
        Thread.Sleep(2000);
        GTM.StopGlobalTicker();



        Console.WriteLine("\nPress any key to stop...");
        Console.ReadKey();
    }
}
