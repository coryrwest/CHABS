﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using CHABS.API.Objects;
using Dapper;
using Npgsql;

namespace CHABS.API.DataAccess {
	public class Database : IDisposable {

		private NpgsqlConnection connection;
		private string connectionString;

		public Database(string conn) {
			SimpleCRUD.SetDialect(SimpleCRUD.Dialect.PostgreSQL);
			connectionString = conn;
		}

		public Guid Insert(DataObject poco) {
			using (connection = new NpgsqlConnection(connectionString)) {
			    connection.Insert<Guid>(poco);
				return poco.Id;
			}
		}

		public Guid Update(DataObject poco) {
			using (connection = new NpgsqlConnection(connectionString)) {
				connection.Update(poco);
				return poco.Id;
			}
		}

		public T GetById<T>(Guid Id) where T : DataObject {
			using (connection = new NpgsqlConnection(connectionString)) {
				var dataObj = connection.Get<T>(Id);
				if (dataObj != null) {
					dataObj.IsNew = false;
				}
				return dataObj;
			}
	    }

        public IEnumerable<T> GetList<T>(string whereClause, object parameters) where T : DataObject {
			using (connection = new NpgsqlConnection(connectionString)) {
				IEnumerable<T> list = Query<T>($"select *, false as IsNew from {SimpleCRUD.GetTableName(typeof(T))} where {whereClause}", parameters);
				return list;
			}
		}

		public void Delete(DataObject poco) {
			using (connection = new NpgsqlConnection(connectionString)) {
				int confirm = connection.Delete(poco);
				if (confirm != 1) {
					throw new Exception(string.Format("{0} item(s) where deleted instead of one.", confirm));
				}
			}
		}

		/// <summary>
		/// Will execute a query against the database, no return value
		/// </summary>
		/// <param name="sql"></param>
		/// <param name="parameters"></param>
		public void Execute(string sql, object parameters) {
			using (connection = new NpgsqlConnection(connectionString)) {
				connection.Execute(sql, parameters);
			}
		}

		public IEnumerable<dynamic> Query(string sql, object parameters) {
			using (connection = new NpgsqlConnection(connectionString)) {
				var results = connection.Query(sql, parameters);
				return results;
			}
		}

		/// <summary>
		/// Query the database with DataObject wrapping.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="sql"></param>
		/// <param name="parameters"></param>
		/// <returns></returns>
		public IEnumerable<T> Query<T>(string sql, object parameters) where T : DataObject {
			using (connection = new NpgsqlConnection(connectionString)) {
				var results = connection.Query<T>(sql, parameters);
				return results;
			}
		}

		/// <summary>
		/// Query the database with DataObject wrapping. Returns first result.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="sql"></param>
		/// <param name="parameters"></param>
		/// <returns></returns>
		public T QuerySingle<T>(string sql, object parameters) where T : DataObject {
			var results = Query<T>(sql, parameters);
			var result = results.FirstOrDefault();
			if (results != null) result.IsNew = false;
			return result;
		}

		/// <summary>
		/// Query the database for raw data. No DataObject wrapping.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="sql"></param>
		/// <param name="parameters"></param>
		/// <returns></returns>
		public IEnumerable<T> RawQuery<T>(string sql, object parameters) {
			using (connection = new NpgsqlConnection(connectionString)) {
				var results = connection.Query<T>(sql, parameters);
				return results;
			}
		}
		public int RawExecute(string sql, object parameters) {
			using (connection = new NpgsqlConnection(connectionString)) {
				var results = connection.Execute(sql, parameters);
				return results;
			}
		}

		public void Close() {
			connection.Close();
		}

		public void Dispose() {
			connection.Close();
		}

        #region Helpers
	    public static Dictionary<string, object> Dyn2Dict(dynamic dynObj) {
	        var dictionary = new Dictionary<string, object>();
	        foreach (PropertyDescriptor propertyDescriptor in TypeDescriptor.GetProperties(dynObj)) {
	            object obj = propertyDescriptor.GetValue(dynObj);
	            dictionary.Add(propertyDescriptor.Name, obj);
	        }
	        return dictionary;
	    }

        private static string BuildWhere(IDictionary<string, object> props, object sourceEntity) {
			var sb = new StringBuilder();
			sb.Append(" where ");
			for (var i = 0; i < props.Count(); i++) {
				//match up generic properties to source entity properties to allow fetching of the column attribute
				//the anonymous object used for search doesn't have the custom attributes attached to them so this allows us to build the correct where clause
				//by converting the model type to the database column name via the column attribute
				var propertyToUse = props.ElementAt(i).Key;
				var sourceProperties = GetScaffoldableProperties(sourceEntity).ToArray();
				for (var x = 0; x < sourceProperties.Count(); x++) {
					if (sourceProperties.ElementAt(x).Name == props.ElementAt(i).Key) {
						propertyToUse = sourceProperties.ElementAt(x).Name;
					}
				}

				sb.AppendFormat("{0} = @{0}", propertyToUse);
				if (i < props.Count() - 1)
					sb.AppendFormat(" and ");
			}
			return sb.ToString();
		}

		//Get all properties that are not decorated with the Editable(false) attribute
		private static IEnumerable<PropertyInfo> GetScaffoldableProperties(object entity) {
			var props = entity.GetType().GetProperties().Where(p => p.GetCustomAttributes(true).Any(attr => attr.GetType().Name == "EditableAttribute" && !IsEditable(p)) == false);
			return props.Where(p => p.PropertyType.IsSimpleType() || IsEditable(p));
		}

		//Determine if the Attribute has an AllowEdit key and return its boolean state
		//fake the funk and try to mimick EditableAttribute in System.ComponentModel.DataAnnotations 
		//This allows use of the DataAnnotations property in the model and have the SimpleCRUD engine just figure it out without a reference
		private static bool IsEditable(PropertyInfo pi) {
			var attributes = pi.GetCustomAttributes(false);
			if (attributes.Length > 0) {
				dynamic write = attributes.FirstOrDefault(x => x.GetType().Name == "EditableAttribute");
				if (write != null) {
					return write.AllowEdit;
				}
			}
			return false;
		}
		#endregion
	}
}
