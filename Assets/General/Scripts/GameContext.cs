using UnityEngine;

public class GameContext : MonoBehaviour
{
    public SOGameProperties SOGameProperties;

    public static SOGameProperties GameProperties { get; private set; }

    void Awake()
    {
        Debug.Assert(SOGameProperties != null, "O SO do gameproperties está nulo no game context!", this);
        GameProperties = SOGameProperties;
    }
}