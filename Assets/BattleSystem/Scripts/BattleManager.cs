using System.Collections;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public bool DelayBewtweenRounds = true;

    QuestionsManager questionsManager;
    ALifeSystem leftEntityLifeSystem;
    ALifeSystem rightEntityLifeSystem;
    int currentEntityTurn = 0;

    bool leftEntityWasCorrect = false;
    bool rightEntityWasCorrect = false;

    Coroutine handleEndRoundRoutine;

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


        if(CurrentEntityTurn == 1)
        {
            HandleEndRound();
        }

        CurrentEntityTurn++;
    }

    public void HandleEndRound()
    {
        if(handleEndRoundRoutine != null)
            StopCoroutine(handleEndRoundRoutine);
        handleEndRoundRoutine = StartCoroutine(HandleEndRoundRoutine());
    }

    private IEnumerator HandleEndRoundRoutine()
    {
        GameContext.ButtonAInstance.interactable = false;
        GameContext.ButtonBInstance.interactable = false;
        GameContext.ButtonCInstance.interactable = false;
        GameContext.ButtonDInstance.interactable = false;


        //Invocar animacao de dano
        if(DelayBewtweenRounds)
            yield return new WaitForSeconds(2.5f); //Depois de rodar todas as animacoes a aplicar os danos

        if (leftEntityWasCorrect)
        {
            CurrentEntity.ApplyDamage(GameContext.GameProperties.DamageOnCorrectAnswer);

        }
        if (rightEntityWasCorrect)
        {
            OtherEntity.ApplyDamage(GameContext.GameProperties.DamageOnCorrectAnswer);
        }

        //Verificar se o other entity morreu. Se sim, executar sequência de game over
        if(CurrentEntity.CurrentLife <= 0 || OtherEntity.CurrentLife <= 0)
        {
            HandleGameOver();
            yield break;
        }

        leftEntityWasCorrect = false;
        rightEntityWasCorrect = false;
        

        if(DelayBewtweenRounds)
            yield return new WaitForSeconds(2.5f); //Depois de rodar todas as animacoes a aplicar os danos

        GameContext.ButtonAInstance.interactable = true;
        GameContext.ButtonBInstance.interactable = true;
        GameContext.ButtonCInstance.interactable = true;
        GameContext.ButtonDInstance.interactable = true;
    }

    private void HandleGameOver()
    {
        GameContext.UIGameoverScreenInstance.SetActive(true);
    }


    private void OnCorrectAnswer()
    {
        if(CurrentEntityTurn == 0)
            leftEntityWasCorrect = true;

        else if(CurrentEntityTurn == 1)
            rightEntityWasCorrect = true;


        OnAnswer();


        print($"Vez do jogador {CurrentEntityTurn}");
    }

    private void OnWrongAnswer()
    {
        OnAnswer();


        print($"Vez do jogador {CurrentEntityTurn}");
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
