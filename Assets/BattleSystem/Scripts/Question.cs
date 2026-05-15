
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

    public Question(string statement, string statementA, string statementB, string statementC, string statementD, string correctOption)
    {
        Statement = statement;
        StatementA = statementA;
        StatementB = statementB;
        StatementC = statementC;
        StatementD = statementD;
        CorrectOption = correctOption;
    }
}
