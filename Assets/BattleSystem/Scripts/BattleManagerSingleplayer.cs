using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManagerSingleplayer : MonoBehaviour, IBattleManager
{
    public bool DelayBewtweenRounds = true;

    private int battlesAmount;
    private List<GameObject> enemiesList;

    QuestionsManager questionsManager;
    ALifeSystem leftEntityLifeSystem;
    ALifeSystem rightEntityLifeSystem;
    
    int currentBattle = 0;
    int currentEntityTurn = 0;

    bool leftEntityWasCorrect = false;
    bool rightEntityWasCorrect = false;

    Animator leftAnimator;
    Animator rightAnimator;
    AudioSource leftDamageSFX;
    AudioSource rightDamageSFX;

    Coroutine handleEndRoundRoutine;
    Coroutine onAnswerRoutine;
    Coroutine onWrongAnswerRoutine;


    void Awake()
    {
        questionsManager = GameContext.QuestionsManagerInstance;

        leftEntityLifeSystem = GameContext.LeftPlayerGameObjectInstance.GetComponent<ALifeSystem>();

        leftAnimator = leftEntityLifeSystem.GetComponent<Animator>();

        leftDamageSFX = leftEntityLifeSystem.GetComponentInChildren<AudioSource>();


        currentEntityTurn = 0;
        currentBattle = 0;
    }


    private void InstatiateRightEntity()
    {
        GameContext.RightPlayerGameObjectInstance = Instantiate(enemiesList[currentBattle]);

        rightEntityLifeSystem = GameContext.RightPlayerGameObjectInstance.GetComponent<ALifeSystem>();
        rightAnimator = rightEntityLifeSystem.GetComponent<Animator>();
        rightDamageSFX = rightEntityLifeSystem.GetComponentInChildren<AudioSource>();

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

    void Start()
    {
        enemiesList = GameContext.GameProperties.enemiesList;
        battlesAmount = enemiesList.Count;

        InstatiateRightEntity();
        CurrentEntity.GetComponentInChildren<UITurnIndicator>(true).UITurnIndicatorObject.SetActive(true);
    }

    public void OnAnswer()
    {
        if (onAnswerRoutine != null)
            StopCoroutine(onAnswerRoutine);
        onAnswerRoutine = StartCoroutine(OnAnswerRoutine());
    }


    private IEnumerator OnAnswerRoutine()
    {
        questionsManager.GetNextQuestionAndUpdateUI();


        CurrentEntityTurn++;
        yield return HandleEndRoundRoutine();

        CurrentEntity.GetComponentInChildren<UITurnIndicator>(true).UITurnIndicatorObject.SetActive(true);
        OtherEntity.GetComponentInChildren<UITurnIndicator>(true).UITurnIndicatorObject.SetActive(false);
    }

    public void HandleEndRound()
    {
        if (handleEndRoundRoutine != null)
            StopCoroutine(handleEndRoundRoutine);
        handleEndRoundRoutine = StartCoroutine(HandleEndRoundRoutine());
    }

    private IEnumerator HandleEndRoundRoutine()
    {
        GameContext.ButtonAInstance.interactable = false;
        GameContext.ButtonBInstance.interactable = false;
        GameContext.ButtonCInstance.interactable = false;
        GameContext.ButtonDInstance.interactable = false;



        if (leftEntityWasCorrect)
        {

            leftAnimator.Play("Attack");
            rightAnimator.Play("TakeDamage");

            yield return new WaitForSeconds(0.5f);
            CurrentEntity.ApplyDamage(GameContext.GameProperties.DamageOnCorrectAnswer);
            rightDamageSFX.Play();

            if (DelayBewtweenRounds)
                yield return new WaitForSeconds(1f); //Depois de rodar todas as animacoes a aplicar os danos
        }
        if (rightEntityWasCorrect)
        {

            rightAnimator.Play("Attack");
            leftAnimator.Play("TakeDamage");

            yield return new WaitForSeconds(0.5f);
            OtherEntity.ApplyDamage(GameContext.GameProperties.DamageOnCorrectAnswer);
            leftDamageSFX.Play();

            if (DelayBewtweenRounds)
                yield return new WaitForSeconds(1f); //Depois de rodar todas as animacoes a aplicar os danos
        }


        //Verificar se a ultima batalha foi finalizada. Se sim, executar sequência de game over
        if (leftEntityLifeSystem.CurrentLife <= 0 || rightEntityLifeSystem.CurrentLife <= 0 && currentBattle >= battlesAmount - 1)
        {
            HandleGameOver();
            yield break;
        }

        if(rightEntityLifeSystem.CurrentLife <= 0)
        {
            print("Fim da batalha!");

            Destroy(rightEntityLifeSystem.gameObject);

            yield return new WaitForSeconds(2f);
            currentBattle++;

            InstatiateRightEntity();
        }

        leftEntityWasCorrect = false;
        rightEntityWasCorrect = false;


        CurrentEntityTurn++;

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
        leftEntityWasCorrect = true;

        OnAnswer();
    }

    private void OnWrongAnswer()
    {
        if(onWrongAnswerRoutine != null)
            StopCoroutine(onWrongAnswerRoutine);
        onWrongAnswerRoutine = StartCoroutine(OnWrongAnswerRoutine());
    }

    private IEnumerator OnWrongAnswerRoutine()
    {
        questionsManager.ShowAnswerExplanation();

        GameContext.ButtonAInstance.interactable = false;
        GameContext.ButtonBInstance.interactable = false;
        GameContext.ButtonCInstance.interactable = false;
        GameContext.ButtonDInstance.interactable = false;

        yield return new WaitForSeconds(GameContext.GameProperties.answerExplanationDuration);

        rightEntityWasCorrect = true;
        OnAnswer();

        GameContext.ButtonAInstance.interactable = true;
        GameContext.ButtonBInstance.interactable = true;
        GameContext.ButtonCInstance.interactable = true;
        GameContext.ButtonDInstance.interactable = true;
    }

    public ALifeSystem CurrentEntity
    {
        get
        {
            if (CurrentEntityTurn == 0)
                return leftEntityLifeSystem;

            return rightEntityLifeSystem;
        }
    }

    public ALifeSystem OtherEntity
    {
        get
        {
            if (CurrentEntityTurn == 0)
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

            if (currentEntityTurn >= 2)
                currentEntityTurn = 0;

            else if (currentEntityTurn < 0)
                currentEntityTurn = 1;
        }
    }
}
