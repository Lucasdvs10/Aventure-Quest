
public class EnemyLifeSystem : ALifeSystem
{
    public bool IsBoss;
    private void Awake()
    {
        InitializeEnemy();
    }

    public void InitializeEnemy()
    {
        if(!IsBoss)
            Initialize(GameContext.GameProperties.EnemyMaxLife);
        else
            Initialize(GameContext.GameProperties.BossMaxLife);

    }
}
