using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Tela de Ranking global: lista os usuários (GET /users) por high_score,
/// rolável, até 100. Tem um X que volta ao menu.
/// </summary>
public class RankingScreenController : MonoBehaviour
{
    private RankingController controller;
    private SceneLoader sceneLoader;

    [Header("Navegação")]
    [SerializeField] private string mainMenuScene = "MainMenu";

    [Header("UI")]
    [SerializeField] private Transform listContainer;   // Content do ScrollView
    [SerializeField] private GameObject rowPrefab;       // prefab RankingRow
    [SerializeField] private Button exitButton;
    [SerializeField] private TMP_Text statusText;

    [Header("Config")]
    [SerializeField] private int topN = 100;

    private readonly List<GameObject> rows = new List<GameObject>();

    private void Awake()
    {
        controller = new RankingController();
        sceneLoader = GetComponent<SceneLoader>();
    }

    private async void Start()
    {
        if (exitButton) exitButton.onClick.AddListener(Exit);
        SetStatus("Carregando ranking...");

        var users = await controller.GetTopUsersAsync(topN);
        BuildRows(users);

        SetStatus(users.Count > 0 ? $"{users.Count} jogadores no ranking." : "Sem jogadores ainda.");
    }

    private void BuildRows(List<UserRankModel> users)
    {
        foreach (var go in rows) Destroy(go);
        rows.Clear();

        int rank = 1;
        foreach (var u in users)
        {
            var go = Instantiate(rowPrefab, listContainer);
            var row = go.GetComponent<RankingRowController>();
            row.Initialize(rank, u.user_name, u.high_score);
            rows.Add(go);
            rank++;
        }
    }

    public void Exit()
    {
        if (sceneLoader) sceneLoader.LoadScene(mainMenuScene);
        else Debug.LogWarning("[Ranking] SceneLoader ausente: adicione-o ao GameObject.");
    }

    private void SetStatus(string message)
    {
        if (statusText) statusText.text = message;
        Debug.Log("[Ranking] " + message);
    }
}
