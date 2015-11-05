using System.Collections.Generic;
using CRWestropp.Utilities.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CRWestropp.UnitTests.Helpers {
	[TestClass]
	public class StringHelperTests {
		[TestMethod]
		public void TrimStringTest() {
			string target = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam ornare sapien lacus, nec mattis lectus pharetra sit amet. Proin elementum nunc magna, ac aliquam metus placerat nec. Maecenas tempor ullamcorper erat ut imperdiet. Maecenas vulputate, arcu at dapibus rhoncus, tellus mi tincidunt ipsum, quis sodales eros nunc quis metus.";

			StringHelper helper = new StringHelper();
			target = helper.TrimStringToBlock("Nullam", "amet", target);

			Assert.AreEqual("Nullam ornare sapien lacus, nec mattis lectus pharetra sit amet", target);
		}

		[TestMethod]
		public void TrimStringTestWithSameMatch() {
			string target = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam ornare sapien lacus, nec mattis lectus pharetra sit amet. Proin elementum nunc magna, ac aliquam metus placerat nec. Maecenas tempor ullamcorper erat ut imperdiet. Maecenas vulputate, arcu at dapibus rhoncus, tellus mi tincidunt ipsum, quis sodales eros nunc quis metus.";

			StringHelper helper = new StringHelper();
			target = helper.TrimStringToBlock("amet", "amet", target);

			Assert.AreEqual("amet", target);
		}

		[TestMethod]
		public void FindStringMatchesSameStringTest() {
			string target = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam ornare sapien lacus, nec mattis lectus pharetra sit amet. Proin elementum nunc magna, ac aliquam metus placerat nec. Maecenas tempor ullamcorper erat ut imperdiet. Maecenas vulputate, arcu at dapibus rhoncus, tellus mi tincidunt ipsum, quis sodales eros nunc quis metus.";

			StringHelper helper = new StringHelper();
			List<string> list = new List<string>();
			list = helper.FindAllStringMatches(target, "amet");

			Assert.AreEqual(list.Count, 2);
		}

		[TestMethod]
		public void FindStringMatchesDiffStringTest() {
			string target = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Lorem ipsum sit amet, adipiscing elit. Lorem ipsum dolor amet, consectetur elit. Maecenas vulputate, arcu at dapibus rhoncus, tellus mi tincidunt ipsum, quis sodales eros nunc quis metus.";

			StringHelper helper = new StringHelper();
			List<string> list = new List<string>();
			list = helper.FindAllStringMatches(target, "Lorem", "elit");

			Assert.AreEqual(list.Count, 3);
		}

		[TestMethod]
		public void FindStringMatchesDiffStringTestNoMatchInclude() {
			string target = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Lorem ipsum sit amet, adipiscing elit. Lorem ipsum dolor amet, consectetur elit. Maecenas vulputate, arcu at dapibus rhoncus, tellus mi tincidunt ipsum, quis sodales eros nunc quis metus.";

			StringHelper helper = new StringHelper();
			List<string> list = new List<string>();
			list = helper.FindAllStringMatches(target, "Lorem", "elit", false);

			Assert.AreEqual(" ipsum dolor sit amet, consectetur adipiscing ", list[0]);
		}

		[TestMethod]
		public void FindStringMatchDiffStringTest() {
			string target = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Lorem ipsum sit amet, adipiscing elit. Lorem ipsum dolor amet, consectetur elit. Maecenas vulputate, arcu at dapibus rhoncus, tellus mi tincidunt ipsum, quis sodales eros nunc quis metus.";

			StringHelper helper = new StringHelper();
			string str = "";
			str = helper.FindStringMatch(target, "Lorem", "elit");

			Assert.AreEqual(str, "Lorem ipsum dolor sit amet, consectetur adipiscing elit");
		}

		[TestMethod]
		public void IndexBeforeTest() {
			string target = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Lorem ipsum sit amet, adipiscing elit. Lorem ipsum dolor amet, consectetur elit. Maecenas vulputate, arcu at dapibus rhoncus, tellus mi tincidunt ipsum, quis sodales eros nunc quis metus.";

			int index = 0;
			index = StringHelper.IndexBefore(target, 100, "amet");

			Assert.AreEqual(index, 73);
		}
	}
}
