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
        Window1 editWindow = new Window1();
        public MainWindow()
        {
            InitializeComponent();
        }

        private void textBox1_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            Rule rule = new Rule();
            rule.name = comboBox2.Text;
            editWindow.rule = rule;
            editWindow.update();
            editWindow.Show();
        //    editWindow.Activate();
        
        }

        private void comboBox2_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            Console.WriteLine("comboBox2_TargetUpdated");
            button2.IsEnabled = true;
        }

        private void comboBox2_TextInput(object sender, TextCompositionEventArgs e)
        {
            Console.WriteLine("comboBox2_TextInput");
            button2.IsEnabled = true;

        }

        private void comboBox2_KeyDown(object sender, KeyEventArgs e)
        {
            Console.WriteLine("comboBox2_KeyDown");
            button2.IsEnabled = true;
        }
    }
}
