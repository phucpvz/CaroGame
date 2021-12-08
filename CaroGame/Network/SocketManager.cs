using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CaroGame.Network
{
    public class SocketManager
    {
        private Form1 mainForm;

        #region Both
        public string IP;  // 127.0.0.1
        public int PORT = 9999;
        private const int BUFFER = 1024;
        private bool isServer;
        private bool connecting;

        public Form1 MainForm { get => mainForm; set => mainForm = value; }
        public bool IsServer { get => isServer; set => isServer = value; }
        public bool Connecting { get => connecting; set => connecting = value; }

        public SocketManager(Form1 mainForm)
        {
            MainForm = mainForm;
        }

        public void CloseConnection()
        {
            if (server != null)
            {
                server.Close();
                server = null;
            }

            if (client != null)
            {
                client.Close();
                client = null;
            }

            Connecting = false;
        }

        public bool Send(object data)
        {
            byte[] sendData = SerializeData(data);

            return SendData(client, sendData);
        }

        public object Receive()
        {
            byte[] receiveData = new byte[BUFFER];
            ReceiveData(client, receiveData);

            return DeserializeData(receiveData);
        }

        private bool SendData(Socket target, byte[] data)
        {
            if (target == null)
            {
                return false;
            }
            return target.Send(data) > 0;
        }

        private bool ReceiveData(Socket target, byte[] data)
        {

            if (target == null)
            {
                return false;
            }
            return target.Receive(data) > 0;
        }

        /// <summary>
        /// Nén đối tượng thành mảng byte[]
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public byte[] SerializeData(Object o)
        {
            MemoryStream ms = new MemoryStream();
            BinaryFormatter bf1 = new BinaryFormatter();
            bf1.Serialize(ms, o);
            return ms.ToArray();
        }

        /// <summary>
        /// Giải nén mảng byte[] thành đối tượng object
        /// </summary>
        /// <param name="theByteArray"></param>
        /// <returns></returns>
        public object DeserializeData(byte[] theByteArray)
        {
            MemoryStream ms = new MemoryStream(theByteArray);
            BinaryFormatter bf1 = new BinaryFormatter();
            ms.Position = 0;
            return bf1.Deserialize(ms);
        }

        /// <summary>
        /// Lấy ra IP V4 của card mạng đang dùng
        /// </summary>
        /// <param name="_type"></param>
        /// <returns></returns>
        public string GetLocalIPv4(NetworkInterfaceType _type)
        {
            string output = "";
            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (item.NetworkInterfaceType == _type && item.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            output = ip.Address.ToString();
                        }
                    }
                }
            }
            return output;
        }

        #endregion

        #region Server
        private Socket server;
        public void CreateServer()
        {
            IPEndPoint ipe = new IPEndPoint(IPAddress.Parse(IP), PORT);
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            server.Bind(ipe);
            server.Listen(10);

            IsServer = true;

            Thread acceptClient = new Thread(() =>
            {
                try
                {
                    client = server.Accept();
                    Connecting = true;
                }
                catch (Exception)
                {

                }
            });

            acceptClient.IsBackground = true;
            acceptClient.Start();
        }

        #endregion

        #region Client
        private Socket client;

        public bool ConnectToServer()
        {
            IPEndPoint ipe = new IPEndPoint(IPAddress.Parse(IP), PORT);
            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                client.Connect(ipe);
                IsServer = false;
                Connecting = true;

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion
    }
}
