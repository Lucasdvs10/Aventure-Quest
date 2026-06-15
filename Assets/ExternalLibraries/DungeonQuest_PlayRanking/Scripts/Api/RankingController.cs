using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// Controller "puro" do ranking global: busca os usuários (GET /users) e
/// ordena por high_score (maior primeiro). Mesmo molde do UserController.
/// </summary>
public class RankingController
{
    private const string BaseUrl = "https://dungeon-quest-api.fly.dev/api";
    private static readonly HttpClient HttpClient = new();

    /// <summary>Top N (a API limita a 100 por requisição), já ordenado.</summary>
    public async Task<List<UserRankModel>> GetTopUsersAsync(int limit = 100)
    {
        if (limit > 100) limit = 100;   // teto da API
        var users = await GetAsync<List<UserRankModel>>($"/users?limit={limit}") ?? new List<UserRankModel>();
        users.Sort((a, b) => b.high_score.CompareTo(a.high_score));
        return users;
    }

    private async Task<T> GetAsync<T>(string endpoint)
    {
        try
        {
            var response = await HttpClient.GetAsync(BaseUrl + endpoint);
            if (!response.IsSuccessStatusCode)
            {
                Debug.LogWarning($"[Ranking] GET {endpoint} -> {response.StatusCode}");
                return default;
            }
            string json = await response.Content.ReadAsStringAsync();
            var envelope = JsonConvert.DeserializeObject<ApiResponse<T>>(json);
            return envelope != null ? envelope.response : default;
        }
        catch (Exception e)
        {
            Debug.LogError($"[Ranking] Erro GET {endpoint}: {e}");
            return default;
        }
    }
}
