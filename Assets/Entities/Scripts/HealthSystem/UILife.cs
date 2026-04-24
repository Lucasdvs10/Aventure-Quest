using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UILife : MonoBehaviour
{
    [SerializeField] private Image lifeBarImage;
    [SerializeField] private TMP_Text lifeText;
    private ALifeSystem entityLifeSystem;

    void Awake()
    {
        entityLifeSystem = GetComponent<ALifeSystem>();
    }

    void Start()
    {
        UpdateAllUI();
    }

    void OnEnable()
    {
        entityLifeSystem.OnTakeDamage.AddListener(UpdateAllUI);
    }

    void OnDisable()
    {
        entityLifeSystem.OnTakeDamage.RemoveListener(UpdateAllUI);
    }

    public void UpdateAllUI()
    {
        UpdateUILifeBar();
        UpdateLifeTexts();
    }

    public void UpdateUILifeBar()
    {
        lifeBarImage.fillAmount = (float) entityLifeSystem.CurrentLife / (float) entityLifeSystem.MaxLife;
    }

    public void UpdateLifeTexts()
    {
        var maxLifeValue = entityLifeSystem.MaxLife;
        var currentLifeValue = entityLifeSystem.CurrentLife;

        lifeText.text = $"{currentLifeValue}/{maxLifeValue} HP";
    }

}
