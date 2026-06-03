using UnityEngine;

public class EnemyLifeSystem : ALifeSystem
{
    private void Awake()
    {
        Initialize(GameContext.GameProperties.EnemyMaxLife);
    }
}
