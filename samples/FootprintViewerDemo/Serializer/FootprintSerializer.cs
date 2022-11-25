using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FootprintViewerDemo.Models;
using Newtonsoft.Json;

namespace FootprintViewerDemo;

public class FootprintSerializer
{
    private readonly string? _path;

    public FootprintSerializer(string path)
    {
        _path = path;
    }

    public async Task<IList<object>> GetValuesAsync()
    {
        if (string.IsNullOrEmpty(_path) == false)
        {
            return await Task.Run(() => GetValues<Footprint>(_path));
        }

        return new List<object>();
    }

    private static IList<object> GetValues<T>(string path)
    {
        return DeserializeFrom<T>(path).Cast<object>().ToList();
    }

    private static IList<T> DeserializeFrom<T>(string path, JsonSerializerSettings? settings = null)
    {
        using var file = File.OpenText(path);

        // file with GeoJSON
        var serializer = NetTopologySuite.IO.GeoJsonSerializer.CreateDefault(settings);

        return (IList<T>)(serializer.Deserialize(file, typeof(IList<T>)) ?? new List<T>());
    }
}
