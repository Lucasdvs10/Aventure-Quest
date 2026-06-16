using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// Controller "puro" do cadastro de perguntas, no mesmo molde do UserController:
/// HttpClient estático + async/await + envelope { "response" } + Newtonsoft.
///
/// DEPENDÊNCIA CIRCULAR (o ponto-chave desta tela):
///   - a PERGUNTA tem answer_id (id da alternativa correta);
///   - as ALTERNATIVAS têm question_id (id da pergunta).
/// Não dá para criar tudo de uma vez. Solução em 3 passos:
///   1) POST /questions com answer_id PLACEHOLDER  -> obtém question_id
///   2) POST /choices (uma por alternativa)        -> obtém os ids
///   3) PATCH /questions/{id} com answer_id = id da alternativa correta
///
/// ROLLBACK: se uma alternativa falhar no passo 2, a pergunta recém-criada é
/// apagada (DELETE) para não deixar pergunta órfã sem alternativas.
/// </summary>
public class QuestionController
{
    private const string BaseUrl = "https://dungeon-quest-api.fly.dev/api";
    private static readonly HttpClient HttpClient = new();

    // answer_id temporário usado na criação (igual ao exemplo do OpenAPI).
    // É substituído pelo id real da alternativa correta no passo 3 (PATCH).
    // Se a API rejeitar este valor, troque por null/omita o campo no PostAsync.
    private const string PlaceholderAnswerId = "00000000-0000-0000-0000-000000000000";

    /// <summary>Resultado do fluxo de criação (para a tela exibir o status).</summary>
    public class CreateQuestionResult
    {
        public bool ok;
        public string error;
        public string questionId;
    }

    // =====================================================================
    // Leitura
    // =====================================================================

    /// <summary>Carrega as tags (temas) para o seletor de temas da pergunta.</summary>
    public async Task<List<TagModel>> GetTagsAsync()
    {
        var tags = await GetAsync<List<TagModel>>("/tags?limit=100");
        return tags ?? new List<TagModel>();
    }

    public async Task<TagModel> CreateTagAsync(string label)
    {
        string createdBy = PlayerPrefs.GetString("CurrentUserID", "");
        if (string.IsNullOrEmpty(createdBy))
        {
            Debug.LogError("[Question] Usuário não autenticado — não dá pra criar tag.");
            return null;
        }
        return await PostAsync<TagModel>("/tags", new { label, created_by = createdBy });
    }

    // =====================================================================
    // Criação (3 passos)
    // =====================================================================

    /// <param name="choiceLabels">Rótulos das alternativas (já sem vazias), em ordem.</param>
    /// <param name="correctIndex">Índice (em choiceLabels) da alternativa correta.</param>

    public async Task<CreateQuestionResult> CreateQuestionAsync(
    string prompt,
    string explanation,
    List<string> tagIds,
    List<string> choiceLabels,
    int correctIndex)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(prompt))
            {
                return new CreateQuestionResult
                {
                    ok = false,
                    error = "Pergunta vazia."
                };
            }

            if (choiceLabels == null || choiceLabels.Count != 4)
            {
                return new CreateQuestionResult
                {
                    ok = false,
                    error = "São necessárias exatamente 4 alternativas."
                };
            }

            if (correctIndex < 0 || correctIndex >= choiceLabels.Count)
            {
                return new CreateQuestionResult
                {
                    ok = false,
                    error = "Alternativa correta inválida."
                };
            }

            string createdBy =
                PlayerPrefs.GetString(
                    "CurrentUserID",
                    ""
                );

            if (string.IsNullOrEmpty(createdBy))
            {
                return new CreateQuestionResult
                {
                    ok = false,
                    error = "Usuário não autenticado."
                };
            }

            var choices = new List<object>();

            for (int i = 0; i < choiceLabels.Count; i++)
            {
                choices.Add(new
                {
                    label = choiceLabels[i],
                    correct = i == correctIndex
                });
            }

            var body = new
            {
                prompt,

                created_by = createdBy,

                // Mantido porque a API ainda espera esse campo.

                answer_explanation = explanation,

                tags = tagIds ?? new List<string>(),

                choices
            };

            var createdQuestion =
                await PostAsync<QuestionModel>(
                    "/questions",
                    body
                );

            if (
                createdQuestion == null ||
                string.IsNullOrEmpty(createdQuestion.id)
            )
            {
                return new CreateQuestionResult
                {
                    ok = false,
                    error = "Falha ao criar pergunta."
                };
            }

            return new CreateQuestionResult
            {
                ok = true,
                questionId = createdQuestion.id
            };
        }
        catch (Exception e)
        {
            Debug.LogError(
                $"[Question] {e}"
            );

            return new CreateQuestionResult
            {
                ok = false,
                error = e.Message
            };
        }
    }

    // =====================================================================
    // Helpers HTTP genéricos (reaproveitáveis) — desembrulham o envelope
    // =====================================================================

    private async Task<T> GetAsync<T>(string endpoint)
    {
        try
        {
            var response = await HttpClient.GetAsync(BaseUrl + endpoint);
            if (!response.IsSuccessStatusCode)
            {
                Debug.LogWarning($"[Question] GET {endpoint} -> {response.StatusCode}");
                return default;
            }
            string json = await response.Content.ReadAsStringAsync();
            var envelope = JsonConvert.DeserializeObject<ApiResponse<T>>(json);
            return envelope != null ? envelope.response : default;
        }
        catch (Exception e)
        {
            Debug.LogError($"[Question] Erro GET {endpoint}: {e}");
            return default;
        }
    }

    private async Task<T> PostAsync<T>(string endpoint, object body)
    {
        try
        {
            string json =
                JsonConvert.SerializeObject(
                    body,
                    Formatting.Indented
                );

            Debug.Log($"[Question] REQUEST:\n{json}");

            var content =
                new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json"
                );

            var response =
                await HttpClient.PostAsync(
                    BaseUrl + endpoint,
                    content
                );

            string respJson =
                await response.Content
                    .ReadAsStringAsync();

            Debug.Log(
                $"[Question] STATUS: {(int)response.StatusCode}"
            );

            Debug.Log(
                $"[Question] RESPONSE:\n{respJson}"
            );

            if (!response.IsSuccessStatusCode)
            {
                Debug.LogWarning(
                    $"[Question] POST falhou"
                );

                return default;
            }

            var envelope =
                JsonConvert
                .DeserializeObject<ApiResponse<T>>(
                    respJson
                );

            return envelope.response;
        }
        catch (Exception e)
        {
            Debug.LogError(
                $"[Question] EXCEPTION:\n{e}"
            );

            return default;
        }
    }
    // PATCH via SendAsync (HttpClient.PatchAsync não existe em todas as versões do Unity).
    private async Task<T> PatchAsync<T>(string endpoint, object body)
    {
        try
        {
            string json = JsonConvert.SerializeObject(body);
            using (var request = new HttpRequestMessage(new HttpMethod("PATCH"), BaseUrl + endpoint))
            {
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await HttpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    Debug.LogWarning($"[Question] PATCH {endpoint} -> {response.StatusCode}");
                    return default;
                }
                string respJson = await response.Content.ReadAsStringAsync();
                var envelope = JsonConvert.DeserializeObject<ApiResponse<T>>(respJson);
                return envelope != null ? envelope.response : default;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[Question] Erro PATCH {endpoint}: {e}");
            return default;
        }
    }

    private async Task<bool> DeleteAsync(string endpoint)
    {
        try
        {
            var response = await HttpClient.DeleteAsync(BaseUrl + endpoint);
            return response.IsSuccessStatusCode;
        }
        catch (Exception e)
        {
            Debug.LogError($"[Question] Erro DELETE {endpoint}: {e}");
            return false;
        }
    }
}
