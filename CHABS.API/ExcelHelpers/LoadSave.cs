﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml;

namespace CHABS.API.ExcelHelpers {
	public static class LoadSave {
		public static ExcelWorkbook LoadWorkbookFromBytes(ExcelPackage package, byte[] bytes) {
			using (var stream = new MemoryStream(bytes)) {
				package.Load(stream);
				return package.Workbook;
			}
		}

		public static byte[] SaveWorkbookToBytes(ExcelPackage package) {
			using (var stream = new MemoryStream()) {
				package.SaveAs(stream);
				return stream.ToArray();
			}
		}
	}
}