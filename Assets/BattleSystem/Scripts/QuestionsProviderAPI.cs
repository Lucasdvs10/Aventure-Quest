using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class QuestionsProviderAPI : IQuestionsProvider
{
    public Task<List<Question>> GetQuestionsAsync()
    {
        Debug.LogWarning(
            "QuestionsProviderAPI ainda não foi implementado. Retornando lista vazia."
        );

        return Task.FromResult(new List<Question>());
    }

    public Task SaveQuestionAsync(Question newQuestion)
    {
        Debug.LogWarning(
            "QuestionsProviderAPI ainda não foi implementado."
        );

        return Task.CompletedTask;
    }
}
