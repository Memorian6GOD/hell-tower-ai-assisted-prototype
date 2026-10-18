using UnityEngine;

// Bonuses belong to one living tower, not to every tower in CombatStats.
[DisallowMultipleComponent]
[RequireComponent(typeof(DefenseTowerHealth))]
public class DefenseTowerRunStats : MonoBehaviour
{
    private float damageBonusPercent;
    private float attackSpeedBonusPercent;

    public float DamageBonusPercent => damageBonusPercent;
    public float AttackSpeedBonusPercent => attackSpeedBonusPercent;

    public float GetDamage(CombatStats baseStats)
    {
        if (baseStats == null) return 0f;
        return baseStats.TowerDamage * (1f + damageBonusPercent / 100f);
    }

    public float GetAttackInterval(CombatStats baseStats)
    {
        if (baseStats == null) return 1f;
        return Mathf.Max(0.01f,
            baseStats.TowerAttackInterval / (1f + attackSpeedBonusPercent / 100f));
    }

    public bool TryAddDamageBonus(float amount)
    {
        if (!Application.isPlaying || float.IsNaN(amount) || float.IsInfinity(amount) || amount <= 0f) return false;
        damageBonusPercent += amount;
        Debug.Log($"DefenseTowerRunStats: {gameObject.name} damage bonus = +{damageBonusPercent:F1}%.", this);
        return true;
    }

    public bool TryAddAttackSpeedBonus(float amount)
    {
        if (!Application.isPlaying || float.IsNaN(amount) || float.IsInfinity(amount) || amount <= 0f) return false;
        attackSpeedBonusPercent += amount;
        Debug.Log($"DefenseTowerRunStats: {gameObject.name} attack speed bonus = +{attackSpeedBonusPercent:F1}%.", this);
        return true;
    }
}
