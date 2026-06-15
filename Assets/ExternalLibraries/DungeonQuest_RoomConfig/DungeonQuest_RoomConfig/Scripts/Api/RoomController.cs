using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// Controller "puro" das Salas, no mesmo molde do UserController:
/// HttpClient estático + async/await + envelope { "response" } + Newtonsoft.
/// </summary>
public class RoomController
{
    private const string BaseUrl = "https://dungeon-quest-api.fly.dev/api";
    private static readonly HttpClient HttpClient = new();

    /// <summary>Tags (disciplinas) para o seletor da sala.</summary>
    public async Task<List<TagModel>> GetTagsAsync()
    {
        var tags = await GetAsync<List<TagModel>>("/tags?limit=100");
        return tags ?? new List<TagModel>();
    }

    /// <summary>Cria a sala. O backend gera o "code" e o devolve no RoomModel.</summary>
    public async Task<RoomModel> CreateRoomAsync(string title, string ownerId, int levelQuantity, string tagTarget)
    {
        return await PostAsync<RoomModel>("/rooms", new
        {
            title = title,
            owner = ownerId,
            level_quantity = levelQuantity,
            tag_target = tagTarget
        });
    }

    /// <summary>Busca uma sala pelo código (aba "Entrar").</summary>
    public async Task<RoomModel> GetRoomByCodeAsync(string code)
    {
        return await GetAsync<RoomModel>("/rooms/code/" + code);
    }

    /// <summary>Adiciona o usuário logado à sala (entrar de fato).</summary>
    public async Task<bool> AddUserToRoomAsync(string roomId, string userId, int score = 0)
    {
        return await PostOkAsync($"/rooms/{roomId}/users", new { user_id = userId, score = score });
    }

    // ---- helpers HTTP (desembrulham o envelope) ----

    private async Task<T> GetAsync<T>(string endpoint)
    {
        try
        {
            var response = await HttpClient.GetAsync(BaseUrl + endpoint);
            if (!response.IsSuccessStatusCode)
            {
                Debug.LogWarning($"[Room] GET {endpoint} -> {response.StatusCode}");
                return default;
            }
            string json = await response.Content.ReadAsStringAsync();
            var envelope = JsonConvert.DeserializeObject<ApiResponse<T>>(json);
            return envelope != null ? envelope.response : default;
        }
        catch (Exception e)
        {
            Debug.LogError($"[Room] Erro GET {endpoint}: {e}");
            return default;
        }
    }

    private async Task<T> PostAsync<T>(string endpoint, object body)
    {
        try
        {
            string json = JsonConvert.SerializeObject(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await HttpClient.PostAsync(BaseUrl + endpoint, content);
            if (!response.IsSuccessStatusCode)
            {
                Debug.LogWarning($"[Room] POST {endpoint} -> {response.StatusCode}");
                return default;
            }
            string respJson = await response.Content.ReadAsStringAsync();
            var envelope = JsonConvert.DeserializeObject<ApiResponse<T>>(respJson);
            return envelope != null ? envelope.response : default;
        }
        catch (Exception e)
        {
            Debug.LogError($"[Room] Erro POST {endpoint}: {e}");
            return default;
        }
    }

    // POST que só precisa saber se deu certo (sem desserializar o corpo).
    private async Task<bool> PostOkAsync(string endpoint, object body)
    {
        try
        {
            string json = JsonConvert.SerializeObject(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await HttpClient.PostAsync(BaseUrl + endpoint, content);
            if (!response.IsSuccessStatusCode)
                Debug.LogWarning($"[Room] POST {endpoint} -> {response.StatusCode}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception e)
        {
            Debug.LogError($"[Room] Erro POST {endpoint}: {e}");
            return false;
        }
    }
}
