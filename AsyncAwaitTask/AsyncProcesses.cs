using static System.Console;

public class ASyncProcesses
{
    public static async Task<string> MakeTeaAsync(int timeToWait = 2000)
    {
        var boilingWater = BoilWaterAsync(timeToWait);
        WriteLine("Take the cups out");
        WriteLine("Put Tea in the cups");
        double a=0;
        for (double i=0 ; i<100_000_000_000; i++) a+=i;
        var water = await boilingWater;
        var tea = $"Pour {water} in cups";
        return tea;
    }

    public static async Task<string> BoilWaterAsync(int timeToWait)
    {
        WriteLine("Start the Kettle");
        WriteLine("waiting for the kettle");
        await Task.Delay(timeToWait);
        WriteLine("Kettle Finished Boiling");
        return "Hot Water";
    }
}