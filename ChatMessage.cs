using Steamworks;

public class ChatMessage
{
    public CSteamID SenderId { get; set; }
    public string SenderName { get; set; }
    public string Message { get; set; }
    public string Timestamp { get; set; }
    public ChatMessage(CSteamID SenderId, string SenderName, string Message)
    {
        this.SenderId = SenderId;
        this.SenderName = SenderName;
        this.Message = Message;
        Timestamp = System.DateTime.Now.ToUniversalTime().ToShortTimeString();
    }
    public override string ToString()
    {
        return $"[{Timestamp}] {SenderName}: {Message}";
    }
}