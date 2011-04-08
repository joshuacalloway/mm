// © Townsend Analytics, Ltd. 2009 All rights reserved.
// 
// Use of the Townsend API, including, but not limited to, all code and 
// documentation, is subject to the Townsend API Terms and Conditions which are 
// posted on www.realtick.com.  The code contained herein is deemed to be part of 
// the "Licensed Product" as referred to in Townsend Analytics end user agreements 
// between the user and Townsend Analytics.
// 
using System;
using RealTick.Api.Application;
using RealTick.Api.Domain;
using RealTick.Api.Domain.Livequote;

namespace mm
{
    class LiveQuoteOneSymbol : Example
    {
        public LiveQuoteOneSymbol()
            : base("Livequote", "request and watch level1", ExampleCategory.MarketData, true)
        {
        }

        const long _maxToDisplay = 20;
        long _numDisplayed = 0;
        long _numIgnored = 0;
        System.Threading.AutoResetEvent _done = new System.Threading.AutoResetEvent(false);
        System.Windows.Controls.TextBox text;


        public override void Run( ToolkitApp app ) {
        
            _numDisplayed = 0;
            _numIgnored = 0;
            using (LiveQuoteTable lq = new LiveQuoteTable(app))
            {
                lq.WantData(lq.TqlForBidAskTrade(_symbol,null), true, true);
                lq.OnData += new EventHandler<DataEventArgs<LivequoteRecord>>(lq_OnData);
                lq.OnDead += new EventHandler<EventArgs>(lq_OnDead);
                lq.Start();

                bool rc = WaitAny(30000, _done);
                if (rc == false)
                    WriteLine("(TIMED OUT OR STOPPED BEFORE {0} TRADES RECEIVED.)", _maxToDisplay);

                WriteLine("DISPLAYED {0} TRADES AND IGNORED {1} QUOTES",
                    _numDisplayed, _numIgnored);
            }
            WriteLine("DONE.");
        }

        void lq_OnDead(object sender, EventArgs e)
        {
            WriteLine("CONNECTION FAILED.");
            _done.Set();
        }

        void lq_OnData(object sender, DataEventArgs<LivequoteRecord> args)
        {
            foreach (LivequoteRecord data in args)
            {
                if (data.Trdprc1 != null)
                {
                    // It's a trade.  Display it (as long as we haven't already displayed our quota.)
                    long n = System.Threading.Interlocked.Increment(ref _numDisplayed);
                    if (n >= _maxToDisplay)
                    {
                        _done.Set();
                    }
                    else
                    {
                        WriteLine("{0} {1} {2}",
                            data.DispName,
                            data.Trdprc1,
                            data.Trdtim1);
                    }
                }
                else
                {
                    // It's a quote.  Ignore it.
                    System.Threading.Interlocked.Increment(ref _numIgnored);
                }
            }
        }


    }

}
