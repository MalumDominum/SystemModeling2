namespace SystemModeling2.Devices.Models;

public class StartedConditions
{
    public StartedConditions(int? busyCount, int[]? inQueue, double? firstInTime)
    {
        BusyCount = busyCount;
        InQueue = inQueue;
        FirstInTime = firstInTime;
    }

    public int? BusyCount { get; set; }

    public int[]? InQueue { get; set; }

    public double? FirstInTime { get; set; }


}