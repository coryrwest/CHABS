using System.Collections.Generic;
using System.Xml.Serialization;
using CRWestropp.Utilities.Extensions;

namespace CRWestropp.Utilities {
	[XmlRoot("KeyValuePairs")]
	public class KeyValuePairs : XmlObject<KeyValuePairs> {
		[XmlIgnore()]
		public Dictionary<string, string> Dictionary { get; private set; }

		/// <summary>
		/// Get the value with the assiciated key
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		[XmlIgnore()]
		public virtual string this[string key] {
			get { return Dictionary[key]; }
			set { Dictionary[key] = value; }
		}

		[XmlElement("Pair")]
		public Pair[] Pairs {
			get {
				Pair[] array = new Pair[Dictionary.Count];
				int i = 0;
				foreach (KeyValuePair<string, string> pair in Dictionary) {
					array[i] = new Pair(pair.Key, pair.Value);
					i++;
				}
				return array;
			}
			set {
				Dictionary.Clear();
				if (value != null) {
					foreach (Pair pair in value) {
						Add(pair.Key, pair.Value);
					}
				}
			}
		}

		public KeyValuePairs() {
			Dictionary = new Dictionary<string, string>();
		}

		public KeyValuePairs(Dictionary<string, string> pairs) {
			Dictionary = pairs;
		}

		/// <summary>
		/// Adds the specified key/value, or replaces the existing key with the new value
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		public void Add(string key, string value) {
			if (!Dictionary.ContainsKey(key))
				Dictionary.Add(key, value);
			else
				Dictionary[key] = value;
		}

		public bool Remove(string key) {
			return Dictionary.Remove(key);
		}

		public bool ContainsKey(string key) {
			return Dictionary.ContainsKey(key);
		}

		/// <summary>
		/// "Safe" get, if the key doesn't exist, NULL will be returned
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public string Get(string key) {
			return Get(key, null);
		}

		/// <summary>
		/// "Safe" get, if the key doesn't exist, defaultValue will be returned
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public string Get(string key, string defaultValue) {
			if (ContainsKey(key))
				return this[key];
			else
				return defaultValue;
		}

		#region Import
		public void Import(KeyValuePairs dataToImport) {
			Import(dataToImport.Dictionary);
		}

		public void Import(KeyValuePairs dataToImport, bool overwriteExistingValues) {
			Import(dataToImport.Dictionary, overwriteExistingValues);
		}

		public void Import(KeyValuePairs dataToImport, bool overwriteExistingValues, bool ignoreEmptyValues) {
			Import(dataToImport.Dictionary, overwriteExistingValues, ignoreEmptyValues);
		}

		public void Import(Dictionary<string, string> dataToImport) {
			Import(dataToImport, false);
		}

		public void Import(Dictionary<string, string> dataToImport, bool overwriteExistingValues) {
			Import(dataToImport, overwriteExistingValues, false);
		}

		public void Import(Dictionary<string, string> dataToImport, bool overwriteExistingValues, bool ignoreEmptyValues) {
			// Loop through each KVP to import
			foreach (var pair in dataToImport) {
				// Check if the value is NULL (add depending on settings)
				if (!pair.Value.IsNull() || !ignoreEmptyValues) {
					// If the doesn't already exist, or overwrite is enabled
					if (!Dictionary.ContainsKey(pair.Key) || overwriteExistingValues)
						// Add/Update the KVP
						Add(pair.Key, pair.Value);
				}
			}
		}
		#endregion

		#region Export
		public void Export(System.Data.DataTable dataToExport) {
			// Create a new row
			System.Data.DataRow row = dataToExport.NewRow();
			// Export the data
			Export(row);
			// Add the row to the table
			dataToExport.Rows.Add(dataToExport);
		}

		public void Export(System.Data.DataRow dataToExport) {
			// Export the data, creating a unique column for each record
			Export(dataToExport, true);
		}

		/// <summary>
		/// Export the KeyValuePairs to a DataRow
		/// </summary>
		/// <param name="dataToExport"></param>
		/// <param name="uniqueColumns">If true a new column with a unique name will be created for each Key. No existing values in the row will be overwritten</param>
		public void Export(System.Data.DataRow dataToExport, bool uniqueColumns) {
			// Loop through each field
			foreach (KeyValuePair<string, string> pair in Dictionary) {
				// Get the column name
				string columnName = pair.Key;
				// Set the index to 0
				int i = 0;
				// Check to see if the column exists
				while (uniqueColumns && dataToExport.Table.Columns.Contains(columnName)) {
					// Incriment the counter
					i++;
					// Adjust the column name
					columnName = string.Format("{0}{1}", pair.Key, i);
				}

				// Make sure the column does not already exist
				if (uniqueColumns || !dataToExport.Table.Columns.Contains(columnName))
					// Add the (unique) column
					dataToExport.Table.Columns.Add(columnName, typeof(string));
				// Set the value
				dataToExport[columnName] = pair.Value;
			}
		}
		#endregion

		public class Pair {
			private string _key;

			/// <summary>
			/// Get or set the key
			/// </summary>
			[XmlAttribute("Key")]
			public string Key {
				get { return _key; }
				set { _key = value; }
			}

			private string _value;

			/// <summary>
			/// Get or set the value
			/// </summary>
			[XmlAttribute("Value")]
			public string Value {
				get { return _value; }
				set { _value = value; }
			}

			public Pair() { }

			public Pair(string key, string value) {
				_key = key;
				_value = value;
			}
		}
	}
}
