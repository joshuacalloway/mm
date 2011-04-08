using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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


using RealTick.Api.Domain.Order;

using RealTick.Api.Application;
using RealTick.Api.ClientAdapter;
using System.Windows.Forms;
using RealTick.Api.Exceptions;
using RealTick.Api.Domain.Livequote;

using RealTick.Api.Application;
using RealTick.Api.Domain;
using RealTick.Api.Domain.Livequote;
using RealTick.Api.Domain.Ticks;
using System.Threading;

namespace mm
{
    class OrderManager
    {
	ClientAdapterToolkitApp app = new ClientAdapterToolkitApp();
	//TicksTimeAndSales ticks = new TicksTimeAndSales();
	LiveQuoteOneSymbol ticks = new LiveQuoteOneSymbol();
	Thread orderManagerThread;
	System.Windows.Controls.TextBox text;
	string symbol;

	private void autobidInBackground()
        {
	    Console.WriteLine("autobidInBackground");
	    // ticks.OnWriteLine += new EventHandler<string>(OnWriteLine);
	    ticks.Run(text, app);

	}

        public void autobid(System.Windows.Controls.TextBox text, string symbol)
        {
	    this.text = text;
	    this.symbol = symbol;
	    ticks._symbol=symbol;
	    orderManagerThread = new Thread(new ThreadStart(autobidInBackground));
	    orderManagerThread.Start();
        }

	// private void OnWriteLine(object sender, string line) {
	//     Console.WriteLine(line);
	// } 
    }
}
