using System.Collections.Generic;

public interface IQuestionsProvider
{
    public void SaveQuestion(Question newQuestion);
    public List<Question> GetQuestions();
}
