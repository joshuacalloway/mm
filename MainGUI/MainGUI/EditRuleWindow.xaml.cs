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
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Rule rule { get; set; }
        public void update()
        {
            comboBox2.Text = rule.name;
        }
        public Window1()
        {
            InitializeComponent();


        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            rule.market = Decimal.Parse(marketComboBox.Text);
            this.Hide();
        }
    }
}
