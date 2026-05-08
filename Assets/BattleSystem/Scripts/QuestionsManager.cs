using System.Collections.Generic;
using TMPro;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.Events;

public class QuestionsManager : MonoBehaviour
{
    List<Question> questionsList;
    int currentQuestionIndex = 0;
    string selectedOption = "";

    public UnityEvent OnCorrectAnswer;
    public UnityEvent OnWrongAnswer;

    void Awake()
    {
         questionsList = new()
    {
        new Question(
            "Qual é a capital do Brasil?",
            "Rio de Janeiro",
            "Brasília",
            "São Paulo",
            "Salvador",
            "Brasília"
        ),

        new Question(
            "Quanto é 7 x 8?",
            "54",
            "56",
            "64",
            "48",
            "56"
        ),

        new Question(
            "Qual linguagem é usada na Unity?",
            "Python",
            "Java",
            "C#",
            "C++",
            "C#"
        )
    };

    }

    void Start()
    {
        var currentQuestion = questionsList[currentQuestionIndex];

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
        var currentQuestion = questionsList[currentQuestionIndex];
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

        // Debug.LogError("A opção selecionada é nula!", this);
    }

    bool CheckAnswer(string optionSelected)
    {
        var currentQuestion = questionsList[currentQuestionIndex];
        return string.Equals(currentQuestion.CorrectOption, optionSelected);
    }

}
