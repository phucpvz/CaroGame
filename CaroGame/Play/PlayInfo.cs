using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaroGame.Play
{
    public class PlayInfo
    {
        private Point playPoint;
        private int currentPlayer;

        public Point PlayPoint { get => playPoint; set => playPoint = value; }
        public int CurrentPlayer { get => currentPlayer; set => currentPlayer = value; }

        public PlayInfo(Point playPoint, int currentPlayer)
        {
            PlayPoint = playPoint;
            CurrentPlayer = currentPlayer;
        }
    }
}
