using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 12f;
    [SerializeField] private float hitDistance = 0.2f;

    [Header("Hit Effect")]
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private float hitEffectOffset = 0.5f;

    private EnemyHealth target;
    private int damage;

    private int initialDamage;
    private int ricochetsRemaining;
    private int ricochetTotalAtLaunch;
    private int ricochetsCompleted;

    private float ricochetDamageMultiplier;
    private float ricochetSearchRadius;
    private LayerMask ricochetEnemyLayer;
    private bool logProjectileEvents;

    private bool poisonEnabled;
    private float poisonPrimaryApplyChance;
    private float poisonRicochetApplyChance;
    private float poisonDamageMultiplier;
    private float poisonDuration;
    private float poisonTickInterval;

    private bool stunEnabled;
    private float stunPrimaryApplyChance;
    private float stunRicochetApplyChance;
    private float stunDuration;
    private bool forceStunProcForDebug;

    private bool slowEnabled;
    private float slowPercent;
    private float slowDuration;

    private readonly HashSet<EnemyHealth>
        visitedTargets = new();

    // Kept for towers and older callers that do not use
    // hero run-upgrade projectile effects.
    public void Initialize(
        EnemyHealth newTarget,
        int newDamage
    )
    {
        Initialize(
            newTarget,
            newDamage,
            0,
            0f,
            0f,
            0,
            false,
            false,
            0f,
            0f,
            0f,
            0f,
            0f,
            false,
            0f,
            0f,
            0f,
            false,
            false,
            0f,
            0f
        );
    }

    // Player projectiles use this overload when run upgrades are active.
    public void Initialize(
        EnemyHealth newTarget,
        int newDamage,
        int newRicochetCount,
        float newRicochetDamageMultiplier,
        float newRicochetSearchRadius,
        LayerMask newRicochetEnemyLayer,
        bool shouldLogProjectileEvents,
        bool newPoisonEnabled,
        float newPoisonPrimaryApplyChance,
        float newPoisonRicochetApplyChance,
        float newPoisonDamageMultiplier,
        float newPoisonDuration,
        float newPoisonTickInterval,
        bool newStunEnabled,
        float newStunPrimaryApplyChance,
        float newStunRicochetApplyChance,
        float newStunDuration,
        bool newForceStunProcForDebug,
        bool newSlowEnabled,
        float newSlowPercent,
        float newSlowDuration
    )
    {
        target = newTarget;

        damage =
            Mathf.Max(
                1,
                newDamage
            );

        initialDamage =
            damage;

        ricochetsRemaining =
            Mathf.Max(
                0,
                newRicochetCount
            );

        ricochetTotalAtLaunch =
            ricochetsRemaining;

        ricochetsCompleted = 0;

        ricochetDamageMultiplier =
            Mathf.Clamp01(
                newRicochetDamageMultiplier
            );

        ricochetSearchRadius =
            Mathf.Max(
                0f,
                newRicochetSearchRadius
            );

        ricochetEnemyLayer =
            newRicochetEnemyLayer;

        logProjectileEvents =
            shouldLogProjectileEvents;

        poisonEnabled =
            newPoisonEnabled;

        poisonPrimaryApplyChance =
            Mathf.Clamp01(
                newPoisonPrimaryApplyChance
            );

        poisonRicochetApplyChance =
            Mathf.Clamp01(
                newPoisonRicochetApplyChance
            );

        poisonDamageMultiplier =
            Mathf.Clamp01(
                newPoisonDamageMultiplier
            );

        poisonDuration =
            Mathf.Max(
                0f,
                newPoisonDuration
            );

        poisonTickInterval =
            Mathf.Max(
                0f,
                newPoisonTickInterval
            );

        stunEnabled =
            newStunEnabled;

        stunPrimaryApplyChance =
            Mathf.Clamp01(
                newStunPrimaryApplyChance
            );

        stunRicochetApplyChance =
            Mathf.Clamp01(
                newStunRicochetApplyChance
            );

        stunDuration =
            Mathf.Max(
                0f,
                newStunDuration
            );

        forceStunProcForDebug =
            newForceStunProcForDebug;

        slowEnabled =
            newSlowEnabled;

        slowPercent =
            Mathf.Clamp(
                newSlowPercent,
                0f,
                0.95f
            );

        slowDuration =
            Mathf.Max(
                0f,
                newSlowDuration
            );

        visitedTargets.Clear();

        if (target != null)
        {
            visitedTargets.Add(
                target
            );
        }
    }

    private void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 targetPosition =
            target.transform.position;

        Vector3 direction =
            (targetPosition -
             transform.position).normalized;

        transform.position +=
            direction *
            moveSpeed *
            Time.deltaTime;

        float distanceToTarget =
            Vector3.Distance(
                transform.position,
                targetPosition
            );

        if (distanceToTarget <=
            hitDistance)
        {
            HitTarget();
        }
    }

    private void HitTarget()
    {
        if (target == null)
        {
            return;
        }

        EnemyHealth hitTarget =
            target;

        bool isRicochetHit =
            ricochetsCompleted > 0;

        Vector3 hitTargetPosition =
            hitTarget.transform.position;

        Vector3 directionFromEnemy =
            (transform.position -
             hitTargetPosition).normalized;

        Vector3 hitEffectPosition =
            hitTargetPosition +
            directionFromEnemy *
            hitEffectOffset;

        hitTarget.TakeDamage(
            damage
        );

        if (!hitTarget.IsDead)
        {
            TryApplyPoison(
                hitTarget,
                isRicochetHit
            );

            TryApplyStun(
                hitTarget,
                isRicochetHit
            );

            TryApplySlow(
                hitTarget
            );
        }

        if (hitEffectPrefab != null)
        {
            Instantiate(
                hitEffectPrefab,
                hitEffectPosition,
                Quaternion.identity
            );
        }

        if (TryContinueRicochet(
                hitTargetPosition
            ))
        {
            return;
        }

        Destroy(gameObject);
    }

    private void TryApplyPoison(
        EnemyHealth hitTarget,
        bool isRicochetHit
    )
    {
        if (!poisonEnabled ||
            hitTarget == null ||
            hitTarget.IsDead ||
            poisonDamageMultiplier <= 0f ||
            poisonDuration <= 0f ||
            poisonTickInterval <= 0f)
        {
            return;
        }

        float applyChance =
            isRicochetHit
                ? poisonRicochetApplyChance
                : poisonPrimaryApplyChance;

        if (applyChance <= 0f)
        {
            return;
        }

        float roll =
            Random.value;

        if (roll > applyChance)
        {
            return;
        }

        bool applied =
            hitTarget.TryApplyPoison(
                damage,
                poisonDamageMultiplier,
                poisonDuration,
                poisonTickInterval
            );

        if (applied &&
            logProjectileEvents)
        {
            string hitType =
                isRicochetHit
                    ? "Ricochet"
                    : "Primary";

            Debug.Log(
                "Projectile: Poison proc succeeded. " +
                $"Hit type = {hitType}. " +
                $"Apply chance = " +
                $"{applyChance * 100f:F0}%. " +
                $"Source hit damage = {damage}.",
                this
            );
        }
    }

    private void TryApplyStun(
        EnemyHealth hitTarget,
        bool isRicochetHit
    )
    {
        if (!stunEnabled ||
            hitTarget == null ||
            hitTarget.IsDead ||
            stunDuration <= 0f)
        {
            return;
        }

        float applyChance =
            isRicochetHit
                ? stunRicochetApplyChance
                : stunPrimaryApplyChance;

        if (applyChance <= 0f &&
            !forceStunProcForDebug)
        {
            return;
        }

        bool procSucceeded =
            forceStunProcForDebug ||
            Random.value <= applyChance;

        if (!procSucceeded)
        {
            return;
        }

        bool applied =
            hitTarget.TryApplyStun(
                stunDuration
            );

        if (applied &&
            logProjectileEvents)
        {
            string hitType =
                isRicochetHit
                    ? "Ricochet"
                    : "Primary";

            Debug.Log(
                "Projectile: Stun proc succeeded. " +
                $"Hit type = {hitType}. " +
                $"Apply chance = " +
                $"{applyChance * 100f:F0}%. " +
                $"Duration = {stunDuration:F2} s. " +
                $"Debug forced = " +
                $"{forceStunProcForDebug}.",
                this
            );
        }
    }

    private void TryApplySlow(
        EnemyHealth hitTarget
    )
    {
        if (!slowEnabled ||
            hitTarget == null ||
            hitTarget.IsDead ||
            slowPercent <= 0f ||
            slowDuration <= 0f)
        {
            return;
        }

        bool applied =
            hitTarget.TryApplySlow(
                slowPercent,
                slowDuration
            );

        if (applied &&
            logProjectileEvents)
        {
            Debug.Log(
                "Projectile: Slow applied/refreshed. " +
                $"Movement reduction = " +
                $"{slowPercent * 100f:F0}%. " +
                $"Duration = {slowDuration:F2} s.",
                this
            );
        }
    }

    private bool TryContinueRicochet(
        Vector3 searchOrigin
    )
    {
        if (ricochetsRemaining <= 0 ||
            ricochetDamageMultiplier <= 0f ||
            ricochetSearchRadius <= 0f)
        {
            return false;
        }

        EnemyHealth nextTarget =
            FindNearestRicochetTarget(
                searchOrigin
            );

        if (nextTarget == null)
        {
            if (logProjectileEvents)
            {
                Debug.Log(
                    "Projectile: Ricochet chain ended because " +
                    "no valid new target was found.",
                    this
                );
            }

            return false;
        }

        ricochetsRemaining--;
        ricochetsCompleted++;

        target =
            nextTarget;

        visitedTargets.Add(
            nextTarget
        );

        damage =
            Mathf.Max(
                1,
                Mathf.RoundToInt(
                    initialDamage *
                    ricochetDamageMultiplier
                )
            );

        if (logProjectileEvents)
        {
            Debug.Log(
                "Projectile: Ricochet " +
                $"{ricochetsCompleted}/" +
                $"{ricochetTotalAtLaunch}. " +
                $"Target = {nextTarget.name}. " +
                $"Damage = {damage}. " +
                $"Multiplier = " +
                $"{ricochetDamageMultiplier * 100f:F0}%.",
                this
            );
        }

        return true;
    }

    private EnemyHealth FindNearestRicochetTarget(
        Vector3 searchOrigin
    )
    {
        Collider[] nearbyEnemies =
            Physics.OverlapSphere(
                searchOrigin,
                ricochetSearchRadius,
                ricochetEnemyLayer
            );

        EnemyHealth nearestTarget =
            null;

        float shortestDistance =
            float.MaxValue;

        foreach (Collider nearbyEnemy
                 in nearbyEnemies)
        {
            if (nearbyEnemy == null)
            {
                continue;
            }

            EnemyHealth candidate =
                nearbyEnemy
                    .GetComponentInParent<EnemyHealth>();

            if (candidate == null ||
                candidate.IsDead ||
                visitedTargets.Contains(candidate))
            {
                continue;
            }

            float distance =
                Vector3.Distance(
                    searchOrigin,
                    candidate.transform.position
                );

            if (distance <
                shortestDistance)
            {
                shortestDistance =
                    distance;

                nearestTarget =
                    candidate;
            }
        }

        return nearestTarget;
    }
}
