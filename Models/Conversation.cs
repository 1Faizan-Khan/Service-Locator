namespace ServiceLocator.Models;

public class Conversation
{
    public int Id { get; set; }

    public int CustomerId { get; set; }
    public Customersignup Customer { get; set; }

    public int ProviderId { get; set; }
    public Providersignup Provider { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Message> Messages { get; set; }
}
