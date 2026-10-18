namespace UPD_chat
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            buttonCreateChat = new Button();
            buttonLoadChats = new Button();
            listBoxChats = new ListBox();
            labelStatus = new Label();
            SuspendLayout();
            // 
            // buttonCreateChat
            // 
            buttonCreateChat.Font = new Font("Segoe UI", 16F);
            buttonCreateChat.Location = new Point(12, 12);
            buttonCreateChat.Name = "buttonCreateChat";
            buttonCreateChat.Size = new Size(321, 54);
            buttonCreateChat.TabIndex = 1;
            buttonCreateChat.Text = "Створити чат";
            buttonCreateChat.UseVisualStyleBackColor = true;
            buttonCreateChat.Click += buttonCreateChat_Click;
            // 
            // buttonLoadChats
            // 
            buttonLoadChats.Font = new Font("Segoe UI", 16F);
            buttonLoadChats.Location = new Point(12, 795);
            buttonLoadChats.Name = "buttonLoadChats";
            buttonLoadChats.Size = new Size(320, 54);
            buttonLoadChats.TabIndex = 1;
            buttonLoadChats.Text = "Отримати список чатів";
            buttonLoadChats.UseVisualStyleBackColor = true;
            buttonLoadChats.Click += buttonLoadChats_Click;
            // 
            // listBoxChats
            // 
            listBoxChats.FormattingEnabled = true;
            listBoxChats.Location = new Point(12, 95);
            listBoxChats.Name = "listBoxChats";
            listBoxChats.Size = new Size(320, 679);
            listBoxChats.TabIndex = 2;
            // 
            // labelStatus
            // 
            labelStatus.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            labelStatus.AutoSize = true;
            labelStatus.Font = new Font("Segoe UI", 16F);
            labelStatus.ImageAlign = ContentAlignment.MiddleRight;
            labelStatus.Location = new Point(694, 24);
            labelStatus.Name = "labelStatus";
            labelStatus.Size = new Size(168, 30);
            labelStatus.TabIndex = 3;
            labelStatus.Text = "Не підключено";
            labelStatus.TextAlign = ContentAlignment.MiddleRight;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(884, 861);
            Controls.Add(labelStatus);
            Controls.Add(listBoxChats);
            Controls.Add(buttonLoadChats);
            Controls.Add(buttonCreateChat);
            MinimumSize = new Size(900, 900);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Button buttonCreateChat;
        private Button buttonLoadChats;
        private ListBox listBoxChats;
        private Label labelStatus;

        private string userName = "";

    }
}
