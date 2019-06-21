using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Direct.Core
{
	public static class DirectHelper
	{

		public static string GetDateTime(DateTime date)
		{
			return date.ToString("yyyy-MM-dd HH:mm:ss:fff");
		}

		public static string ConstructQuery(string query, params object[] parameters)
		{
			for (int i = 0; i < parameters.Length; i++)
			{
				string fullName = parameters[i] != null ? parameters[i].GetType().FullName : "null";
				string pattern = "{" + i + "}";
				string value = "";

				if (fullName.Equals("System.Int32") || fullName.Equals("System.Double"))
					value = parameters[i].ToString();
				else if (fullName.Equals("System.String"))
					value = string.Format("'{0}'", parameters[i].ToString());
				else if (fullName.Equals("null"))
					value = "NULL";
				else if (fullName.Equals("System.DateTime"))
				{
					DateTime? dt = parameters[i] as DateTime?;
					if (dt != null)
						value = string.Format("'{0}'", dt.Value.ToString("yyyy-MM-dd HH:mm:ss:fff"));
				}


				query = query.Replace(pattern, value);
			}
			return query;
		}

		internal static Object GetPropValue(this Object obj, String name)
		{
			foreach (String part in name.Split('.'))
			{
				if (obj == null) { return null; }

				Type type = obj.GetType();
				PropertyInfo info = type.GetProperty(part);
				if (info == null) { return null; }

				obj = info.GetValue(obj, null);
			}
			return obj;
		}

		internal static T GetPropValue<T>(this Object obj, String name)
		{
			Object retval = GetPropValue(obj, name);
			if (retval == null) { return default(T); }

			// throws InvalidCastException if types are incompatible
			return (T)retval;
		}


		public static void Insert(this DirectDatabaseBase db, DirectModel model)
		{
			string command = string.Format("INSERT INTO {0}.{1}.{2} ({3}) VALUES ({4});",
				db.DatabaseName, db.DatabaseScheme, model.GetTableName(),
				model.GetPropertyNamesForInsert(), model.GetPropertyValuesForInsert());
			int? id = db.Execute(command);
			if (id.HasValue)
				model.SetID(id.Value);
		}

		public static void Update(this DirectDatabaseBase db, DirectModel model)
		{
			if (model.ID() <= 0)
				throw new Exception("ID is not set, maybe this table was not loaded");

			// UPDATE MobilePaywall.core.A SET A=1 WHERE AID=1
			string command = string.Format("UPDATE {0}.{1}.{2} SET {3} WHERE {2}ID={4};",
				db.DatabaseName, db.DatabaseScheme, model.GetTableName(),
				model.GetUpdateData(), model.GetID());
			db.Execute(command);
		}

		public static void Delete(this DirectDatabaseBase db, DirectModel model)
		{
			string command = string.Format("DELETE FROM {0}.{1}.{2} WHERE {2}ID={3};",
				db.DatabaseName, db.DatabaseScheme, model.GetTableName(),
				model.GetID());
			db.Execute(command);
		}

		public static T Load<T>(this DirectDatabaseBase db, string whereCommand)
		{
			T temp = (T)Activator.CreateInstance(typeof(T));
			DirectModel model = temp as DirectModel;
			if (model == null)
				throw new Exception("Cast error");

			string command = string.Format("SELECT * FROM {0}.{1}.{2} WHERE {3}",
				db.DatabaseName, db.DatabaseScheme, model.GetTableName(), whereCommand);

			DirectContainer dc = db.LoadContainer(command);
			return dc.Convert<T>();
		}

		public static List<T> LoadMany<T>(this DirectDatabaseBase db, string whereCommand = "")
		{
			T temp = (T)Activator.CreateInstance(typeof(T));
			DirectModel model = temp as DirectModel;
			if (model == null)
				throw new Exception("Cast error");

			string command = string.Format("SELECT * FROM {0}.{1}.{2}{3}",
				db.DatabaseName, db.DatabaseScheme, model.GetTableName(), (!string.IsNullOrEmpty(whereCommand) ? " WHERE " + whereCommand : ""));

			DirectContainer dc = db.LoadContainer(command);
			return dc.ConvertList<T>();
		}

	}
}
