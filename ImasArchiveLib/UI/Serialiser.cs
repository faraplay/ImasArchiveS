using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Imas.UI
{
    class Serialiser
    {
        private static readonly Dictionary<Type, Action<Binary, object>> serialiseMethods = new Dictionary<Type, Action<Binary, object>>
        {
            { typeof(byte), (binary, obj) => binary.WriteByte((byte)obj) },
            { typeof(uint), (binary, obj) => binary.WriteUInt32((uint)obj) },
            { typeof(int), (binary, obj) => binary.WriteInt32((int)obj) },
            { typeof(float), (binary, obj) => binary.WriteFloat((float)obj) },
        };

        public static void Serialise(Binary binary, object obj)
        {
            Type type = obj.GetType();
            if (serialiseMethods.TryGetValue(type, out var method))
            {
                method(binary, obj);
                return;
            }
            if (type.IsArray || IsList(type))
            {
                SerialiseIList(binary, obj);
                return;
            }
            SerialiseClass(binary, type, obj);
        }

        private static void SerialiseClass(Binary binary, Type type, object obj)
        {
            object[] attrs = type.GetCustomAttributes(typeof(SerialisationDerivedTypeAttribute), false);
            if (attrs.Length > 0)
            {
                binary.WriteInt32(((SerialisationDerivedTypeAttribute)attrs[0]).DerivedTypeID);
            }
            SerialiseProperties(binary, type, obj);
        }

        private static void SerialiseProperties(Binary binary, Type objType, object obj)
        {
            var props = objType.GetProperties();
            foreach ((var prop, var attr) in props
                .Select(prop => (prop, attrs: prop.GetCustomAttributes(typeof(SerialisePropertyAttribute), true)))
                .Where(tuple => tuple.attrs.Length == 1)
                .Select(tuple => (tuple.prop, attr: (SerialisePropertyAttribute)tuple.attrs[0]))
                .OrderBy(tuple => tuple.attr.Order))
            {
                SerialiseProperty(binary, objType, obj, prop, attr);
            }
        }

        private static void SerialiseProperty(Binary binary, Type objType, object obj, PropertyInfo prop, SerialisePropertyAttribute attribute)
        {
            if (attribute.ConditionProperty != null)
            {
                var conditionProperty = objType.GetProperty(attribute.ConditionProperty);
                if (conditionProperty == null)
                    throw new Exception($"Property {attribute.ConditionProperty} was not found.");
                if (conditionProperty.PropertyType != typeof(bool))
                    throw new Exception($"Property {attribute.ConditionProperty} is not of type bool.");
                if (!(bool)conditionProperty.GetValue(obj))
                {
                    return;
                }
            }
            Serialise(binary, prop.GetValue(obj));
        }

        private static bool IsList(Type type) => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);

        private static void SerialiseIList(Binary binary, object obj)
        {
            foreach (object item in obj as IList)
            {
                Serialise(binary, item);
            }
        }
    }
}
