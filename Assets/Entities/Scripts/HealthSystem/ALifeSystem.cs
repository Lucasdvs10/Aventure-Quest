using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public abstract class ALifeSystem : MonoBehaviour
{
    private int maxLife;
    private int currentLife;
    [Space]
    
    public UnityEvent OnLifeChanged;
    public UnityEvent OnTakeDamage;
    public UnityEvent OnDeath;

    public void Initialize(int maxLife)
    {
        MaxLife = maxLife;
        CurrentLife = MaxLife;
    }

    public virtual void ApplyDamage(int damageValue)
    {
        CurrentLife -= damageValue;
        OnTakeDamage.Invoke();

        if(CurrentLife <= 0)
            OnDeath.Invoke();
    }

    [ContextMenu("10 de dano")]
    public void Apply10Damage()
    {
        ApplyDamage(10);
    }

    public int MaxLife
    {
        get => maxLife;
        set => maxLife = value;
    }
    public int CurrentLife
    {
        get => currentLife;
        set
        {
            currentLife = value;
            OnLifeChanged.Invoke();
        }
    }
}