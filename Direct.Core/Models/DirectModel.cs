using Direct.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Direct.Core
{
	public abstract class DirectModel
	{
		internal string TableName { get; set; } = string.Empty;
		internal List<DirectProperties> Properties { get; set; } = null;
		internal DirectDatabaseBase _database = null;
		internal string _propertyNames = string.Empty;

		protected DirectDatabaseBase GetDatabase() => this._database;
		public int ID() => int.Parse(this.GetID());
		public string GetID()
		{
			this.SetProperties();
			if (this.Properties == null || this.Properties.Count == 0)
				throw new Exception("Properties are not OK");
			return this.GetSqlValue(0);
		}
		
		public DirectModel(DirectDatabaseBase database, string tableName)
		{
			this._database = database;
			this.TableName = tableName;
		}

		internal string GetTableName() => this.TableName;
		internal string GetValue(string propertyName)
		{
			var val = this.GetType().GetProperty(propertyName).GetValue(this, null);
			return val == null ? "NULL" : val.ToString();
		}
		internal string GetSqlValue(int propertyID)
		{
			string value = this.GetValue(this.Properties[propertyID].PropetryName);
			if (value.Equals("NULL"))
				return value;

			if (this.Properties[propertyID].Type == DirectPropertyType.String ||
				this.Properties[propertyID].Type == DirectPropertyType.DateTime)
				value = "'" + value + "'";
			else if (this.Properties[propertyID].Type == DirectPropertyType.Bool)
				value = value.Equals("True") ? "1" : "0";
			return value;
		}
		internal void SetProperties()
		{
			if (this.Properties != null)
				return;

			var properties = this.GetType().GetProperties();
			this.Properties = new List<DirectProperties>();
			for (int i = 0; i < properties.Length; i++)
				this.Properties.Add(new DirectProperties(properties[i], (i == 0)));
		}

		/// 
		/// INSERT 
		/// 

		public virtual void Insert() => this._database.Insert(this);
		internal string GetPropertyNamesForInsert()
		{
			if (!string.IsNullOrEmpty(this._propertyNames))
				return this._propertyNames;
			this.SetProperties();
			for (int i = 1; i < this.Properties.Count; i++)
				this._propertyNames += this.Properties.ElementAt(i).PropetryName + (i != this.Properties.Count - 1 ? "," : "");
			return this._propertyNames;
		}
		internal string GetPropertyValuesForInsert()
		{
			string result = "";
			var properties = this.GetType().GetProperties();
			for (int i = 1; i < properties.Length; i++)
				result += this.GetSqlValue(i) + (i != this.Properties.Count - 1 ? "," : "");
			return result;
		}
		internal void SetID(int id)
		{
			if (this.Properties == null || this.Properties.Count == 0)
				return;

			this.GetType().GetProperty(this.Properties.ElementAt(0).PropetryName).SetValue(this, id, null);
		}
		
		/// 
		/// UPDATE
		/// 

		public virtual void Update() => this._database.Update(this);
		internal string GetUpdateData()
		{
			this.SetProperties();
			if (this.Properties == null || this.Properties.Count == 0)
				throw new Exception("Properties are not OK");

			string result = "";
			for (int i = 1; i < this.Properties.Count; i++)
				if(!this.Properties[i].PropetryName.Equals(this.TableName + "ID"))
					result += (result.Equals("") ? "" : ",") + string.Format("{0}={1}", this.Properties.ElementAt(i).PropetryName , this.GetSqlValue(i));
			return result;
		}
		
		/// 
		/// UPDATE ( or not if there are no changes )
		/// 
		
		private Dictionary<string, string> _transactionUpdateObj = null; // Propery name / sql value
		public void BeginTransactionUpdate()
		{
			this.SetProperties();
			this._transactionUpdateObj = new Dictionary<string, string>();
			for (int i = 1; i < this.Properties.Count; i++)
				this._transactionUpdateObj.Add(this.Properties.ElementAt(i).PropetryName, this.GetSqlValue(i));
		}
		public void CommitTransactionUpdate()
		{
			this.SetProperties();
			bool shouldUpdate = false;
			for(int i = 1; i < this.Properties.Count; i++)
			{
				if (!this._transactionUpdateObj.ContainsKey(this.Properties[i].PropetryName))
					throw new Exception("WTF? Some properties are changed in meanwhile");
				if(this._transactionUpdateObj[this.Properties[i].PropetryName] != this.GetSqlValue(i))
				{
					shouldUpdate = true;
					break;
				}
			}

			this._transactionUpdateObj.Clear();
			this._transactionUpdateObj = null;
			if (shouldUpdate)
				this.Update();
		}
		
	}
}
