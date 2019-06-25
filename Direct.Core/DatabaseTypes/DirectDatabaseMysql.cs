using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Direct.Core.DatabaseTypes
{
  public class DirectDatabaseMysql : DirectDatabaseBase
  {
		protected object lock_obj = new object();
    private MySqlConnection _connection = null;
    private MySqlCommand _command = null;
    private MySqlDataReader _reader = null;
    private MySqlTransaction _transaction = null;

    public override string CurrentDateQueryString => "CURRENT_TIMESTAMP";

    public DirectDatabaseMysql(string databaseName)
      : base(databaseName, string.Empty)
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

      if (this._connected || this._connection != null)
      {
        //this._error = true;
        this._connectionString = "Connection exists";
        return this._error;
      }

      try
      {
        this._connection = new MySqlConnection(this._connectionString);
        this._connection.Open();
        this._error = false;
        this._connected = true;
        return this._error;
      }
      catch (Exception e)
      {
        this._error = false;
        this._lastErrorMessage = e.Message;
      }
      return this._error;
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
        this._lastErrorMessage = "Connection string is empty";
        return this._error;
      }

      try
      {
        this._connection.Close();
        this._connection = null;
        this._connected = false;
        return this._error;
      }
      catch (Exception e)
      {
        this._error = false;
        this._lastErrorMessage = e.Message;
      }
      return this._error;
    }
    protected override string ComposeCommand(string command)
    {
      // return "START TRANSACTION; " + command + " COMMIT;";
      return command;
    }
    public override void SetTimeout(int timeout)
    {
      this._timeout = timeout;
    }

		public override DataTable Load(string query, params object[] parameters) => this.Load(this.Construct(query, parameters));
		public override System.Data.DataTable Load(string command)
    {
      if (this.Connect()) return null;
      command = this.ComposeCommand(command);
      command = this.ConstructDatabaseNameAndScheme(command);

      try
      {
        this._command = new MySqlCommand(command, this._connection);
        this._transaction = this._connection.BeginTransaction(IsolationLevel.ReadCommitted);
        this._command.Transaction = this._transaction;
        if (this._timeout != -1) this._command.CommandTimeout = this._timeout;
        MySqlDataAdapter adapter = new MySqlDataAdapter(this._command);
        DataTable table = new DataTable();
        adapter.Fill(table);

        this._transaction.Commit();
        adapter.Dispose();
        this.Disconnect();
        return table;
      }
      catch (Exception e)
      {
        this._lastErrorMessage = e.Message;
        return null;
      }
    }

		public override int? Execute(string query, params object[] parameters) => this.Execute(this.Construct(query, parameters));
		public override int? Execute(string command)
    {
			lock(this.lock_obj)
			{
				if (this.Connect()) return null;
				command = this.ComposeCommand(command);
        command = this.ConstructDatabaseNameAndScheme(command);

        try
				{
					this._command = new MySqlCommand(command, this._connection);
					this._command.CommandText = command;

					var a = this._command.ExecuteNonQuery();
					long id = this._command.LastInsertedId;
					this._command.Parameters.Clear();

					this.Disconnect();
					return (int)id; //  find way to get newly inserted id
				}
				catch(Exception e)
				{
					this._lastErrorMessage = e.Message;
					return null;
				}
			}
    }

    public override DirectContainer GetDataTableTemplate(string tableName)
    {
      throw new NotImplementedException();
    }
		public override void BulkCopy(DirectContainer table, string tableName)
    {
      throw new NotImplementedException();
    }

    // overrides
    protected override string ConstructDatabaseNameAndScheme(string query) => query.Replace("[]", string.Format("{0}", this.DatabaseName));
		protected override string ConstructDateTimeParam(DateTime dt) => string.Format("'{0}'", dt.ToString("yyyy-MM-dd HH:mm:ss"));

	}
}
