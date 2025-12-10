namespace WorkerService
{
    public class HsysNotificationPayload
    {
        public long VaccineApplicationId { get; set; }
        public string? Message { get; set; }
        public long? ChildId { get; set; }
        public long? VaccineId { get; set; }
        public int RetryCount { get; set; }
        public DateTime AddedTime { get; set; } = DateTime.UtcNow;
    }
}
