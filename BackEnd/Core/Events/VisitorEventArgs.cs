namespace OhMyWord.Core.Events;

public class VisitorEventArgs : EventArgs
{
    public string VisitorId { get; }
    public int VisitorCount { get; }
    public string ConnectionId { get; }

    public VisitorEventArgs(string visitorId, int visitorCount, string connectionId)
    {
        VisitorId = visitorId;
        VisitorCount = visitorCount;
        ConnectionId = connectionId;
    }
}
