using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>Controller "puro" da tela Jogar: carrega as disciplinas (tags).</summary>
public class PlaySetupController
{
    private const string BaseUrl = "https://dungeon-quest-api.fly.dev/api";
    private static readonly HttpClient HttpClient = new();
    public static List<TagModel> allTagsLoaded = new();
    public static TagModel SelectedTag;

    public async Task<List<TagModel>> GetTagsAsync()
    {
        var tags = await GetAsync<List<TagModel>>("/tags?limit=100");
        allTagsLoaded = tags;
        return tags ?? new List<TagModel>();
    }

    private async Task<T> GetAsync<T>(string endpoint)
    {
        try
        {
            var response = await HttpClient.GetAsync(BaseUrl + endpoint);
            if (!response.IsSuccessStatusCode)
            {
                Debug.LogWarning($"[Play] GET {endpoint} -> {response.StatusCode}");
                return default;
            }
            string json = await response.Content.ReadAsStringAsync();
            var envelope = JsonConvert.DeserializeObject<ApiResponse<T>>(json);
            return envelope != null ? envelope.response : default;
        }
        catch (Exception e)
        {
            Debug.LogError($"[Play] Erro GET {endpoint}: {e}");
            return default;
        }
    }
}
