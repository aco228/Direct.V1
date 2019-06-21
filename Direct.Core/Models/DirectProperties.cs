using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Direct.Core.Models
{
	internal enum DirectPropertyType { Default, Int, String, Double, DateTime, Bool, Null }
	internal class DirectProperties
	{
		public bool IsPrimary { get; set; } = false;
		public DirectPropertyType Type { get; set; } = DirectPropertyType.Default;
		public string PropetryName { get; set; } = "";

		public DirectProperties(PropertyInfo info, bool primary)
		{
			this.IsPrimary = primary;
			this.PropetryName = info.Name;

			if (info.PropertyType.FullName.Equals("System.Int32"))
				this.Type = DirectPropertyType.Int;
			else if (info.PropertyType.FullName.Equals("System.Boolean"))
				this.Type = DirectPropertyType.Bool;
			else if (info.PropertyType.FullName.Equals("System.String"))
				this.Type = DirectPropertyType.String;
			else if (info.PropertyType.FullName.Equals("System.Double"))
				this.Type = DirectPropertyType.Double;
			else if (info.PropertyType.FullName.Equals("System.DateTime"))
				this.Type = DirectPropertyType.DateTime;
			else if (info.PropertyType.FullName.StartsWith("System.Nullable"))
			{
				if (info.PropertyType.FullName.StartsWith("System.Nullable`1[[System.DateTime"))
					this.Type = DirectPropertyType.DateTime;
				else
					this.Type = DirectPropertyType.Null;
			}
		}
	}
}
