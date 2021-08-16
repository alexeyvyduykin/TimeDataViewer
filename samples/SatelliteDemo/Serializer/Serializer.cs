using Newtonsoft.Json;

namespace SatelliteDemo
{
    public class Serializer
    {
        private readonly JsonSerializerSettings _settings;

        public Serializer()
        {
            _settings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Objects,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                NullValueHandling = NullValueHandling.Ignore,
            };
        }

        public string Serialize<T>(T value)
        {
            return JsonConvert.SerializeObject(value, _settings);
        }

        public T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, _settings);
        }

        public string SerializerWithSettings<T>(T value)
        {
            return JsonConvert.SerializeObject(value, _settings);
        }

        public T DeserializeWithSettings<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, _settings);
        }
    }
}
