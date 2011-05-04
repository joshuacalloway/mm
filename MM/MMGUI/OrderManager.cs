

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
    public Rules rules { get; set; }
    public OrderDirections directions { get; set; }
    OrderExecutor executor;
 
    enum Market { FIVE_CENT, TEN_CENT, UNKNOWN }
    Market market;
    enum State { Idle, Watching, OrderPlaced };
    State state = State.Idle;
    ClientAdapterToolkitApp app = new ClientAdapterToolkitApp();
    RegionalTable querytable;
    private new event EventHandler<DataEventArgs<StringEvent>> WriteLineListeners;
    public new event EventHandler<DataEventArgs<AutobidStatus>> AutobidStatusListeners;

    public void AddWriteLineListeners(EventHandler<DataEventArgs<StringEvent>> WriteLineListener) {
      WriteLineListeners += WriteLineListener;
      executor.WriteLineListeners += WriteLineListener;
    }

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
      if (market == Market.FIVE_CENT) {
	withinRules = withinRules && (totalBidSize[bestBid] >= rules.MinTotalBidSizeFiveCent);
	WriteLine("FIVE_CENT (totalBidSize[bestBid] >= rules.MinTotalBidSizeFiveCent) : {0}", (totalBidSize[bestBid] >= rules.MinTotalBidSizeFiveCent));

	WriteLog("FIVE_CENT (totalBidSize[bestBid] >= rules.MinTotalBidSizeFiveCent) : {0}", (totalBidSize[bestBid] >= rules.MinTotalBidSizeFiveCent));
	withinRules = withinRules && (totalAskSize[bestAsk] <= totalBidSize[bestBid] + totalBidSize[bestBid] * rules.MaxAskSizeBuyTriggerFiveCent * 0.01);

	WriteLine("FIVE_CENT (totalAskSize[bestAsk] <= totalBidSize[bestBid] + totalBidSize[bestBid] * rules.MaxAskSizeBuyTriggerFiveCent * 0.01) : {0}", (totalAskSize[bestAsk] <= totalBidSize[bestBid] + totalBidSize[bestBid] * rules.MaxAskSizeBuyTriggerFiveCent * 0.01));
	
	WriteLog("FIVE_CENT (totalAskSize[bestAsk] <= totalBidSize[bestBid] + totalBidSize[bestBid] * rules.MaxAskSizeBuyTriggerFiveCent * 0.01) : {0}", (totalAskSize[bestAsk] <= totalBidSize[bestBid] + totalBidSize[bestBid] * rules.MaxAskSizeBuyTriggerFiveCent * 0.01));
      }
      if (market == Market.TEN_CENT) {
	WriteLine("TEN_CENT, (totalBidSize[bestBid]:{0} >= rules.MinTotalBidSizeTenCent):{1} is {2}", totalBidSize[bestBid], rules.MinTotalBidSizeTenCent, (totalBidSize[bestBid] >= rules.MinTotalBidSizeTenCent));
	withinRules = withinRules && (totalBidSize[bestBid] >= rules.MinTotalBidSizeTenCent);

	WriteLog("TEN_CENT, (totalBidSize[bestBid] >= rules.MinTotalBidSizeTenCent) : {0}", (totalBidSize[bestBid] >= rules.MinTotalBidSizeTenCent));
	withinRules = withinRules && (totalBidSize[bestBid] >= rules.MinTotalBidSizeTenCent);
	withinRules = withinRules && (totalAskSize[bestAsk] <= totalBidSize[bestBid] + totalBidSize[bestBid] * rules.MaxAskSizeBuyTriggerTenCent * 0.01);


	WriteLine("TEN_CENT, (totalAskSize[bestAsk] <= totalBidSize[bestBid] + totalBidSize[bestBid] * rules.MaxAskSizeBuyTriggerTenCent * 0.01) : {0}", (totalAskSize[bestAsk] <= totalBidSize[bestBid] + totalBidSize[bestBid] * rules.MaxAskSizeBuyTriggerTenCent * 0.01));

	WriteLog("TEN_CENT, (totalAskSize[bestAsk] <= totalBidSize[bestBid] + totalBidSize[bestBid] * rules.MaxAskSizeBuyTriggerTenCent * 0.01) : {0}", (totalAskSize[bestAsk] <= totalBidSize[bestBid] + totalBidSize[bestBid] * rules.MaxAskSizeBuyTriggerTenCent * 0.01));
      }
      else {
	withinRules = false;
      }

      WriteLine("toDouble(bestAsk) < rules.MaxAskPrice : {0}", (toDouble(bestAsk) < rules.MaxAskPrice));

      WriteLog("toDouble(bestAsk) < rules.MaxAskPrice : {0}", (toDouble(bestAsk) < rules.MaxAskPrice));
      withinRules = withinRules && toDouble(bestAsk) < rules.MaxAskPrice;

      WriteLine("(totalBidSize[bestBid] > rules.MinCoreExchangeBidSize) : {0}", (totalBidSize[bestBid] > rules.MinCoreExchangeBidSize));
      WriteLog("(totalBidSize[bestBid] > rules.MinCoreExchangeBidSize) : {0}", (totalBidSize[bestBid] > rules.MinCoreExchangeBidSize));
      withinRules = withinRules || (totalBidSize[bestBid] > rules.MinCoreExchangeBidSize);
      return withinRules;
    }

    private double toDouble(Price? p)
    {
      return Convert.ToDouble(p.ToString());
    }
    
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

    protected void WriteLog(string msg) {
        MessageAppEx.LogSev(Severity.Info, "{0}", msg);       
    }

    protected void WriteLog(string fmt, params object[] args) {
      string st = String.Format(fmt, args);
      WriteLog(st);
    }

    protected void WriteLine(string fmt, params object[] args)
    {
      fmt += "\n";
      Write(fmt, args);
    }

    void OnConnectionDead(object sender, EventArgs e)
    {
      WriteLine("CONNECTION FAILED.");
      WriteLog("CONNECTION FAILED.");
    }

    class OfferSize : Dictionary<Price?, int?> {}

    OfferSize totalBidSize = new OfferSize();
    OfferSize totalAskSize = new OfferSize();


    Dictionary<string, RegionalRecord> dataByExchanges = new Dictionary<string, RegionalRecord>();
    Price bestBid = Price.Zero;
    Price bestAsk = new Price("9999999");

    private void calculateTotalBidAskSizes()
    {
      totalBidSize = new OfferSize();
      totalAskSize = new OfferSize();
      bestBid = Price.Zero;
      bestAsk = new Price("9999999");

      foreach (var data in dataByExchanges.Values) {
	if (data.RegionalBid != null) {
	  if (data.RegionalBid.HasValue && data.RegionalBid > bestBid) {
	    bestBid = data.RegionalBid.Value;
	  }
	  if (!totalBidSize.ContainsKey(data.RegionalBid)) totalBidSize[data.RegionalBid] = 0;
	  totalBidSize[data.RegionalBid] += data.RegionalBidsize;
	}
	if (data.RegionalAsk != null)
	  {
	    if (data.RegionalAsk.HasValue && data.RegionalAsk < bestAsk)
	      {
		bestAsk = data.RegionalAsk.Value;
	      }
	    if (!totalAskSize.ContainsKey(data.RegionalAsk)) totalAskSize[data.RegionalAsk] = 0;
	    totalAskSize[data.RegionalAsk] += data.RegionalAsksize;
	  }
      }

      market = getMarketFromSpread(bestAsk, bestBid);
      WriteLog("Market for bestAsk {0}, bestBid {1} is {2} : ",  bestAsk, bestBid, market);
    }
    
    private Market getMarketFromSpread(Price? bestAsk, Price? bestBid) {
      var spread = bestAsk - bestBid;
      Market market;
      Price fiveCent = new Price(5, Basecode.Cents);
      Price tenCent = new Price(10, Basecode.Cents);
      if (!spread.HasValue) return market = Market.UNKNOWN;
      if (spread.Value == fiveCent)
	{
	  market = Market.FIVE_CENT;
	}
      else if (spread.Value == tenCent)
	{
	  market = Market.TEN_CENT;
	}
      else
	{
	  market = Market.UNKNOWN;
	}
      return market;
    }

    string time;

    private void updateTime(RegionalRecord rec)
    {
        FieldInformation fi6058 = FieldInformation.GetFieldInformation(6058);
        Field field6058 = rec.GetField(fi6058);
        if (field6058 != null)
        {
            time = field6058.TimeValue.ToString();
        }
        /*
        FieldInformation fiREGIONAL_ASK_TIME = FieldInformation.GetFieldInformation("REGIONAL_ASK_TIME");
        Field fieldREGIONAL_ASK_TIME = data.GetField(fiREGIONAL_ASK_TIME);
        if (fieldREGIONAL_ASK_TIME != null)
        {
            Console.WriteLine("REGIONAL_ASK_TIME = {0}", fieldREGIONAL_ASK_TIME.TimeValue.ToString());
        }
         */
    }

    void OnData(object sender, DataEventArgs<RegionalRecord> args)
    {
      foreach (RegionalRecord data in args) {
	var bld = new StringBuilder();
	dataByExchanges[data.RegionalExchid] = data;
    updateTime(data);
      }
      calculateTotalBidAskSizes();
      placeCancelOrder();
    }
 
    private void placeCancelOrder()
    {
      AutobidStatus status = new AutobidStatus();
      status.Symbol = directions.Symbol;
      
      if (WithinRules() && state == State.Watching) {
          directions.LimitPrice = bestBid;
	  WriteLog("Within rules.. am placing Order");
	  WriteLine("Within rules.. am placing order for {0} at price of {1} ", directions.Symbol, directions.LimitPrice);
	  executor.placeOrder(directions);
	  state = State.OrderPlaced;
      }
      else if (!WithinRules() && state == State.OrderPlaced) {
	WriteLog("NOT Within rules.. cancelling any live orders. still watching."); 
	executor.cancelOrder();
	state = State.Watching;
      }
      status.TotalAsk = totalAskSize[bestAsk].Value;
      status.TotalBid = totalBidSize[bestBid].Value;
      status.BestBid = bestBid;
      status.BestAsk = bestAsk;
      status.Time = time;
      status.Status = state.ToString();
      EventHandler<DataEventArgs<AutobidStatus>> hnd = AutobidStatusListeners;
      if (hnd != null)
	hnd(this, new DataEventArgs<AutobidStatus>(status, true));
  
    }

    public void autobid(OrderDirections directions)
    {
      WriteLog("autobid on directions {0} " + directions.ToString());
      this.directions = directions;
      this.state = State.Watching;
      string tql = querytable.TqlForBidAskTrade(directions.Symbol, null, "A", "B", "C", "D", "E", "I", "J", "K", "M", "N", "P", "Q", "W", "X", "Y");

      querytable.WantData(tql, true, true);
      querytable.OnRegional += new EventHandler<DataEventArgs<RegionalRecord>>(OnData);
      querytable.OnDead += new EventHandler<EventArgs>(OnConnectionDead);
      if (!querytable.Connected) {
	WriteLog("!querytable.Connected, querytable.Start()");
	querytable.Start(); 
      }
    }

    public void Cancel()
    {
      WriteLog("Cancel()");
      querytable.Stop();
      this.state = State.Idle;
    }
  }
}