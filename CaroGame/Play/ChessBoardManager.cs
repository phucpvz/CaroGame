using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CaroGame.Play
{
    public class ChessBoardManager
    {
        #region Properties
        private Panel chessBoard;
        private List<Player> players;
        private int currentPlayer;
        private TextBox playerName;
        private PictureBox playerAvatar;
        private List<List<Button>> matrix;
        private Stack<PlayInfo> playTimeLine;

        private event EventHandler<ButtonClickedEvent> playerClicked;
        public event EventHandler<ButtonClickedEvent> PlayerClicked
        {
            add { playerClicked += value; }
            remove { playerClicked -= value; }
        }

        private event EventHandler gameNotEnded;
        public event EventHandler GameNotEnded
        {
            add { gameNotEnded += value; }
            remove { gameNotEnded -= value; }
        }

        private event EventHandler gameEnded;
        public event EventHandler GameEnded
        {
            add { gameEnded += value; }
            remove { gameEnded -= value; }
        }

        public Panel ChessBoard { get => chessBoard; set => chessBoard = value; }
        public List<Player> Players { get => players; set => players = value; }
        public int CurrentPlayer { get => currentPlayer; set => currentPlayer = value; }
        public TextBox PlayerName { get => playerName; set => playerName = value; }
        public PictureBox PlayerAvatar { get => playerAvatar; set => playerAvatar = value; }
        public List<List<Button>> Matrix { get => matrix; set => matrix = value; }
        public Stack<PlayInfo> PlayTimeLine { get => playTimeLine; set => playTimeLine = value; }

        #endregion

        #region Constructors
        public ChessBoardManager(Form1 parent, Panel panelChessBoard, TextBox playerName, PictureBox playerAvatar)
        {
            parent.Time.TimeUp += Time_TimeUp; ;

            ChessBoard = panelChessBoard;

            Players = parent.MySettings.Players;

            PlayerName = playerName;
            PlayerAvatar = playerAvatar;

            CurrentPlayer = 0;
            ShowInfo();

            PlayTimeLine = new Stack<PlayInfo>();
        }

        private void Time_TimeUp(object sender, EventArgs e)
        {
            EndGameAsTimeUp();
        }
        #endregion

        #region Methods

        // Trò chơi kết thúc do hết thời gian
        private void EndGameAsTimeUp()
        {
            NotifyGameEnded();

            // Người chơi hiện tại thua, người kia là người thắng!
            CurrentPlayer = (CurrentPlayer == 0) ? 1 : 0;
            MessageBox.Show("Đã hết thời gian chờ!\n\n" +
                string.Format("Người chiến thắng là {0}!", Players[CurrentPlayer].Name),
                "Kết quả", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ShowInfo()
        {
            PlayerName.Text = Players[CurrentPlayer].Name;
            PlayerAvatar.Image = Players[CurrentPlayer].Avatar;
        }

        public void DrawChessBoard()
        {
            Button btnRoot = new Button() { Width = 0, Height = 0, Location = new Point(0, 0) };
            Button oldButton = btnRoot;
            Matrix = new List<List<Button>>();

            for (int i = 0; i < Constants.CHESS_BOARD_HEIGHT; i++)
            {
                Matrix.Add(new List<Button>());
                for (int j = 0; j < Constants.CHESS_BOARD_WIDTH; j++)
                {
                    Button btn = new Button()
                    {
                        Width = Constants.CHESS_WIDTH,
                        Height = Constants.CHESS_HEIGHT,
                        Location = new Point(oldButton.Location.X + oldButton.Width, oldButton.Location.Y),
                        BackgroundImageLayout = ImageLayout.Stretch,
                        Tag = string.Format("{0} {1}", i, j),
                    };

                    btn.Click += Btn_Click;

                    Matrix[i].Add(btn);

                    ChessBoard.Controls.Add(btn);
                    oldButton = btn;
                }

                oldButton = btnRoot;
                oldButton.Width = 0;
                oldButton.Height = 0;
                oldButton.Location = new Point(0, oldButton.Location.Y + Constants.CHESS_HEIGHT);
            }
        }

        private void Btn_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;

            if (btn.BackgroundImage != null)
            {
                return;
            }

            btn.BackgroundImage = Players[CurrentPlayer].Avatar;

            Point clickedPoint = GetChessPoint(btn);
            PlayTimeLine.Push(new PlayInfo(clickedPoint, CurrentPlayer));

            // Gửi thông tin tọa độ được đánh cho phía kia
            if (playerClicked != null)
            {
                playerClicked(this, new ButtonClickedEvent(clickedPoint));
            }

            // Kiểm tra thắng thua
            if (IsVictory(btn))
            {
                EndGameAs5InALine();
            }
            else
            {
                CurrentPlayer = (CurrentPlayer == 0) ? 1 : 0;
                ShowInfo();
                NotifyGameNotEnded();
            }
        }

        private void NotifyGameNotEnded()
        {
            if (gameNotEnded != null)
            {
                gameNotEnded(this, new EventArgs());
            }
        }

        private bool IsVictory(Button btn)
        {
            return WinByHorizontal(btn) || WinByVertical(btn) || WinByMainDiagonal(btn) || WinBySubDiagonal(btn);
        }

        private Point GetChessPoint(Button btn)
        {
            string[] coordinates = btn.Tag.ToString().Split(' ');
            int x = int.Parse(coordinates[1]);
            int y = int.Parse(coordinates[0]);

            Point point = new Point(x, y);
            return point;
        }

        private bool WinByHorizontal(Button btn)
        {
            Point point = GetChessPoint(btn);

            int countLeft = 0;
            for (int i = point.X; i >= 0; i--)
            {
                if (Matrix[point.Y][i].BackgroundImage == btn.BackgroundImage)
                {
                    ++countLeft;
                }
                else
                {
                    break;
                }
            }

            int countRight = 0;
            for (int i = point.X + 1; i < Constants.CHESS_BOARD_WIDTH; i++)
            {
                if (Matrix[point.Y][i].BackgroundImage == btn.BackgroundImage)
                {
                    ++countRight;
                }
                else
                {
                    break;
                }
            }

            return countLeft + countRight == 5;
        }

        private bool WinByVertical(Button btn)
        {
            Point point = GetChessPoint(btn);

            int countTop = 0;
            for (int i = point.Y; i >= 0; i--)
            {
                if (Matrix[i][point.X].BackgroundImage == btn.BackgroundImage)
                {
                    ++countTop;
                }
                else
                {
                    break;
                }
            }

            int countBottom = 0;
            for (int i = point.Y + 1; i < Constants.CHESS_BOARD_HEIGHT; i++)
            {
                if (Matrix[i][point.X].BackgroundImage == btn.BackgroundImage)
                {
                    ++countBottom;
                }
                else
                {
                    break;
                }
            }

            return countTop + countBottom == 5;
        }

        private bool WinByMainDiagonal(Button btn)
        {
            Point point = GetChessPoint(btn);

            int countTopLeft = 0;
            for (int i = 0; i <= point.X; i++)
            {
                if (point.Y - i < 0 || point.X - i < 0)
                {
                    break;
                }

                if (Matrix[point.Y - i][point.X - i].BackgroundImage == btn.BackgroundImage)
                {
                    ++countTopLeft;
                }
                else
                {
                    break;
                }
            }

            int countBottomRight = 0;
            for (int i = 1; i < Constants.CHESS_BOARD_WIDTH - point.X; i++)
            {
                if (point.Y + i >= Constants.CHESS_BOARD_HEIGHT || point.X + i >= Constants.CHESS_BOARD_WIDTH)
                {
                    break;
                }

                if (Matrix[point.Y + i][point.X + i].BackgroundImage == btn.BackgroundImage)
                {
                    ++countBottomRight;
                }
                else
                {
                    break;
                }
            }

            return countTopLeft + countBottomRight == 5;
        }

        private bool WinBySubDiagonal(Button btn)
        {
            Point point = GetChessPoint(btn);

            int countTopRight = 0;
            for (int i = 0; i < Constants.CHESS_BOARD_WIDTH - point.X; i++)
            {
                if (point.Y - i < 0 || point.X + i >= Constants.CHESS_BOARD_WIDTH)
                {
                    break;
                }

                if (Matrix[point.Y - i][point.X + i].BackgroundImage == btn.BackgroundImage)
                {
                    ++countTopRight;
                }
                else
                {
                    break;
                }
            }

            int countBottomLeft = 0;
            for (int i = 1; i <= point.X; i++)
            {
                if (point.Y + i >= Constants.CHESS_BOARD_HEIGHT || point.X - i < 0)
                {
                    break;
                }

                if (Matrix[point.Y + i][point.X - i].BackgroundImage == btn.BackgroundImage)
                {
                    ++countBottomLeft;
                }
                else
                {
                    break;
                }
            }

            return countTopRight + countBottomLeft == 5;
        }

        // Trò chơi kết thúc do đã có 5 quân trên 1 đường
        private void EndGameAs5InALine()
        {
            NotifyGameEnded();

            MessageBox.Show("Đã có 5 quân trên một đường!\n\n" +
                string.Format("Người chiến thắng là {0}!", Players[CurrentPlayer].Name),
                "Kết quả", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // Thông báo trò chơi đã kết thúc
        private void NotifyGameEnded()
        {
            if (gameEnded != null)
            {
                gameEnded(this, new EventArgs());
            }
        }

        // Tạo game mới
        public void CreateNewGame()
        {
            PlayTimeLine.Clear();

            Button btn;
            for (int i = 0; i < Matrix.Count; i++)
            {
                for (int j = 0; j < Matrix[i].Count; j++)
                {
                    btn = Matrix[i][j];
                    btn.BackgroundImage = null;
                }
            }

            CurrentPlayer = 0;
            ShowInfo();

            ChessBoard.Enabled = true;
        }

        // Hoàn tác 2 lần (chế độ 2 người chơi)
        public bool UndoTwoStep()
        {
            return UndoAStep() && UndoAStep();
        }

        // Hoàn tác 1 lần
        public bool UndoAStep()
        {
            if (PlayTimeLine.Count == 0)
            {
                return false;
            }

            PlayInfo oldPlayInfo = PlayTimeLine.Pop();

            Button btn = Matrix[oldPlayInfo.PlayPoint.Y][oldPlayInfo.PlayPoint.X];
            btn.BackgroundImage = null;

            CurrentPlayer = oldPlayInfo.CurrentPlayer;
            ShowInfo();

            return true;
        }

        #endregion

        //========================== Multiplayer (LAN) ============================//

        // Trường hợp 2 người chơi, đối thủ đánh
        public void Opponent_Click(Point p)
        {
            Button btn = Matrix[p.Y][p.X];

            if (btn.BackgroundImage != null)
            {
                return;
            }

            btn.BackgroundImage = Players[CurrentPlayer].Avatar;

            PlayTimeLine.Push(new PlayInfo(GetChessPoint(btn), CurrentPlayer));

            // Kiểm tra thắng thua
            if (IsVictory(btn))
            {
                EndGameAs5InALine();
            }
            else
            {
                CurrentPlayer = (CurrentPlayer == 0) ? 1 : 0;
                ShowInfo();
                NotifyGameNotEnded();
            }
        }

    }

    public class ButtonClickedEvent: EventArgs
    {
        private Point clickedPoint;

        public Point ClickedPoint { get => clickedPoint; set => clickedPoint = value; }

        public ButtonClickedEvent(Point clickedPoint)
        {
            ClickedPoint = clickedPoint;
        }
    }
}
