using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CHABS.API.Objects;

namespace CHABS.API.Services.DataServices
{
	public class BankConnectionService : BaseService<BankConnection> {
		public BankConnectionService(Session session) : base(session) {
		}

		public List<Guid> GetListForHousehold(Guid householdId) {
			const string query = "select id from logins where householdId = @householdId and deleted = False;";
			return db.RawQuery<Guid>(query, new { householdId }).ToList();
		}

		public override void Delete(Guid id) {
			// Build query
			const string query = "update accounts set shown = false where loginid = @loginId; update logins set deleted = true where id = @loginId;";
			db.Execute(query, new { loginId = id });
		}

		// Plaid Methods
		public void SavePublicToken(string token) {
			db.Execute("insert into public_token_temp (id, publictoken, userid) values (@Id, @Token, @UserId);", new { Id = Guid.NewGuid(), Token = token, UserId = Session.UserId });
		}

		public void DeletePublicToken(string token) {
			db.Execute("delete from public_token_temp where publictoken = @Token and userid = @UserId;", new { Token = token, UserId = Session.UserId });
		}
	}
}
