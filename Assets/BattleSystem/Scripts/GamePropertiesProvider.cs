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
        EnemyType[] enemyTypes = {EnemyType.SKELETON, EnemyType.GOLLEM, EnemyType.SKELETON};
        sOGameProperties.enemiesList = GetEnemiesFromTypesList(enemyTypes);
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
