using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Imas.UI
{
    class Deserialiser
    {
        private static readonly Dictionary<Type, Func<Binary, object>> deserialiseMethods = new Dictionary<Type, Func<Binary, object>>
        {
            { typeof(byte), binary => binary.ReadByte() },
            { typeof(uint), binary => binary.ReadUInt32() },
            { typeof(int), binary => binary.ReadInt32() },
            { typeof(float), binary => binary.ReadFloat() },
        };

        public static object Deserialise(Binary binary, Type type)
        {
            if (deserialiseMethods.TryGetValue(type, out var method))
            {
                return method(binary);
            }
            else
                return DeserialiseClass(binary, type);
        }

        public static object DeserialiseClass(Binary binary, Type type)
        {
            if (type.GetCustomAttributes(typeof(SerialisationBaseTypeAttribute), false).Length > 0)
            {
                return DeserialiseBaseClass(binary, type);
            }
            object newObject = Activator.CreateInstance(type);
            SetProperties(binary, type, newObject);
            return newObject;
        }

        private static object DeserialiseBaseClass(Binary binary, Type type)
        {
            int derivedTypeID = binary.ReadInt32();
            var rightType = type.Assembly.GetTypes()
                .Where(dType => dType != type
                && type.IsAssignableFrom(dType)
                && IsDerivedWithID(dType, derivedTypeID))
                .ToArray();
            if (rightType.Length == 0)
            {
                throw new NotSupportedException($"A derived class with type ID {derivedTypeID} could not be found.");
            }
            if (rightType.Length > 1)
            {
                throw new Exception("Multiple classes have the same type ID.");
            }
            return DeserialiseClass(binary, rightType[0]);
        }

        private static bool IsDerivedWithID(Type dType, int derivedTypeID)
        {
            object[] attributes = dType.GetCustomAttributes(typeof(SerialisationDerivedTypeAttribute), false);
            return attributes.Length > 0 && ((SerialisationDerivedTypeAttribute)attributes[0]).DerivedTypeID == derivedTypeID;
        }

        private static void SetProperties(Binary binary, Type objType, object newObject)
        {
            var props = objType.GetProperties();
            foreach ((var prop, var attr) in props
                .Select(prop => (prop, attrs: prop.GetCustomAttributes(typeof(SerialisePropertyAttribute), true)))
                .Where(tuple => tuple.attrs.Length == 1)
                .Select(tuple => (tuple.prop, attr: (SerialisePropertyAttribute)tuple.attrs[0]))
                .OrderBy(tuple => tuple.attr.Order))
            {
                SetProperty(binary, objType, newObject, prop, attr);
            }
        }

        private static void SetProperty(Binary binary, Type objType, object newObject, PropertyInfo prop, SerialisePropertyAttribute attribute)
        {
            if (attribute.ConditionProperty != null)
            {
                var conditionProperty = objType.GetProperty(attribute.ConditionProperty);
                if (conditionProperty == null)
                    throw new Exception($"Property {attribute.ConditionProperty} was not found.");
                if (conditionProperty.PropertyType != typeof(bool))
                    throw new Exception($"Property {attribute.ConditionProperty} is not of type bool.");
                if (!(bool)conditionProperty.GetValue(newObject))
                {
                    return;
                }
            }
            if (prop.PropertyType.IsArray)
            {
                int count = GetCount(objType, newObject, attribute);
                prop.SetValue(newObject, DeserialiseArray(binary, prop.PropertyType, count));
                return;
            }
            if (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
            {
                int count = GetCount(objType, newObject, attribute);
                prop.SetValue(newObject, DeserialiseList(binary, prop.PropertyType, count));
                return;
            }
            prop.SetValue(newObject, Deserialise(binary, prop.PropertyType));
        }

        private static int GetCount(Type objType, object newObject, SerialisePropertyAttribute attribute)
        {
            if (attribute.CountProperty != null)
            {
                var countProperty = objType.GetProperty(attribute.CountProperty);
                if (countProperty == null)
                    throw new Exception($"Property {attribute.CountProperty} was not found.");
                if (countProperty.PropertyType != typeof(int))
                    throw new Exception($"Property {attribute.CountProperty} is not of type int.");
                return (int)countProperty.GetValue(newObject);
            }
            return attribute.FixedCount;
        }

        public static Array DeserialiseArray(Binary binary, Type arrayType, int count)
        {
            Type elementType = arrayType.GetElementType();
            Array newArray = Array.CreateInstance(elementType, count);
            for (int i = 0; i < count; i++)
            {
                newArray.SetValue(Deserialise(binary, elementType), i);
            }
            return newArray;

        }

        public static object DeserialiseList(Binary binary, Type listType, int count)
        {
            //Type listType = typeof(List<>).MakeGenericType(genericParameter);
            Type genericParameter = listType.GenericTypeArguments[0];
            object newList = Activator.CreateInstance(listType, count);
            for (int i = 0; i < count; i++)
            {
                listType.GetMethod("Add").Invoke(newList, new object[] { Deserialise(binary, genericParameter) });
            }
            return newList;
        }
    }
}
