using TMPro;
using UnityEngine;

/// <summary>Uma linha do ranking: posição, nome e pontuação.</summary>
public class RankingRowController : MonoBehaviour
{
    [SerializeField] private TMP_Text rankLabel;
    [SerializeField] private TMP_Text nameLabel;
    [SerializeField] private TMP_Text scoreLabel;

    public void Initialize(int rank, string userName, int score)
    {
        if (rankLabel) rankLabel.text = rank + "º";
        if (nameLabel) nameLabel.text = string.IsNullOrEmpty(userName) ? "—" : userName;
        if (scoreLabel) scoreLabel.text = score.ToString();
    }
}
