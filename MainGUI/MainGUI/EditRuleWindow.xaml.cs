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
using System.Windows.Shapes;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for EditRuleWindow.xaml
    /// </summary>
    public partial class EditRuleWindow : Window
    {
        public EditRuleWindow()
        {
            InitializeComponent();
	    string appPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
	    SavedRules.Source = new Uri(appPath + @"\Rules.xml");

        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
	    string source = SavedRules.Source.LocalPath;
	    SavedRules.Document.Save(source);
	    Console.WriteLine("Save Button Clicked " + source);
            this.Hide();
        }
    }
}
