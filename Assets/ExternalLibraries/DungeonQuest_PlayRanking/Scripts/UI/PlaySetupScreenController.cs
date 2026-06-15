using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Tela Jogar: escolhe a disciplina e dá Play (carrega o combate). Tem um X
/// que volta ao menu. Mesmo molde do LoginScreenController.
/// </summary>
public class PlaySetupScreenController : MonoBehaviour
{
    private PlaySetupController controller;
    private SceneLoader sceneLoader;   // mesmo componente do LoginScreenController

    [Header("Navegação")]
    [SerializeField] private string mainMenuScene = "MainMenu";
    [SerializeField] private string combatScene = "Combat";

    [Header("UI")]
    [SerializeField] private TMP_Dropdown disciplineDropdown;
    [SerializeField] private Button playButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private TMP_Text statusText;

    private readonly List<TagModel> tags = new List<TagModel>();

    private void Awake()
    {
        controller = new PlaySetupController();
        sceneLoader = GetComponent<SceneLoader>();
    }

    private async void Start()
    {
        SetInteractable(false);
        SetStatus("Carregando disciplinas...");

        if (playButton) playButton.onClick.AddListener(Play);
        if (exitButton) exitButton.onClick.AddListener(Exit);

        var loaded = await controller.GetTagsAsync();
        tags.Clear();
        tags.AddRange(loaded);
        BuildOptions();

        SetInteractable(true);
        SetStatus(tags.Count > 0 ? "Escolha a disciplina e jogue." : "Nenhuma disciplina cadastrada.");
    }

    private void BuildOptions()
    {
        if (!disciplineDropdown) return;
        disciplineDropdown.ClearOptions();
        var options = new List<string> {};
        foreach (var t in tags) options.Add(t.label);
        disciplineDropdown.AddOptions(options);
        disciplineDropdown.value = 0;
    }

    private string SelectedTagTarget()
    {
        int idx = disciplineDropdown ? disciplineDropdown.value : 0;
        return tags[idx - 1].label;
    }

    public void Play()
    {
        MatchConfig.TagTarget = SelectedTagTarget();
        SetStatus($"Iniciando duelo · {MatchConfig.TagTarget}...");
        if (sceneLoader) sceneLoader.LoadScene(combatScene);
        else Debug.LogWarning("[Play] SceneLoader ausente: adicione-o ao GameObject para carregar o combate.");
    }

    public void Exit()
    {
        if (sceneLoader) sceneLoader.LoadScene(mainMenuScene);
        else Debug.LogWarning("[Play] SceneLoader ausente: adicione-o ao GameObject.");
    }

    private void SetInteractable(bool value)
    {
        if (playButton) playButton.interactable = value;
    }

    private void SetStatus(string message)
    {
        if (statusText) statusText.text = message;
        Debug.Log("[Play] " + message);
    }
}
