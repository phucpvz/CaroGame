using CaroGame.Network;
using CaroGame.Play;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CaroGame
{
    public partial class Form1 : System.Windows.Forms.Form
    {
        // Properties
        private SettingsData mySettings;
        private ChessBoardManager chessBoard;
        private TimeManager time;
        private SocketManager socketManager;
        private ChatManager chatManager;

        private bool gameEnded = false;

        public ChessBoardManager ChessBoard { get => chessBoard; set => chessBoard = value; }
        public TimeManager Time { get => time; set => time = value; }
        public SettingsData MySettings { get => mySettings; set => mySettings = value; }
        public SocketManager Socketer { get => socketManager; set => socketManager = value; }
        public ChatManager Chatter { get => chatManager; set => chatManager = value; }

        // Constructor
        public Form1()
        {
            InitializeComponent();

            // Vì thay đổi giao diện trong thread của Listen() nên phải thêm dòng này
            Control.CheckForIllegalCrossThreadCalls = false;

            MySettings = Controller.DeserializeFromXML() as SettingsData;
            if (MySettings != null)
            {
                // Lấy hình ảnh của người chơi từ đường dẫn đã lưu
                List<Player> players = MySettings.Players;
                for (int i = 0; i < players.Count; i++)
                {
                    players[i].Avatar = Image.FromFile(players[i].ImagePath);
                }
            }
            else
            {
                // Lấy giá trị cài đặt mặc định
                MySettings = new SettingsData();
                MySettings.Players = Constants.Players.Select(p => new Player(p)).ToList();
                MySettings.WaitingTime = Constants.WAITING_TIME;
            }

            // Tạo bộ đếm thời gian
            Time = new TimeManager(this, timerCoolDown, progressBarCoolDown, textBoxTimeLeft);
            Time.Ready();

            // Tạo bàn cờ - người chơi
            ChessBoard = new ChessBoardManager(this, panelChessBoard, textBoxPlayerName, pictureBoxPlayerAvatar);
            ChessBoard.DrawChessBoard();
            ChessBoard.PlayerClicked += ChessBoardManager_PlayerClicked;
            ChessBoard.GameNotEnded += ChessBoard_GameNotEnded;
            ChessBoard.GameEnded += ChessBoard_GameEnded;

            Chatter = new ChatManager(this, textBoxChat, textBoxInput, btnSend);

        }

        private void ChessBoardManager_PlayerClicked(object sender, ButtonClickedEvent e)
        {
            toolStripUndo.Enabled = true;

            // Nếu là chế độ Multiplayer và đang kết nối
            if (Socketer != null && Socketer.Connecting)
            {
                ChessBoard.ChessBoard.Enabled = false;

                // Không được hoàn tác khi vừa đánh xong trong chế độ này
                toolStripUndo.Enabled = false;

                Socketer.Send(new SocketData((int)SocketCommand.SEND_POINT, e.ClickedPoint));
            }
        }

        private void ChessBoard_GameNotEnded(object sender, EventArgs e)
        {
            Time.Restart();
            gameEnded = false;

        }

        private void ChessBoard_GameEnded(object sender, EventArgs e)
        {
            Time.Pause();
            gameEnded = true;

            ChessBoard.ChessBoard.Enabled = false;
            toolStripUndo.Enabled = false;
        }

        private void NewGame()
        {
            Time.Ready();
            toolStripUndo.Enabled = false;
            ChessBoard.CreateNewGame();
        }

        private void Undo()
        {
            if (Socketer == null)
            {
                ChessBoard.UndoAStep();
            }
            else
            {
                ChessBoard.UndoTwoStep();
            }

            // Không có gì để hoàn tác => Bàn cờ trống
            if (ChessBoard.PlayTimeLine.Count == 0)
            {
                toolStripUndo.Enabled = false;
                Time.Ready();
            }
            else
            {
                Time.Restart();
            }
        }

        private void Quit()
        {
            Application.Exit();
        }

        private void toolStripNewGame_Click(object sender, EventArgs e)
        {
            NewGame();
            if (Socketer != null && Socketer.Connecting)
            {
                Socketer.Send(new SocketData((int)SocketCommand.NEW_GAME));
            }
        }

        private void toolStripUndo_Click(object sender, EventArgs e)
        {
            Undo();
            if (Socketer != null && Socketer.Connecting)
            {
                Socketer.Send(new SocketData((int)SocketCommand.UNDO));
            }
        }

        private void toolStripSettings_Click(object sender, EventArgs e)
        {
            // Tạm dừng đếm thời gian
            Time.Pause();

            SettingsBox mySettings = new SettingsBox(this);
            mySettings.ShowDialog();

            // Nếu trò chơi chưa kết thúc thì tiếp tục
            if (!gameEnded)
            {
                Time.Continue();
            }
        }

        private void toolStripQuit_Click(object sender, EventArgs e)
        {
            Quit();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Tạm dừng đếm thời gian
            Time.Pause();

            if (MessageBox.Show("Bạn có chắc muốn thoát khỏi trò chơi không?", "Xác nhận",
                MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.Cancel)
            {
                e.Cancel = true;

                // Nếu trò chơi chưa kết thúc thì tiếp tục
                if (!gameEnded)
                {
                    Time.Continue();
                }
            }
            else
            {
                // Gửi thông báo đã thoát cho người chơi kia
                try
                {
                    if (Socketer == null)
                    {
                        return;
                    }
                    Socketer.Send(new SocketData((int)SocketCommand.QUIT));
                    Socketer.CloseConnection();
                }
                catch (Exception)
                {

                }
            }
        }

        //========================== Multiplayer (LAN) ============================//

        private void toolStripMultiplayerLAN_Click(object sender, EventArgs e)
        {
            if (Socketer != null)
            {
                DialogResult choice = MessageBox.Show("Bạn có chắc muốn thoát khỏi phòng?",
                "Xác nhận", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);

                if (choice == DialogResult.Cancel)
                {
                    return;
                }

                if (Socketer.Connecting)
                {
                    // Gửi thông báo đã thoát khỏi phòng cho phía kia (chỉ khi trong phòng còn người)
                    Socketer.Send(new SocketData((int)SocketCommand.QUIT));
                }

                Socketer.CloseConnection();
                Socketer = null;
                SetTitle("Trò chơi Caro");
            }
            else
            {
                CreateOrJoinRoom();
            }

            toolStripMultiplayerLAN.Checked = !toolStripMultiplayerLAN.Checked;
        }

        private void CreateOrJoinRoom()
        {
            Socketer = new SocketManager(this);
            // Kết nối bằng sóng Wifi
            string ip = Socketer.GetLocalIPv4(NetworkInterfaceType.Wireless80211);
            // Kết nối bằng dây
            if (string.IsNullOrEmpty(ip))
            {
                ip = Socketer.GetLocalIPv4(NetworkInterfaceType.Ethernet);
            }
            Socketer.IP = ip;

            // Nếu chưa có server thì tạo mới
            if (!Socketer.ConnectToServer())
            {
                // Server
                Socketer.CreateServer();
                SetTitle("Trò chơi Caro - Máy chủ");

                MessageBox.Show(string.Format("Đã tạo phòng thành công!\n\n" +
                    "IP Address: {0}\nPort: {1}", Socketer.IP, Socketer.PORT),
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                // Client
                SetTitle("Trò chơi Caro - Máy khách");

                MessageBox.Show(string.Format("Đã tham gia phòng!\n\n" +
                    "IP Address: {0}\nPort: {1}", Socketer.IP, Socketer.PORT),
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Gửi tin kết nối thành công đến server
                Socketer.Send(new SocketData((int)SocketCommand.CONNECTED_SUCCESSFULLY));

                Chatter.Enabled = true;
                Chatter.ClearChat();

                ChessBoard.CreateNewGame();
                ChessBoard.ChessBoard.Enabled = false;
            }

            // Lắng nghe tín hiệu từ phía kia
            Listen();
        }

        // Lắng nghe tín hiệu từ phía kia
        private void Listen()
        {
            Thread listenThread = new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        if (Socketer == null)
                        {
                            return;
                        }

                        SocketData data = (SocketData)Socketer.Receive();

                        // Xử lý dữ liệu nhận được
                        ProcessData(data);
                    }
                    catch (Exception)
                    {

                    }
                }
            });
            listenThread.IsBackground = true;
            listenThread.Start();

        }

        // Xử lý dữ liệu nhận được
        private void ProcessData(SocketData data)
        {
            switch (data.Command)
            {
                case (int)SocketCommand.CONNECTED_SUCCESSFULLY:
                    MessageBox.Show("Đã có máy khách tham gia phòng!",
                        "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Time.Ready();
                    ChessBoard.CreateNewGame();

                    Chatter.Enabled = true;
                    Chatter.ClearChat();
                    break;

                case (int)SocketCommand.CHAT:
                    Chatter.ShowMessage(data.Message);
                    break;

                case (int)SocketCommand.SEND_POINT:
                    // Luồng vẽ của giao diện khác luồng chính của Main đang chạy
                    Invoke((MethodInvoker)(() =>
                    {
                        ChessBoard.Opponent_Click(data.PointData);

                        ChessBoard.ChessBoard.Enabled = true;
                        toolStripUndo.Enabled = true;

                        // Hệ quả: Người bị thua lượt trước sẽ là người được đánh tiếp
                        // Nên tự động tạo game mới
                        if (gameEnded)
                        {
                            toolStripNewGame.PerformClick();
                        }
                    }));
                    break;

                case (int)SocketCommand.NEW_GAME:
                    Invoke((MethodInvoker)(() =>
                    {
                        NewGame();
                        gameEnded = false;
                        ChessBoard.ChessBoard.Enabled = false;
                    }));
                    break;

                case (int)SocketCommand.UNDO:
                    Undo();
                    break;

                case (int)SocketCommand.QUIT:
                    Chatter.Enabled = false;
                    Socketer.CloseConnection();
                    Socketer = null;

                    Time.Pause();
                    ChessBoard.ChessBoard.Enabled = true;

                    DialogResult choice = MessageBox.Show("Người chơi kia đã thoát!\n" +
                        "Bạn muốn ở lại hay rời khỏi phòng?\n\n" +
                        "Chọn OK nếu muốn ở lại (Bạn là chủ phòng).\n" +
                        "Chọn Cancel nếu muốn thoát.",
                        "Thông báo", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                    if (choice == DialogResult.Cancel)
                    {
                        toolStripMultiplayerLAN.Checked = false;
                        SetTitle("Trò chơi Caro");
                        return;
                    }
                    CreateOrJoinRoom();
                    break;

                default:
                    break;
            }

        }

        private void SetTitle(string name)
        {
            Text = name;
        }
    }
}
