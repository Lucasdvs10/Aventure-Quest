using UnityEngine;

public class GameContext : MonoBehaviour
{
    public SOGameProperties SOGameProperties;
    public GameObject PlayerGameObject;

    public static SOGameProperties GameProperties { get; private set; }
    public static GameObject PlayerGameObjectInstance { get; private set; }

    void Awake()
    {
        Debug.Assert(SOGameProperties != null, "O SO do gameproperties está nulo no game context!", this);
        GameProperties = SOGameProperties;

        Debug.Assert(PlayerGameObjectInstance != null, "O Player GameObject está nulo no game context!", this);
        PlayerGameObjectInstance = PlayerGameObject;
    }
}