using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameContext : MonoBehaviour
{
    public SOGameProperties SOGameProperties;
    public GameObject PlayerGameObject;
    public QuestionsManager questionsManager;
    public BattleManager battleManager;

    public TMP_Text StatementText;
    public Button buttonA;
    public Button buttonB;
    public Button buttonC;
    public Button buttonD;

    public static SOGameProperties GameProperties { get; private set; }
    public static QuestionsManager QuestionsManagerInstance { get; private set; }
    public static BattleManager BattleManagerInstance { get; private set; }
    public static GameObject PlayerGameObjectInstance { get; private set; }
    public static TMP_Text StatementTextInstance { get; private set; }
    public static Button ButtonAInstance { get; private set; }
    public static Button ButtonBInstance { get; private set; }
    public static Button ButtonCInstance { get; private set; }
    public static Button ButtonDInstance { get; private set; }

    void Awake()
    {
        Debug.Assert(SOGameProperties != null, "O SO do gameproperties está nulo no game context!", this);
        GameProperties = SOGameProperties;

        Debug.Assert(PlayerGameObject != null, "O Player GameObject está nulo no game context!", this);
        PlayerGameObjectInstance = PlayerGameObject;

        Debug.Assert(questionsManager != null, "O QuestionsManager está nulo no game context!", this);
        QuestionsManagerInstance = questionsManager;

        Debug.Assert(battleManager != null, "O BattleManager está nulo no game context!", this);
        BattleManagerInstance = battleManager;

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