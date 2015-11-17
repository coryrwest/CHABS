using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CHABS.API.Objects;
using CRWestropp.Utilities.Extensions;
using DbExtensions;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using Database = CHABS.API.DataAccess.Database;

namespace CHABS.API.Services {
	public interface IDataService {
		Session Session { get; }
	}

	public class BaseService<T> : IDataService, IDisposable where T : DataObject {
		protected Database db =
			new Database((System.Configuration.ConfigurationManager.AppSettings["Environment"] ?? "Development") == "Production"
				? "LiveConnection"
				: "DefaultConnection");
		public Session Session { get; private set; }

		public BaseService(Session session) {
			Session = session;
		}

		/// <summary>
		/// Will run before the DataObjects BeforeSave and the actual save.
		/// daObj will be the DataObject that is saving.
		/// </summary>
		/// <param name="isInserting"></param>
		/// <param name="daObj"></param>
		protected virtual void BeforeSave(bool isInserting, DataObject daObj) { }
		/// <summary>
		/// Will run after the DataObjects AfterSave and the actual save.
		/// daObj will be the DataObject that is saving.
		/// </summary>
		/// <param name="isInserting"></param>
		/// <param name="daObj"></param>
		protected virtual void AfterSave(bool isInserting, DataObject daObj) { }

		public T Upsert(T dataObject) {
			// Get new flag for the isInserting of the save events
			var wasNew = dataObject.IsNew;
			// Ebfore save events
			BeforeSave(wasNew, dataObject);
			dataObject.BeforeSave();
			// Save it
			if (dataObject.IsNew) {
				Insert(dataObject);
			} else {
				Update(dataObject);
			}
			// After save events
			dataObject.AfterSave();
			AfterSave(wasNew, dataObject);
			return dataObject;
		}

		public virtual void Delete(Guid id) {
			T item = GetById(id);
			if (!PermissionsService.CheckObjectPermissions(item, Session)) {
				throw new PermissionsException();
			}
			Delete(item);
		}

		#region DB Interaction
		public void Delete(T dataObject) {
			if (!PermissionsService.CheckObjectPermissions(dataObject, Session)) {
				throw new PermissionsException();
			}

			if (dataObject.Perpetual) {
				dataObject.Deleted = true;
				Update(dataObject);
			} else {
				db.Delete(dataObject);
			}
		}

		public T GetById(Guid id) {
			if (id == Guid.Empty) { return null; }

			T item = db.GetById<T>(id);
			if (item.Deleted) {
				return null;
			}
			if (!PermissionsService.CheckObjectPermissions(item, Session)) {
				throw new PermissionsException();
			}
			return item;
		}

		public List<T> GetList(object whereClause) {
			// Add requires params
			var poco = Activator.CreateInstance<T>();
			if (poco.Perpetual) {
				dynamic where = whereClause.ToDynamic();
				where.deleted = false;
				whereClause = where;
			}
			return db.GetList<T>(whereClause);
		}

		public List<T> GetList(string whereClause) {
			var poco = Activator.CreateInstance<T>();
			if (poco.Perpetual) {
				whereClause = whereClause.InsertBefore(" and deleted = false ", new[] { "order by", "group by" });
			}
			var results =  db.GetList<T>(whereClause);
			return results;
		}

		public T GetSingle(object whereClause) {
			// Add requires params
			var poco = Activator.CreateInstance<T>();
			if (poco.Perpetual) {
				dynamic where = whereClause.ToDynamic();
				where.deleted = false;
				whereClause = where;
			}
			var item = db.GetList<T>(whereClause).FirstOrDefault();
			return item;
		}

		#region Private Members
		private T Update(T dataObject) {
			if (!PermissionsService.CheckObjectPermissions(dataObject, Session)) {
				throw new PermissionsException();
			}
			db.Update(dataObject);
			dataObject.IsNew = false;
			return dataObject;
		}

		private T Insert(T dataObject) {
			if (!PermissionsService.CheckObjectPermissions(dataObject, Session)) {
				throw new PermissionsException();
			}
			db.Insert(dataObject);
			dataObject.IsNew = false;
			return dataObject;
		}
		#endregion
		#endregion

		public void Dispose() {
			
		}
	}
}
