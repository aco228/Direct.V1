using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Direct.Core
{
  public interface IDirectContainer
  {
    bool HasValue { get; }
    string[] ColumnNames { get; }
    DirectContainerRow this[int i] { get; }
    int ColumnCount { get; }
    int RowsCount { get; }
    DirectColumnType GetColumnType(int index);
    DirectColumnType GetColumnType(string columnName);
    IEnumerable<DirectContainerRow> Rows { get; }
    IEnumerable<string> PrintColumn(int columnIndex);
    IEnumerable<string> PrintColumn(string columnname);
    string GetString(string columnName, int depth = 0);
    List<string> GetStringList(string columnName);
    bool? GetBool(string columnName, int depth = 0);
		bool GetBoolean(string columnName, int depth = 0);
    DateTime? GetDate(string columnName, int depth = 0);
    int? GetInt(string columnName, int depth = 0);
    Guid? GetGuid(string columnname, int depth = 0);
    T Convert<T>(int depth = 0);
    List<T> ConvertList<T>();
  }

  public enum DirectColumnType { TypeInt, TypeGuid, TypeDateTime, TypeBit, TypeDataByte, TypeString, Unknown }

  public class DirectContainer : IDirectContainer
  {
    private DataTable _table = null;
    private string[] _columnNames = null;

    public bool HasValue { get { return this._table != null && this._table.Rows.Count != 0; } }

    public string[] ColumnNames
    {
      get
      {
        if (this._columnNames != null)
          return this._columnNames;

        List<string> columnnames = new List<string>();
        for (int i = 0; i < this._table.Columns.Count; i++)
          columnnames.Add(this._table.Columns[i].ColumnName);
        this._columnNames = columnnames.ToArray();

        return this._columnNames;
      }
    }

    public DirectContainerRow this[int i]
    {
      get
      {
        if (i > this.RowsCount)
          return null;
        return new DirectContainerRow(this._table.Rows[i]);
      }
    }

    public DataTable DataTable { get { return this._table; } }
    public int ColumnCount { get { return this._table != null ? this._table.Columns.Count : 0; } }
    public int RowsCount { get { return this._table != null ? this._table.Rows.Count : 0; } }

    public DirectContainer(DataTable table)
    {
      this._table = table;
    }

    // SUMMARY: Get Column index by name
    protected int? GetColumnIndexByName(string columnName)
    {
      int index = -1;
      for (int i = 0; i < this._table.Columns.Count; i++)
        if (this._table.Columns[i].ColumnName.ToLower().Equals(columnName.ToLower()))
        {
          index = i;
          break;
        }

      if (index == -1)
        return null;
      return index;
    }

    public IEnumerable<DirectContainerRow> Rows
    {
      get
      {
        foreach (DataRow row in this._table.Rows)
          yield return new DirectContainerRow(row);
      }
    }

    // SUMMARY: Get type of column
    public DirectColumnType GetColumnType(int index)
    {
      if (index > this._table.Columns.Count)
        return DirectColumnType.Unknown;

      switch (this._table.Columns[index].DataType.Name)
      {
        case "Int32": return DirectColumnType.TypeInt;
        case "String": return DirectColumnType.TypeString;
        case "DateTime": return DirectColumnType.TypeDateTime;
        case "Boolean": return DirectColumnType.TypeBit;
        case "Byte[]": return DirectColumnType.TypeDataByte;
        default: return DirectColumnType.Unknown;
      }
    }

    // SUMMARY: Get type of column
    public DirectColumnType GetColumnType(string columnName)
    {
      int? columnIndex = this.GetColumnIndexByName(columnName);
      if (columnIndex.HasValue)
        return this.GetColumnType(columnIndex.Value);
      return DirectColumnType.Unknown;
    }

    // SUMMARY: Print rows in specific column
    public IEnumerable<string> PrintColumn(int columnIndex)
    {
      foreach (DataRow row in this._table.Rows)
        yield return row[columnIndex].ToString();
    }

    // SUMMARY: Print columns in specific row by column name
    public IEnumerable<string> PrintColumn(string columnname)
    {
      int? columnIndex = this.GetColumnIndexByName(columnname);
      if (!columnIndex.HasValue)
        throw new Exception("Column with name='" + columnname + "' does not exists!");
      return this.PrintColumn(columnIndex.Value);
    }

    // SUMMARY: Get string by Column name and Row count
    public virtual string GetString(string columnName, int depth = 0)
    {
      if (depth > this._table.Rows.Count)
        return string.Empty;

      int? columnIndex = this.GetColumnIndexByName(columnName);
      if (!columnIndex.HasValue)
        return string.Empty;

      if (this._table.Rows.Count == 0)
        return string.Empty;

      return this._table.Rows[depth][columnIndex.Value].ToString();
    }
    public virtual List<string> GetStringList(string columnName)
    {
      List<string> result = new List<string>();

      int? columnIndex = this.GetColumnIndexByName(columnName);
      if (!columnIndex.HasValue)
        return result;

      foreach (DataRow row in this._table.Rows)
        result.Add(row[columnIndex.Value].ToString());

      return result;
    }
    public virtual bool? GetBool(string columnName, int depth = 0)
    {
      bool result;
      if (bool.TryParse(this.GetString(columnName, depth), out result))
        return result;
      return null;
    }
		public virtual bool GetBoolean(string columnName, int depth = 0)
		{
			bool? result = GetBool(columnName, depth);
			return result.HasValue ? result.Value : false;
		}
		public virtual DateTime? GetDate(string columnName, int depth = 0)
    {
      DateTime result;
      if (!DateTime.TryParse(this.GetString(columnName, depth), out result))
        return null;
      return result;
    }
    public virtual int? GetInt(string columnName, int depth = 0)
    {
      int result = -1;
      if (Int32.TryParse(this.GetString(columnName, depth), out result))
        return result;
      return null;
    }
    public virtual Guid? GetGuid(string columnname, int depth = 0)
    {
      Guid result;
      if (Guid.TryParse(this.GetString(columnname, depth), out result))
        return result;
      return null;
    }
		public virtual double? GetDouble(string columnname, int depth = 0)
		{
			double result = 0.0;
			if (double.TryParse(this.GetString(columnname, depth), out result))
				return result;
			return null;
		}


    // SUMMARY: Convert informations into class
    public T Convert<T>(int depth = 0)
    {
      if (this._table == null || depth > this._table.Rows.Count)
        return default(T);

      T temp = (T)Activator.CreateInstance(typeof(T));

      foreach (string column in this.ColumnNames)
      {
				PropertyInfo property = (from p in typeof(T).GetProperties() where p.Name.Equals(column) select p).FirstOrDefault();
        if (property == null || !property.CanWrite)
          continue;
				
        string typename = property.PropertyType.Name;
        try
        {
          switch (typename.ToLower())
          {
						case "string":
              property.SetValue(temp, this.GetString(column, depth));
              break;
						case "int32":
              int? intResult = this.GetInt(column, depth);
              if (intResult.HasValue)
                property.SetValue(temp, intResult.Value);
              break;
						case "datetime":
							DateTime? dateTimeResult = this.GetDate(column, depth);
							if (dateTimeResult.HasValue)
								property.SetValue(temp, dateTimeResult.Value);
							break;
						case "double":
							double? doubleResult = this.GetDouble(column, depth);
							if (doubleResult.HasValue)
								property.SetValue(temp, doubleResult.Value);
							break;
						case "boolean":
							bool? boolResult = this.GetBool(column, depth);
              if (boolResult.HasValue)
                property.SetValue(temp, boolResult.Value);
              break;

						case "nullable`1":
							if (property.PropertyType.FullName.StartsWith("System.Nullable`1[[System.Double"))
							{
								double? doubleNullResult = this.GetDouble(column, depth);
								if (doubleNullResult.HasValue)
									property.SetValue(temp, doubleNullResult.Value);
								break;
							}
							else if(property.PropertyType.FullName.StartsWith("System.Nullable`1[[System.Int32"))
							{
								int? intNullResult = this.GetInt(column, depth);
								if (intNullResult.HasValue)
									property.SetValue(temp, intNullResult.Value);
								break;
							}
							else if(property.PropertyType.FullName.StartsWith("System.Nullable`1[[System.DateTime"))
							{
								DateTime? dateTimeNullResult = this.GetDate(column, depth);
								if (dateTimeNullResult.HasValue)
									property.SetValue(temp, dateTimeNullResult.Value);
								break;
							}
							break;

						default:
              break;
          }
        }
        catch (Exception e)
        {
          int a = 0;
        }

      }

      return temp;
    }

    public List<T> ConvertList<T>()
    {
      List<T> list = new List<T>();
      if (this._table == null || this._table.Rows.Count == 0)
        return list;

      for (int i = 0; i < this._table.Rows.Count; i++)
        list.Add(this.Convert<T>(i));

      return list;
    }

    #region # Convert to Basic list #

    public enum ListConvertType { String, Int, IntNullable, DateTime, DateTimeNullable, Guid, GuidNullable, Bool, Class }
    private ListConvertType GetConvertType(string typeName)
    {
      switch (typeName)
      {
        case "System.String": return ListConvertType.String;
        case "System.Int32": return ListConvertType.Int;
        case "System.Nullable`1[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]": return ListConvertType.IntNullable;
        case "System.DateTime": return ListConvertType.DateTime;
        case "System.Nullable`1[[System.DateTime, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]": return ListConvertType.DateTimeNullable;
        case "System.Guid": return ListConvertType.Guid;
        case "System.Nullable`1[[System.Guid, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]": return ListConvertType.GuidNullable;
        case "System.Boolean": return ListConvertType.Bool;
        default: return ListConvertType.Class;
      }
    }

    public List<string> ConvertToStringList()
    {
      if (this._table == null)
        return new List<string>();

      List<string> result = new List<string>();

      try
      {
        foreach (DataRow row in this._table.Rows)
          result.Add(row[0].ToString());
      }
      catch (Exception e)
      { }

      return result;
    }

    public List<int> ConvertToIntList()
    {
      if (this._table == null)
        return new List<int>();
      List<int> result = new List<int>();

      try
      {
        foreach (DataRow row in this._table.Rows)
        {
          int a;
          if (Int32.TryParse(row[0].ToString(), out a))
            result.Add(a);
        }
      }
      catch (Exception e)
      { }

      return result;
    }

    public List<int?> ConvertToIntNullableList()
    {
      if (this._table == null)
        return new List<int?>();
      List<int?> result = new List<int?>();

      try
      {
        foreach (DataRow row in this._table.Rows)
        {
          int a;
          if (Int32.TryParse(row[0].ToString(), out a))
            result.Add(a);
          else result.Add(null);
        }
      }
      catch (Exception e)
      { }

      return result;
    }

    public List<Guid> ConvertToGuidList()
    {
      if (this._table == null)
        return new List<Guid>();
      List<Guid> result = new List<Guid>();

      try
      {
        foreach (DataRow row in this._table.Rows)
        {
          Guid a;
          if (Guid.TryParse(row[0].ToString(), out a))
            result.Add(a);
        }
      }
      catch (Exception e)
      { }

      return result;
    }

    public List<Guid?> ConvertToGuidNullableList()
    {
      if (this._table == null)
        return new List<Guid?>();
      List<Guid?> result = new List<Guid?>();

      try
      {
        foreach (DataRow row in this._table.Rows)
        {
          Guid a;
          if (Guid.TryParse(row[0].ToString(), out a))
            result.Add(a);
          else
            result.Add(null);
        }
      }
      catch (Exception e)
      { }

      return result;
    }

    public List<DateTime> ConvertToDateTimeList()
    {
      if (this._table == null)
        return new List<DateTime>();
      List<DateTime> result = new List<DateTime>();

      try
      {
        foreach (DataRow row in this._table.Rows)
        {
          DateTime a;
          if (DateTime.TryParse(row[0].ToString(), out a))
            result.Add(a);
        }
      }
      catch (Exception e)
      { }

      return result;
    }

    public List<DateTime?> ConvertToDateTimeNullableString()
    {
      if (this._table == null)
        return new List<DateTime?>();
      List<DateTime?> result = new List<DateTime?>();

      try
      {
        foreach (DataRow row in this._table.Rows)
        {
          DateTime a;
          if (!DateTime.TryParse(row[0].ToString(), out a))
            result.Add(a);
          else
            result.Add(null);
        }
      }
      catch (Exception e)
      { }

      return result;
    }

    public List<bool> ConvertToBoolList()
    {
      if (this._table == null)
        return new List<bool>();
      List<bool> result = new List<bool>();

      try
      {
        foreach (DataRow row in this._table.Rows)
        {
          if (row[0].ToString().Equals(1))
            result.Add(true);
          else
            result.Add(false);
        }
      }
      catch (Exception e)
      { }

      return result;
    }
		

		#endregion

	}
}
