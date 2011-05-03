
using System;
using RealTick.Api.Application;
using RealTick.Api.ClientAdapter;
using RealTick.Api.Data;
using RealTick.Api.Domain.Livequote;
using RealTick.Api.Domain.Order;
using RealTick.Api.Domain.Regional;
using RealTick.Api.Domain.Ticks;
using RealTick.Api.Domain;
using RealTick.Api.Exceptions;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows;
using System.Xml;

namespace mm
{


    public class Configuration
    {
        static XmlDocument doc = new XmlDocument();

        static Configuration()
        {
            LoadData();
        }

        static void LoadData() {
            string appPath = System.IO.Path.GetDirectoryName(
                                     System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
            doc.Load(appPath + @"\Configuration.xml");
        }

        static public string getValue(string xslpath)
        {
            return doc.SelectSingleNode(xslpath).InnerText;
        }

    }
}