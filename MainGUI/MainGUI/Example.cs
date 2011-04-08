// © Townsend Analytics, Ltd. 2009 All rights reserved.
// 
// Use of the Townsend API, including, but not limited to, all code and 
// documentation, is subject to the Townsend API Terms and Conditions which are 
// posted on www.realtick.com.  The code contained herein is deemed to be part of 
// the "Licensed Product" as referred to in Townsend Analytics end user agreements 
// between the user and Townsend Analytics.
// 
using System;
using System.Windows.Forms;
using RealTick.Api.Application;
using RealTick.Api.Domain.Order;

namespace mm
{
    /// <summary>
    /// This is the base class from which each of our tutorial examples is derived.  It's set up so that 
    /// the tutorials all look like simple console applications, even as they run within a GUI that 
    /// also permits logging and data exploration.
    /// </summary>
    abstract class Example
    {
        string _label;
        public string Label { get { return _label; } }
        string _table;
        public string Table { get { return _table; } }
        IWin32Window _form;
        public IWin32Window Form { get { return _form; } }
        ExampleCategory _category;
        public ExampleCategory Category { get { return _category; } }
        bool _usesSymbol;
        public bool UsesSymbol { get { return _usesSymbol; } }


        protected bool _usesRoute = false;
        public bool UsesRoute { get { return _usesRoute; } }
        protected bool _testSymbolsOnly = false;
        public bool TestSymbolsOnly { get { return _testSymbolsOnly; } }
        public bool CanStop { get; protected set; }

        public string Route { get; set; }

        public event EventHandler OnRunCompleted;

        public Example(string table, string label, ExampleCategory category, bool usesSymbol)
        {
            CanStop = true;
            _table = table;
            _label = label;
            _category = category;
            _usesSymbol = usesSymbol;
        }

        public abstract void Run( ToolkitApp app );

        public override string ToString()
        {
            return string.Format("{0} -- {1}", Table.ToUpper(), Label);
        }

        delegate void DelWriteLine(string st);
        DelWriteLine _writeLine;

	protected void WriteLineNow(string msg) {
	    _term.Text += msg;
	}

	private delegate void TextChanger(string msg);

        protected void WriteLine(string fmt, params object[] args)
        {
	    if (_term != null && _writeLine != null) {
		string st = string.Format(fmt, args);
		if (_term.Dispatcher.CheckAccess()) {
		    _term.Text += st;
		}
		else {
		    _term.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new TextChanger(this.WriteLineNow), st);
		}
	    }
        }


        // This is our replacement for System.Console.ReadLine; it gets its input
        // from the on-screen terminal emulator.  Because the example is running in a
        // worker thread, we must wait on an event set by the GUI thread.
        protected string ReadLine()
        {
            _enterEvent.WaitOne();
            return "";
        }


	public string _symbol { get; set; }
        Terminal _term;
        EventHandler<EventArgs> _onEnterHandler;
        System.Threading.AutoResetEvent _enterEvent = new System.Threading.AutoResetEvent(false);
        System.Threading.AutoResetEvent _doneEvent = new System.Threading.AutoResetEvent(false);
        System.Threading.AutoResetEvent _stopEvent = new System.Threading.AutoResetEvent(false);


        delegate void ClearTerm();

	private void ClearTerminal() {
	    _term.Clear();
	}

        // Launch a worker thread, and use it to run the current example
        // on the given terminal.  When the thread completes, it marshals back
        // to the GUI thread to call the RunDone notifier below, which cleans
        // everything up.
        public void RunOnTerminal(Terminal term, ToolkitApp app, string symbol)
        {
	   

            _symbol = symbol;
            _term = term;
	    term.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new ClearTerm(this.ClearTerminal));
            
            _onEnterHandler = new EventHandler<EventArgs>(_term_OnEnter);
            _term.OnEnter += _onEnterHandler;
            _runDone = new DelRunDone(RunDone);
            _writeLine = new DelWriteLine(term.WriteLine);
            System.Threading.ThreadPool.QueueUserWorkItem(
                    delegate(object x)
                    {
                        Run( app );
                        _term.Dispatcher.BeginInvoke(_runDone, new object[0] );
                    }
                );
        }

        public void Stop()
        {
            _stopEvent.Set();
        }


        delegate void DelRunDone();
        private DelRunDone _runDone;
        private void RunDone()
        {
            _writeLine = null;
            _term.OnEnter -= _onEnterHandler;
            _term = null;

            EventHandler hnd = OnRunCompleted;
            if (hnd != null)
                hnd(this, EventArgs.Empty);
        }

        void _term_OnEnter(object sender, EventArgs e)
        {
            _enterEvent.Set();
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


        protected void DisplayOrder(OrderRecord ord)
        {
            WriteLine("  --> got event {0}", ord.OrderId );
            WriteLine("      ({0}: {1})", ord.Type, ord.CurrentStatus);
            WriteLine("      ({0} {1} {2} at {3})", ord.Buyorsell, ord.Volume, ord.DispName,
                (ord.Price==null || ord.Price.Value.Numerator == 0) ? "MKT" : ord.Price.Value.ToString() );
            if (!string.IsNullOrEmpty(ord.Reason))
                WriteLine("      ({0})", ord.Reason);
        }

    }


}
