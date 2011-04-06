using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using RealTick.Api.Domain.Livequote;
using RealTick.Api.Application;
using RealTick.Api.ClientAdapter;


namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private class TempForm : Form
        {
            LiveQuoteTable table;
            ClientAdapterToolkitApp app;

            public TempForm()
            {
                app = new ClientAdapterToolkitApp();
                table = new LiveQuoteTable(app);
                table.OnData += new EventHandler<RealTick.Api.Domain.DataEventArgs<LivequoteRecord>>(table_OnData);

                table.OnLive += new EventHandler<EventArgs>(table_OnLive);
                table.Start(this);
                Console.WriteLine("TempForm() exit");
            }

            void table_OnLive(object sender, EventArgs e)
            {
                Console.WriteLine("CONNECTED OK");
            }

            // void table_OnDead(object sender, EventArgs e)
            // {
            //     Log("CONNECTION FAILED OR LOST");
            // }

            void table_OnData(object sender,  RealTick.Api.Domain.DataEventArgs<LivequoteRecord> e)
            {
                foreach (LivequoteRecord rec in e)
                {
                    if (rec.Trdprc1 != null)
                        Console.WriteLine("TRADE {0}", rec.Trdprc1);
                       // Log("TRADE {0}", rec.Trdprc1);
                    if (rec.Bid != null)
                        Console.WriteLine("BID {0} ASK {1}", rec.Bid, rec.Ask);
                        //Log("BID {0} ASK {1}", rec.Bid, rec.Ask);
                }
            }

        }

        TempForm form;


        public MainWindow()
        {
            InitializeComponent();
	    string appPath = System.IO.Path.GetDirectoryName(
							     System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
	    Rule.Source = new Uri(appPath + @"\Rules.xml");
	    //	    form = new TempForm();
        }


        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
	    string source = Rule.Source.LocalPath;
	    Rule.Document.Save(source);
	    Console.WriteLine("Save Button Clicked " + source);
	}

        private void ruleNameCombBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            Console.WriteLine("comboBox2_KeyDown");
           // editRuleButton.IsEnabled = true;
        }


    }
}
