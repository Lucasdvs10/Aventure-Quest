using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonQuest.Trails
{
    /// <summary>
    /// One configurable phase ("fase") in the trail. Lives on the
    /// TrailPhaseRow prefab. Mirrors the choice-row style used in the
    /// question-registration screen: a reusable row the parent spawns,
    /// reads from, and removes.
    /// </summary>
    public class TrailPhaseRowController : MonoBehaviour
    {
        [Header("UI references")]
        [SerializeField] private TMP_Text orderLabel;     // shows "1", "2"... or "BOSS"
        [SerializeField] private TMP_Dropdown themeDropdown;
        [SerializeField] private TMP_InputField countInput;
        [SerializeField] private TMP_InputField enemyInput;
        [SerializeField] private Toggle bossToggle;
        [SerializeField] private Button removeButton;

        private List<TagOption> _tags;

        /// <summary>Raised when the user taps the row's remove button.</summary>
        public event Action<TrailPhaseRowController> OnRemoveRequested;

        public void Initialize(List<TagOption> tags, TrailPhaseDto data, int order)
        {
            _tags = tags ?? new List<TagOption>();

            themeDropdown.ClearOptions();
            var labels = new List<string>(_tags.Count);
            foreach (var t in _tags) labels.Add(t.label);
            themeDropdown.AddOptions(labels);

            if (data != null)
            {
                int idx = _tags.FindIndex(t => t.id == data.tag_id);
                themeDropdown.value = Mathf.Max(0, idx);
                countInput.text = data.question_count.ToString();
                if (enemyInput) enemyInput.text = data.enemy_name ?? string.Empty;
                if (bossToggle) bossToggle.isOn = data.is_boss;
            }
            else
            {
                countInput.text = "5";
            }

            SetOrder(order);

            if (bossToggle)
            {
                bossToggle.onValueChanged.RemoveAllListeners();
                bossToggle.onValueChanged.AddListener(_ => SetOrder(_lastOrder));
            }
            if (removeButton)
            {
                removeButton.onClick.RemoveAllListeners();
                removeButton.onClick.AddListener(() => OnRemoveRequested?.Invoke(this));
            }
        }

        private int _lastOrder = 1;

        public void SetOrder(int order)
        {
            _lastOrder = order;
            if (!orderLabel) return;
            orderLabel.text = (bossToggle && bossToggle.isOn) ? "BOSS" : order.ToString();
        }

        public TrailPhaseDto ToModel(int order)
        {
            int.TryParse(countInput.text, out int count);
            TagOption tag = (_tags != null && _tags.Count > 0)
                ? _tags[Mathf.Clamp(themeDropdown.value, 0, _tags.Count - 1)]
                : null;

            return new TrailPhaseDto
            {
                order = order,
                tag_id = tag?.id,
                tag_label = tag?.label,
                question_count = count,
                enemy_name = enemyInput ? enemyInput.text : string.Empty,
                is_boss = bossToggle && bossToggle.isOn
            };
        }

        public bool Validate(out string error)
        {
            error = null;
            if (_tags == null || _tags.Count == 0) { error = "nenhum tema disponível"; return false; }
            if (!int.TryParse(countInput.text, out int count) || count < 1)
            {
                error = "nº de perguntas inválido";
                return false;
            }
            return true;
        }
    }
}
