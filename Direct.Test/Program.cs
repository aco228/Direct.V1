using Direct.Core;
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
      Test();
    }

    public static async void Test()
    {
      MobilePaywallDirect db = MobilePaywallDirect.Instance;
      int? id = db.LoadInt("SELECT TOP 1 ServiceID FROM MobilePaywall.core.Service", DirectTime.Now);
      Console.ReadKey();
    }
  }
}
