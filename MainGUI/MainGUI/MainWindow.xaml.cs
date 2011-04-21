using System;
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
using System;
using System.Windows.Controls;
using System.Drawing;
using RealTick.Api.Application;
using RealTick.Api.ClientAdapter;
using RealTick.Api.Domain.Livequote;
using RealTick.Api.Domain.Order;
using RealTick.Api.Domain.Ticks;
using RealTick.Api.Domain;
using RealTick.Api.Exceptions;
using System.Collections.Generic;
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
using RealTick.Api.Domain.Regional;
using RealTick.Api.Data;

using mm;

namespace WpfApplication1
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {

      List<string> recentSymbols = new List<string>();
    OrderManager orderManager = new OrderManager();
    
    public MainWindow() {
      InitializeComponent();
      string appPath = System.IO.Path.GetDirectoryName(
						       System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
      Rule.Source = new Uri(appPath + @"\Rules.xml");
      recentSymbols.Add("Hello"); 
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
      Terminal.WriteHeader();
      orderManager.autobid(optionSymbolComboBox.Text);

      orderManager.AutobidStatusListeners += UpdateTableStatus;
      recentSymbols.Add(optionSymbolComboBox.Text);
      optionSymbolComboBox.Items.Refresh();
    }

    private void saveButton_Click(object sender, RoutedEventArgs e)
    {
      string source = Rule.Source.LocalPath;
      Rule.Document.Save(source);
      Console.WriteLine("Save Button Clicked " + source);
    }


    public void UpdateTableStatus(object sender, DataEventArgs<AutobidStatus> e)
    {
      foreach (AutobidStatus data in e)
      {
          if (Dispatcher.CheckAccess())
          {
              UpdateTableStatusNow(data);
          }
          else
          {
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
    } 
    
  }
}
