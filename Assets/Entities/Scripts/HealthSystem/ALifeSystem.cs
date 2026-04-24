using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public abstract class ALifeSystem : MonoBehaviour
{
    [SerializeField] private int maxLife;
    private int currentLife;
    [Space]
    
    public UnityEvent OnTakeDamage;
    public UnityEvent OnDeath;

    protected virtual void Awake()
    {
        CurrentLife = MaxLife;
    }

    public virtual void ApplyDamage(int damageValue)
    {
        CurrentLife -= damageValue;
        OnTakeDamage.Invoke();

        if(CurrentLife <= 0)
            OnDeath.Invoke();
    }

    public int MaxLife
    {
        get => maxLife;
        set => maxLife = value;
    }
    public int CurrentLife
    {
        get => currentLife;
        set => currentLife = value;
    }
}