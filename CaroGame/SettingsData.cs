using CaroGame.Play;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaroGame
{
    [Serializable]
    public class SettingsData
    {
        private List<Player> players;
        // Đơn vị là milisecond
        private int waitingTime;

        public List<Player> Players { get => players; set => players = value; }
        public int WaitingTime { get => waitingTime; set => waitingTime = value; }
    }
}
