using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Helpers.Reflection
{
    public static class ReflectionHelper
    {
        public static bool IsPrimitiveType(this PropertyInfo Property)
        {
            Type[] primitiveTypes = new Type[] 
            { 
                typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(double), typeof(float), typeof(decimal), 
                typeof(short?), typeof(ushort?), typeof(int?), typeof(uint?), typeof(long?), typeof(ulong?), typeof(double?), typeof(float?), typeof(decimal?), 
                typeof(char), typeof(string), 
                typeof(char?),
                typeof(DateTime), typeof(TimeSpan),
                typeof(DateTime?), typeof(TimeSpan?),
                typeof(bool),
                typeof(bool?)
            };
            
            Type t = Property.PropertyType;
            
            return primitiveTypes.Contains(t);
        }

        /// <summary>
        /// Copies all primitive values from one object to another.
        /// </summary>
        /// <param name="From">Object to take values from.</param>
        /// <param name="To">Object to copy values to.</param>
        public static void CopyTo(this object From, object To, string[] SkipProperties = null)
        {
            if (From == null)
            {
                throw new ArgumentNullException("From");
            }

            if (To == null)
            {
                throw new ArgumentNullException("To");
            }

            if (SkipProperties == null)
            {
                SkipProperties = new string[] { };
            }

            foreach (PropertyInfo piOld in From.GetType().GetProperties())
            {
                if (!piOld.IsPrimitiveType())
                {
                    continue;
                }

                if (SkipProperties.Contains(piOld.Name))
                {
                    continue;
                }

                PropertyInfo piNew = To.GetType().GetProperty(piOld.Name);

                if (piNew == null)
                {
                    continue;
                }

                if (!piNew.CanWrite)
                {
                    continue;
                }

                piNew.SetValue(To, piOld.GetValue(From, null), null);
            }
        }

        /// <summary>
        /// Copies all primitive values from one object to another.
        /// </summary>
        /// <param name="From">Object to take values from.</param>
        /// <param name="To">Object to copy values to.</param>
        /// <param name="SkipProperties">Semicolon separated properties to skip.</param>
        public static void CopyTo(this object From, object To, string SkipProperties)
        {
            if (SkipProperties.IsNullOrEmpty())
            {
                From.CopyTo(To);
                return;
            }

            From.CopyTo(To, SkipProperties.Split(";"));
        }

        public static object GetValue(this object From, string Property)
        {
            List<string> property = Property.Split(".").ToList();
            return GetValue(From, property);
        }

        public static object GetValue(this object From, List<string> Property)
        {
            if (From == null || Property == null || !Property.Any())
            {
                return null;
            }

            string p = Property[0];
            Type fromType = From.GetType();
            PropertyInfo pi = fromType.GetProperty(p);

            if (pi == null)
            {
                throw new Exception(string.Format("There is no property {0} in type {1}!", p, fromType.FullName));
            }

            object v = pi.GetValue(From, null);

            Property.RemoveAt(0);

            if (Property.Any())
            {
                return GetValue(v, Property);
            }

            return v;
        }

        public static void SetValue(this object From, string Property, object Value)
        {
            List<string> property = Property.Split(".").ToList();
            SetValue(From, property, Value);
        }

        public static void SetValue(this object From, List<string> Property, object Value)
        {
            if (From == null || Property == null || !Property.Any())
            {
                return;
            }

            string p = Property[0];
            PropertyInfo pi = From.GetType().GetProperty(p);

            Property.RemoveAt(0);

            if (Property.Any())
            {
                object v = pi.GetValue(From, null);
                SetValue(v, Property, Value);
            }
            else
            {
                pi.SetValue(From, Value, null);
            }
        }

        public static Dictionary<string, object> ToDictionary(this object From)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();

            if (From == null)
            {
                return result;
            }

            foreach (PropertyInfo pi in From.GetType().GetProperties())
            {
                if (!pi.CanRead)
                {
                    continue;
                }

                result.Add(pi.Name, pi.GetValue(From, null));
            }

            return result;
        }
    }
}
