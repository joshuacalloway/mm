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
using System;

using mm;

namespace mmgui
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MMWindow : Window
  {

    HashSet<string> recentSymbols = new HashSet<string>();
    OrderManager orderManager = new OrderManager();
    public bool Simulated { get; set; }
    public MMWindow() {
      InitializeComponent();
      string appPath = System.IO.Path.GetDirectoryName(
						       System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
      Rule.Source = new Uri(appPath + @"\Rules.xml");
      optionSymbolComboBox.DataContext = recentSymbols;

    }


    private void cancelButton_Click(object sender, RoutedEventArgs e) {
      orderManager.Cancel();
    }

    private void autobidButton_Click(object sender, RoutedEventArgs e)
    {
      Console.WriteLine("Autobid Button Clicked ");
      OrderManager.Rules rules = new OrderManager.Rules();
      rules.MinTotalBidSizeTenCent = Convert.ToInt32(minTotalBidSizeTenCentTextBox.Text );
      rules.MinTotalBidSizeFiveCent = Convert.ToInt32(minTotalBidSizeFiveCentTextBox.Text);
      rules.MaxAskSizeBuyTriggerFiveCent = Convert.ToInt32(maxAskSizeBuyFiveCentTriggerTextBox.Text);
      rules.MaxAskSizeBuyTriggerTenCent = Convert.ToInt32(maxAskSizeBuyTenCentTriggerTextBox.Text);
      rules.MaxAskPrice = Convert.ToDouble(maxAskPriceTextBox.Text);
      rules.MinCoreExchangeBidSize = Convert.ToInt32(MinCoreExchangeBidSizeTextBox.Text);
      orderManager.rules = rules;

      orderManager.WriteLineListeners += Terminal.OnWriteLine;
      Terminal.Clear();
      OrderDirections directions = new OrderDirections();
      XmlDataProvider xml = (XmlDataProvider)FindName("Rule");
      Simulated = Convert.ToBoolean(xml.Document.SelectSingleNode("Rule/Simulated").InnerText);
      directions.Simulated = Simulated;
      directions.Symbol = optionSymbolComboBox.Text;
      directions.Route = routeComboBox.Text;
      directions.Size = Convert.ToInt32( sizeTextBox.Text );
      directions.Box = boxCheckBox.IsChecked.Value;
      directions.Cbo = cboCheckBox.IsChecked.Value;
      directions.Ise = iseCheckBox.IsChecked.Value;
      directions.Ase = aseCheckBox.IsChecked.Value;
      directions.Phs = phsCheckBox.IsChecked.Value;
            
      orderManager.autobid(directions);

      orderManager.AutobidStatusListeners += UpdateTableStatus;
      recentSymbols.Add(optionSymbolComboBox.Text);
      optionSymbolComboBox.Items.Refresh();
    }

    private bool columnsHidden = false;
    private void hideShowColumnsButton_click(object sender, RoutedEventArgs e)
    {
      if (columnsHidden)
        {
	  BestBidColumn.Width = new System.Windows.GridLength(100.0);
	  BestAskColumn.Width = new System.Windows.GridLength(100.0);
	  columnsHidden = false;
        }
      else
        {
	  BestBidColumn.Width = new System.Windows.GridLength(0.0);
	  BestAskColumn.Width = new System.Windows.GridLength(0.0);
	  columnsHidden = true;
        }
    }

    private void saveButton_Click(object sender, RoutedEventArgs e)
    {
      string source = Rule.Source.LocalPath;
      Rule.Document.Save(source);
      Console.WriteLine("Save Button Clicked " + source);
    }


    public void UpdateTableStatus(object sender, DataEventArgs<AutobidStatus> e)
    {
      foreach (AutobidStatus data in e) {
	if (Dispatcher.CheckAccess()) {
	  UpdateTableStatusNow(data);
	}
	else {
	  Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new UpdateTableStatusDeleg(this.UpdateTableStatusNow), data);
	}
      }
    }
          
    private delegate void UpdateTableStatusDeleg(AutobidStatus data);
    public void UpdateTableStatusNow(AutobidStatus data) {

      StatusTable_Status.Text = data.Status.ToString();
      StatusTable_Symbol.Text = String.Format("{0}", data.Symbol);
      StatusTable_TotalAsk.Text = String.Format("{0}",data.TotalAsk);
      StatusTable_TotalBid.Text = String.Format("{0}",data.TotalBid);
      StatusTable_Time.Text = data.Time;
      StatusTable_BestBid.Text = String.Format("{0}",data.BestBid);
      StatusTable_BestAsk.Text = String.Format("{0}",data.BestAsk);
    }

    private void simulatedCheckBox_Click(object sender, RoutedEventArgs e)
    {
        bool newSimulated = simulatedCheckBox.IsChecked.Value;
	string source = Rule.Source.LocalPath;
	Rule.Document.Save(source);
    } 
    
  }
}