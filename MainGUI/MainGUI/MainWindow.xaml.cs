﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows;
//using System.Windows.Controls;
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

using mm;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {


        OrderManager orderManager = new OrderManager();
	//Terminal terminal = new Terminal();

        public MainWindow()
        {
            InitializeComponent();
   	    string appPath = System.IO.Path.GetDirectoryName(
							     System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
	    Rule.Source = new Uri(appPath + @"\Rules.xml");
           // t1.Background
            //terminal.Width = 200;
            //terminal.Height = 600;
      //  TerminalContainerGroupBox. = terminal;
        }

        private void autobidButton_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Autobid Button Clicked ");
            //orderManager.autobid(MonitorTextBox, optionSymbolTextBox.Text);
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
