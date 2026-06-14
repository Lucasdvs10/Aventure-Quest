using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Um tema selecionável no prefab TagToggle (um Toggle + rótulo).
/// A tela instancia um por tag e, ao salvar, coleta os que estão marcados.
/// </summary>
public class TagToggleController : MonoBehaviour
{
    [SerializeField] private Toggle toggle;
    [SerializeField] private TMP_Text label;

    /// <summary>Id da tag (vai para tag_ids da pergunta).</summary>
    public string TagId { get; private set; }

    public bool IsOn => toggle && toggle.isOn;

    public void Initialize(TagModel tag)
    {
        TagId = tag.id;
        if (label) label.text = tag.label;
        if (toggle) toggle.SetIsOnWithoutNotify(false);
    }

    public void SetOn(bool value)
    {
        if (toggle) toggle.SetIsOnWithoutNotify(value);
    }
}
