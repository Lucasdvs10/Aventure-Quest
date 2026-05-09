using UnityEngine;

public class BattleManager : MonoBehaviour
{
    QuestionsManager questionsManager;
    ALifeSystem leftEntityLifeSystem;
    ALifeSystem rightEntityLifeSystem;
    int currentEntityTurn = 0;

    void Awake()
    {
        questionsManager = GameContext.QuestionsManagerInstance;

        leftEntityLifeSystem = GameContext.LeftPlayerGameObjectInstance.GetComponent<ALifeSystem>();
        rightEntityLifeSystem = GameContext.RightPlayerGameObjectInstance.GetComponent<ALifeSystem>();

        currentEntityTurn = 0;
    }

    void OnEnable()
    {
        questionsManager.OnCorrectAnswer.AddListener(OnCorrectAnswer);
        questionsManager.OnWrongAnswer.AddListener(OnWrongAnswer);
    }

    void OnDisable()
    {
        questionsManager.OnCorrectAnswer.RemoveListener(OnCorrectAnswer);
        questionsManager.OnWrongAnswer.RemoveListener(OnWrongAnswer);
    }

    private void OnAnswer()
    {
        questionsManager.GetNextQuestionAndUpdateUI();
    }

    private void OnCorrectAnswer()
    {
        OnAnswer();

        OtherEntity.ApplyDamage(GameContext.GameProperties.DamageOnCorrectAnswer);

        CurrentEntityTurn++;
    }

    private void OnWrongAnswer()
    {
        OnAnswer();

        CurrentEntityTurn++;
    }

    public ALifeSystem CurrentEntity
    {
        get
        {
            if(CurrentEntityTurn == 0)
                return leftEntityLifeSystem;

            return rightEntityLifeSystem;
        }
    }
    public ALifeSystem OtherEntity
    {
        get
        {
            if(CurrentEntityTurn == 0)
                return rightEntityLifeSystem;

            return leftEntityLifeSystem;
        }
    }

    public int CurrentEntityTurn
    {
        get => currentEntityTurn;
        set
        {
            currentEntityTurn = value;

            if(currentEntityTurn >= 2)
                currentEntityTurn = 0;
            
            else if(currentEntityTurn < 0)
            currentEntityTurn = 1;
        }
    }
}
