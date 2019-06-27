using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Direct.Core
{
  public class DirectTransactionalManager
  {
    private DirectDatabaseBase _database = null;
    private List<string> _queries = new List<string>();

    public DirectTransactionalManager(DirectDatabaseBase db)
    {
      this._database = db;
    }

    public void Execute(string query, params object[] parameters) => Execute(this._database.Construct(query, parameters));
    public void Execute(string command) => this._queries.Add(command);

    public void Start() => this._queries = new List<string>();
    public void Run()
    {
      if (this._queries.Count == 0)
        return;

      string mainQuery = "";
      foreach (string query in this._queries)
        mainQuery += query;
      this._database.Execute(mainQuery);
      this._queries = new List<string>();
    }


  }
}
