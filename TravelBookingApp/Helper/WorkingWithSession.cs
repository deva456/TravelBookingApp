using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;

namespace TravelBooking.Helper
{
    public static class WorkingWithSession
    {
        public static void SetObjectasJson(this ISession session, string Key, object value)
        {
            session.SetString(Key, JsonConvert.SerializeObject(value));
        }
        public static T? GetObjectFromJson<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default : JsonConvert.DeserializeObject<T>(value);
        }
    }
}
