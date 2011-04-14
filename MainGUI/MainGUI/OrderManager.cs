

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

namespace mm
{
  class OrderManager
  {
    public class Settings
    {
      public int MinTotalBidSizeTenCent { get; set; }
      public int MinTotalBidSizeFiveCent { get; set; }
      public int MaxAskSizeBuyTriggerFiveCent { get; set; }
      public int MaxAskSizeBuyTriggerTenCent { get; set; }

      public string ToString()
      {
	StringBuilder bld = new StringBuilder();
	bld.AppendLine("MIN TOTAL BID SIZE in 0.05 : " + MinTotalBidSizeFiveCent);
	bld.AppendLine("MIN TOTAL BID SIZE in 0.10 : " + MinTotalBidSizeTenCent);
	return bld.ToString();
      }
    }

    public bool WithinSettings() {
      bool withinRules = true;
      if (market == Market.FIVE_CENT) {
	withinRules = withinRules &&  (totalBidSize[bestBid] >= settings.MinTotalBidSizeFiveCent);
	withinRules = withinRules && (totalAskSize[bestAsk] <= totalBidSize[bestBid] + totalBidSize[bestBid] * settings.MaxAskSizeBuyTriggerFiveCent * 0.01);
      }
      if (market == Market.TEN_CENT) {
	withinRules = withinRules &&  (totalBidSize[bestBid] >= settings.MinTotalBidSizeTenCent);
	withinRules = withinRules && (totalAskSize[bestAsk] <= totalBidSize[bestBid] + totalBidSize[bestBid] * settings.MaxAskSizeBuyTriggerTenCent * 0.01);
 
      }
      else {
	withinRules = false;
      }
     
      return withinRules;
    }
    public Settings settings { get; set; }

    enum Market
    {
      FIVE_CENT,
	TEN_CENT,
	UNKNOWN
	}

    Market market;
    ClientAdapterToolkitApp app = new ClientAdapterToolkitApp();
    RegionalTable lq;
    Thread orderManagerThread;
    Terminal terminal;
    string symbol;
    bool running=false;

    public OrderManager() {
      lq = new RegionalTable(app);
    }

    protected void WriteLineNow(string msg) {
      terminal.Text += msg;
      terminal.Text += "\n";
      Console.WriteLine("B: " + msg);
    }

    protected void WriteNow(string msg) {
      terminal.Text += msg;
      Console.Write("B: " + msg);
    }

    private delegate void TextChanger(string msg);
    

    protected void Write(string fmt, params object[] args)
    {
      string st = string.Format(fmt, args);
      if (terminal.Dispatcher.CheckAccess()) {
	Console.Write("A: " + st);
	terminal.Text += st;
      }
      else {
	terminal.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new TextChanger(this.WriteNow), st);
      }
    }    

    protected void WriteLine(string fmt, params object[] args)
    {
      string st = string.Format(fmt, args);
      if (terminal.Dispatcher.CheckAccess()) {
	Console.WriteLine("A: " + st);
	terminal.Text += st;
	terminal.Text += "\n";
      }
      else {
	terminal.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new TextChanger(this.WriteLineNow), st);
      }
    }    
    
    private void autobidInBackground()
    {
      Console.WriteLine("autobidInBackground");
      string tql = lq.TqlForBidAskTrade(symbol, null, "A","B","C","D","E","I","J","K","M","N","P","Q","W","X","Y");

      lq.WantData(tql, true, true);
      lq.OnRegional += new EventHandler<DataEventArgs<RegionalRecord>>(lq_OnData);
      lq.OnDead += new EventHandler<EventArgs>(lq_OnDead);
      if (!lq.Connected) {
	lq.Start();  // 1 minutes
      }
    }

    void lq_OnDead(object sender, EventArgs e)
    {
      WriteLine("CONNECTION FAILED.");
    }

    class OfferSize : Dictionary<Price?, int?>
    {

    }

    OfferSize totalBidSize = new OfferSize();
    OfferSize totalAskSize = new OfferSize();


    Dictionary<string, RegionalRecord> dataByExchanges = new Dictionary<string,RegionalRecord>();
    Price? bestBid = Price.Zero;
    Price? bestAsk = new Price("9999999");

    private void calculateTotalBidAskSizes() {
      totalBidSize = new OfferSize();
      totalAskSize = new OfferSize();
      bestBid = Price.Zero;
      bestAsk = new Price("9999999");

      foreach (var data in dataByExchanges.Values) {
	if (data.RegionalBid != null) {
	  if (data.RegionalBid > bestBid)
            {
	      bestBid = data.RegionalBid;
            } 
	  if (!totalBidSize.ContainsKey(data.RegionalBid)) totalBidSize[data.RegionalBid] = 0;
	  totalBidSize[data.RegionalBid] += data.RegionalBidsize;
	}
	if (data.RegionalAsk != null) {
          if (data.RegionalAsk < bestAsk)
	    {
              bestAsk = data.RegionalAsk;
	    }
	  if (!totalAskSize.ContainsKey(data.RegionalAsk)) totalAskSize[data.RegionalAsk] = 0;
	  totalAskSize[data.RegionalAsk] += data.RegionalAsksize;
	}
      }
      var spread = bestAsk - bestBid;
      Price? fiveCent = new Price( 5, Basecode.Cents);
      Price? tenCent = new Price(10, Basecode.Cents);
      if (spread == fiveCent) {
	market = Market.FIVE_CENT;
      }
      else if (spread == tenCent) {
	market = Market.TEN_CENT;
      }
      else {
	market = Market.UNKNOWN;
      }
    }

    void lq_OnData(object sender, DataEventArgs<RegionalRecord> args)
    {
      foreach (RegionalRecord data in args) {
	var bld = new StringBuilder();
	dataByExchanges[data.RegionalExchid] = data;
      }

      calculateTotalBidAskSizes();
      /*
	foreach (KeyValuePair<Price?, int?> bidSize in totalBidSize) {
	WriteLine("Bid size @ {0} is {1}", bidSize.Key, bidSize.Value);
	}
	foreach (KeyValuePair<Price?, int?> askSize in totalAskSize) {
	WriteLine("Ask size @ {0} is {1}", askSize.Key, askSize.Value);
	}
      */
      string status;
      if (WithinSettings())
	{
	  status = "within rules, will place an order.";
	}
      else {
	status = "not within rules.  still watching";
      }
      StringBuilder line = new StringBuilder();
      line.Append(String.Format("{0:H:mm:ss,12}|", DateTime.Now));
      line.Append(String.Format("{0,8}|", totalBidSize[bestBid]));
      line.Append(String.Format("{0,8}|", totalAskSize[bestAsk]));
      if (market == Market.FIVE_CENT)
      {
          line.Append(String.Format("{0,5}|", 0.05));
      }
      else if (market == Market.TEN_CENT)
      {
          line.Append(String.Format("{0,5}|", 0.1));
      }
      else
      {
          line.Append(String.Format("{0,5}|", "?"));
      }
      line.Append(status);
      WriteLine(line.ToString());
    }

    public void autobid(Terminal aterminal, string symbol)
    {
      this.terminal = aterminal;
      this.symbol = symbol;
      aterminal.Clear();
      this.running=true;
      StringBuilder header = new StringBuilder();
      header.Append(String.Format("{0,12}|", "TIME"));
      header.Append(String.Format("{0,8}|", "BID SZ"));
      header.Append(String.Format("{0,8}|", "ASK SZ"));
      header.Append(String.Format("{0,5}|", "MRKT"));
      header.Append(String.Format("{0,8}|", "STATUS"));
      WriteLine(header.ToString());
      WriteLine("----------------------------------");

      autobidInBackground();
    }

  }
}