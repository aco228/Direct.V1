using Direct.Core;
using Direct.Core.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Direct.Data
{  
  public class ServiceDireposBase : DirectRepositoryBase<ServiceDireposBase>
  {
    public ServiceDireposBase(DirectContainer dc) : base("Service", "core", MobilePaywallDirect.Instance, dc) { }
    public enum Columns { ServiceID, Name, Description }
  }


  public class ServiceDirepos
  {

  }
}
