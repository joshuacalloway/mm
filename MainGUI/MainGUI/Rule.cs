using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WpfApplication1
{
    public class Rule
    {
        public string name { get; set; }
        public decimal market { get; set; }  // 0.05 or 0.10 cent wide
	
       	public int minTotalBidSize { get; set; }
	public int minSize { get; set; }
	public int maxSize { get; set; }
	public int size { get; set; }
	public int maxAskPrice { get; set; }
	public bool cboe { get; set; }
	public bool ise { get; set; }
	public bool ase { get; set; }
	public bool phs { get; set; }

	public int minCoreExchangeBidSize { get; set; }
	public int maxAskPercBidSizeBuyTrigger { get; set; }
	public decimal extraDegradeBeforeCancel { get; set; }
	public int maxMidPtBeforeCancel { get; set; }
	
	public string route { get; set; }
    }
}
