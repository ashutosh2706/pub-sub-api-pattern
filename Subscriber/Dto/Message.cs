namespace Subscriber.Dto;

public class Message
{
    public int Id { get; set; }
    public string? TopicMessage { get; set; }
    public DateTime Expires { get; set; }
    public string MessageStatus { get; set; }
}