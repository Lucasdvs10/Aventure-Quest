using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class QuestionsProviderLocalJson : IQuestionsProvider
{
    private const string FilePath = "QuestionsDataBase.json";

    public async Task<List<Question>> GetQuestionsAsync()
    {
        string fullPath = Path.Combine(Application.persistentDataPath, FilePath);

        if (!File.Exists(fullPath))
            return new List<Question>();

        try
        {
            string jsonString = await File.ReadAllTextAsync(fullPath);

            var wrapper = JsonUtility.FromJson<QuestionsListWrapper>(jsonString);

            return wrapper?.questionslist ?? new List<Question>();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            return new List<Question>();
        }
    }

    public async Task SaveQuestionAsync(Question newQuestion)
    {
        string fullPath = Path.Combine(Application.persistentDataPath, FilePath);

        try
        {
            List<Question> questionsList = new() { newQuestion };

            string json = JsonUtility.ToJson(
                new QuestionsListWrapper(questionsList),
                true);

            await File.WriteAllTextAsync(fullPath, json);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    [Serializable]
    public class QuestionsListWrapper
    {
        public List<Question> questionslist;

        public QuestionsListWrapper(List<Question> questions)
        {
            questionslist = questions;
        }
    }
}