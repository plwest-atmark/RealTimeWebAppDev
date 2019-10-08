using Newtonsoft.Json;
using System;

namespace ASC.Utilities
{
    public static class ObjectExtension
    {

        /// <summary>
        /// This is an extension method on the "object" class that will allow us to "copy".
        /// 
        /// The method takes any object of any types, creates a string JSON of that type and
        /// then creates a NEW object exactly like the old one, but it's a new object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objSource"></param>
        /// <returns></returns>
        public static T CopyObject<T>(this object objSource)
        {
            var serialized = JsonConvert.SerializeObject(objSource);
            return JsonConvert.DeserializeObject<T>(serialized);
        }
    }
}
