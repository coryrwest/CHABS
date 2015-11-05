using System;
using System.IO;
using System.Xml.Serialization;

namespace CRWestropp.Utilities {
	[Serializable()]
	public abstract class XmlObject<T> : XmlObject where T : XmlObject {
		#region Static Methods
		public static T Load(string path) {
			return (T)Load(path, typeof(T));
		}
		#endregion
	}

	[Serializable()]
	public abstract class XmlObject : System.Runtime.Serialization.ISerializable {
		/// <summary>
		/// Loads the object from an XML file
		/// </summary>
		/// <param name="path"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static XmlObject Load(string path, Type type) {
			XmlObject obj = null;
			using (TextReader reader = new StreamReader(path)) {
				XmlSerializer xs = new XmlSerializer(type);
				obj = (XmlObject)xs.Deserialize(reader);
				reader.Close();
			}
			return obj;
		}

		#region ISerializable Members
		public void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { }
		#endregion
	}
}
