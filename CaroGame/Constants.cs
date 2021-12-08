using CaroGame.Play;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CaroGame
{
    public class Constants
    {
        // Đơn vị là pixel
        public const int CHESS_WIDTH = 60;
        public const int CHESS_HEIGHT = 60;

        // Đơn vị là ô
        public const int CHESS_BOARD_WIDTH = 10;
        public const int CHESS_BOARD_HEIGHT = 10;

        // Đơn vị là milisecond
        public const int COOL_DOWN_STEP = -100;
        public static int WAITING_TIME = 10000;
        public const int COOL_DOWN_INTERVAL = 100;

        public const int SECOND_TO_MILISECOND = 1000;

        // Cài đặt người chơi mặc định
        public static readonly List<Player> Players = new List<Player>()
            {
                new Player("P1", Application.StartupPath + @"\..\..\Resources\Red X.png"),
                new Player("P2", Application.StartupPath + @"\..\..\Resources\Blue O.png"),
            };
        
    }
}
