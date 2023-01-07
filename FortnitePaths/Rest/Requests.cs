using FortnitePaths.Managers;
using FortnitePaths.Rest.Models;
using RestSharp;

namespace FortnitePaths.Rest;

public static class Requests
{
    private const string _mappingsEndpoint = "https://fortnitecentral.genxgames.gg/api/v1/mappings";
    private const string _aesEndpoint = "https://fortnitecentral.genxgames.gg/api/v1/aes";

    public static MappingsResponse[] TryGetMappings()
    {
        RestClient _client = new RestClient();

        var request = new RestRequest(_mappingsEndpoint, Method.Get);
        var response = _client.Execute<MappingsResponse[]>(request);

        if (!response.IsSuccessStatusCode || string.IsNullOrEmpty(response.Content))
        {
            Console.WriteLine("Unsuccessfull mappings response.");
            Console.ReadKey();
        }

        MappingsResponse[] mappings = response.Data;
        return mappings;
    }

    public static void downloadMappings(string url, string mappingName)
    {
        RestClient _client = new RestClient();

        var request = new RestRequest(url, Method.Get);
        var response = _client.DownloadData(request);

        File.WriteAllBytes(Path.Combine(DirectoryManager.mappingsDir, mappingName), response);
        Console.WriteLine("Mappings downloaded and saved in .mappings folder.");
    }

    public static AesKeys GetAesKeys()
    {
        RestClient _client = new RestClient();

        var request = new RestRequest(_aesEndpoint, Method.Get);
        var response = _client.Execute<AesKeys>(request);

        if (!response.IsSuccessStatusCode || string.IsNullOrEmpty(response.Content)) return new AesKeys();

        AesKeys keys = response.Data;
        return keys;
    }
}
