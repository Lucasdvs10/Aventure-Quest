using UnityEngine;

public class PlayerLifeSystem : ALifeSystem
{
    private void Awake()
    {
        MaxLife = GameContext.GameProperties.PlayerMaxLife;
        CurrentLife = MaxLife;
    }
}
