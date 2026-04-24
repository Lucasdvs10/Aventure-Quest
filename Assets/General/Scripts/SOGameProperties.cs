using UnityEngine;

[CreateAssetMenu(fileName = "GameProperties", menuName = "Scriptable Objects/GameProperties", order = 0)]
public class SOGameProperties : ScriptableObject
{
    [Header("Player")]
    public int PlayerMaxLife;
}
