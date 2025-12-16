using System.ComponentModel.DataAnnotations;

namespace ServiceLocator.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        // "Customer" or "Provider"
        [Required]
        public string InitiatorType { get; set; }

        // Id of the customer or provider who clicked
        [Required]
        public int InitiatorId { get; set; }

        // "Customer" or "Provider"
        [Required]
        public string TargetType { get; set; }

        // Id of the customer or provider receiving it
        [Required]
        public int TargetId { get; set; }

        // Used for bell counter
        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}