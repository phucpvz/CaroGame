using CaroGame.Play;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CaroGame
{
    public partial class SettingsBox : Form
    {
        private Form1 mainForm;
        private List<PictureBox> pictures;
        private List<TextBox> txtNames;

        private List<string> filePaths;

        public Form1 MainForm { get => mainForm; set => mainForm = value; }
        public List<PictureBox> Pictures { get => pictures; set => pictures = value; }
        public List<TextBox> TxtNames { get => txtNames; set => txtNames = value; }

        public SettingsBox(Form1 mainForm)
        {
            InitializeComponent();
            txtNameP1.LostFocus += TxtNameP1_LostFocus;
            txtNameP2.LostFocus += TxtNameP2_LostFocus;

            MainForm = mainForm;
            SettingsData mySettings = MainForm.MySettings;
            List<Player> players = mySettings.Players;
            int waitingTime = mySettings.WaitingTime;

            Pictures = new List<PictureBox>();
            Pictures.Add(pictureBoxP1);
            Pictures.Add(pictureBoxP2);

            TxtNames = new List<TextBox>();
            TxtNames.Add(txtNameP1);
            TxtNames.Add(txtNameP2);

            filePaths = new List<string>();

            for (int i = 0; i < players.Count; i++)
            {
                TxtNames[i].Text = players[i].Name;
                Pictures[i].Image = players[i].Avatar;
                filePaths.Add(players[i].ImagePath);
            }

            numericWaitingTime.Value = waitingTime / Constants.SECOND_TO_MILISECOND;

            openFileDialog = new OpenFileDialog()
            {
                InitialDirectory = @"D:\",
                Title = "Chọn ảnh đại diện của bạn",

                CheckFileExists = true,
                CheckPathExists = true,

                DefaultExt = "png",
                Filter = "PNG files (*.png)|*.png|JPG files (*.jpg)|*.jpg|All files (*.*)|*.*",
                FilterIndex = 3,
                RestoreDirectory = true,

                ReadOnlyChecked = true,
                ShowReadOnly = true
            };

        }

        private void TxtNameP1_LostFocus(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtNameP1.Text))
            {
                return;
            }
            MessageBox.Show("Tên người chơi không được để trống!",
                "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtNameP1.Text = "P1";
        }

        private void TxtNameP2_LostFocus(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtNameP2.Text))
            {
                return;
            }
            MessageBox.Show("Tên người chơi không được để trống!",
                "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtNameP2.Text = "P2";
        }

        private void ChangeAvatar(int idx)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                filePaths[idx] = openFileDialog.FileName;
                Pictures[idx].Image = Image.FromFile(filePaths[idx]);
            }
        }

        // Thay ảnh đại diện P1
        private void btnChangeAvatarP1_Click(object sender, EventArgs e)
        {
            ChangeAvatar(0);
        }
        // Thay ảnh đại diện P2
        private void btnChangeAvatarP2_Click(object sender, EventArgs e)
        {
            ChangeAvatar(1);
        }

        // Đặt lại mặc định
        private void btnDefault_Click(object sender, EventArgs e)
        {
            List<Player> defaultPlayers = Constants.Players;
            for (int i = 0; i < defaultPlayers.Count; i++)
            {
                TxtNames[i].Text = defaultPlayers[i].Name;
                Pictures[i].Image = defaultPlayers[i].Avatar;
                filePaths[i] = defaultPlayers[i].ImagePath;
            }

            numericWaitingTime.Value = 10;
        }

        // Chấp nhận thay đổi
        private void btnOK_Click(object sender, EventArgs e)
        {
            ChessBoardManager chessBoard = MainForm.ChessBoard;
            SettingsData mySettings = MainForm.MySettings;
            List<Player> players = mySettings.Players;

            // Thay đổi các quân cờ tương ứng trên bàn cờ
            Button btn;
            for (int i = 0; i < Constants.CHESS_BOARD_HEIGHT; i++)
            {
                for (int j = 0; j < Constants.CHESS_BOARD_WIDTH; j++)
                {
                    btn = chessBoard.Matrix[i][j];
                    if (btn.BackgroundImage == null)
                    {
                        continue;
                    }

                    for (int k = 0; k < players.Count; k++)
                    {
                        if (btn.BackgroundImage == players[k].Avatar)
                        {
                            btn.BackgroundImage = Pictures[k].Image;
                        }
                    }
                }
            }

            // Thay đổi thông số Players
            Player p;
            for (int i = 0; i < players.Count; i++)
            {
                p = players[i];
                p.Name = TxtNames[i].Text;
                p.ImagePath = filePaths[i];
                p.Avatar = Pictures[i].Image;
            }

            // Cập nhật lại tên và ảnh của người chơi hiện tại
            p = players[chessBoard.CurrentPlayer];
            chessBoard.PlayerName.Text = p.Name;
            chessBoard.PlayerAvatar.Image = p.Avatar;

            // Cập nhật thời gian chờ
            TimeManager time = MainForm.Time;
            mySettings.WaitingTime = time.ProgressBarCoolDown.Maximum 
                = (int)numericWaitingTime.Value * Constants.SECOND_TO_MILISECOND;

            // Lưu thông tin cài đặt xuống file
            Controller.SerializeToXML(mySettings);

            Dispose();
        }
    }
}
