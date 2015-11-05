using System;
using System.IO;

namespace WorkingBudgetUpdater.Job {
	class Program {
		static void Main(string[] args) {

			var file = File.ReadAllBytes(Environment.CurrentDirectory + "/Working Budget.xlsm");
			var str = Convert.ToBase64String(file);

			byte[] bytes = Convert.FromBase64String(str);


			//byte[] bytes = new byte[0];
			//if (args.Length != 0 && args[0] != null) {
			//	bytes = new byte[args[0].Length * sizeof(char)];
			//	System.Buffer.BlockCopy(args[0].ToCharArray(), 0, bytes, 0, bytes.Length);
			//}

			if (bytes.Length == 0) {
				throw new Exception("Byte array was null. Either the argument was missing or it could not be converted.");
			}

			var updater = new WorkingBudgetUpdater.API.WorkingBudgetUpdater();
			updater.Update(bytes);
		}
	}
}
