using System;
using System.Collections.Generic;
using Newtonsoft.Json;

// ============================================================================
// Modelos da tela de Cadastro de Perguntas. Campos em snake_case = JSON da API.
//
// Obs.: ApiResponse<T> e TagModel também aparecem no módulo de trilhas (que está
// de lado por enquanto). Se um dia juntar os dois, mantenha apenas UMA definição
// de cada (ou mova para um namespace).
// ============================================================================

/// <summary>Envelope padrão da API: { "response": ... }.</summary>
[Serializable]
public class ApiResponse<T>
{
    [JsonProperty("response")] public T response;
}

/// <summary>/api/tags -> { id, label }.</summary>
[Serializable]
public class TagModel
{
    public string id;
    public string label;
}

/// <summary>
/// /api/questions -> retorna pergunta completa.
/// </summary>
[Serializable]
public class QuestionModel
{
    public string id;

    public string prompt;

    public string answer_id;

    public string answer_explanation;

    public List<string> tag_ids = new();

    public List<ChoiceModel> choices = new();
}

/// <summary>
/// Alternativa retornada dentro de Question.
/// </summary>
[Serializable]
public class ChoiceModel
{
    public string id;

    public string label;

    public string question;
}