using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace DungeonQuest.Trails
{
    /// <summary>
    /// Self-contained networking for the trail-config module. It follows the
    /// SAME pattern as the project's ApiClient (coroutine + UnityWebRequest +
    /// Newtonsoft.Json + the { "response": ... } envelope) so it can be folded
    /// into ApiClient later if you prefer a single client.
    ///
    /// To route through your existing client instead, replace the body of
    /// <see cref="Send{T}"/> with a call to ApiClient.Instance and keep the
    /// same (method, url, body, onSuccess, onError) shape.
    /// </summary>
    public static class TrailApiService
    {
        // Single source of truth for the API host (same backend as ApiClient).
        public const string BaseUrl = "https://dungeon-quest-api.fly.dev";

        /// <summary>
        /// Generic request. T is the INNER payload type (envelope is unwrapped).
        /// Run with StartCoroutine from any MonoBehaviour.
        /// </summary>
        public static IEnumerator Send<T>(string method, string url, object body,
                                          Action<T> onSuccess, Action<string> onError)
        {
            using (var req = new UnityWebRequest(url, method))
            {
                req.downloadHandler = new DownloadHandlerBuffer();

                if (body != null)
                {
                    string json = JsonConvert.SerializeObject(body);
                    req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
                    req.SetRequestHeader("Content-Type", "application/json");
                }
                req.SetRequestHeader("Accept", "application/json");

                yield return req.SendWebRequest();

                if (req.result != UnityWebRequest.Result.Success)
                {
                    onError?.Invoke($"{(int)req.responseCode} {req.error} :: {req.downloadHandler.text}");
                    yield break;
                }

                try
                {
                    var env = JsonConvert.DeserializeObject<ApiEnvelope<T>>(req.downloadHandler.text);
                    onSuccess?.Invoke(env != null ? env.response : default);
                }
                catch (Exception e)
                {
                    onError?.Invoke("Falha ao interpretar resposta: " + e.Message);
                }
            }
        }

        /// <summary>Loads all tags to populate the phase theme dropdowns.</summary>
        public static IEnumerator LoadTags(Action<List<TagOption>> onSuccess, Action<string> onError)
        {
            return Send<List<TagOption>>(UnityWebRequest.kHttpVerbGET,
                                         BaseUrl + "/api/tags?limit=100", null, onSuccess, onError);
        }

        /// <summary>
        /// Loads every question once and tallies how many exist per tag_id.
        /// Used for a soft (non-blocking) "not enough questions" warning.
        /// </summary>
        public static IEnumerator LoadQuestionCountsByTag(Action<Dictionary<string, int>> onSuccess,
                                                          Action<string> onError)
        {
            return Send<List<QuestionTagInfo>>(UnityWebRequest.kHttpVerbGET,
                BaseUrl + "/api/questions?limit=100", null,
                questions =>
                {
                    var counts = new Dictionary<string, int>();
                    if (questions != null)
                    {
                        foreach (var q in questions)
                        {
                            if (q.tag_ids == null) continue;
                            foreach (var t in q.tag_ids)
                                counts[t] = counts.TryGetValue(t, out int c) ? c + 1 : 1;
                        }
                    }
                    onSuccess?.Invoke(counts);
                },
                onError);
        }
    }
}
