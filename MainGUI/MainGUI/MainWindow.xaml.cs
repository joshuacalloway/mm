using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
	    string appPath = System.IO.Path.GetDirectoryName(
							     System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
	    Rule.Source = new Uri(appPath + @"\Rules.xml");
        }


        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
	    string source = Rule.Source.LocalPath;
	    Rule.Document.Save(source);
	    Console.WriteLine("Save Button Clicked " + source);
	}

        private void ruleNameCombBox_KeyDown(object sender, KeyEventArgs e)
        {
            Console.WriteLine("comboBox2_KeyDown");
           // editRuleButton.IsEnabled = true;
        }
    }
}
