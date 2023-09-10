using GlobalTimeManagment;
using MainClasses;


class MainProgram
{
    static void Main()
    {
        var GTM = new GlobalTimeManager();

        //GTM.StartGlobalTicker();
        Thread.Sleep(10000);
        GTM.StopGlobalTicker();

        //GTM.GetAllDelays();


        Console.WriteLine("\nPress any key to stop...");
        Console.ReadKey();
    }
}
