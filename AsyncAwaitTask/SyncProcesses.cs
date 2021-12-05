using static System.Console;

public class SyncProcesses
{
    public static string MakeTea()
    {
        var water = BoilWater();
        WriteLine("Take the cups out");
        WriteLine("Put Tea in the cups");
        var tea = $"Pour {water} in cups";
        return tea;
    }

    public static string BoilWater()
    {
        WriteLine("Start the Kettle");
        WriteLine("waiting for the kettle");
        Task.Delay(2000).GetAwaiter().GetResult();
        WriteLine("Kettle Finished Boiling");
        return "Hot Water";
    }
}