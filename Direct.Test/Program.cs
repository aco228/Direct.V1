using Direct.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Direct.Test
{
  class Program
  {
    static void Main(string[] args)
    {
      int a = 1, b = 1;
      a = b = (a + b);
      Console.WriteLine($"{a} {b}");
      Console.ReadKey();
    }

    public static async void Test()
    {
      MobilePaywallDirect db = MobilePaywallDirect.Instance;
      int? id = await db.LoadIntAsync("SELECT TOP 1 ServiceID FROM MobilePaywall.core.Service");
      Console.ReadKey();
    }
  }
}
