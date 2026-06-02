using System.Collections;
using UnityEngine;

public class BattleManagerSingleplayer : MonoBehaviour, IBattleManager
{
    public bool DelayBewtweenRounds = true;

    QuestionsManager questionsManager;
    ALifeSystem leftEntityLifeSystem;
    ALifeSystem rightEntityLifeSystem;
    int currentEntityTurn = 0;

    bool leftEntityWasCorrect = false;
    bool rightEntityWasCorrect = false;

    Animator leftAnimator;
    Animator rightAnimator;
    AudioSource leftDamageSFX;
    AudioSource rightDamageSFX;

    Coroutine handleEndRoundRoutine;
    Coroutine onAnswerRoutine;


    void Awake()
    {
        questionsManager = GameContext.QuestionsManagerInstance;

        leftEntityLifeSystem = GameContext.LeftPlayerGameObjectInstance.GetComponent<ALifeSystem>();
        rightEntityLifeSystem = GameContext.RightPlayerGameObjectInstance.GetComponent<ALifeSystem>();

        leftAnimator = leftEntityLifeSystem.GetComponent<Animator>();
        rightAnimator = rightEntityLifeSystem.GetComponent<Animator>();

        leftDamageSFX = leftEntityLifeSystem.GetComponentInChildren<AudioSource>();
        rightDamageSFX = rightEntityLifeSystem.GetComponentInChildren<AudioSource>();

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

    void Start()
    {
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


        yield return HandleEndRoundRoutine();

        CurrentEntityTurn++;

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

            //Invocar animacao de dano
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

            //Invocar animacao de dano
            if (DelayBewtweenRounds)
                yield return new WaitForSeconds(1f); //Depois de rodar todas as animacoes a aplicar os danos
        }


        //Verificar se o other entity morreu. Se sim, executar sequência de game over
        if (CurrentEntity.CurrentLife <= 0 || OtherEntity.CurrentLife <= 0)
        {
            HandleGameOver();
            yield break;
        }

        leftEntityWasCorrect = false;
        rightEntityWasCorrect = false;



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

        // else if(CurrentEntityTurn == 1)
        //     rightEntityWasCorrect = true;

        OnAnswer();
    }

    private void OnWrongAnswer()
    {
        OnAnswer();
        rightEntityWasCorrect = true;


        // print($"Vez do jogador {CurrentEntityTurn}");
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
