using System.Collections.Generic;

public sealed class ProgressTrackingState
{
    private readonly HashSet<int> countedBoxIds = new HashSet<int>();

    public bool IsRunning { get; private set; }

    public int Count
    {
        get { return countedBoxIds.Count; }
    }

    public void StartSession()
    {
        countedBoxIds.Clear();
        IsRunning = true;
    }

    public bool TryCountBox(int boxId)
    {
        if (!IsRunning)
            return false;

        return countedBoxIds.Add(boxId);
    }

    public bool HasCountedBox(int boxId)
    {
        return countedBoxIds.Contains(boxId);
    }

    public void ResetSession()
    {
        countedBoxIds.Clear();
        IsRunning = false;
    }
}