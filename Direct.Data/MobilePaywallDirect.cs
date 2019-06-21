using Direct.Core.DatabaseTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Direct.Data
{
  public class MobilePaywallDirect : DirectDatabaseMsSql
  {
    private static MobilePaywallDirect _instance = null;

    public MobilePaywallDirect()
      : base("MobilePaywall", "core")
    {
      this.SetConnectionString("Data Source=192.168.11.104;Initial Catalog=MobilePaywall;uid=sa;pwd=m_q-6dGyRwcTf+b;");
    }

    public static MobilePaywallDirect Instance
    {
      get
      {
        if (MobilePaywallDirect._instance != null)
          return MobilePaywallDirect._instance;
        MobilePaywallDirect._instance = new MobilePaywallDirect();
        return MobilePaywallDirect._instance;
      }
    }

  }
}
