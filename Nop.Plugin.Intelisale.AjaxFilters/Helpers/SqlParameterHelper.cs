using System;
using System.Data;
using LinqToDB;
using LinqToDB.Data;

namespace Nop.Plugin.Intelisale.AjaxFilters.Helpers
{
	public static class SqlParameterHelper
	{
		private static DataParameter GetParameter(DataType dbType, string parameterName, object parameterValue)
		{
			DataParameter val = new DataParameter();
			val.Name = parameterName;
			val.Value = parameterValue;
			val.DataType = dbType;
			return val;
		}

		private static DataParameter GetOutputParameter(DataType dbType, string parameterName)
		{
			DataParameter val = new DataParameter();
			val.Name = parameterName;
			val.DataType = dbType;
			val.Direction = ParameterDirection.Output;
			return val;
		}

		public static DataParameter GetStringParameter(string parameterName, string parameterValue)
		{
			return GetParameter((DataType)5, parameterName, parameterValue);
		}

		public static DataParameter GetOutputStringParameter(string parameterName)
		{
			return GetOutputParameter((DataType)5, parameterName);
		}

		public static DataParameter GetInt32Parameter(string parameterName, int? parameterValue)
		{
			return GetParameter((DataType)15, parameterName, parameterValue);
		}

		public static DataParameter GetOutputInt32Parameter(string parameterName)
		{
			return GetOutputParameter((DataType)15, parameterName);
		}

		public static DataParameter GetBooleanParameter(string parameterName, bool? parameterValue)
		{
			return GetParameter((DataType)11, parameterName, parameterValue);
		}

		public static DataParameter GetDecimalParameter(string parameterName, decimal? parameterValue)
		{
			return GetParameter((DataType)23, parameterName, parameterValue);
		}

		public static DataParameter GetDateTimeParameter(string parameterName, DateTime? parameterValue)
		{
			return GetParameter((DataType)28, parameterName, parameterValue);
		}
	}
}
