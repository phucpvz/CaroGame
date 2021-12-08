using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace CaroGame
{
    public class Controller
    {
        private static readonly string filePath = Application.StartupPath + @"\data.xml";

        public static bool SerializeToXML(object data)
        {
            FileStream fs = null;
            try
            {
                fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                XmlSerializer xml = new XmlSerializer(typeof(SettingsData));

                xml.Serialize(fs, data);
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                }
            }
            return false;
        }

        public static object DeserializeFromXML()
        {
            FileStream fs = null;
            object obj = null;

            try
            {
                fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                XmlSerializer xml = new XmlSerializer(typeof(SettingsData));

                obj = xml.Deserialize(fs);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                }
            }
            return obj;
        }
    }
}
