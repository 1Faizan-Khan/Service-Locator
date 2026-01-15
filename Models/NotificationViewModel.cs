namespace ServiceLocator.Models {


    public class NotificationViewModel
    {
        public int NotificationId { get; set; }
        public string InitiatorName { get; set; }
        public string Service { get; set; }
        public bool viewerIsCustomer { get; set; }
        public bool IsAccepted { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }

        // 🔹 NEW: list of messages associated with this notification
        public List<MessageViewModel> Messages { get; set; } = new List<MessageViewModel>();
        public int InitiatorId { get; set; }   // REQUIRED for messaging

    }

}