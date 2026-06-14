using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Tela de Cadastro de Perguntas. Mesmo molde do LoginScreenController:
/// instancia o controller "puro" no Awake e usa async void nos botões.
///
/// Monta: enunciado + explicação + seleção de temas (multi) + alternativas
/// (uma marcada como correta) e dispara o fluxo de 3 passos no QuestionController.
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
        if (saveButton) saveButton.onClick.AddListener(Save);

        // Integração viva: temas vêm da API.
        var tags = await questionController.GetTagsAsync();
        BuildTagToggles(tags);

        // Semeia algumas alternativas em branco.
        for (int i = 0; i < initialChoiceCount; i++) AddChoice();

        SetInteractable(true);
        SetStatus(tags.Count > 0 ? "Pronto. Cadastre a pergunta."
                                 : "Atenção: nenhum tema cadastrado (crie tags antes).");
    }

    // ---- Temas --------------------------------------------------------------

    private void BuildTagToggles(List<TagModel> tags)
    {
        foreach (var tag in tags)
        {
            var go = Instantiate(tagTogglePrefab, tagListContainer);
            var item = go.GetComponent<TagToggleController>();
            item.Initialize(tag);
            tagToggles.Add(item);
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
            print("entra aqui?");
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
        if (saveButton) saveButton.interactable = value;
    }

    private void SetStatus(string message)
    {
        if (statusText) statusText.text = message;
        Debug.Log("[QuestionRegister] " + message);
    }
}
