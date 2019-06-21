using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Direct.Core.Repository
{
  public abstract class DirectRepositoryBase<T>
  {
    private int _id = -1;
    private string _tableName = string.Empty;
    private string _schemeName = string.Empty;
    private IDirectDatabase _database = null;
    private DirectContainer _container = null;

    public int ID { get { return this._id; } protected set { this._id = value; } }
    public string TableName { get { return this._tableName; } }
    public string SchemeName { get { return this._schemeName; } }

    protected DirectContainer Container { get { return this._container; } }

    public DirectRepositoryBase(string tableName, string schemeName, IDirectDatabase database, DirectContainer container)
    {
      this._tableName = tableName;
      this._schemeName = schemeName;
      this._database = database;
    }

    public T Load(int id)
    {
      string queryCore = "SELECT * FROM {databaseName}{scheme}{table} WHERE {table}ID=" + id + ";";
      queryCore.Replace("{databaseName}", this._database.DatabaseName)
        .Replace("{scheme}", (string.IsNullOrEmpty(this._schemeName) ? "" : "." + this._schemeName + "."))
        .Replace("{table}", this._tableName);
      
      DirectContainer container = this._database.LoadContainer(queryCore);
      if (container == null || !container.HasValue || container.RowsCount == 0)
        return default(T);

      return default(T);
    }

    public void GetBaseInterfacesProperties()
    {
      Type type = typeof(T);
      if (type.GetTypeInfo().ImplementedInterfaces.Count() == 0)
        return;

      Type interfaceType = type.GetTypeInfo().ImplementedInterfaces.ElementAt(0);
      PropertyInfo[] interfaceProperties = this.GetPublicProperties(interfaceType);
      int a = 0;
    }

    private PropertyInfo[] GetPublicProperties(Type type)
    {
      if (type.IsInterface)
      {
        var propertyInfos = new List<PropertyInfo>();

        var considered = new List<Type>();
        var queue = new Queue<Type>();
        considered.Add(type);
        queue.Enqueue(type);
        while (queue.Count > 0)
        {
          var subType = queue.Dequeue();
          foreach (var subInterface in subType.GetInterfaces())
          {
            if (considered.Contains(subInterface)) continue;

            considered.Add(subInterface);
            queue.Enqueue(subInterface);
          }

          var typeProperties = subType.GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance);
          var newPropertyInfos = typeProperties.Where(x => !propertyInfos.Contains(x));

          propertyInfos.InsertRange(0, newPropertyInfos);
        }

        return propertyInfos.ToArray();
      }

      return type.GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance);
    }


  }
}
