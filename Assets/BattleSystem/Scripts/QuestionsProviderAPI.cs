using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class QuestionsProviderAPI : IQuestionsProvider
{
    private const string BaseUrl = "https://dungeon-quest-api.fly.dev/api";
    private string QuestionsEndpoint = $"{BaseUrl}/questions";

    private static readonly HttpClient HttpClient = new();

    public async Task<List<Question>> GetQuestionsAsync()
    {
        try
        {
            string json = await HttpClient.GetStringAsync(QuestionsEndpoint);

            ApiResponse response =
                JsonUtility.FromJson<ApiResponse>(json);

            List<Question> result = new();

            if (response?.response == null)
                return result;

            foreach (QuestionApiDto apiQuestion in response.response)
            {
                if (apiQuestion.choices == null ||
                    apiQuestion.choices.Length != 4)
                {
                    Debug.LogWarning(
                        $"Pergunta ignorada: {apiQuestion.prompt}. Esperado exatamente 4 choices."
                    );

                    continue;
                }

                string correctOption = "";

                foreach (ChoiceDto choice in apiQuestion.choices)
                {
                    if (choice.id == apiQuestion.answer)
                    {
                        correctOption = choice.label;
                        break;
                    }
                }

                result.Add(
                    new Question(
                        apiQuestion.prompt,
                        apiQuestion.choices[0].label,
                        apiQuestion.choices[1].label,
                        apiQuestion.choices[2].label,
                        apiQuestion.choices[3].label,
                        correctOption,
                        apiQuestion.answer_explanation
                    )
                );
            }

            return result;
        }
        catch (Exception e)
        {
            Debug.LogError($"Erro ao carregar perguntas da API: {e}");

            return new List<Question>();
        }
    }

    public Task SaveQuestionAsync(Question newQuestion)
    {
        // Ainda não implementado
        return Task.CompletedTask;
    }

    [Serializable]
    private class ApiResponse
    {
        public QuestionApiDto[] response;
    }

    [Serializable]
    private class QuestionApiDto
    {
        public string id;
        public string prompt;
        public string answer;
        public string answer_explanation;
        public string[] tags;
        public string created_at;
        public string created_by;

        public ChoiceDto[] choices;
    }

    [Serializable]
    private class ChoiceDto
    {
        public string id;
        public string label;
        public string question;
        public string created_at;
        public string created_by;
    }
}