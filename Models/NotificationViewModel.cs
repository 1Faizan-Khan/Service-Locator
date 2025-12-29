namespace ServiceLocator.Models
{
    public class NotificationViewModel
    {
        public int NotificationId { get; set; }
        public string InitiatorName { get; set; }
        public string Service { get; set; }
        public bool viewerIsCustomer { get; set; }
        public bool IsAccepted { get; set; }

        // Contact info for accepted notifications
        public string Phone { get; set; }
        public string Email { get; set; }
    }
}
