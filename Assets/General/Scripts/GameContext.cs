using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameContext : MonoBehaviour
{
    public bool UseLocalQuestionsProvider = true;

    public SOGameProperties SOGameProperties;
    public GameObject LeftPlayerGameObject;
    public GameObject RightPlayerGameObject;
    public QuestionsManager questionsManager;
    public BattleManagerMultiplayer battleManager;

    public GameObject uIGameoverScreen;

    public TMP_Text StatementText;
    public Button buttonA;
    public Button buttonB;
    public Button buttonC;
    public Button buttonD;

    public static SOGameProperties GameProperties { get; private set; }
    public static IQuestionsProvider QuestionsProviderInstance { get; private set; }
    public static QuestionsManager QuestionsManagerInstance { get; private set; }
    public static BattleManagerMultiplayer BattleManagerInstance { get; private set; }
    public static GameObject LeftPlayerGameObjectInstance { get; private set; }
    public static GameObject RightPlayerGameObjectInstance { get; private set; }
    public static GameObject UIGameoverScreenInstance { get; private set; }
    public static TMP_Text StatementTextInstance { get; private set; }
    public static Button ButtonAInstance { get; private set; }
    public static Button ButtonBInstance { get; private set; }
    public static Button ButtonCInstance { get; private set; }
    public static Button ButtonDInstance { get; private set; }

    void Awake()
    {
        if(UseLocalQuestionsProvider)
            QuestionsProviderInstance = new QuestionsProviderLocalJson();
        else
            QuestionsProviderInstance = new QuestionsProviderAPI();

        Debug.Assert(SOGameProperties != null, "O SO do gameproperties está nulo no game context!", this);
        GameProperties = SOGameProperties;

        Debug.Assert(LeftPlayerGameObject != null, "O Left Player GameObject está nulo no game context!", this);
        LeftPlayerGameObjectInstance = LeftPlayerGameObject;

        Debug.Assert(RightPlayerGameObject != null, "O Right Player GameObject está nulo no game context!", this);
        RightPlayerGameObjectInstance = RightPlayerGameObject;

        Debug.Assert(questionsManager != null, "O QuestionsManager está nulo no game context!", this);
        QuestionsManagerInstance = questionsManager;

        Debug.Assert(battleManager != null, "O BattleManager está nulo no game context!", this);
        BattleManagerInstance = battleManager;

        Debug.Assert(uIGameoverScreen != null, "O UIGameoverScreen está nulo no game context!", this);
        UIGameoverScreenInstance = uIGameoverScreen;

        Debug.Assert(StatementText != null, "O StatementText está nulo no game context!", this);
        StatementTextInstance = StatementText;

        Debug.Assert(buttonA != null, "O Button A está nulo no game context!", this);
        ButtonAInstance = buttonA;

        Debug.Assert(buttonB != null, "O Button B está nulo no game context!", this);
        ButtonBInstance = buttonB;

        Debug.Assert(buttonC != null, "O Button C está nulo no game context!", this);
        ButtonCInstance = buttonC;

        Debug.Assert(buttonD != null, "O Button D está nulo no game context!", this);
        ButtonDInstance = buttonD;
    }
}