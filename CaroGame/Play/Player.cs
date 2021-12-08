using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CaroGame.Play
{
    public class Player
    {
        #region Properties
        private string name;
        private string imagePath;
        private Image avatar;

        public string Name { get => name; set => name = value; }
        public string ImagePath { get => imagePath; set => imagePath = value; }

        [XmlIgnore]
        public Image Avatar { get => avatar; set => avatar = value; }
        #endregion

        #region Constructor

        public Player()
        {
            Name = "";
            ImagePath = "";
            Avatar = null;
        }

        public Player(string name, string imagePath)
        {
            Name = name;
            ImagePath = imagePath;
            Avatar = Image.FromFile(imagePath);
        }

        public Player(Player p)
        {
            Name = p.Name;
            ImagePath = p.ImagePath;
            Avatar = Image.FromFile(ImagePath);
        }
        #endregion
    }
}
