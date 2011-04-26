
using RealTick.Api.Data;
using System.Text;

namespace mm {

  public class OrderDirections {
    public bool Simulated { get; set; }
    public Price LimitPrice { get; set; }
    public string Symbol { get; set; }
    public string Route { get; set; }
    public int Size { get; set; }
    public bool Cbo { get; set; }
    public bool Box { get; set; }
    public bool Ise { get; set; }
    public bool Ase { get; set; }
    public bool Phs { get; set; }
    public string ToString() {
      StringBuilder ret = new StringBuilder();
      ret.AppendLine("");
      ret.AppendLine("----OrderDirections----");
      ret.AppendLine("Simulated : " + Simulated);
      ret.AppendLine("Symbol : " + Symbol);
      ret.AppendLine("Route : " + Route);
      return ret.ToString();
    }
  }
}