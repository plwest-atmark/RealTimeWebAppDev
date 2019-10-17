using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Text;

namespace ASC.Utilities
{
    public static class SessionExtensions
    {

        /// <summary>
        /// This is simply an exension method on the ISession to be able to use a method called "SetSession"
        /// and have it do the hard work of encoding and serializing the object.
        /// 
        /// We could use ".Set(key, Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(value)))" every time we
        /// need to set the session in our web application, but creating this extension method makes our life
        /// easier when it comes to coding.
        /// </summary>
        /// <param name="session"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void SetSession(this ISession session, string key, object value)
        {
            session.Set(key, Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(value)));
        }

        /// <summary>
        /// As with SetSession, this is simply a means to make our coding easier for the rest of the application
        /// by extending the ISession to use GetSession<T> instead of having to code everything anytime we need
        /// to get the session information.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="session"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T GetSession<T>(this ISession session, string key)
        {
            byte[] value;
            if (session.TryGetValue(key, out value))
            {
                return JsonConvert.DeserializeObject<T>(Encoding.ASCII.GetString(value));
            }
            else
            {
                return default(T);
            }
        }
    }
}
