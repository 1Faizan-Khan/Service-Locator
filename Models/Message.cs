public class Message
{
    public int Id { get; set; }

    public int SenderId { get; set; }
    public string SenderType { get; set; } // "Customer" or "Provider"

    public int RecipientId { get; set; }
    public string RecipientType { get; set; }

    public string Text { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public bool IsRead { get; set; } = false;

}
