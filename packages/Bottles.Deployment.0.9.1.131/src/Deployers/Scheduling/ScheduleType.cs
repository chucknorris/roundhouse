namespace Bottles.Deployment.Deployers.Scheduling
{
    public enum ScheduleType
    {
        Minute,
        Hourly,
        Daily,
        Weekly,
        Monthly,
        Once,
        OnStart,
        OnLogon,
        OnIdle
    }
}