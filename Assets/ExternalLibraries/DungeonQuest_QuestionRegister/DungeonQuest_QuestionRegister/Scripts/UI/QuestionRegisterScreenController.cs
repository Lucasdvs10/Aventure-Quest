using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Tela de Cadastro de Perguntas. Além de enunciado/explicação, seleção de
/// temas (multi) e alternativas, agora permite CRIAR um tema novo na hora
/// (POST /tags) — o tema criado já entra na lista e fica selecionado.
/// </summary>
public class QuestionRegisterScreenController : MonoBehaviour
{
    private QuestionController questionController;

    [Header("Campos da pergunta")]
    [SerializeField] private TMP_InputField promptInput;        // enunciado (multiline)
    [SerializeField] private TMP_InputField explanationInput;   // explicação (multiline)

    [Header("Temas (multi-seleção)")]
    [SerializeField] private Transform tagListContainer;        // Content do scroll de temas
    [SerializeField] private GameObject tagTogglePrefab;        // prefab TagToggle
    [SerializeField] private TMP_InputField newTagInput;        // nome do tema novo
    [SerializeField] private Button addTagButton;               // cria o tema

    [Header("Alternativas")]
    [SerializeField] private Transform choiceListContainer;     // Content do scroll de alternativas
    [SerializeField] private GameObject choiceRowPrefab;        // prefab ChoiceRow
    [SerializeField] private Button addChoiceButton;

    [Header("Ações")]
    [SerializeField] private Button saveButton;
    [SerializeField] private TMP_Text statusText;

    [Header("Comportamento")]
    [SerializeField] private int initialChoiceCount = 4;        // alternativas em branco ao abrir

    private readonly List<TagToggleController> tagToggles = new List<TagToggleController>();
    private readonly List<ChoiceRowController> choiceRows = new List<ChoiceRowController>();

    private void Awake()
    {
        questionController = new QuestionController();
    }

    private async void Start()
    {
        SetInteractable(false);
        SetStatus("Carregando temas...");

        if (addChoiceButton) addChoiceButton.onClick.AddListener(AddChoice);
        if (addTagButton) addTagButton.onClick.AddListener(AddTag);
        if (saveButton) saveButton.onClick.AddListener(Save);

        // Integração viva: temas vêm da API.
        var tags = await questionController.GetTagsAsync();
        BuildTagToggles(tags);

        // Semeia algumas alternativas em branco.
        for (int i = 0; i < initialChoiceCount; i++) AddChoice();

        SetInteractable(true);
        SetStatus(tags.Count > 0 ? "Pronto. Cadastre a pergunta."
                                 : "Nenhum tema ainda — crie um abaixo.");
    }

    // ---- Temas --------------------------------------------------------------

    private void BuildTagToggles(List<TagModel> tags)
    {
        foreach (var tag in tags) AddTagToggle(tag);
    }

    private TagToggleController AddTagToggle(TagModel tag, bool selected = false)
    {
        var go = Instantiate(tagTogglePrefab, tagListContainer);
        var item = go.GetComponent<TagToggleController>();
        item.Initialize(tag);
        if (selected) item.SetOn(true);
        tagToggles.Add(item);
        return item;
    }

    // Cria um tema novo (POST /tags) e já o adiciona/seleciona na lista.
    public async void AddTag()
    {
        string label = newTagInput ? newTagInput.text?.Trim() : null;
        if (string.IsNullOrEmpty(label)) { SetStatus("Digite o nome do tema."); return; }

        // Já existe? Apenas seleciona (evita duplicar).
        var existing = tagToggles.FirstOrDefault(
            t => t.Label != null && t.Label.Trim().ToLower() == label.ToLower());
        if (existing != null)
        {
            existing.SetOn(true);
            if (newTagInput) newTagInput.text = string.Empty;
            SetStatus($"Tema \"{label}\" já existe — selecionado.");
            return;
        }

        if (addTagButton) addTagButton.interactable = false;
        SetStatus("Criando tema...");

        var created = await questionController.CreateTagAsync(label);

        if (addTagButton) addTagButton.interactable = true;

        if (created != null && !string.IsNullOrEmpty(created.id))
        {
            AddTagToggle(created, selected: true);
            if (newTagInput) newTagInput.text = string.Empty;
            SetStatus($"Tema \"{created.label}\" criado e selecionado.");
        }
        else
        {
            SetStatus("Falha: faça login ou verifique a conexão.");
        }
    }

    // ---- Alternativas -------------------------------------------------------

    public void AddChoice()
    {
        if (!choiceRowPrefab || !choiceListContainer) return;

        var go = Instantiate(choiceRowPrefab, choiceListContainer);
        var row = go.GetComponent<ChoiceRowController>();
        row.Initialize(string.Empty, false);
        row.OnRemoveRequested += RemoveChoice;
        row.OnMarkedCorrect += MarkSingleCorrect;   // comportamento de rádio
        choiceRows.Add(row);
    }

    private void RemoveChoice(ChoiceRowController row)
    {
        choiceRows.Remove(row);
        Destroy(row.gameObject);
    }

    // Garante uma única alternativa correta: desmarca todas as outras.
    private void MarkSingleCorrect(ChoiceRowController chosen)
    {
        foreach (var row in choiceRows)
            if (row != chosen) row.IsCorrect = false;
    }

    // ---- Salvar -------------------------------------------------------------

    public async void Save()
    {
        string prompt = promptInput ? promptInput.text?.Trim() : null;
        string explanation = explanationInput ? explanationInput.text?.Trim() : string.Empty;

        if (string.IsNullOrEmpty(prompt)) { SetStatus("Escreva o enunciado da pergunta."); return; }

        // Monta as alternativas (ignora vazias) e descobre a correta.
        var labels = new List<string>();
        int correctIndex = -1;
        foreach (var row in choiceRows)
        {
            string l = row.Label;
            if (string.IsNullOrEmpty(l)) continue;
            if (row.IsCorrect) correctIndex = labels.Count;
            labels.Add(l);
        }

        if (labels.Count < 2) { SetStatus("Adicione ao menos 2 alternativas preenchidas."); return; }
        if (correctIndex < 0) { SetStatus("Marque qual alternativa é a correta."); return; }

        var tagIds = tagToggles.Where(t => t.IsOn).Select(t => t.TagId).ToList();
        if (tagIds.Count == 0) { SetStatus("Selecione ao menos um tema."); return; }

        SetInteractable(false);
        SetStatus("Salvando pergunta...");

        var result = await questionController.CreateQuestionAsync(
            prompt, explanation, tagIds, labels, correctIndex);

        SetInteractable(true);

        if (result.ok)
        {
            SetStatus("Pergunta cadastrada com sucesso!");
            ClearForm();
        }
        else
        {
            SetStatus("Falha: " + result.error);
        }
    }

    private void ClearForm()
    {
        if (promptInput) promptInput.text = string.Empty;
        if (explanationInput) explanationInput.text = string.Empty;
        foreach (var t in tagToggles) t.SetOn(false);

        foreach (var r in new List<ChoiceRowController>(choiceRows))
        {
            choiceRows.Remove(r);
            Destroy(r.gameObject);
        }
        for (int i = 0; i < initialChoiceCount; i++) AddChoice();
    }

    // ---- Util ---------------------------------------------------------------

    private void SetInteractable(bool value)
    {
        if (addChoiceButton) addChoiceButton.interactable = value;
        if (addTagButton) addTagButton.interactable = value;
        if (saveButton) saveButton.interactable = value;
    }

    private void SetStatus(string message)
    {
        if (statusText) statusText.text = message;
        Debug.Log("[QuestionRegister] " + message);
    }
}