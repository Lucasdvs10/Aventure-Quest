using System.Collections.Generic;
using UnityEngine;

public class GamePropertiesProvider : MonoBehaviour
{
    [SerializeField] SOGameProperties sOGameProperties;
    [SerializeField] GameObject skeletonPrefab;
    [SerializeField] GameObject gollemPrefab;
    [SerializeField] GameObject dragonPrefab;
    [SerializeField] GameObject magePrefab;
    [SerializeField] GameObject batsPrefab;
    [SerializeField] GameObject darkKinghtPrefab;

    public void InitializePropertiesFromAPI()
    {
        if (sOGameProperties.IsTutorial)
        {
            EnemyType[] enemyTypes = {EnemyType.MAGE};
            sOGameProperties.enemiesList = GetEnemiesFromTypesList(enemyTypes);
        }

        else
        {

            var enemiesAmount = PlaySetupController.enemiesAmount;

            EnemyType[] enemyTypes = new EnemyType[enemiesAmount];

            for (int i = 0; i < enemiesAmount - 1; i++)
            {
                if(i % 5 != 0 || i == 0)
                    enemyTypes[i] = GetRandomEnemy();
                else
                    enemyTypes[i] = GetRandomBoss();
            }

            enemyTypes[enemiesAmount - 1] = GetRandomBoss();

            sOGameProperties.enemiesList = GetEnemiesFromTypesList(enemyTypes);
        }

    }

    public EnemyType GetRandomBoss()
    {
        var rng = Random.Range(0f,1f);

        if(rng <= 0.5f)
            return EnemyType.DRAGON;
        return EnemyType.GOLLEM;
    }

    public EnemyType GetRandomEnemy()
    {
        var rng = Random.Range(0f, 1f);

        if (rng <= 0.25f)
            return EnemyType.SKELETON;
        else if (rng <= 0.5f)
            return EnemyType.MAGE;
        else if (rng <= 0.75f)
            return EnemyType.BATS;
        return EnemyType.DARK_KNIGHT;
    }

    public List<GameObject> GetEnemiesFromTypesList(EnemyType[] enemyType)
    {
        List<GameObject> enemiesList = new();

        foreach (var item in enemyType)
        {
            switch (item)
            {
                case EnemyType.SKELETON:
                    enemiesList.Add(skeletonPrefab);
                    break;

                case EnemyType.GOLLEM:
                    enemiesList.Add(gollemPrefab);
                    break;

                case EnemyType.DRAGON:
                    enemiesList.Add(dragonPrefab);
                    break;

                case EnemyType.MAGE:
                    enemiesList.Add(magePrefab);
                    break;

                case EnemyType.BATS:
                    enemiesList.Add(batsPrefab);
                    break;

                case EnemyType.DARK_KNIGHT:
                    enemiesList.Add(darkKinghtPrefab);
                    break;
            }
        }

        return enemiesList;
    }
}

public enum EnemyType
{
    SKELETON,
    GOLLEM,
    DRAGON,
    MAGE,
    BATS,
    DARK_KNIGHT
}
