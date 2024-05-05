using CustomDataStructures;
using ExperimentManager;
using G711MicStream;
using GlobalTimeManagment;



public class MainProgram
{
    //private InterlinkedCollection<AttributeSet> interlinkedCollection;

    public MainProgram()
    {
        InterlinkedCollection<AttributeSet> interLinkedCollection = new()
        {
            new AttributeSet(name: "Hanna", age: 21, isNormal: true),
            new AttributeSet(name: "Haanna", age: 241, isNormal: false),
            new AttributeSet(name: "Anna", age: 22, isNormal: false)
        };

        /*foreach (var item in interLinkedCollection)
        {
            Console.WriteLine($"Name: {item.name}");
        }*/

        /*Console.WriteLine(interLinkedCollection.FindRelatedSet("Anna").age);
        Console.WriteLine(interLinkedCollection.FindRelatedSet("Hanna").isNormal);
        interLinkedCollection.Update("Hanna", "isNormal", true);
        Console.WriteLine(interLinkedCollection.FindRelatedSet("Hanna").isNormal);*/
        var hannaData = interLinkedCollection.FindRelatedSet("Hanna");
        Console.WriteLine($"isNormal: {hannaData.isNormal}, age: {hannaData.age}");
        Console.Read();
    }

    public static void Main()
    {
        var app = new MainProgram();
    }
}


public class AttributeSet
{
    [CanBeKey(true)]
    public string name { get; set; }

    [CanBeKey(true)]
    public int age { get; set; }

    [CanBeKey(false)]
    public bool isNormal { get; set; }


    public AttributeSet() { }

    public AttributeSet(string name, int age, bool isNormal)
    {
        this.name = name;
        this.age = age;
        this.isNormal = isNormal;
    }
}

















/*private static void AudioTest()
{
    Console.WriteLine("Test2");
}*/

/*private static void InitClosingHandlers()
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
}*/

/*private void TimingTest()
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

    *//*Console.WriteLine("\nPress any key to stop...");
    Console.ReadKey();*//*
}*/


/* TODO:
*   1) Add field to GUI: "reseacher notes about experiment"
*   2) Add to global notes that the release version should be precompiled for better performance (AOT compilation: "dotnet publish -r win-x64 -c Release")
*   3) Update GTM documentation
*/