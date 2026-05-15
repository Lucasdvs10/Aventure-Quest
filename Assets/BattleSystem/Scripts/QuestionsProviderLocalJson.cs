using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class QuestionsProviderLocalJson : IQuestionsProvider
{
    const string FilePath = "QuestionsDataBase.json";

    public List<Question> GetQuestions()
    {
        List<Question> questionsList = new();

        var jsonString = File.ReadAllText($"{Application.persistentDataPath}/{FilePath}");
        questionsList = JsonUtility.FromJson<QuestionsListWrapper>(jsonString).questionslist;

        return questionsList;
    }

    public void SaveQuestion(Question newQuestion)
    {
        List<Question> questionsList = new (){newQuestion};

        var json = JsonUtility.ToJson(new QuestionsListWrapper(questionsList), true);
        Debug.Log(json);

        try
        {

            File.WriteAllText($"{Application.persistentDataPath}/{FilePath}", json);
        }
        catch(Exception e)
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
