using Godot;
using System;

public partial class ChatPanel : Control
{
    [Export] private LineEdit messageInput;
    [Export] private Button sendButton;
    [Export] private VBoxContainer chatList;
    [Export] private PackedScene chatMessageScene; // Optional: if you want to use a custom scene for chat messages

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
        Label chatLabel = chatMessageScene.Instantiate<Label>();
        chatLabel.Text = chatMsg.ToString();
        chatList.AddChild(chatLabel);
    }
}
