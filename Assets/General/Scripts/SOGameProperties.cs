using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameProperties", menuName = "Scriptable Objects/GameProperties", order = 0)]
public class SOGameProperties : ScriptableObject
{
    [Header("Player")]
    public int PlayerMaxLife;
    public int DamageOnCorrectAnswer;

    [Header("Enemy")]
    public int EnemyMaxLife;
    public int BossMaxLife;

    [Header("Configuration")]
    public float answerExplanationDuration;

    public List<GameObject> enemiesList;

    public bool IsTutorial = false;
}
