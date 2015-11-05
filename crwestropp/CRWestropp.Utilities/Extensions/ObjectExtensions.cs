using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace CRWestropp.Utilities.Extensions
{
    public static class ObjectExtentions
    {
        #region Property Values
        /// <summary>
        /// Gets the value of an Objects property by name
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="name">The name of the property you want the value of</param>
        /// <returns>Variable of the same type as the Object property you are trying to get</returns>
        public static Object GetPropValue(this Object obj, String name)
        {
            foreach (String part in name.Split('.'))
            {
                if (obj == null) { return null; }

                Type type = obj.GetType();
                PropertyInfo info = type.GetProperty(part);
                if (info == null) { return null; }

                obj = info.GetValue(obj, null);
            }
            return obj;
        }

        /// <summary>
        /// Gets the value of an Objects property by name of type T
        /// </summary>
        /// <typeparam name="T">Type of the object property you are trying to get</typeparam>
        /// <param name="obj"></param>
        /// <param name="name">The name of the property you want the value of</param>
        /// <returns>Variable of the same type as the Object property you are trying to get</returns>
        public static T GetPropValue<T>(this Object obj, String name)
        {
            Object retval = GetPropValue(obj, name);
            if (retval == null) { return default(T); }

            // throws InvalidCastException if types are incompatible
            return (T)retval;
        }

		public static bool HasProperty(this object obj, string propertyName) {
			return obj.GetType().GetProperty(propertyName) != null;
		}
        #endregion

        #region Object Copy
        /// <summary>
        /// Object exension to copy all public properties of one object to all available properties of another
        /// </summary>
        /// <typeparam name="TConvert">Object you want to convert to</typeparam>
        /// <param name="entity">Object you want to convert from</param>
        /// <returns>New instance of Object you want to convert to</returns>
        public static TConvert ConvertTo<TConvert>(this object entity) where TConvert : new()
        {
            // Get properties of target object
            var convertProperties = TypeDescriptor.GetProperties(typeof(TConvert)).Cast<PropertyDescriptor>();
            // Get properties of source object
            var entityProperties = TypeDescriptor.GetProperties(entity).Cast<PropertyDescriptor>();

            // Create new target object
            var convert = new TConvert();

            // Copy available properties
            foreach (var entityProperty in entityProperties)
            {
                var property = entityProperty;
                var convertProperty = convertProperties.FirstOrDefault(prop => prop.Name == property.Name);
                if (convertProperty != null)
                {
                    convertProperty.SetValue(convert, Convert.ChangeType(entityProperty.GetValue(entity), convertProperty.PropertyType));
                }
            }

            return convert;
        }

		/// <summary>
		/// Extension for 'Object' that copies the properties to a destination object.
		/// </summary>
		/// <param name="source">The source.</param>
		/// <param name="destination">The destination.</param>
		public static void CopyProperties(this object source, object destination) {
			// If any this null throw an exception
			if (source == null || destination == null)
				throw new Exception("Source or/and Destination Objects are null");
			// Getting the Types of the objects
			Type typeDest = destination.GetType();
			Type typeSrc = source.GetType();

			// Iterate the Properties of the source instance and  
			// populate them from their desination counterparts  
			PropertyInfo[] srcProps = typeSrc.GetProperties();
			foreach (PropertyInfo srcProp in srcProps) {
				if (!srcProp.CanRead) {
					continue;
				}
				PropertyInfo targetProperty = typeDest.GetProperty(srcProp.Name);
				if (targetProperty == null) {
					continue;
				}
				if (!targetProperty.CanWrite) {
					continue;
				}
				if (targetProperty.GetSetMethod(true) != null && targetProperty.GetSetMethod(true).IsPrivate) {
					continue;
				}
				if ((targetProperty.GetSetMethod().Attributes & MethodAttributes.Static) != 0) {
					continue;
				}
				if (!targetProperty.PropertyType.IsAssignableFrom(srcProp.PropertyType)) {
					continue;
				}
				// Passed all tests, lets set the value
				targetProperty.SetValue(destination, srcProp.GetValue(source, null), null);
			}
		}
        #endregion

        #region SafeString
        /// <summary>
        /// Returns a safe string (empty string if null)
        /// </summary>
        /// <param name="obj">object to check</param>
        /// <returns>string</returns>
        public static string ToSafeString(this object obj) {
            return obj != null ? obj.ToString() : string.Empty;
        }
        #endregion

        #region ToGuid
        /// <summary>
        /// Converts the string to a Guid, returns Guid.Empty if an invalid string was specified
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>Guid or Guid.Empty</returns>
        public static Guid ToGuid(this object obj) {
            return obj != null ? obj.ToGuid(default(Guid)) : default(Guid);
        }

        /// <summary>
        /// Converts the string to a Guid
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="defaultValue">value to return if string is invalid</param>
        /// <returns>Guid or defaultValue</returns>
        public static Guid ToGuid(this object obj, Guid defaultValue) {
            if (obj == null)
                return defaultValue;
            else if (obj is Guid)
                return (Guid)obj;
            else {
                // Construct the default output
                Guid output = defaultValue;
                var str = obj.ToString();
                // Make sure the string is the correct length
                if (str != null && str.Length == 36) {
                    try {
                        output = new Guid(str);
                    } catch (FormatException) {

                    }
                }
                // Return the output
                return output;
            }
        }
        #endregion

        #region ToInt
        /// <summary>
        /// Converts the object to an integer (using int.TryParse(..)). If the string is not a valid number, 0 is returned
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static int ToInt(this object obj) {
            return obj.ToInt(0);
        }

        /// <summary>
        /// Converts the object to an integer (using int.TryParse(..)). If the string is not a valid number, defaultValue is returned
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static int ToInt(this object obj, int defaultValue) {
            int output = defaultValue;
            if (obj == null)
                return output;
            else if (obj is int)
                return (int)obj;
            else if (int.TryParse(obj.ToString(), out output))
                return output;
            else {
                // Try decimal
                decimal? dec = obj.ToNullableDecimal();
                if (dec.HasValue)
                    // Round to int
                    return (int)Math.Round(dec.Value, 0);
                else
                    // Return the default value
                    return defaultValue;
            }
        }

        /// <summary>
        /// Converts the object to a nullable integer (using int.TryParse(..)). If the string is not a valid number, null is returned
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static int? ToNullableInt(this object obj) {
            if (obj == null)
                return null;
            else if (obj is int)
                return (int)obj;
            else {
                int output = 0;
                if (int.TryParse(obj.ToString(), out output))
                    return output;
                else
                    return null;
            }
        }
        #endregion

        #region ToDecimal
        /// <summary>
        /// Converts the object to an decimal (using decimal.TryParse(..)). If the string is not a valid number, 0 is returned
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static decimal ToDecimal(this object obj) {
            return obj.ToDecimal(0.0m);
        }

        /// <summary>
        /// Converts the object to a decimal (using decimal.TryParse(..)). If the string is not a valid number, defaultValue is returned
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static decimal ToDecimal(this object obj, decimal defaultValue) {
            decimal output = defaultValue;
            if (obj == null)
                return output;
            else if (obj is decimal)
                return (decimal)obj;
            else if (decimal.TryParse(obj.ToString(), out output))
                return output;
            else
                return defaultValue;
        }

        /// <summary>
        /// Converts the string to a nullable decimal (using decimal.TryParse(..)). If the string is not a valid number, null is returned
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static decimal? ToNullableDecimal(this object obj) {
            decimal output = 0;
            if (obj == null)
                return null;
            else if (obj is decimal)
                return (decimal)obj;
            else if (decimal.TryParse(obj.ToString(), out output))
                return output;
            else
                return null;
        }
        #endregion

		#region ToDictionary
		public static IDictionary<string, object> ToDictionary(this object source) {
			IDictionary<string, object> dict;

			try {
				dict = (IDictionary<string, object>) source;
			} catch (Exception) {
				dict = source.ToDictionary<object>();
			}

			return dict;
		}

		public static IDictionary<string, T> ToDictionary<T>(this object source) {
			if (source == null)
				ThrowExceptionWhenSourceArgumentIsNull();

			var dictionary = new Dictionary<string, T>();
			foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(source))
				AddPropertyToDictionary<T>(property, source, dictionary);
			return dictionary;
		}

		private static void AddPropertyToDictionary<T>(PropertyDescriptor property, object source, Dictionary<string, T> dictionary) {
			object value = property.GetValue(source);
			if (IsOfType<T>(value))
				dictionary.Add(property.Name, (T)value);
		}

		private static bool IsOfType<T>(object value) {
			return value is T;
		}

		private static void ThrowExceptionWhenSourceArgumentIsNull() {
			throw new ArgumentNullException("source", "Unable to convert object to a dictionary. The source object is null.");
		}
		#endregion
	}

	public static class DynamicExtensions {
		public static dynamic ToDynamic(this object value) {
			IDictionary<string, object> expando = new ExpandoObject();

			foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(value.GetType()))
				expando.Add(property.Name, property.GetValue(value));

			return expando as ExpandoObject;
		}
	}
}
