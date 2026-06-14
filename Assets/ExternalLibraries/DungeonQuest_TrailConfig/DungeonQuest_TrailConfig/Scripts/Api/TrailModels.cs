using System.Collections.Generic;
using Newtonsoft.Json;

namespace DungeonQuest.Trails
{
    // ---------------------------------------------------------------------
    // Trilha (trail) domain models.
    // Field names are snake_case so they round-trip directly with the
    // FastAPI envelope convention used by the rest of the project
    // ({ "response": ... }). They are also used as-is for local JSON storage.
    // ---------------------------------------------------------------------

    [System.Serializable]
    public class TrailDto
    {
        public string id;                       // null on create; assigned by repo/backend
        public string name;
        public List<TrailPhaseDto> phases = new List<TrailPhaseDto>();
    }

    [System.Serializable]
    public class TrailPhaseDto
    {
        public int order;                       // 1-based position in the trail
        public string tag_id;                   // theme -> /api/tags id
        public string tag_label;                // denormalized for display/editing
        public int question_count = 5;          // how many questions this phase draws
        public string enemy_name;               // e.g. "Esqueleto", "Goblin", "Dragão"
        public bool is_boss;                    // highlights the final fight
    }

    // ---------------------------------------------------------------------
    // Lightweight read models. Deliberately self-contained (NOT the project's
    // ApiModels DTOs) so this module compiles regardless of those names.
    // The JSON shapes come straight from the live OpenAPI spec.
    // ---------------------------------------------------------------------

    // Standard project envelope: every response is wrapped in { "response": ... }
    public class ApiEnvelope<T>
    {
        [JsonProperty("response")] public T response;
    }

    // /api/tags item -> { "id": "...", "label": "historia" }
    public class TagOption
    {
        public string id;
        public string label;
    }

    // /api/questions item (only the field we need to tally availability)
    public class QuestionTagInfo
    {
        public List<string> tag_ids = new List<string>();
    }
}
