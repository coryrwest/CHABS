using System;
using System.Collections.Generic;
using System.Linq;
using CHABS.API.Objects;
using Database = CHABS.API.DataAccess.Database;

namespace CHABS.API.Services {
	public interface IDataService {
		Session Session { get; }
	}

	public class BaseService<T> : IDataService, IDisposable where T : DataObject {
	    protected Database db;
		public Session Session { get; private set; }

		public BaseService(Session session) {
			Session = session;
		    db = new Database(session.ConnectionString);
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
		/// <summary>
		/// Will run before the service insert save.
		/// daObj will be the DataObject that is saving.
		/// </summary>
		/// <param name="isInserting"></param>
		/// <param name="daObj"></param>
		protected virtual void BeforeInsert(DataObject daObj) { }

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

		public virtual void DeleteObject(T item) {
			if (!PermissionsService.CheckObjectPermissions(item, Session)) {
				throw new PermissionsException();
			}
			Delete(item);
		}

		public virtual void Restore(Guid id) {
			T item = GetById(id, true);
			if (!PermissionsService.CheckObjectPermissions(item, Session)) {
				throw new PermissionsException();
			}
			Restore(item);
		}

		public virtual void RestoreObject(T item) {
			if (!PermissionsService.CheckObjectPermissions(item, Session)) {
				throw new PermissionsException();
			}
			Restore(item);
		}

		public virtual List<T> GetAllForHousehold(bool includeDeleted = false) {
			var items = GetList(new {householdid = Session.HouseholdId}, includeDeleted).ToList();
			return items;
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

		public void Restore(T dataObject) {
			if (!PermissionsService.CheckObjectPermissions(dataObject, Session)) {
				throw new PermissionsException();
			}

			if (dataObject.Perpetual) {
				dataObject.Deleted = false;
				Update(dataObject);
			}
		}

		public T GetById(Guid id, bool includeDeleted = false) {
			if (id == Guid.Empty) { return null; }

			T item = db.GetById<T>(id);
			if (item.Deleted && !includeDeleted) {
				return null;
			}
			if (!PermissionsService.CheckObjectPermissions(item, Session)) {
				throw new PermissionsException();
			}
			return item;
		}

		public List<T> GetList(object whereClause, bool includeDeleted = false) {
			// Add requires params
			var poco = Activator.CreateInstance<T>();
			if (poco.Perpetual && !includeDeleted) {
				//dynamic where = whereClause.ToDynamic();
				//where.deleted = false;
				//whereClause = where;
			}
			var items = db.GetList<T>(whereClause);

			// Sort perpetual items with deleted at the bottom
			if (poco.Perpetual) {
				items = items.OrderBy(i => i.Deleted).ToList();
			}

			return items;
		}

		public List<T> GetList(string whereClause, bool includeDeleted = false) {
			var poco = Activator.CreateInstance<T>();
			if (poco.Perpetual && !includeDeleted) {
				//whereClause = whereClause.InsertBefore(" and deleted = false ", new[] { "order by", "group by" });
			}
			var results =  db.GetList<T>(whereClause);
			return results;
		}

		/// <summary>
		/// Will return nul on no results
		/// </summary>
		/// <param name="whereClause"></param>
		/// <param name="ignorePerpetual"></param>
		/// <returns></returns>
		public T GetSingle(object whereClause, bool ignorePerpetual = false) {
			// Add requires params
			var poco = Activator.CreateInstance<T>();
			if (poco.Perpetual && !ignorePerpetual) {
				//dynamic where = whereClause.ToDynamic();
				//where.deleted = false;
				//whereClause = where;
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
			BeforeInsert(dataObject);
		    dataObject.Id = Guid.NewGuid();
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
