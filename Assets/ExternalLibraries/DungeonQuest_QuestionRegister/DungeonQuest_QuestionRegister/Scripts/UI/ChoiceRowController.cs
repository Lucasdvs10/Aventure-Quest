using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Uma alternativa no prefab ChoiceRow: campo de texto + toggle "correta" + "X".
/// O "correta" funciona como rádio: a tela desmarca as outras quando esta liga.
/// </summary>
public class ChoiceRowController : MonoBehaviour
{
    [Header("Referências de UI")]
    [SerializeField] private TMP_InputField labelInput;
    [SerializeField] private Toggle correctToggle;
    [SerializeField] private Button removeButton;

    public event Action<ChoiceRowController> OnRemoveRequested;
    public event Action<ChoiceRowController> OnMarkedCorrect;

    public void Initialize(string label, bool isCorrect)
    {
        if (labelInput) labelInput.text = label ?? string.Empty;
        if (correctToggle) correctToggle.SetIsOnWithoutNotify(isCorrect);

        if (correctToggle)
        {
            correctToggle.onValueChanged.RemoveAllListeners();
            // Só avisamos quando LIGA (para a tela desmarcar as demais).
            correctToggle.onValueChanged.AddListener(on => { if (on) OnMarkedCorrect?.Invoke(this); });
        }
        if (removeButton)
        {
            removeButton.onClick.RemoveAllListeners();
            removeButton.onClick.AddListener(() => OnRemoveRequested?.Invoke(this));
        }
    }

    /// <summary>Texto da alternativa (sem espaços nas pontas).</summary>
    public string Label => labelInput ? labelInput.text?.Trim() : string.Empty;

    /// <summary>Marca/lê se é a correta. O set não dispara evento (evita loop).</summary>
    public bool IsCorrect
    {
        get => correctToggle && correctToggle.isOn;
        set { if (correctToggle) correctToggle.SetIsOnWithoutNotify(value); }
    }
}
