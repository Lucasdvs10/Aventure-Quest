using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Tela de Configuração de Sala (antiga "trilha"). Mesmo molde do
/// LoginScreenController: controller "puro" no Awake + async void nos botões.
///
/// O usuário escolhe uma disciplina (tag) e a quantidade de inimigos; ao criar,
/// o backend gera o código da sala, que é exibido na tela.
/// </summary>
public class RoomConfigScreenController : MonoBehaviour
{
    private RoomController roomController;

    [Header("Dono da sala")]
    [Tooltip("Deixe vazio para usar Session.CurrentUserId (definido no login). " +
             "Preencha apenas para testar sem login.")]
    [SerializeField] private string ownerIdOverride = "";

    [Header("Formulário")]
    [SerializeField] private TMP_InputField titleInput;          // opcional
    [SerializeField] private TMP_Dropdown disciplineDropdown;    // "Variado" + tags
    [SerializeField] private Button minusButton;                 // - inimigos
    [SerializeField] private Button plusButton;                  // + inimigos
    [SerializeField] private TMP_Text quantityValueLabel;        // mostra a quantidade
    [SerializeField] private Button createButton;

    [Header("Resultado")]
    [SerializeField] private TMP_Text codeText;                  // código gerado
    [SerializeField] private TMP_Text statusText;

    [Header("Quantidade de inimigos")]
    [SerializeField] private int minQuantity = 1;
    [SerializeField] private int maxQuantity = 20;
    [SerializeField] private int defaultQuantity = 10;

    private readonly List<TagModel> tags = new List<TagModel>();  // sem o "Variado"
    private int quantity;
    private RoomModel lastRoom;

    private void Awake()
    {
        roomController = new RoomController();
    }

    private async void Start()
    {
        SetInteractable(false);
        SetCode("——");
        SetStatus("Carregando disciplinas...");

        if (minusButton) minusButton.onClick.AddListener(() => ChangeQuantity(-1));
        if (plusButton) plusButton.onClick.AddListener(() => ChangeQuantity(+1));
        if (createButton) createButton.onClick.AddListener(CreateRoom);

        quantity = Mathf.Clamp(defaultQuantity, minQuantity, maxQuantity);
        UpdateQuantityLabel();

        var loaded = await roomController.GetTagsAsync();
        tags.Clear();
        tags.AddRange(loaded);
        BuildDisciplineOptions();

        SetInteractable(true);
        SetStatus("Pronto. Configure a sala.");
    }

    // ---- Disciplina ---------------------------------------------------------

    private void BuildDisciplineOptions()
    {
        disciplineDropdown.ClearOptions();
        var options = new List<string> { "Variado (todas)" };  // índice 0
        foreach (var t in tags) options.Add(t.label);
        disciplineDropdown.AddOptions(options);
        disciplineDropdown.value = 0;
    }

    // tag_target = "variado" (índice 0) ou o label da tag escolhida.
    private string SelectedTagTarget()
    {
        int idx = disciplineDropdown ? disciplineDropdown.value : 0;
        if (idx <= 0 || idx - 1 >= tags.Count) return "variado";
        return tags[idx - 1].label;
    }

    // ---- Quantidade de inimigos --------------------------------------------

    private void ChangeQuantity(int delta)
    {
        quantity = Mathf.Clamp(quantity + delta, minQuantity, maxQuantity);
        UpdateQuantityLabel();
    }

    private void UpdateQuantityLabel()
    {
        if (quantityValueLabel) quantityValueLabel.text = quantity.ToString();
    }

    // ---- Criar sala ---------------------------------------------------------

    public async void CreateRoom()
    {
        string ownerId = !string.IsNullOrEmpty(ownerIdOverride) ? ownerIdOverride : PlayerPrefs.GetString("CurrentUserID");
        if (string.IsNullOrEmpty(ownerId))
        {
            SetStatus("Faça login primeiro (dono da sala ausente).");
            return;
        }

        string tagTarget = SelectedTagTarget();

        // Título opcional: se vazio, gera um padrão a partir da disciplina.
        string title = titleInput ? titleInput.text?.Trim() : null;
        if (string.IsNullOrEmpty(title)) title = $"Sala de {tagTarget}";

        SetInteractable(false);
        SetStatus("Criando sala...");

        lastRoom = await roomController.CreateRoomAsync(title, ownerId, quantity, tagTarget);

        SetInteractable(true);

        if (lastRoom != null && !string.IsNullOrEmpty(lastRoom.code))
        {
            SetCode(lastRoom.code);
            SetStatus($"Sala criada! {quantity} inimigos · {tagTarget}.");
        }
        else
        {
            SetStatus("Falha ao criar a sala.");
        }
    }

    // ---- Util ---------------------------------------------------------------

    private void SetCode(string code)
    {
        if (codeText) codeText.text = code;
    }

    private void SetInteractable(bool value)
    {
        if (minusButton) minusButton.interactable = value;
        if (plusButton) plusButton.interactable = value;
        if (createButton) createButton.interactable = value;
    }

    private void SetStatus(string message)
    {
        if (statusText) statusText.text = message;
        Debug.Log("[RoomConfig] " + message);
    }
}
