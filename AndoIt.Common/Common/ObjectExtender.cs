using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace AndoIt.Common
{
    public static class ObjectExtender
    {
        public static string ObjectToJsonString(this object extended) {
            string result = JsonConvert.SerializeObject(extended,
                new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });
            return result;
        }

        public static Dictionary<string, object> GetKeyValue(this object obj, string prefix = "", HashSet<object> visitedObjects = null)
        {
            var keyValuePairs = new Dictionary<string, object>();

            if (obj == null)
                return keyValuePairs;

            // Si es la primera llamada, inicializar el conjunto de objetos visitados
            visitedObjects ??= new HashSet<object>(new ReferenceEqualityComparer());

            // Si ya hemos visitado este objeto, evitamos un bucle infinito
            if (!visitedObjects.Add(obj))
                return keyValuePairs;
            //if (visitedObjects.Contains(obj))
            //    return keyValuePairs;
            //else 
            //    visitedObjects.Add(obj);

            Type objType = obj.GetType();

            if (objType.IsPrimitive || obj is string || obj is decimal)
            {
                keyValuePairs[prefix] = obj;
                return keyValuePairs;
            }

            if (obj is IEnumerable enumerable && !(obj is string))
            {
                int index = 0;
                foreach (var item in enumerable)
                {
                    foreach (var kvp in item.GetKeyValue($"{prefix}[{index}]", visitedObjects))
                    {
                        keyValuePairs[kvp.Key] = kvp.Value;
                    }
                    index++;
                }
                return keyValuePairs;
            }

            foreach (PropertyInfo prop in objType.GetProperties())
            {
                string key = string.IsNullOrEmpty(prefix) ? prop.Name : $"{prefix}.{prop.Name}";
                object value = prop.GetValue(obj);

                if (value == null || value.GetType().IsPrimitive || value is string || value is decimal)
                {
                    keyValuePairs[key] = value;
                }
                else
                {
                    foreach (var kvp in value.GetKeyValue(key, visitedObjects))
                    {
                        keyValuePairs[kvp.Key] = kvp.Value;
                    }
                }
            }

            return keyValuePairs;
        }
    }
}
