
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AzureStorageTest
{
    using Newtonsoft.Json;
    public static class ObjectExtension
    {
        public static T CopyObject<T>(this object objSource)
        {
            var serialized = JsonConvert.SerializeObject(objSource);
            return JsonConvert.DeserializeObject<T>(serialized);
        }
    }
}
