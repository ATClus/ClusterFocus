namespace Hub.Domain
{
    public class IActivity
    {
        string Title { get; }
        DateOnly Date { get; }
        TimeSpan TotalTimeSpent { get; }
    }
}
