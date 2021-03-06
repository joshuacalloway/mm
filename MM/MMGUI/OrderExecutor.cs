
using RealTick.Api.Application;
using RealTick.Api.ClientAdapter;
using RealTick.Api.Domain.Livequote;
using RealTick.Api.Domain.Order;
using RealTick.Api.Domain.Ticks;
using RealTick.Api.Domain;
using RealTick.Api.Exceptions;
using System.Collections.Generic;
using System.Threading;
using System;
using RealTick.Api.Domain.Regional;
using RealTick.Api.Data;

namespace mm
{

  class OrderExecutor : CanWait
  {
    public new event EventHandler<DataEventArgs<StringEvent>> WriteLineListeners;
    OrderCache cache;
    ClientAdapterToolkitApp app;
    public OrderDirections directions { get; set; }
    State state;
    enum State { ConnectionPending, OrderPending, CancelPending, OrderFinished, ConnectionDead };

    public OrderExecutor(ClientAdapterToolkitApp app) {
      this.app = app;
    }
 
    public void cancelOrder()
    {
      if (ord == null) {
	if (cache != null) cache.Dispose();
	return;
      }
      DisplayOrder(ord);
      if (ord.CurrentStatus == "LIVE") {
	WriteLine("ORDER LIVE -- SUBMITTING CANCEL");
	state = State.CancelPending;
	CancelBuilder cxl = new CancelBuilder(ord);
	cache.SubmitCancel(cxl);
	ord = null;
      }
      cache.Dispose();
    }

    private string getExchange(OrderDirections directions) {
      if (directions.Cbo) {
	WriteLine("Am placing order {0} on Cbo", directions.Symbol);
	return "CBO";
      }
      if (directions.Box) {
	WriteLine("Am placing order {0} on Box", directions.Symbol);
	return "Box";
      }
      if (directions.Ise) {
	WriteLine("Am placing order {0} on Ise", directions.Symbol);
	return "Ise";
      }
      if (directions.Ase) {
	WriteLine("Am placing order {0} on Ase", directions.Symbol);
	return "Ase";
      }
      if (directions.Phs) {
	WriteLine("Am placing order {0} on Phs", directions.Symbol);
	return "Phs";
      }
      WriteLine("don't know what exhange to send order for {0}", directions.Symbol);
      return "Uknown";
    }

    public void placeOrder(OrderDirections directions) {
      cancelOrder();
      cache = new OrderCache(app);
      this.directions = directions;
      OrderBuilder bld = new OrderBuilder(cache);
      state = State.ConnectionPending;
      using (OrderWatcher watch = new OrderWatcher(cache, bld.OrderTag)) {
	cache.Start();
	while (state != State.ConnectionDead && state != State.OrderFinished) {
          if (!WaitAny(10000, watch.WaitHandles) ) {
	    WriteLine("TIMED OUT WaitAny(10000) WAITING FOR RESPONSE");
	    break;
          }
	  OrderWatcher.Action action;
          while (watch.GetNext(out action, out ord)) {
	    switch (action) {
	    case OrderWatcher.Action.Live:
	      if (state == State.ConnectionPending) {
		WriteLine("SUBMITTING ORDER");
		state = State.OrderPending;
		bld.SetAccount(Configuration.getValue("Configuration/Account/Bank"),
			       Configuration.getValue("Configuration/Account/Branch"),
			       Configuration.getValue("Configuration/Account/Customer"),
			       Configuration.getValue("Configuration/Account/Deposit"));
		bld.SetBuySell(OrderBuilder.BuySell.BUY);
		bld.SetExpiration(OrderBuilder.Expiration.DAY);
		bld.SetRoute(directions.Route);
		bld.SetSymbol(directions.Symbol, getExchange(directions), OrderBuilder.SecurityType.STOCKOPT);
		bld.SetVolume(directions.Size);
		bld.SetPriceLimit(directions.LimitPrice);
		if (directions.Simulated) {
		  WriteLine("Am sending simulated order");
		}
		else {
		  WriteLine("Am sending real order");
		  cache.SubmitOrder(bld);
		}
	      }
	      break;
	    case OrderWatcher.Action.Dead:
	      WriteLine("CONNECTION FAILED");
	      state = State.ConnectionDead;
	      break;
	    case OrderWatcher.Action.Order:
	      DisplayOrder(ord);
	      if( state==State.OrderPending && ord.Type=="UserSubmitOrder" ) {
		if( ord.CurrentStatus=="COMPLETED") {
		    WriteLine("Order Completed");
		    state = State.OrderFinished;
		}
		else {
		  WriteLine("Order UNEXPECTEDLY FINISHED" );
		  state = State.OrderFinished;
		}
	      }
	      else if (state == State.CancelPending) {
		if( ord.CurrentStatus=="COMPLETED" || ord.CurrentStatus=="DELETED" )
		  state = State.OrderFinished;
	      }

	      if (ord.Type == "ExchangeTradeOrder")
		WriteLine("GOT FILL FOR {0} {1} AT {2}", ord.Buyorsell, ord.Volume, ord.Price);
	      if (ord.Type == "ExchangeKillOrder")
		WriteLine("GOT KILL");

	      break;
	    } // end switch
	  } // end while getNext
	} // end while state
      } // end using watch
      WriteLine("DONE PLACING ORDER");
    }

    protected void WriteLog(string msg) {
        MessageAppEx.LogSev(Severity.Info, "{0}", msg);       
    }

    protected void WriteLog(string fmt, params object[] args) {
      string st = String.Format(fmt, args);
      WriteLog(st);
    }

    protected override void WriteLine(string fmt, params object[] args)
    {
      fmt += "\n";
      Write(fmt, args);
    }
        
    protected void Write(string fmt, params object[] args)
    {
      string st = string.Format(fmt, args);
      EventHandler<DataEventArgs<StringEvent>> hnd = WriteLineListeners;
      if (hnd != null)
	hnd(this, new DataEventArgs<StringEvent>(new StringEvent(st), true));
      WriteLog(fmt, args);
    }

    protected void DisplayOrder(OrderRecord ord)
    {
      if (ord == null) return;
      WriteLine("  --> got event {0}", ord.OrderId);
      WriteLine("      ({0}: {1})", ord.Type, ord.CurrentStatus);
      WriteLine("      ({0} {1} {2} at {3})", ord.Buyorsell, ord.Volume, ord.DispName,
		(ord.Price == null || ord.Price.Value.Numerator == 0) ? "MKT" : ord.Price.Value.ToString());
      if (!string.IsNullOrEmpty(ord.Reason))
	WriteLine("      ({0})", ord.Reason);
    }
    OrderRecord ord; 
  }
}