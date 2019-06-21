using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Direct.Core
{
  public class DirectContainerRow
  {
    private DataRow _row = null;

    // SUMMARY: Does this row has values at all
    public bool HasValue
    {
      get
      {
        return this._row != null;
      }
    }

    // SUMMARY: Return value by column index
    public string this[int columnIndex]
    {
      get
      {
        if (columnIndex > this._row.Table.Columns.Count)
          return string.Empty;
        return this._row[columnIndex].ToString();
      }
    }

    public DirectContainerRow(DataRow row)
    {
      this._row = row;
    }

    // SUMMARY: Get Column index by name
    protected int? GetColumnIndexByName(string columnName)
    {
      int index = -1;
      for (int i = 0; i < this._row.Table.Columns.Count; i++)
        if (this._row.Table.Columns[i].ColumnName.ToLower().Equals(columnName.ToLower()))
        {
          index = i;
          break;
        }

      if (index == -1)
        return null;
      return index;
    }

    public virtual string GetString(string columnName)
    {
      if (!this.HasValue)
        return string.Empty;

      int? columnIndex = this.GetColumnIndexByName(columnName);
      if (!columnIndex.HasValue)
        return string.Empty;

      //this._row[columnIndex.Value];

      if (this._row[columnIndex.Value].GetType().ToString() == "System.DateTime")
      {
        DateTime? tempDate = this._row[columnIndex.Value] as DateTime?;
        if (tempDate.HasValue)
          return string.Format("{0}-{1}-{2} {3}:{4}:{5}.{6}",
            tempDate.Value.Year, tempDate.Value.Month, tempDate.Value.Day, tempDate.Value.Hour, tempDate.Value.Minute, tempDate.Value.Second, tempDate.Value.Millisecond);
      }

      return this._row[columnIndex.Value].ToString();
    }

    public virtual bool? GetBool(string columnName)
    {
      bool result;
      if (bool.TryParse(this.GetString(columnName), out result))
        return result;
      return null;
    }

    public virtual DateTime? GetDate(string columnName)
    {
      DateTime result;
      if (!DateTime.TryParse(this.GetString(columnName), out result))
        return null;
      return result;
    }

    public virtual int? GetInt(string columnName)
    {
      int result = -1;
      if (Int32.TryParse(this.GetString(columnName), out result))
        return result;
      return null;
    }

    public virtual decimal? GetDecimal(string columnName)
    {
      decimal result = -1;
      if (decimal.TryParse(this.GetString(columnName), out result))
        return result;
      return null;
    }

		public virtual double? GetDouble(string columnName)
		{
			double result = -1;
			if (double.TryParse(this.GetString(columnName), out result))
				return result;
			return null;
		}

    public virtual Guid? GetGuid(string columnname)
    {
      Guid result;
      if (Guid.TryParse(this.GetString(columnname), out result))
        return result;
      return null;
    }

  }
}
