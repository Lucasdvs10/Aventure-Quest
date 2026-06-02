using System.Collections.Generic;
using System.Threading.Tasks;

public interface IQuestionsProvider
{
    Task SaveQuestionAsync(Question newQuestion);
    Task<List<Question>> GetQuestionsAsync();
}
