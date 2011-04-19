
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

  class OrderExecutor
  {

    OrderCache cache;
    public OrderExecutor(ClientAdapterToolkitApp app) {
      cache = new OrderCache(app);
    }
    enum State { ConnectionPending, OrderPending, CancelPending, OrderFinished, ConnectionDead };


    public void cancelOrder()
    {
      DisplayOrder(ord);
      if (state == State.OrderPending && ord.Type == "UserSubmitOrder") {
	if (ord.CurrentStatus == "LIVE") {
	  WriteLine("ORDER LIVE -- SUBMITTING CANCEL");
	  state = State.CancelPending;
	  CancelBuilder cxl = new CancelBuilder(ord);
	  cache.SubmitCancel(cxl);
	}
      }
    }


        protected bool WaitAny(int millisecondsTimeout, params System.Threading.WaitHandle[] handles)
        {
            // We are basically just calling System.Threading.WaitHandle.WaitAny on the handle(s) provided,
            // but we also include the example's _stopHandle in the list of handles;  this is an event that
            // gets fired when the user clicks the "Stop" button, allowing us to have a more responsive GUI.
            // In a command-line version of the same example, you would leave that part out.

            System.Threading.WaitHandle[] ar = new System.Threading.WaitHandle[handles.Length + 1];
            ar[0] = _stopEvent;
            for (int i = 0; i < handles.Length; i++)
                ar[i + 1] = handles[i];

            int n = System.Threading.WaitHandle.WaitAny(ar, millisecondsTimeout);
            if (n == System.Threading.WaitHandle.WaitTimeout)
            {
                WriteLine("TIMED OUT WAITING FOR A RESPONSE");
                return false;
            }
            if (n == 0)
            {
                WriteLine("CANCELLED BY USER");
                return false;
            }
            return true;
        }

        System.Threading.AutoResetEvent _enterEvent = new System.Threading.AutoResetEvent(false);
        System.Threading.AutoResetEvent _doneEvent = new System.Threading.AutoResetEvent(false);
        System.Threading.AutoResetEvent _stopEvent = new System.Threading.AutoResetEvent(false);

    public string Route { get; set; }
    State state;
    public void placeOrder(string symbol) {
      OrderBuilder bld = new OrderBuilder(cache);
      state = State.ConnectionPending;
      using (OrderWatcher watch = new OrderWatcher(cache, bld.OrderTag)) {
	cache.Start();
	while (state != State.ConnectionDead && state != State.OrderFinished) {
          if (!WaitAny(10000, watch.WaitHandles) ) {
	    WriteLine("TIMED OUT WAITING FOR RESPONSE");
	    break;
          }
	  
          OrderWatcher.Action action;
          while (watch.GetNext(out action, out ord)) {
	    switch (action) {
	    case OrderWatcher.Action.Live:
	      if (state == State.ConnectionPending) {
		WriteLine("SUBMITTING ORDER");
		state = State.OrderPending;
		
		bld.SetAccount(null, "TEST", null, null);
		bld.SetBuySell(OrderBuilder.BuySell.BUY);
		bld.SetExpiration(OrderBuilder.Expiration.DAY);
		bld.SetRoute(Route);
		bld.SetSymbol(symbol, "NYS", OrderBuilder.SecurityType.STOCKOPT);
		bld.SetVolume(1);
		bld.SetPriceMarket();
		// We set a market price (instead of a limit) to ensure that for
		// this example we don't get a rejection due to the order being out
		// of the bid/ask range.  However we are trying to demonstrate an order
		// that we cancel before it trades, so we must ensure it won't trade.
		// An easy way to do this is by making it a good-from order that won't
		// go live anytime soon:
		bld.SetGoodFrom(DateTime.Now.AddMinutes(60));
		cache.SubmitOrder(bld);
	      }
	      break;
	    case OrderWatcher.Action.Dead:
	      WriteLine("CONNECTION FAILED");
	      state = State.ConnectionDead;
	      break;
	    case OrderWatcher.Action.Order:
	      DisplayOrder(ord);
	      if( state==State.OrderPending && ord.Type=="UserSubmitOrder" )
		{
		  if( ord.CurrentStatus == "LIVE" )
		    {
		      WriteLine( "ORDER LIVE -- SUBMITTING CANCEL" );
		      state = State.CancelPending;
		      CancelBuilder cxl = new CancelBuilder(ord);
		      cache.SubmitCancel(cxl);
		    }
		  else if( ord.CurrentStatus=="COMPLETED" || ord.CurrentStatus=="DELETED")
		    {
		      WriteLine("ORDER UNEXPECTEDLY FINISHED" );
		      state = State.OrderFinished;
		    }
		}
	      else if (state == State.CancelPending)
		{
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
      WriteLine("DONE");
    }

    protected void WriteLine(string fmt, params object[] args)
    {
      string st = String.Format(fmt, args);
      MessageAppEx.LogSev(Severity.Info, st);
    }

    protected void DisplayOrder(OrderRecord ord)
    {
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