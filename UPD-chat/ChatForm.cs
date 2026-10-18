using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualBasic;

namespace UPD_chat;

public partial class ChatForm : Form
{
    private readonly ChatClient.ChatInfo chat;

    public ChatForm(ChatClient.ChatInfo chat)
    {
        InitializeComponent();

        this.chat = chat;
        Text = chat.Name;

        Shown += async (s, e) => await LoadChat();
    }

    private async Task LoadChat()
    {
        try
        {
            var response = await ChatClient.SendRequest(
                "GetChatData",
                chatId: chat.Id);

            listBoxParticipants.Items.Clear();
            listBoxMessages.Items.Clear();

            foreach (string participant in response.Participants)
                listBoxParticipants.Items.Add(participant);

            foreach (string message in response.Messages)
                listBoxMessages.Items.Add(message);

            if (listBoxMessages.Items.Count > 0)
                listBoxMessages.TopIndex = listBoxMessages.Items.Count - 1;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Не вдалося завантажити чат: " + ex.Message);
        }
    }

    private async void buttonSend_Click(object sender, EventArgs e)
    {
        string message = textBoxMessage.Text.Trim();

        if (message == "")
            return;

        try
        {
            await ChatClient.SendRequest(
                "SendMessage",
                chatId: chat.Id,
                text: message);

            textBoxMessage.Clear();
            await LoadChat();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Не вдалося надіслати повідомлення: " + ex.Message);
        }
    }

    private void InitializeComponent()
    {
        listBoxParticipants = new ListBox();
        listBoxMessages = new ListBox();
        buttonSend = new Button();
        buttonAddParticipant = new Button();
        textBoxMessage = new TextBox();
        SuspendLayout();
        // 
        // listBoxParticipants
        // 
        listBoxParticipants.FormattingEnabled = true;
        listBoxParticipants.Location = new Point(12, 12);
        listBoxParticipants.Name = "listBoxParticipants";
        listBoxParticipants.Size = new Size(252, 769);
        listBoxParticipants.TabIndex = 0;
        // 
        // listBoxMessages
        // 
        listBoxMessages.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        listBoxMessages.FormattingEnabled = true;
        listBoxMessages.Location = new Point(285, 12);
        listBoxMessages.Name = "listBoxMessages";
        listBoxMessages.Size = new Size(1273, 769);
        listBoxMessages.TabIndex = 0;
        // 
        // buttonSend
        // 
        buttonSend.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        buttonSend.Font = new Font("Segoe UI", 16F);
        buttonSend.Location = new Point(1340, 798);
        buttonSend.Name = "buttonSend";
        buttonSend.Size = new Size(218, 51);
        buttonSend.TabIndex = 1;
        buttonSend.Text = "Надіслати";
        buttonSend.UseVisualStyleBackColor = true;
        buttonSend.Click += buttonSend_Click;
        // 
        // buttonAddParticipant
        // 
        buttonAddParticipant.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        buttonAddParticipant.Font = new Font("Segoe UI", 16F);
        buttonAddParticipant.Location = new Point(12, 798);
        buttonAddParticipant.Name = "buttonAddParticipant";
        buttonAddParticipant.Size = new Size(252, 51);
        buttonAddParticipant.TabIndex = 1;
        buttonAddParticipant.Text = "Додати учасника";
        buttonAddParticipant.UseVisualStyleBackColor = true;
        buttonAddParticipant.Click += buttonAddParticipant_Click;
        // 
        // textBoxMessage
        // 
        textBoxMessage.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        textBoxMessage.Font = new Font("Segoe UI", 16F);
        textBoxMessage.Location = new Point(285, 798);
        textBoxMessage.Multiline = true;
        textBoxMessage.Name = "textBoxMessage";
        textBoxMessage.Size = new Size(1024, 51);
        textBoxMessage.TabIndex = 2;
        // 
        // ChatForm
        // 
        ClientSize = new Size(1584, 861);
        Controls.Add(textBoxMessage);
        Controls.Add(buttonAddParticipant);
        Controls.Add(buttonSend);
        Controls.Add(listBoxMessages);
        Controls.Add(listBoxParticipants);
        Name = "ChatForm";
        ResumeLayout(false);
        PerformLayout();

    }

    private async void buttonAddParticipant_Click(object sender, EventArgs e)
    {
        string ip = Interaction.InputBox(
            "Введіть IP-адресу учасника:",
            "Додати учасника");

        if (string.IsNullOrWhiteSpace(ip))
            return;

        try
        {
            var response = await ChatClient.SendRequest(
                "AddParticipant",
                chatId: chat.Id,
                ip: ip);

            MessageBox.Show(response.Message);
            await LoadChat();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Не вдалося додати учасника: " + ex.Message);
        }
    }

    private ListBox listBoxParticipants;
    private ListBox listBoxMessages;
    private Button buttonSend;
    private TextBox textBoxMessage;
    private Button buttonAddParticipant;

    
}
