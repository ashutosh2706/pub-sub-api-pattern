using System.ComponentModel.DataAnnotations;

namespace MessageBroker.Models;

public class Message
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string? TopicMessage { get; set; }
    [Required]
    public int SubscriptionId { get; set; }
    [Required]
    public DateTime Expires { get; set; } = DateTime.Now.AddDays(1);
    [Required]
    public string MessageStatus { get; set; } = "NEW";
}