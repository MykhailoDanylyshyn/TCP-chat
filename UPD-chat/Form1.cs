
using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualBasic;

namespace UPD_chat;

public partial class Form1 : Form
{
    public Form1()
    {
        InitializeComponent();

        userName = Interaction.InputBox(
            "Введіть ваше ім'я:",
            "Ім'я користувача");

        if (string.IsNullOrWhiteSpace(userName))
        {
            Environment.Exit(0);
        }

        listBoxChats.DisplayMember = "Name";
        labelStatus.Text = "Не підключено";

        Shown += async (s, e) => await CheckServer();
    }

    private async Task CheckServer()
    {
        try
        {
            await LoadChats();
            buttonLoadChats.Text = "Відновити чат";
        }
        catch
        {
            labelStatus.Text = "Не підключено";
            buttonLoadChats.Text = "Отримати список чатів";
        }
    }

    private async Task LoadChats()
    {
        var response = await ChatClient.SendRequest("GetChats");

        listBoxChats.Items.Clear();

        foreach (var chat in response.Chats)
        {
            listBoxChats.Items.Add(chat);
        }

        labelStatus.Text = "Підключено";
    }

    private async void buttonCreateChat_Click(object sender, EventArgs e)
    {
        string name = Interaction.InputBox(
            "Введіть назву чату:",
            "Створення чату");

        if (string.IsNullOrWhiteSpace(name))
            return;

        try
        {
            var response = await ChatClient.SendRequest(
                "CreateChat",
                name,
                userName: userName);

            labelStatus.Text = "Підключено";

            MessageBox.Show(response.Message);

            await LoadChats();
        }
        catch (Exception ex)
        {
            labelStatus.Text = "Не підключено";

            MessageBox.Show(
                "Помилка створення чату:\n" + ex.Message);
        }
    }

    private async void buttonLoadChats_Click(object sender, EventArgs e)
    {
        if (listBoxChats.SelectedItem is ChatClient.ChatInfo chat)
        {
            ChatForm form = new ChatForm(chat);
            form.Show();
            return;
        }

        try
        {
            await LoadChats();

            if (listBoxChats.Items.Count == 0)
            {
                listBoxChats.Items.Add("Створених чатів немає");
            }

            buttonLoadChats.Text = "Відновити чат";
        }
        catch (Exception ex)
        {
            labelStatus.Text = "Не підключено";

            MessageBox.Show(
                "Помилка завантаження чатів:\n" + ex.Message);
        }
    }
}
