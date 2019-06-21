using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Direct.Core.DatabaseTypes
{
  public class DirectDatabaseMsSql : DirectDatabaseBase
  {

    #region #logging#
    private static ILog _log = null;

    protected static ILog Log
    {
      get
      {
        if (DirectDatabaseMsSql._log == null)
          DirectDatabaseMsSql._log = LogManager.GetLogger(typeof(DirectDatabaseMsSql));
        return DirectDatabaseMsSql._log;
      }
    }
		#endregion

		protected object _lockObj = new object();
    protected SqlConnection _connection = null;
    protected SqlCommand _command = null;
    protected SqlDataReader _reader = null;
    protected SqlTransaction _transaction = null;
    
    public DirectDatabaseMsSql(string databaseName, string schemaName)
      : base(databaseName, schemaName)
    {
    }

    protected override void SetConnectionString(string connectionString)
    {
      this._connectionString = connectionString;
    }
    protected override bool Connect()
    {
      this._lastErrorMessage = string.Empty;
      this._error = false;

      if (string.IsNullOrEmpty(this._connectionString))
      {
        this._error = true;
        this._connectionString = "Connection string is empty";
        return this._error;
      }

      try
      {
        this._connection = new SqlConnection(this._connectionString);
        this._connection.Open();
        this._error = false;
        this._connected = true;
        return this._error;
      }
      catch (Exception e)
      {
        Log.Error("DIRECT_DATABASE_FATAL: on Connect", e);
        this._error = true;
        this._lastErrorMessage = e.Message;
				this.OnFatalAction?.Invoke("OnConnection:: " + e.ToString());
        return this._error;
      }
    }
    protected override bool Disconnect()
    {
      this._lastErrorMessage = string.Empty;
      this._error = false;

      if (!this._connected)
      {
        this._error = true;
        this._lastErrorMessage = "Connection is not open";
        return this._error;
      }

      if (string.IsNullOrEmpty(this._connectionString))
      {
        this._error = true;
        this._lastErrorMessage = "Connection string is not set up";
        return this._error;
      }

      try
      {
        this._connection.Close();
        this._connected = false;
        return this._error;
      }
      catch (Exception e)
      {
        Log.Error("DIRECT_DATABASE_FATAL: on Disconect", e);
				this.OnFatalAction?.Invoke("OnDisconnect:: " + e.ToString());
				this._error = true;
        this._lastErrorMessage = e.Message;
        return this._error;
      }
    }
    protected override string ComposeCommand(string command)
    {
      return "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED " + command;
      //return command;
    }
    public override void SetTimeout(int timeout)
    {
      this._timeout = timeout;
    }

		public override System.Data.DataTable Load(string query, params object[] parameters) { return this.Load(this.Construct(query, parameters)); }
		public override System.Data.DataTable Load(string command)
    {
      lock(this._lockObj)
			{
				if (this.Connect()) return null;
				command = this.ComposeCommand(command);
        command = this.ConstructDatabaseNameAndScheme(command);

        try
				{
					this._command = new SqlCommand(command, this._connection);
					this._transaction = this._connection.BeginTransaction();
					this._command.Transaction = this._transaction;
					if (this._timeout != -1)
					{
						if (this._timeout > 30) this._timeout = 30;
						this._command.CommandTimeout = this._timeout;
					}
					SqlDataAdapter adapter = new SqlDataAdapter(this._command);
					DataTable table = new DataTable();
					adapter.Fill(table);

					this._transaction.Commit();
					adapter.Dispose();
					this.Disconnect();
					return table;
				}
				catch (Exception e)
				{
					Log.Error("DIRECT_DATABASE_FATAL: on Load | " + command, e);
					this.OnFatalAction?.Invoke("OnLoad:: " + e.ToString() + ", Command:: " + command);
					this._lastErrorMessage = e.Message;
					return null;
				}
			}
    }

		public override int? Execute(string query, params object[] parameters) { return this.Execute(this.Construct(query, parameters)); }
		public override int? Execute(string command)
    {
      lock(this._lockObj)
			{
				if (string.IsNullOrEmpty(command)) return null;
				if (this.Connect()) return null;
        
        command = this.ConstructDatabaseNameAndScheme(command);

        try
				{
					this._command = new SqlCommand();
					this._command.Connection = this._connection;
					this._command.CommandType = CommandType.Text;
					int? newID = null;

					this._transaction = this._connection.BeginTransaction(IsolationLevel.ReadUncommitted);
					this._command.Transaction = this._transaction;
					if (this._timeout != -1)
					{
						this._command.CommandTimeout = this._timeout;
					}

					this._command.CommandText = command;
					this._command.ExecuteNonQuery();
					this._transaction.Commit();

					#region # get id of inserted object #

					if (command.ToLower().Contains("insert into "))
					{
						this._command.CommandText = "SELECT SCOPE_IDENTITY()";
						SqlDataAdapter adapter = new SqlDataAdapter(this._command);
						DataTable table = new DataTable();
						adapter.Fill(table);
						string result = "";
						if (table != null)
							result = table.Rows[0][0].ToString();
						int insertedID;
						if (Int32.TryParse(result, out insertedID))
							newID = insertedID;
					}

					#endregion

					this.Disconnect();
					return newID;
				}
				catch (Exception e)
				{

					Log.Error("DIRECT_DATABASE_FATAL: on Execute | " + command, e);
					this.OnFatalAction?.Invoke("OnExecute:: " + e.ToString() + ", Command:: " + command);
					this._lastErrorMessage = e.Message;
					return null;
				}
			}
    }
		
    public override DirectContainer GetDataTableTemplate(string tableName)
    {
      DirectContainer dc = this.LoadContainer(string.Format("SELECT TOP 1 * FROM {0}.{1}.{2};", this.DatabaseName, this.DatabaseScheme, tableName));
      if (dc == null)
        return null;

      dc.DataTable.TableName = tableName;
      dc.DataTable.Rows.Clear();

      return dc;
    }
    public override void BulkCopy(DirectContainer table, string tableName)
    {
      if (this.Connect()) return;

      try
      {

        using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(
          this._connection, 
          SqlBulkCopyOptions.TableLock |
          SqlBulkCopyOptions.FireTriggers |
          SqlBulkCopyOptions.UseInternalTransaction, null))
        {
          sqlBulkCopy.DestinationTableName = string.Format("{0}.{1}", this.DatabaseScheme, tableName);
          sqlBulkCopy.BatchSize = 10000;
          foreach (var column in table.DataTable.Columns)
            sqlBulkCopy.ColumnMappings.Add(column.ToString(), column.ToString());
          sqlBulkCopy.WriteToServer(table.DataTable);
        }

        this.Disconnect();
      }
      catch (Exception e)
      {
        Log.Error("DIRECT_DATABASE_FATAL: on BULK COPY | ", e);
				this.OnFatalAction?.Invoke("OnBulkCopy:: " + e.ToString());
				this._lastErrorMessage = e.Message;
        return;
      }
    }

  }
}
