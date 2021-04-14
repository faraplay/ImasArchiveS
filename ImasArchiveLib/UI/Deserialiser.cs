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
            var fields = type.GetFields();
            object newObject = Activator.CreateInstance(type);
            ;
            foreach (var tuple in fields
                .Select(field => (field, field.GetCustomAttributes(typeof(SerialiseFieldAttribute), true)))
                .Where(tuple => tuple.Item2.Length == 1)
                .OrderBy(tuple => ((SerialiseFieldAttribute)tuple.Item2[0]).Order))
            {
                tuple.field.SetValue(newObject, Deserialise(binary, tuple.field.FieldType));
            }
            return newObject;
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
