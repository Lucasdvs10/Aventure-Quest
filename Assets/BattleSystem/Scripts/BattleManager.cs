using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    QuestionsManager questionsManager;
    ALifeSystem leftEntityLifeSystem;
    ALifeSystem rightEntityLifeSystem;
    void Awake()
    {
        questionsManager = GameContext.QuestionsManagerInstance;

        leftEntityLifeSystem = GameContext.LeftPlayerGameObjectInstance.GetComponent<ALifeSystem>();
        rightEntityLifeSystem = GameContext.RightPlayerGameObjectInstance.GetComponent<ALifeSystem>();
    }

    void Start()
    {
        questionsManager.OnWrongAnswer.AddListener(() => leftEntityLifeSystem.ApplyDamage(10));
    }
}
