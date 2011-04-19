

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
    public class Rules
    {
      public int MinTotalBidSizeTenCent { get; set; }
      public int MinTotalBidSizeFiveCent { get; set; }
      public int MaxAskSizeBuyTriggerFiveCent { get; set; }
      public int MaxAskSizeBuyTriggerTenCent { get; set; }
      public double MaxAskPrice { get; set; }

      public int MinCoreExchangeBidSize { get; set; }

      public string ToString()
      {
	StringBuilder bld = new StringBuilder();
	bld.AppendLine("MIN TOTAL BID SIZE in 0.05 : " + MinTotalBidSizeFiveCent);
	bld.AppendLine("MIN TOTAL BID SIZE in 0.10 : " + MinTotalBidSizeTenCent);
	return bld.ToString();
      }
    }

    public bool WithinRules()
    {
      bool withinRules = true;
      if (market == Market.FIVE_CENT)
	{
	  withinRules = withinRules && (totalBidSize[bestBid] >= rules.MinTotalBidSizeFiveCent);
	  MessageAppEx.LogSev(Severity.Trace, "(totalBidSize[bestBid] >= rules.MinTotalBidSizeFiveCent) : {0}", (totalBidSize[bestBid] >= rules.MinTotalBidSizeFiveCent));
	  withinRules = withinRules && (totalAskSize[bestAsk] <= totalBidSize[bestBid] + totalBidSize[bestBid] * rules.MaxAskSizeBuyTriggerFiveCent * 0.01);

	  MessageAppEx.LogSev(Severity.Trace, "(totalAskSize[bestAsk] <= totalBidSize[bestBid] + totalBidSize[bestBid] * rules.MaxAskSizeBuyTriggerFiveCent * 0.01) : {0}", (totalAskSize[bestAsk] <= totalBidSize[bestBid] + totalBidSize[bestBid] * rules.MaxAskSizeBuyTriggerFiveCent * 0.01));


	}
      if (market == Market.TEN_CENT)
	{
	  withinRules = withinRules && (totalBidSize[bestBid] >= rules.MinTotalBidSizeTenCent);
	  withinRules = withinRules && (totalAskSize[bestAsk] <= totalBidSize[bestBid] + totalBidSize[bestBid] * rules.MaxAskSizeBuyTriggerTenCent * 0.01);

	}
      else
	{
	  withinRules = false;
	}
      MessageAppEx.LogSev(Severity.Trace, "toDouble(bestAsk) < rules.MaxAskPrice : {0}", (toDouble(bestAsk) < rules.MaxAskPrice));
      withinRules = withinRules && toDouble(bestAsk) < rules.MaxAskPrice;
      MessageAppEx.LogSev(Severity.Trace, "(totalBidSize[bestBid] > rules.MinCoreExchangeBidSize) : {0}", (totalBidSize[bestBid] > rules.MinCoreExchangeBidSize));
      withinRules = withinRules || (totalBidSize[bestBid] > rules.MinCoreExchangeBidSize);
      return withinRules;
    }
    private double toDouble(Price? p)
    {
      return Convert.ToDouble(p.ToString());
    }
    public Rules rules { get; set; }

    enum Market { FIVE_CENT, TEN_CENT, UNKNOWN }
    Market market;

    enum State { Idle, Watching, OrderPlaced };
    State state = State.Idle;
    ClientAdapterToolkitApp app = new ClientAdapterToolkitApp();
    RegionalTable querytable;
    string symbol;

    public new event EventHandler<DataEventArgs<StringEvent>> WriteLineListeners;

    public OrderManager()
    {
      querytable = new RegionalTable(app);
      executor = new OrderExecutor(app);
    }


    protected void Write(string fmt, params object[] args)
    {
      string st = string.Format(fmt, args);
      EventHandler<DataEventArgs<StringEvent>> hnd = WriteLineListeners;
      if (hnd != null)
	hnd(this, new DataEventArgs<StringEvent>(new StringEvent(st), true));
    }

    protected void WriteLine(string fmt, params object[] args)
    {
      fmt += "\n";
      Write(fmt, args);
    }

    void querytable_OnDead(object sender, EventArgs e)
    {
      WriteLine("CONNECTION FAILED.");
    }

    class OfferSize : Dictionary<Price?, int?>
    {

    }

    OfferSize totalBidSize = new OfferSize();
    OfferSize totalAskSize = new OfferSize();


    Dictionary<string, RegionalRecord> dataByExchanges = new Dictionary<string, RegionalRecord>();
    Price? bestBid = Price.Zero;
    Price? bestAsk = new Price("9999999");

    private void calculateTotalBidAskSizes()
    {
      totalBidSize = new OfferSize();
      totalAskSize = new OfferSize();
      bestBid = Price.Zero;
      bestAsk = new Price("9999999");

      foreach (var data in dataByExchanges.Values)
	{
	  if (data.RegionalBid != null)
	    {
	      if (data.RegionalBid > bestBid)
		{
		  bestBid = data.RegionalBid;
		}
	      if (!totalBidSize.ContainsKey(data.RegionalBid)) totalBidSize[data.RegionalBid] = 0;
	      totalBidSize[data.RegionalBid] += data.RegionalBidsize;
	    }
	  if (data.RegionalAsk != null)
	    {
	      if (data.RegionalAsk < bestAsk)
		{
		  bestAsk = data.RegionalAsk;
		}
	      if (!totalAskSize.ContainsKey(data.RegionalAsk)) totalAskSize[data.RegionalAsk] = 0;
	      totalAskSize[data.RegionalAsk] += data.RegionalAsksize;
	    }
	}
      var spread = bestAsk - bestBid;
      Price? fiveCent = new Price(5, Basecode.Cents);
      Price? tenCent = new Price(10, Basecode.Cents);
      if (spread == fiveCent)
	{
	  market = Market.FIVE_CENT;
	}
      else if (spread == tenCent)
	{
	  market = Market.TEN_CENT;
	}
      else
	{
	  market = Market.UNKNOWN;
	}
    }

    void querytable_OnData(object sender, DataEventArgs<RegionalRecord> args)
    {
      foreach (RegionalRecord data in args)
	{
	  var bld = new StringBuilder();
	  dataByExchanges[data.RegionalExchid] = data;
	}

      calculateTotalBidAskSizes();
      placeCancelOrder();
    }
    OrderExecutor executor;
 
    private void placeCancelOrder()
    {

      if (WithinRules() && state == State.Watching) {
	executor.placeOrder(symbol);
    state = State.OrderPlaced;
      }
      else if (!WithinRules() && state == State.OrderPlaced)
	{
          executor.cancelOrder();
          state = State.Watching;
	}
      StringBuilder line = new StringBuilder();
      line.Append(String.Format("{0,12}|", String.Format("{0:H:mm:ss}", DateTime.Now)));
      line.Append(String.Format("{0,15}|", symbol));
      line.Append(String.Format("{0,8}|", totalBidSize[bestBid]));
      line.Append(String.Format("{0,8}|", totalAskSize[bestAsk]));
      if (market == Market.FIVE_CENT) {
	line.Append(String.Format("{0,5}|", 0.05));
      }
      else if (market == Market.TEN_CENT) {
	line.Append(String.Format("{0,5}|", 0.1));
      }
      else {
	line.Append(String.Format("{0,5}|", "?"));
      }
      line.Append(state);
      WriteLine(line.ToString());
    }

    public void autobid(string symbol)
    {
      this.symbol = symbol;
      this.state = State.Watching;
      string tql = querytable.TqlForBidAskTrade(symbol, null, "A", "B", "C", "D", "E", "I", "J", "K", "M", "N", "P", "Q", "W", "X", "Y");

      querytable.WantData(tql, true, true);
      querytable.OnRegional += new EventHandler<DataEventArgs<RegionalRecord>>(querytable_OnData);
      querytable.OnDead += new EventHandler<EventArgs>(querytable_OnDead);
      if (!querytable.Connected) {
	querytable.Start();  // 1 minutes
      }
    }

    public void Cancel()
    {
      querytable.Stop();
      this.state = State.Idle;
    }
  }
}