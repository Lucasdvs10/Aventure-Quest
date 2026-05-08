using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    QuestionsManager questionsManager;
    void Awake()
    {
        questionsManager = GameContext.QuestionsManagerInstance;
    }

    void Start()
    {
        var player = GameContext.PlayerGameObjectInstance.GetComponent<ALifeSystem>();
        questionsManager.OnWrongAnswer.AddListener(() => player.ApplyDamage(10));
    }
}
