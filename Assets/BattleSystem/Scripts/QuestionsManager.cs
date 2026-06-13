using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class QuestionsManager : MonoBehaviour
{
    private IQuestionsProvider questionsProvider;
    private List<Question> questionsList;
    private int currentQuestionIndex = 0;
    private string selectedOption = "";

    public UnityEvent OnCorrectAnswer;
    public UnityEvent OnWrongAnswer;


    async void Awake()
    {
        questionsProvider = GameContext.QuestionsProviderInstance;
        questionsList = await questionsProvider.GetQuestionsAsync();

        Shuffle(ref questionsList);

        var currentQuestion = questionsList[CurrentQuestionIndex];

        GameContext.StatementTextInstance.text = currentQuestion.Statement;

        GameContext.ButtonAInstance.GetComponentInChildren<TMP_Text>().text = currentQuestion.StatementA;
        GameContext.ButtonBInstance.GetComponentInChildren<TMP_Text>().text = currentQuestion.StatementB;
        GameContext.ButtonCInstance.GetComponentInChildren<TMP_Text>().text = currentQuestion.StatementC;
        GameContext.ButtonDInstance.GetComponentInChildren<TMP_Text>().text = currentQuestion.StatementD;
    }

    private void Shuffle<T>(ref List<T> list)
    {
        System.Random random = new();

        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);

            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    public void ShowAnswerExplanation()
    {
        var currentQuestion = questionsList[CurrentQuestionIndex];
        GameContext.StatementTextInstance.text = $"Errado! {currentQuestion.AnswerExplanation}";
    }

    public void GetNextQuestionAndUpdateUI()
    {
        CurrentQuestionIndex++;

        var currentQuestion = questionsList[CurrentQuestionIndex];

        GameContext.StatementTextInstance.text = currentQuestion.Statement;

        GameContext.ButtonAInstance.GetComponentInChildren<TMP_Text>().text = currentQuestion.StatementA;
        GameContext.ButtonBInstance.GetComponentInChildren<TMP_Text>().text = currentQuestion.StatementB;
        GameContext.ButtonCInstance.GetComponentInChildren<TMP_Text>().text = currentQuestion.StatementC;
        GameContext.ButtonDInstance.GetComponentInChildren<TMP_Text>().text = currentQuestion.StatementD;
    }

    public void ChooseAndAnwser(string optionLetter)
    {
        SelectOption(optionLetter);
        
        var responseIsRight = CheckAnswer(selectedOption);

        if(responseIsRight)
            OnCorrectAnswer.Invoke();
        else
            OnWrongAnswer.Invoke();
    }


    public void SelectOption(string optionLetter)
    {
        var currentQuestion = questionsList[CurrentQuestionIndex];
        switch (optionLetter)
        {
            case "A":
                selectedOption = currentQuestion.StatementA;
                break;
            case "B":
                selectedOption = currentQuestion.StatementB;
                break;
            case "C":
                selectedOption =  currentQuestion.StatementC;
                break;
            case "D":
                selectedOption =  currentQuestion.StatementD;
                break;
        }

    }

    bool CheckAnswer(string optionSelected)
    {
        var currentQuestion = questionsList[CurrentQuestionIndex];
        return string.Equals(currentQuestion.CorrectOption, optionSelected);
    }

    public int CurrentQuestionIndex
    {
        get => currentQuestionIndex;
        set
        {
            currentQuestionIndex = value;
            
            if(currentQuestionIndex < 0)
                currentQuestionIndex = questionsList.Count - 1;
            else if(currentQuestionIndex >= questionsList.Count)
            {
                currentQuestionIndex = 0;
            }
        }
    }

    public Question CurrentQuestion
    {
        get => questionsList[CurrentQuestionIndex];
    }
}
