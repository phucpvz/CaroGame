using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CaroGame.Network
{
    public class ChatManager : INotifyPropertyChanged
    {
        private Form1 mainForm;
        private TextBox txtChat;
        private TextBox txtInput;
        private Button btnSend;
        private bool enabled;

        public event PropertyChangedEventHandler PropertyChanged;

        public TextBox TxtChat { get => txtChat; set => txtChat = value; }
        public TextBox TxtInput { get => txtInput; set => txtInput = value; }
        public Button BtnSend { get => btnSend; set => btnSend = value; }
        public Form1 MainForm { get => mainForm; set => mainForm = value; }
        public bool Enabled { get => enabled; set { enabled = value; OnPropertyChanged(); } }

        public ChatManager(Form1 mainForm, TextBox txtChat, TextBox txtInput, Button btnSend)
        {
            MainForm = mainForm;
            TxtChat = txtChat;
            TxtInput = txtInput;
            BtnSend = btnSend;

            TxtInput.KeyDown += TxtInput_KeyDown;
            BtnSend.Click += BtnSend_Click;

            PropertyChanged += ChatManager_PropertyChanged;
        }

        private void ChatManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            TxtInput.Enabled = BtnSend.Enabled = Enabled;
        }

        protected virtual void OnPropertyChanged()
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("Enabled"));
            }
        }

        public void ClearChat()
        {
            TxtChat.Clear();
        }

        // Hiển thị tin nhắn nhận được lên khung chat
        public void ShowMessage(string message)
        {
            int pIdx = MainForm.Socketer.IsServer ? 1 : 0;
            string name = MainForm.ChessBoard.Players[pIdx].Name;

            TxtChat.AppendText(string.Format("{0}: {1}\r\n\r\n", name, message));
        }

        // Chat
        private void BtnSend_Click(object sender, EventArgs e)
        {
            string message = TxtInput.Text;

            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            TxtInput.Clear();

            TxtChat.AppendText(string.Format("{0}: {1}\r\n\r\n", "You", message));

            // Gửi tin nhắn
            MainForm.Socketer.Send(new SocketData((int)SocketCommand.CHAT, message));
        }

        // Người chơi có thể nhấn phím Enter thay cho việc nhấn phím gửi
        private void TxtInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnSend.PerformClick();
                // Loại bỏ âm thanh khi nhấn phím Enter
                e.SuppressKeyPress = true;
            }
        }

    }
}
