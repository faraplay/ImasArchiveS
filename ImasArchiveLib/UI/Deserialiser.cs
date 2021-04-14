using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Imas.UI
{
    class Deserialiser
    {
        Dictionary<Type, Func<Binary, object>> deserialiseMethods = new Dictionary<Type, Func<Binary, object>>
        {
            { typeof(byte), binary => binary.ReadByte() },
            { typeof(int), binary => binary.ReadInt32() },
            { typeof(float), binary => binary.ReadFloat() },
        };

        public object Deserialise(Binary binary, Type type)
        {
            if (deserialiseMethods.TryGetValue(type, out var method))
            {
                return method(binary);
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                return DeserialiseList(binary, type);
            }
            else
                return DeserialiseClass(binary, type);
        }

        public object DeserialiseClass(Binary binary, Type type)
        {
            if (type.GetCustomAttributes(typeof(SerialisationBaseTypeAttribute), false).Length > 0)
            {
                return DeserialiseBaseClass(binary, type);
            }
            object newObject = Activator.CreateInstance(type);
            SetFields(binary, type, newObject);
            return newObject;
        }

        private object DeserialiseBaseClass(Binary binary, Type type)
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

        private bool IsDerivedWithID(Type dType, int derivedTypeID)
        {
            object[] attributes = dType.GetCustomAttributes(typeof(SerialisationDerivedTypeAttribute), false);
            return attributes.Length > 0 && ((SerialisationDerivedTypeAttribute)attributes[0]).DerivedTypeID == derivedTypeID;
        }

        private void SetFields(Binary binary, Type type, object newObject)
        {
            var fields = type.GetFields();
            foreach (var tuple in fields
                .Select(field => (field, field.GetCustomAttributes(typeof(SerialiseFieldAttribute), true)))
                .Where(tuple => tuple.Item2.Length == 1)
                .Select(tuple => (tuple.field, (SerialiseFieldAttribute)tuple.Item2[0]))
                .OrderBy(tuple => tuple.Item2.Order))
            {
                if (tuple.Item2.Condition != null)
                {
                    var conditionProperty = type.GetProperty(tuple.Item2.Condition);
                    if (conditionProperty != null
                        && conditionProperty.PropertyType == typeof(bool)
                        && !(bool)conditionProperty.GetMethod.Invoke(newObject, null))
                    {
                        continue;
                    }
                }
                if (tuple.field.FieldType.IsArray)
                {
                    tuple.field.SetValue(newObject, DeserialiseArray(binary, tuple.field.FieldType, tuple.Item2.ArraySize));
                }
                else
                {
                    tuple.field.SetValue(newObject, Deserialise(binary, tuple.field.FieldType));
                }
            }
        }

        public Array DeserialiseArray(Binary binary, Type arrayType, int count)
        {
            Type elementType = arrayType.GetElementType();
            Array newArray = Array.CreateInstance(elementType, count);
            for (int i = 0; i < count; i++)
            {
                newArray.SetValue(Deserialise(binary, elementType), i);
            }
            return newArray;

        }

        public object DeserialiseList(Binary binary, Type listType)
        {
            //Type listType = typeof(List<>).MakeGenericType(genericParameter);
            Type genericParameter = listType.GenericTypeArguments[0];
            int count = binary.ReadInt32();
            object newList = Activator.CreateInstance(listType, count);
            for (int i = 0; i < count; i++)
            {
                listType.GetMethod("Add").Invoke(newList, new object[] { Deserialise(binary, genericParameter) });
            }
            return newList;
        }
    }
}
