using FubuCore;

namespace Bottles.Deployment.Deployers.Scheduling
{
    /// <summary>
    /// //http://www.microsoft.com/resources/documentation/windows/xp/all/proddocs/en-us/schtasks.mspx?mfr=true
    /// </summary>
    public class ScheduledTask : IDirective
    {
        public ScheduledTask()
        {
            UserAccount = "System";
        }
        public string Name { get; set; }
        public string ScheduleType { get; set; } //could be enum?
        public string Modifier { get; set; } //could be int?
        public string UserAccount { get; set; } //username/password/domain account???
        public string TaskToRun { get; set; }

        public string InstallLocation { get; set; } //where we should install the bits

        public override string ToString()
        {
            return "Scheduled Task '{0}'".ToFormat(Name);
        }
    }
}