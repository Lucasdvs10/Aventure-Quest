
using System;

[Serializable]
public class Question
{
    public string Statement;
    public string StatementA;
    public string StatementB;
    public string StatementC;
    public string StatementD;

    public string CorrectOption;
    public string AnswerExplanation;

    public Question(string statement, string statementA, string statementB, string statementC, string statementD, string correctOption, string answerExplanation="")
    {
        Statement = statement;
        StatementA = statementA;
        StatementB = statementB;
        StatementC = statementC;
        StatementD = statementD;
        CorrectOption = correctOption;
        AnswerExplanation = answerExplanation;
    }
}
