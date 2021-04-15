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
        private static Dictionary<Type, Action<Binary, object>> serialiseMethods = new Dictionary<Type, Action<Binary, object>>
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
            SerialiseFields(binary, type, obj);
        }

        private static void SerialiseFields(Binary binary, Type objType, object obj)
        {
            var fields = objType.GetFields();
            foreach ((var field, var attr) in fields
                .Select(field => (field, field.GetCustomAttributes(typeof(SerialiseFieldAttribute), true)))
                .Where(tuple => tuple.Item2.Length == 1)
                .Select(tuple => (tuple.field, (SerialiseFieldAttribute)tuple.Item2[0]))
                .OrderBy(tuple => tuple.Item2.Order))
            {
                SerialiseField(binary, objType, obj, field, attr);
            }
        }

        private static void SerialiseField(Binary binary, Type objType, object obj, FieldInfo field, SerialiseFieldAttribute attribute)
        {
            if (attribute.ConditionProperty != null)
            {
                var conditionProperty = objType.GetProperty(attribute.ConditionProperty);
                if (conditionProperty == null)
                    throw new Exception($"Field {attribute.ConditionProperty} was not found.");
                if (conditionProperty.PropertyType != typeof(bool))
                    throw new Exception($"Field {attribute.ConditionProperty} is not of type bool.");
                if (!(bool)conditionProperty.GetValue(obj))
                {
                    return;
                }
            }
            if (attribute.IsCountOf != null)
            {
                var collectionField = objType.GetField(attribute.IsCountOf);
                if (collectionField == null)
                    throw new Exception($"Field {attribute.IsCountOf} was not found.");
                if (field.FieldType != typeof(int))
                    throw new Exception($"Field {field} is not of type int.");
                // get count of array / list
                if (!collectionField.FieldType.IsArray
                    && !IsList(collectionField.FieldType))
                    throw new Exception($"Field {attribute.IsCountOf} is not an array or list.");
                binary.WriteInt32(((IList)collectionField.GetValue(obj)).Count);
                return;
            }
            Serialise(binary, field.GetValue(obj));
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
