using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaroGame.Network
{
    [Serializable]
    public class SocketData
    {
        private int command;
        private string message;
        private Point pointData;

        public int Command { get => command; set => command = value; }
        public string Message { get => message; set => message = value; }
        public Point PointData { get => pointData; set => pointData = value; }

        public SocketData(int command)
        {
            Command = command;
        }

        public SocketData(int command, string message)
        {
            Command = command;
            Message = message;
        }

        public SocketData(int command, Point pointData)
        {
            Command = command;
            PointData = pointData;
        }

        public SocketData(int command, string message, Point pointData)
        {
            Command = command;
            Message = message;
            PointData = pointData;
        }

    }

    public enum SocketCommand
    {
        CONNECTED_SUCCESSFULLY,
        CHAT,
        SEND_POINT,
        NEW_GAME,
        UNDO,
        QUIT
    }
}
