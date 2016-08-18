using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jovian.BigMap.classes
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    /// <summary>
    /// LPY 2015-9-17 添加
    /// </summary>
    class JsonHelper
    {
        /// <summary>
        /// Json序列化
        /// </summary>
        public static string ToJson(object item)
        {
            return JsonConvert.SerializeObject(item);
        }

        /// <summary>
        /// Json反序列化
        /// </summary>
        public static T FromJson<T>(string jsonString)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(jsonString);
            }
            catch (Exception)
            {
                return default(T);
            }
        }

        /// <summary>
        /// Json反序列化
        /// </summary>
        public static object FromJson(Type type, string jsonString)
        {
            return JsonConvert.DeserializeObject(jsonString, type);
        }
    }
}
