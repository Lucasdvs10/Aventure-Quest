using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace DungeonQuest.Trails
{
    /// <summary>
    /// Stores trails as JSON in Application.persistentDataPath/trails.json.
    /// This is the DEFAULT repository: it works today, with no backend changes.
    /// Replace with RemoteTrailRepository once /api/trilhas exists.
    /// </summary>
    public class LocalTrailRepository : ITrailRepository
    {
        private static string FilePath => Path.Combine(Application.persistentDataPath, "trails.json");

        public IEnumerator SaveTrail(TrailDto trail, Action<TrailDto> onSuccess, Action<string> onError)
        {
            TrailDto saved = null;
            string error = null;
            try
            {
                List<TrailDto> all = ReadAll();
                if (string.IsNullOrEmpty(trail.id))
                    trail.id = Guid.NewGuid().ToString();

                all.RemoveAll(t => t.id == trail.id);   // upsert
                all.Add(trail);

                File.WriteAllText(FilePath, JsonConvert.SerializeObject(all, Formatting.Indented));
                saved = trail;
                Debug.Log("[Trails] Salvo localmente em " + FilePath);
            }
            catch (Exception e)
            {
                error = e.Message;
            }

            if (error != null) onError?.Invoke(error);
            else onSuccess?.Invoke(saved);
            yield break;
        }

        public IEnumerator LoadTrails(Action<List<TrailDto>> onSuccess, Action<string> onError)
        {
            try { onSuccess?.Invoke(ReadAll()); }
            catch (Exception e) { onError?.Invoke(e.Message); }
            yield break;
        }

        private static List<TrailDto> ReadAll()
        {
            if (!File.Exists(FilePath)) return new List<TrailDto>();
            string json = File.ReadAllText(FilePath);
            return JsonConvert.DeserializeObject<List<TrailDto>>(json) ?? new List<TrailDto>();
        }
    }
}
