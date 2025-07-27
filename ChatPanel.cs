using Godot;
using System;

public partial class ChatPanel : VBoxContainer
{
    [Export] private LineEdit messageInput;
    [Export] private Button sendButton;
    [Export] private VBoxContainer chatList;

    public override void _Ready()
    {
        sendButton.Pressed += OnSendButtonPressed;
        messageInput.TextSubmitted += OnMessageSubmitted;
        LobbyManager.Instance.ChatMessageReceived += OnChatMessageReceived;
        GD.Print("ChatPanel initialized successfully.");
    }

    private void OnSendButtonPressed()
    {
        OnMessageSubmitted(messageInput.Text);
    }

    private void OnMessageSubmitted(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            GD.Print("Cannot send an empty message.");
            return;
        }

        if (LobbyManager.Instance.SendChatMessage(message)) // attempt to send
        {
            // if successful, empty text input
            messageInput.Text = string.Empty;
        }
    }

    private void OnChatMessageReceived(ChatMessage chatMsg)
    {
        Label chatLabel = new Label();
        chatLabel.AddThemeFontSizeOverride("font_size", 12); // Set font size
        chatLabel.Text = chatMsg.ToString();
        chatList.AddChild(chatLabel);
    }
}
