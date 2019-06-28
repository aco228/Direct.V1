using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Direct.Core
{
	public abstract class DirectDatabaseBase : IDirectDatabase
	{
		private Action<string> _onFatalAction = null;

		protected string _databaseName = string.Empty;
		protected string _databaseScheme = string.Empty;
		protected int _timeout = -1;
    protected DirectTransactionalManager _transactionalManager = null;

		protected bool _error = false;
		protected bool _connected = false;
		protected string _connectionString = string.Empty;
		protected string _lastErrorMessage = string.Empty;

		public virtual string Date(DateTime date) { return date.ToString("yyyy-MM-dd HH:mm:ss"); }
		public virtual bool IsConnected { get { return this._connected; } }
		public string DatabaseName { get { return this._databaseName; } }
		public string DatabaseScheme { get { return this._databaseScheme; } }
    public virtual string CurrentDateQueryString { get { return string.Empty; } }
    public virtual string QueryScopeID { get { return string.Empty; } }
    public Action<string> OnFatalAction { get { return this._onFatalAction; } set { this._onFatalAction = value; } }
    public DirectTransactionalManager Transactional { get { return this._transactionalManager; } }

		public DirectDatabaseBase(string databaseName, string databaseScheme)
		{
			this._databaseName = databaseName;
			this._databaseScheme = databaseScheme;
      this._transactionalManager = new DirectTransactionalManager(this);
    }
    
		protected abstract bool Connect();
    protected abstract bool Disconnect();

    public Task<bool> ConnectAsync() => Task.Factory.StartNew(() => { return this.Connect(); });
    public Task<bool> DisconnectAsync() => Task.Factory.StartNew(() => { return this.Disconnect(); });

		protected abstract void SetConnectionString(string connectionString);
		protected abstract string ComposeCommand(string command);
		public abstract void SetTimeout(int timeout);
    
    public abstract DataTable Load(string query, params object[] parameters);
		public abstract DataTable Load(string command);
    public Task<DataTable> LoadAsync(string command) => Task.Factory.StartNew(() => { return this.Load(command); });
    public Task<DataTable> LoadAsync(string query, params object[] parameters) => Task.Factory.StartNew(() => { return this.Load(query, parameters); });

    public abstract int? Execute(string query, params object[] parameters);
		public abstract int? Execute(string command);
    public Task<int?> ExecuteAsync(string command) => Task.Factory.StartNew(() => { return this.Execute(command); });
    public Task<int?> ExecuteAsync(string query, params object[] parameters) => Task.Factory.StartNew(() => { return this.Execute(query, parameters); });

    public abstract DirectContainer GetDataTableTemplate(string tableName);
		public abstract void BulkCopy(DirectContainer table, string tableName);

    #region # Load Partial #

    public virtual int? LoadInt(string query, params object[] parameters) { return this.LoadInt(this.Construct(query, parameters)); } 
		public virtual int? LoadInt(string command)
		{
			int result = -1;
			if (Int32.TryParse(this.LoadString(command), out result))
				return result;
			return null;
		}
    public virtual Task<int?> LoadIntAsync(string query, params object[] parameters) => Task.Factory.StartNew(() => { return LoadInt(query, parameters); });
    public virtual Task<int?> LoadIntAsync(string command) => Task.Factory.StartNew(() => { return LoadInt(command); });

    public virtual double? LoadDouble(string query, params object[] parameters) { return this.LoadDouble(this.Construct(query, parameters)); }
    public virtual double? LoadDouble(string command)
		{
			double result = -1;
			if (double.TryParse(this.LoadString(command), out result))
				return result;
			return null;
		}
    public virtual Task<double?> LoadDoubleAsync(string query, params object[] parameters) => Task.Factory.StartNew(() => { return LoadDouble(query, parameters); });
    public virtual Task<double?> LoadDoubleAsync(string command) => Task.Factory.StartNew(() => { return LoadDouble(command); });

		public virtual List<int> LoadArrayInt(string query, params object[] parameters) { return this.LoadArrayInt(this.Construct(query, parameters)); } 
		public virtual List<int> LoadArrayInt(string command)
		{
			List<int> result = new List<int>();
			DataTable table = this.Load(command);
			if (table == null || table.Rows.Count == 0)
				return result;

			foreach (DataRow row in table.Rows)
			{
				int p;
				if (Int32.TryParse(row[0].ToString(), out p))
					result.Add(p);
			}

			return result;
		}
    public virtual Task<List<int>> LoadArrayIntAsync(string query, params object[] parameters) => Task.Factory.StartNew(() => { return LoadArrayInt(query, parameters); });
    public virtual Task<List<int>> LoadArrayIntAsync(string command) => Task.Factory.StartNew(() => { return LoadArrayInt(command); });

    public virtual List<string> LoadArrayString(string query, params object[] parameters) { return this.LoadArrayString(this.Construct(query, parameters)); } 
		public virtual List<string> LoadArrayString(string command)
		{
			List<string> result = new List<string>();
			DataTable table = this.Load(command);
			if (table == null || table.Rows.Count == 0)
				return result;

			foreach (DataRow row in table.Rows)
				result.Add(row[0].ToString());

			return result;
		}
    public virtual Task<List<string>> LoadArrayStringAsync(string query, params object[] parameters) => Task.Factory.StartNew(() => { return LoadArrayString(query, parameters); });
    public virtual Task<List<string>> LoadArrayStringAsync(string command) => Task.Factory.StartNew(() => { return LoadArrayString(command); });

    public virtual bool? LoadBool(string query, params object[] parameters) { return this.LoadBool(this.Construct(query, parameters)); } 
		public virtual bool? LoadBool(string command)
		{
			string result = this.LoadString(command);
			if (string.IsNullOrEmpty(result))
				return null;
			return result.ToLower().Equals("1") || result.ToLower().Equals("true");
		}
    public virtual Task<bool?> LoadBoolAsync(string query, params object[] parameters) => Task.Factory.StartNew(() => { return LoadBool(query, parameters); });
    public virtual Task<bool?> LoadBoolAsync(string command) => Task.Factory.StartNew(() => { return LoadBool(command); });

    public virtual bool LoadBoolean(string query, params object[] parameters) { return this.LoadBoolean(this.Construct(query, parameters)); }
    public virtual bool LoadBoolean(string command)
    {
      string result = this.LoadString(command);
      if (string.IsNullOrEmpty(result))
        return false;
      return result.ToLower().Equals("1") || result.ToLower().Equals("true");
    }
    public virtual Task<bool> LoadBooleanAsync(string query, params object[] parameters) => Task.Factory.StartNew(() => { return LoadBoolean(query, parameters); });
    public virtual Task<bool> LoadBooleanAsync(string command) => Task.Factory.StartNew(() => { return LoadBoolean(command); });

    public virtual Guid? LoadGuid(string query, params object[] parameters) { return this.LoadGuid(this.Construct(query, parameters)); } 
		public virtual Guid? LoadGuid(string command)
		{
			Guid result;
			if (Guid.TryParse(this.LoadString(command), out result))
				return result;
			return null;
    }
    public virtual Task<Guid?> LoadGuidAsync(string query, params object[] parameters) => Task.Factory.StartNew(() => { return LoadGuid(query, parameters); });
    public virtual Task<Guid?> LoadGuidAsync(string command) => Task.Factory.StartNew(() => { return LoadGuid(command); });

    public virtual DateTime? LoadDateTime(string query, params object[] parameters) { return this.LoadDateTime(this.Construct(query, parameters)); } 
		public virtual DateTime? LoadDateTime(string command)
		{
			DateTime result;
			if (DateTime.TryParse(this.LoadString(command), out result))
				return result;
			return null;
    }
    public virtual Task<DateTime?> LoadDateTimeAsync(string query, params object[] parameters) => Task.Factory.StartNew(() => { return LoadDateTime(query, parameters); });
    public virtual Task<DateTime?> LoadDateTimeAsync(string command) => Task.Factory.StartNew(() => { return LoadDateTime(command); });

    public virtual string LoadString(string query, params object[] parameters) { return this.LoadString(this.Construct(query, parameters)); } 
		public virtual string LoadString(string command)
		{
			DataTable table = this.Load(command);
			if (table == null || table.Rows.Count == 0)
				return string.Empty;
			return table.Rows[0][0].ToString();
    }
    public virtual Task<string> LoadStringAsync(string query, params object[] parameters) => Task.Factory.StartNew(() => { return LoadString(query, parameters); });
    public virtual Task<string> LoadStringAsync(string command) => Task.Factory.StartNew(() => { return LoadString(command); });

    public virtual DirectContainer LoadContainer(string query, params object[] parameters) { return this.LoadContainer(this.Construct(query, parameters)); } 
		public virtual DirectContainer LoadContainer(string command)
		{
			DataTable table = this.Load(command);
			return new DirectContainer(table);
    }
    public virtual Task<DirectContainer> LoadContainerAsync(string query, params object[] parameters) => Task.Factory.StartNew(() => { return LoadContainer(query, parameters); });
    public virtual Task<DirectContainer> LoadContainerAsync(string command) => Task.Factory.StartNew(() => { return LoadContainer(command); });

    #endregion

    // Construct query with multiple parameters
    public string Construct(string query, params object[] parameters)
		{
			if(query.ToLower().Trim().StartsWith("insert into") && !query.ToLower().Contains("values"))
			{
				string[] split = query.Split('(');
				if(split.Length == 2)
				{
					split = split[1].Split(',');
					query += " VALUES ( ";
					for (int i = 0; i < split.Length; i++) query += "{" + i + "}" + (i != split.Length - 1 ? "," : "");
					query += " ); ";
				}
			}

			for (int i = 0; i < parameters.Length; i++)
			{
				string fullName = parameters[i] != null ? parameters[i].GetType().FullName : "null";
				string pattern = "{" + i + "}";
				string value = "";

        if (fullName.Equals("Direct.Core.DirectTime"))
          value = this.CurrentDateQueryString;
        if (fullName.Equals("Direct.Core.DirectScopeID"))
          value = this.QueryScopeID;
        else if (fullName.Equals("System.Int32") || fullName.Equals("System.Double"))
					value = parameters[i].ToString();
				else if (fullName.Equals("System.String"))
					value = string.Format("'{0}'", parameters[i].ToString().Replace("'", string.Empty));
				else if (fullName.Equals("null"))
					value = "NULL";
				else if (fullName.Equals("System.DateTime"))
				{
					DateTime? dt = parameters[i] as DateTime?;
					if (dt != null)
						value = this.ConstructDateTimeParam(dt.Value);
				}
				
				query = query.Replace(pattern, value);
			}
			return this.ConstructDatabaseNameAndScheme(query);
		}

		protected virtual string ConstructDatabaseNameAndScheme(string query) => query.Replace("[].", string.Format("{0}.{1}.", this.DatabaseName, this.DatabaseScheme));
		protected virtual string ConstructDateTimeParam(DateTime dt) => string.Format("'{0}'", dt.ToString("yyyy-MM-dd HH:mm:ss:fff"));
	}
}
