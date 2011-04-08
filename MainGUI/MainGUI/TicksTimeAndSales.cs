// © Townsend Analytics, Ltd. 2009 All rights reserved.
// 
// Use of the Townsend API, including, but not limited to, all code and 
// documentation, is subject to the Townsend API Terms and Conditions which are 
// posted on www.realtick.com.  The code contained herein is deemed to be part of 
// the "Licensed Product" as referred to in Townsend Analytics end user agreements 
// between the user and Townsend Analytics.
// 
using System;
using System.Text;
using RealTick.Api.Application;
using RealTick.Api.Domain;
using RealTick.Api.Domain.Ticks;

using System.Windows.Controls;
using System.Windows.Input;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows;
using System.Windows.Controls;
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


namespace mm
{

    // This example requests tick data and updates in a single symbolVisible, then prints
    // data to the console as it arrives.  Embedded into a GUI program, code 
    // like this could be the basis of a Time and Sales type window.
    class TicksTimeAndSales : Example
    {
        public TicksTimeAndSales() : base( "Ticks", "request time and sales", ExampleCategory.MarketData, true )
        {
        }

//         public event EventHandler<DataEventArgs<string>> OnWriteLine;

// 	protected override void RaiseOnData(string item, IDataBlock block, bool isRequestData)
//         {
//             OnWriteLine.Raise<DataEventArgs<string>>( this, new DataEventArgs<string>( block, isRequestData, item ) );
//         }
// 1

//     }
        System.Windows.Controls.TextBox text;

        public override void Run( ToolkitApp app ) {

        }
        public  void Run( System.Windows.Controls.TextBox atext, ToolkitApp app )
        {
	    text = atext;
            _numDisplayed = 0;
            using (TicksTable table = new TicksTable(app))
            {
                table.WantData( table.TqlForTimeAndSales(_symbol, DateTime.Now, null, null, null, TickType.Bid, TickType.Ask, TickType.Trade), 
                    true, true);
                table.OnTick += new EventHandler<DataEventArgs<TicksRecord>>(table_OnTick);
                table.OnDead += new EventHandler<EventArgs>(table_OnDead);

                table.Start();

                bool rc = WaitAny( 30000, _done);
                if (rc == false)
                    WriteLine("TIMED OUT OR STOPPED BEFORE {0} UPDATES RECEIVED.", _maxBlocksToDisplay);
            }

            WriteLine("DONE.");
        }

        void table_OnDead(object sender, EventArgs e)
        {
            WriteLine("CONNECTION FAILED.");
            _done.Set();
        }


        System.Threading.AutoResetEvent _done = new System.Threading.AutoResetEvent(false);
        long _numDisplayed = 0;
        const int _maxRowsPerBlock = 5;
        const int _maxBlocksToDisplay = 20;
	private delegate void TextChanger(string msg);

	protected void WriteLineNow(string msg) {
	    text.Text += msg;
	}

        protected void WriteLine(string fmt, params object[] args)
        {
	    string st = string.Format(fmt, args);
	    if (text.Dispatcher.CheckAccess()) {
		text.Text += st;
	    }
	    else {
		text.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new TextChanger(this.WriteLineNow), st);
	    }
	    
        }

        void table_OnTick(object sender, DataEventArgs<TicksRecord> e)
        {
            foreach (TicksRecord x in e)
            {
                if (x.TickType == null)
                    continue;
                long n = System.Threading.Interlocked.Increment(ref _numDisplayed);
                if (n > _maxBlocksToDisplay)
                {
                    _done.Set();
                    return;
                }

                // For purposes of this example, we'll only print out 10 rows of the original request
                // data, otherwise the screen gets bogged down.  A real app would use all of it.

                for (int i = 0; i < x.Count && i < _maxRowsPerBlock; i++)
                {
                    switch (x.GetTickType(i))
                    {
                        case BidAskTrade.Bid:
                        case BidAskTrade.Ask:
                            PrintBidAsk(x, i);
                            break;
                        case BidAskTrade.Trade:
                            PrintTrade(x, i);
                            break;
                    }
                }

                if (x.Count > _maxRowsPerBlock)
                    WriteLine("--- OMITTED {0} ROWS ---", x.Count - _maxRowsPerBlock);
            }
        }

        private void PrintBidAsk(TicksRecord x, int idx)
        {
            StringBuilder bld = new StringBuilder();
            bld.AppendFormat("{0}\t{1}\t{2}\t",
               x.TrdDate[idx],
               x.TrdTim1[idx],
               x.GetTickDescription(idx) );

            if( x.GetTickType(idx) == BidAskTrade.Ask )
            {
                bld.AppendFormat( "\t\t\t{0}\t{1}\t{2}",
                    x.TrdPrc1[idx], x.TrdVol1[idx], x.TrdXid1[idx] );
            }
            else
            {
                bld.AppendFormat( "{0}\t{1}\t{2}\t\t\t",
                    x.TrdPrc1[idx], x.TrdVol1[idx], x.TrdXid1[idx] );
            }

            WriteLine("{0}", bld);
        }

        private void PrintTrade(TicksRecord x, int idx)
        {
            WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\n",
                x.TrdDate[idx].ToShortDateString(),
                x.TrdTim1[idx].ToString(),
                x.TrdPrc1[idx].ToString(),
                x.TrdVol1[idx].ToString(),
                x.TrdXid1[idx],
                x.GetTickDescription(idx));
        }


    }
}
