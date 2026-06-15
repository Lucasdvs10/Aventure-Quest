using System;

// ============================================================================
// Modelo de Sala (/api/rooms). O backend gera o "code" ao criar.
//
// ApiResponse<T> e TagModel JÁ existem em QuestionModels.cs (módulo de perguntas),
// então NÃO são redefinidos aqui para não dar classe duplicada.
// Se for usar ESTE módulo sozinho, descomente o bloco no fim do arquivo.
// ============================================================================

[Serializable]
public class RoomModel
{
    public string id;
    public string title;
    public string code;             // gerado pelo backend (ex.: "QUIZ123")
    public string created_at;
    public string owner;            // user_id do criador
    public string question_pack_id;
    public int level_quantity;      // quantidade de inimigos/fases
    public string tag_target;       // disciplina (label) ou "variado"
}

// --- Use apenas se este módulo for importado SEM o de perguntas ---
// Lembre de adicionar no topo: using Newtonsoft.Json;
//
// [Serializable]
// public class ApiResponse<T> { [JsonProperty("response")] public T response; }
//
// [Serializable]
// public class TagModel { public string id; public string label; }
