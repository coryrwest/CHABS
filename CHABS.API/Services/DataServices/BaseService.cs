using System;
using System.Collections.Generic;
using System.Dynamic;
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
			// Before save events
			BeforeSave(wasNew, dataObject);
			// Save it
			if (dataObject.IsNew) {
				Insert(dataObject);
			} else {
				Update(dataObject);
			}
			AfterSave(wasNew, dataObject);
			return dataObject;
		}

		public virtual void Delete(Guid id) {
			T item = GetById(id);
			Delete(item);
		}

		public virtual void DeleteObject(T item) {
			Delete(item);
		}

		public virtual void Restore(Guid id) {
			T item = GetById(id, true);
			Restore(item);
		}

		public virtual void RestoreObject(T item) {
			Restore(item);
		}

		public virtual List<T> GetAllForHousehold(bool includeDeleted = false) {
			var items = GetList("householdid = ", includeDeleted).ToList();
			return items;
		} 



		#region DB Interaction
		private void Delete(T dataObject) {
			if (dataObject.Perpetual) {
				dataObject.Deleted = true;
				Update(dataObject);
			} else {
				db.Delete(dataObject);
			}
		}

		private void Restore(T dataObject) {
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
			return item;
		}

		public IEnumerable<T> RawQuery<T>(string query, object parameters, bool includeDeleted = false) {
			var results = db.RawQuery<T>(query, parameters);
			return results;
		}

		public int RawExecute(string query, object parameters) {
			var results = db.RawExecute(query, parameters);
			return results;
		}


		public IEnumerable<T> GetList(string whereClause, object parameters, bool includeDeleted = false) {
			var poco = Activator.CreateInstance<T>();
			if (poco.Perpetual && !includeDeleted) {
				whereClause = whereClause.InsertBefore(" and deleted = false ", new[] { "order by", "group by" });
			}
			var results =  db.GetList<T>(whereClause, parameters);
	        // Sort perpetual items with deleted at the bottom
	        if (poco.Perpetual) {
		        results = results.OrderBy(i => i.Deleted).ToList();
	        }
			return results;
		}

		/// <summary>
		/// Will return null on no results
		/// </summary>
		/// <param name="whereClause"></param>
		/// <param name="ignorePerpetual"></param>
		/// <returns></returns>
		public T GetSingle(string whereClause, object parameters, bool ignorePerpetual = false) {
			// Add required params
			var poco = Activator.CreateInstance<T>();
			if (poco.Perpetual && !ignorePerpetual) {
				whereClause = whereClause.InsertBefore(" and deleted = true ", new[] { "order by", "group by" });
			}
			var item = db.GetList<T>(whereClause, parameters).FirstOrDefault();
			return item;
		}

		#region Private Members
		private T Update(T dataObject) {
			db.Update(dataObject);
			dataObject.IsNew = false;
			return dataObject;
		}

		private T Insert(T dataObject) {
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
