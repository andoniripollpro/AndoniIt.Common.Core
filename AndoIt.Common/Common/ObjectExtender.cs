using Newtonsoft.Json;

namespace AndoIt.Common.Core.Common
{
    public static class ObjectExtender
    {
        public static string ObjectToJsonString(this object extended) {
            string result = JsonConvert.SerializeObject(extended);
            return result;
        }
    }
}
