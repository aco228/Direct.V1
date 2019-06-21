using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Direct.Core
{
  public interface IDirectDatabase
  {

    bool IsConnected { get; }
    string DatabaseName { get; }

    void SetTimeout(int timeout);
    DataTable Load(string command);
    int? Execute(string command);
    string Date(DateTime date);

    string LoadString(string command);
    int? LoadInt(string command);
    bool? LoadBool(string command);
    Guid? LoadGuid(string command);
    DateTime? LoadDateTime(string command);
    DirectContainer LoadContainer(string commands);

  }
}
