using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Direct.Core.Snapshot
{
  public abstract class SnapshotObjectBase<T> where T : SnapshotObjectBase<T>
  {
    private static string ReservedChars = "{}|";
    private enum Positions { Name, Type, Value }

    private bool _hasError = false;
    private int _referenceID = -1;
    private string _tableName = string.Empty;
    private string _context = string.Empty;
    private DirectDatabaseBase _database = null;
    private DirectContainer _container = null;

    public bool HasError { get { return this._hasError; } }
    public int ReferenceID { get { return this._referenceID; }  }
    public string Context { get { return this._context; } }
    public string TableName { get { return this._tableName; } }
    public DirectDatabaseBase Database { get { return this._database; } }

    public abstract void Validate();
    
    public SnapshotObjectBase(DirectDatabaseBase database, int reference, string tableName, string context)
    {
      this._database = database;
      this._referenceID = reference;
      this._tableName = tableName;
      this._context = context;
    }
    
    // SUMMARY: Prepare data for database insert
    public override string ToString()
    {
      string result = string.Empty;
      
      foreach (var prop in this.GetType().GetProperties())
      {
        string value = (prop.GetValue(this) != null ? prop.GetValue(this).ToString() : "NULL");
        foreach (char c in SnapshotObjectBase<T>.ReservedChars)
          value = value.Replace(c.ToString(), string.Empty);
        
        string snap = "{[propName]|[type]|[value]}"
          .Replace("[propName]", prop.Name)
          .Replace("[type]", prop.PropertyType.Name)
          .Replace("[value]", value);
        
        result += snap;
      }

      return result;
    }

    public void Insert()
    {

    }

    public DirectContainer Load()
    {
      if (this._container != null)
        return this._container;

      string query = "SELECT * FROM [databaseName].[scheme].Snapshop WHERE ReferenceID=[refID] AND Context='[context]' AND TableName='[tableName]'"
        .Replace("[databaseName]", this._database.DatabaseName)
        .Replace("[scheme]'", this._database.DatabaseScheme)
        .Replace("[tableName]", this._tableName)
        .Replace("[refID]", this._referenceID.ToString())
        .Replace("[context]", this._context);

      this._container = this._database.LoadContainer(query);
      return this._container;
    }
    

    // SUMMARY: Convert data from database data to object
    public static T Convert(string input)
    {
      string[] split = input.Split('}');
      T newT = (T)Activator.CreateInstance(typeof(T));
      
      foreach (string segment in split)
      {
        if (string.IsNullOrEmpty(segment) || segment[0] != '{') continue;
        string segmentValue = segment.Substring(1);
        string[] info = segmentValue.Split('|');

        PropertyInfo property = (from t in newT.GetType().GetProperties() where t.Name.Equals(info[(int)SnapshotObjectBase<T>.Positions.Name]) select t).FirstOrDefault();
        if (property == null)
          continue;

        string value = info[(int)Positions.Value];
        switch (info[(int)Positions.Type])
        {
          case "Int32":
            int int32Value;
            if (!int.TryParse(segmentValue, out int32Value))
              property.SetValue(newT, int32Value);
            break;
          case "String":
            property.SetValue(newT, segmentValue);
            break;
          case "DateTime":
            DateTime dateTimeProp;
            if(DateTime.TryParse(segmentValue, out dateTimeProp))
              property.SetValue(newT, dateTimeProp);
            break;
          case "Boolean":
            bool boolValue = (value.Equals("1") || value.ToLower().Equals("true"));
            property.SetValue(newT, boolValue);
            break;
          default:
            break;
        }
      }

      newT.Validate();
      return newT;
    }
    

  }
}
