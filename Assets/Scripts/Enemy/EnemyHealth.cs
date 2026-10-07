using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyHealth : MonoBehaviour
{
    private const float PoisonTimeEpsilon = 0.0001f;

    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;

    [Header("Health Bar Settings")]
    [SerializeField]
    private bool showHealthBarOnStart;

    [Header("Poison Debug - Do Not Edit")]
    [SerializeField]
    private bool debugPoisonActive;

    [SerializeField]
    private int debugPoisonTickDamage;

    [SerializeField]
    private float debugPoisonRemainingDuration;

    [SerializeField]
    private float debugPoisonTimeUntilNextTick;

    [Header("Stun Debug - Do Not Edit")]
    [SerializeField]
    private bool debugStunActive;

    [SerializeField]
    private float debugStunRemainingDuration;

    [SerializeField]
    private int debugDisabledAttackBehaviourCount;

    [Header("Slow Debug - Do Not Edit")]
    [SerializeField]
    private bool debugSlowActive;

    [SerializeField]
    private float debugSlowPercent;

    [SerializeField]
    private float debugSlowRemainingDuration;

    [SerializeField]
    private float debugCurrentAgentSpeed;

    private int currentHealth;
    private EnemyHealthBar healthBar;
    private bool healthMultiplierApplied;

    private bool poisonActive;
    private int poisonTickDamage;
    private float poisonExpireTime;
    private float poisonNextTickTime;
    private float poisonTickInterval;

    private bool stunActive;
    private float stunExpireTime;
    private Quaternion stunFrozenRotation;

    private bool slowActive;
    private float slowPercent;
    private float slowExpireTime;

    private NavMeshAgent statusAgent;
    private float baseAgentSpeed;
    private float baseAgentAngularSpeed;
    private bool baseAgentValuesCaptured;

    private readonly List<MonoBehaviour>
        stunAttackBehaviours = new();

    private readonly List<bool>
        stunAttackBehaviourWasEnabled = new();

    public event Action<EnemyHealth> Died;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead { get; private set; }

    public bool IsPoisoned =>
        poisonActive;

    public bool IsStunned =>
        stunActive;

    public bool IsSlowed =>
        slowActive;

    private void Start()
    {
        currentHealth = maxHealth;
        IsDead = false;

        healthBar =
            GetComponentInChildren<EnemyHealthBar>(
                true
            );

        if (healthBar != null)
        {
            healthBar.SetHealth(
                currentHealth,
                maxHealth
            );

            if (showHealthBarOnStart)
            {
                healthBar.Show();
            }
            else
            {
                healthBar.Hide();
            }
        }
    }

    private void Update()
    {
        UpdatePoison();
        UpdateSlow();
        UpdateStun();

        RefreshPoisonDebugInfo();
        RefreshSlowDebugInfo();
        RefreshStunDebugInfo();
    }

    private void LateUpdate()
    {
        if (!stunActive)
        {
            return;
        }

        // NavMesh movement uses speed = 0 while stunned.
        // Some movement scripts rotate the transform directly,
        // so restore the original stun rotation after all Updates.
        transform.rotation =
            stunFrozenRotation;

        if (statusAgent != null &&
            statusAgent.isOnNavMesh)
        {
            statusAgent.velocity =
                Vector3.zero;
        }
    }

    public void ApplyHealthMultiplier(float healthMultiplier)
    {
        if (healthMultiplierApplied)
        {
            Debug.LogWarning(
                "EnemyHealth: Health multiplier was already applied."
            );

            return;
        }

        if (healthMultiplier <= 0f)
        {
            Debug.LogWarning(
                "EnemyHealth: Health multiplier must be greater than 0."
            );

            return;
        }

        maxHealth = Mathf.Max(
            1,
            Mathf.CeilToInt(
                maxHealth * healthMultiplier
            )
        );

        healthMultiplierApplied = true;
    }

    public void TakeDamage(int damageAmount)
    {
        if (IsDead)
        {
            return;
        }

        if (damageAmount <= 0)
        {
            return;
        }

        currentHealth -= damageAmount;

        if (currentHealth < 0)
        {
            currentHealth = 0;
        }

        Debug.Log(
            "Enemy health: " +
            currentHealth
        );

        if (healthBar != null)
        {
            healthBar.SetHealth(
                currentHealth,
                maxHealth
            );
        }

        if (currentHealth == 0)
        {
            Die();
        }
    }

    public bool TryApplyPoison(
        int sourceHitDamage,
        float damageMultiplier,
        float duration,
        float tickInterval
    )
    {
        if (IsDead ||
            sourceHitDamage <= 0 ||
            damageMultiplier <= 0f ||
            duration <= 0f ||
            tickInterval <= 0f)
        {
            return false;
        }

        // Process any tick that became due before this new hit.
        // This keeps poison timing correct even if a hit and a tick
        // happen during the same frame.
        if (poisonActive)
        {
            UpdatePoison();

            if (IsDead)
            {
                return false;
            }
        }

        float now =
            Time.time;

        if (poisonActive)
        {
            // A successful re-application refreshes ONLY duration.
            // Damage and next tick time intentionally stay unchanged.
            poisonExpireTime =
                now + duration;

            Debug.Log(
                "EnemyHealth: Poison duration refreshed. " +
                $"Tick damage remains {poisonTickDamage}. " +
                $"Next tick in " +
                $"{Mathf.Max(0f, poisonNextTickTime - now):F2} s. " +
                $"New duration = {duration:F2} s.",
                this
            );

            return true;
        }

        poisonTickDamage =
            Mathf.Max(
                1,
                Mathf.RoundToInt(
                    sourceHitDamage *
                    damageMultiplier
                )
            );

        poisonTickInterval =
            tickInterval;

        poisonNextTickTime =
            now + poisonTickInterval;

        poisonExpireTime =
            now + duration;

        poisonActive = true;

        Debug.Log(
            "EnemyHealth: Poison applied. " +
            $"Source hit damage = {sourceHitDamage}. " +
            $"Tick damage = {poisonTickDamage}. " +
            $"Duration = {duration:F2} s. " +
            $"Tick interval = {poisonTickInterval:F2} s.",
            this
        );

        RefreshPoisonDebugInfo();

        return true;
    }

    public bool TryApplyStun(
        float duration
    )
    {
        if (IsDead ||
            duration <= 0f)
        {
            return false;
        }

        if (IsBossEnemy())
        {
            Debug.Log(
                "EnemyHealth: Stun ignored on Boss until " +
                "boss diminishing returns are implemented.",
                this
            );

            return false;
        }

        float now =
            Time.time;

        if (stunActive)
        {
            // Re-application refreshes only the remaining duration.
            stunExpireTime =
                now + duration;

            Debug.Log(
                "EnemyHealth: Stun duration refreshed. " +
                $"New duration = {duration:F2} s.",
                this
            );

            RefreshStunDebugInfo();

            return true;
        }

        stunActive = true;
        stunExpireTime =
            now + duration;

        stunFrozenRotation =
            transform.rotation;

        FreezeMovementForStun();
        DisableAttackBehavioursForStun();

        Debug.Log(
            "EnemyHealth: Stun applied. " +
            $"Duration = {duration:F2} s. " +
            $"Disabled attack behaviours = " +
            $"{stunAttackBehaviours.Count}.",
            this
        );

        RefreshStunDebugInfo();

        return true;
    }

    public bool TryApplySlow(
        float movementReductionPercent,
        float duration
    )
    {
        if (IsDead ||
            movementReductionPercent <= 0f ||
            duration <= 0f)
        {
            return false;
        }

        if (IsBossEnemy())
        {
            Debug.Log(
                "EnemyHealth: Slow ignored on Boss until " +
                "boss control rules are implemented.",
                this
            );

            return false;
        }

        EnsureBaseAgentValuesCaptured();

        if (statusAgent == null)
        {
            Debug.LogWarning(
                "EnemyHealth: Slow could not find NavMeshAgent.",
                this
            );

            return false;
        }

        float now =
            Time.time;

        bool wasActive =
            slowActive;

        slowActive = true;

        slowPercent =
            Mathf.Clamp(
                movementReductionPercent,
                0f,
                0.95f
            );

        slowExpireTime =
            now + duration;

        ApplyMovementStatusToAgent();

        if (wasActive)
        {
            Debug.Log(
                "EnemyHealth: Slow refreshed. " +
                $"Movement reduction = " +
                $"{slowPercent * 100f:F0}%. " +
                $"Duration = {duration:F2} s.",
                this
            );
        }
        else
        {
            Debug.Log(
                "EnemyHealth: Slow applied. " +
                $"Movement reduction = " +
                $"{slowPercent * 100f:F0}%. " +
                $"Duration = {duration:F2} s.",
                this
            );
        }

        RefreshSlowDebugInfo();

        return true;
    }

    public void ShowHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.Show();
        }
    }

    public void HideHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.Hide();
        }
    }

    private void UpdatePoison()
    {
        if (!poisonActive)
        {
            return;
        }

        if (IsDead)
        {
            ClearPoisonState();
            return;
        }

        float now =
            Time.time;

        // The tick scheduled exactly at the poison expiration time
        // is still valid. With 5 s duration and 2.5 s interval this
        // gives ticks at 2.5 s and 5.0 s.
        while (
            poisonActive &&
            now + PoisonTimeEpsilon >=
                poisonNextTickTime &&
            poisonNextTickTime <=
                poisonExpireTime +
                PoisonTimeEpsilon
        )
        {
            Debug.Log(
                "EnemyHealth: Poison tick for " +
                $"{poisonTickDamage} damage.",
                this
            );

            TakeDamage(
                poisonTickDamage
            );

            if (IsDead)
            {
                ClearPoisonState();
                return;
            }

            poisonNextTickTime +=
                poisonTickInterval;
        }

        if (now + PoisonTimeEpsilon >=
                poisonExpireTime &&
            poisonNextTickTime >
                poisonExpireTime +
                PoisonTimeEpsilon)
        {
            Debug.Log(
                "EnemyHealth: Poison expired.",
                this
            );

            ClearPoisonState();
        }
    }

    private void UpdateSlow()
    {
        if (!slowActive)
        {
            return;
        }

        if (IsDead)
        {
            ClearSlowState();
            return;
        }

        if (Time.time <
            slowExpireTime)
        {
            return;
        }

        Debug.Log(
            "EnemyHealth: Slow expired.",
            this
        );

        ClearSlowState();
    }

    private void UpdateStun()
    {
        if (!stunActive)
        {
            return;
        }

        if (IsDead)
        {
            ClearStunState();
            return;
        }

        if (statusAgent != null &&
            statusAgent.isOnNavMesh)
        {
            statusAgent.velocity =
                Vector3.zero;
        }

        if (Time.time <
            stunExpireTime)
        {
            return;
        }

        Debug.Log(
            "EnemyHealth: Stun expired.",
            this
        );

        ClearStunState();
    }

    private void FreezeMovementForStun()
    {
        EnsureBaseAgentValuesCaptured();
        ApplyMovementStatusToAgent();

        if (statusAgent != null &&
            statusAgent.isOnNavMesh)
        {
            statusAgent.velocity =
                Vector3.zero;
        }
    }

    private void EnsureBaseAgentValuesCaptured()
    {
        if (baseAgentValuesCaptured)
        {
            return;
        }

        statusAgent =
            GetComponent<NavMeshAgent>();

        if (statusAgent == null)
        {
            return;
        }

        baseAgentSpeed =
            statusAgent.speed;

        baseAgentAngularSpeed =
            statusAgent.angularSpeed;

        baseAgentValuesCaptured = true;
    }

    private void ApplyMovementStatusToAgent()
    {
        EnsureBaseAgentValuesCaptured();

        if (statusAgent == null ||
            !baseAgentValuesCaptured)
        {
            return;
        }

        if (stunActive)
        {
            statusAgent.speed = 0f;
            statusAgent.angularSpeed = 0f;
            return;
        }

        statusAgent.angularSpeed =
            baseAgentAngularSpeed;

        if (slowActive)
        {
            statusAgent.speed =
                Mathf.Max(
                    0f,
                    baseAgentSpeed *
                    (1f - slowPercent)
                );

            return;
        }

        statusAgent.speed =
            baseAgentSpeed;
    }

    private void DisableAttackBehavioursForStun()
    {
        stunAttackBehaviours.Clear();
        stunAttackBehaviourWasEnabled.Clear();

        MonoBehaviour[] behaviours =
            GetComponents<MonoBehaviour>();

        foreach (MonoBehaviour behaviour
                 in behaviours)
        {
            if (behaviour == null ||
                behaviour == this)
            {
                continue;
            }

            string typeName =
                behaviour.GetType().Name;

            if (!typeName.EndsWith(
                    "EnemyAttack",
                    StringComparison.Ordinal))
            {
                continue;
            }

            stunAttackBehaviours.Add(
                behaviour
            );

            stunAttackBehaviourWasEnabled.Add(
                behaviour.enabled
            );

            behaviour.enabled = false;
        }
    }

    private void RestoreAttackBehavioursAfterStun()
    {
        int restoreCount =
            Mathf.Min(
                stunAttackBehaviours.Count,
                stunAttackBehaviourWasEnabled.Count
            );

        for (int i = 0;
             i < restoreCount;
             i++)
        {
            MonoBehaviour behaviour =
                stunAttackBehaviours[i];

            if (behaviour == null)
            {
                continue;
            }

            behaviour.enabled =
                stunAttackBehaviourWasEnabled[i];
        }
    }

    private bool IsBossEnemy()
    {
        MonoBehaviour[] behaviours =
            GetComponents<MonoBehaviour>();

        foreach (MonoBehaviour behaviour
                 in behaviours)
        {
            if (behaviour == null)
            {
                continue;
            }

            if (behaviour.GetType().Name ==
                "BossAttackController")
            {
                return true;
            }
        }

        return false;
    }

    private void RefreshSlowDebugInfo()
    {
        debugSlowActive =
            slowActive;

        debugSlowPercent =
            slowActive
                ? slowPercent * 100f
                : 0f;

        debugSlowRemainingDuration =
            slowActive
                ? Mathf.Max(
                    0f,
                    slowExpireTime - Time.time
                )
                : 0f;

        debugCurrentAgentSpeed =
            statusAgent != null
                ? statusAgent.speed
                : 0f;
    }

    private void ClearSlowState()
    {
        slowActive = false;
        slowPercent = 0f;
        slowExpireTime = 0f;

        ApplyMovementStatusToAgent();
        RefreshSlowDebugInfo();
    }

    private void RefreshStunDebugInfo()
    {
        debugStunActive =
            stunActive;

        debugStunRemainingDuration =
            stunActive
                ? Mathf.Max(
                    0f,
                    stunExpireTime - Time.time
                )
                : 0f;

        debugDisabledAttackBehaviourCount =
            stunActive
                ? stunAttackBehaviours.Count
                : 0;
    }

    private void ClearStunState()
    {
        if (!stunActive &&
            stunAttackBehaviours.Count == 0)
        {
            RefreshStunDebugInfo();
            return;
        }

        stunActive = false;
        stunExpireTime = 0f;

        RestoreAttackBehavioursAfterStun();
        ApplyMovementStatusToAgent();

        stunAttackBehaviours.Clear();
        stunAttackBehaviourWasEnabled.Clear();

        RefreshStunDebugInfo();
        RefreshSlowDebugInfo();
    }

    private void RefreshPoisonDebugInfo()
    {
        debugPoisonActive =
            poisonActive;

        debugPoisonTickDamage =
            poisonActive
                ? poisonTickDamage
                : 0;

        if (!poisonActive)
        {
            debugPoisonRemainingDuration = 0f;
            debugPoisonTimeUntilNextTick = 0f;
            return;
        }

        float now =
            Time.time;

        debugPoisonRemainingDuration =
            Mathf.Max(
                0f,
                poisonExpireTime - now
            );

        debugPoisonTimeUntilNextTick =
            Mathf.Max(
                0f,
                poisonNextTickTime - now
            );
    }

    private void ClearPoisonState()
    {
        poisonActive = false;
        poisonTickDamage = 0;
        poisonExpireTime = 0f;
        poisonNextTickTime = 0f;
        poisonTickInterval = 0f;

        RefreshPoisonDebugInfo();
    }

    private void Die()
    {
        if (IsDead)
        {
            return;
        }

        IsDead = true;

        ClearPoisonState();
        ClearStunState();
        ClearSlowState();

        Died?.Invoke(this);

        Destroy(gameObject);
    }
}
