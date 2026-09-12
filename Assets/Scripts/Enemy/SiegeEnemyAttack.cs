using UnityEngine;

[RequireComponent(typeof(SiegeEnemyMovement))]
public class SiegeEnemyAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private int attackDamage = 50;
    [SerializeField] private float attackInterval = 1.5f;
    [SerializeField] private float attackWindup = 0.6f;

    [Header("Animation Settings")]
    [SerializeField]
    private EnemyAttackAnimation attackAnimation;

    [Header("Hit Effect")]
    [SerializeField] private HitEffect hitEffectPrefab;
    [SerializeField] private float hitEffectOffset = 0.15f;

    [Header("Debug - Do Not Edit")]
    [SerializeField] private string debugAttackTarget;
    [SerializeField] private float debugAttackTimer;
    [SerializeField] private bool debugCanAttack;
    [SerializeField] private bool debugAnimationStarted;

    private SiegeEnemyMovement siegeMovement;

    private float attackTimer;
    private bool attackAnimationStarted;

    private void Start()
    {
        siegeMovement =
            GetComponent<SiegeEnemyMovement>();

        attackTimer = attackWindup;

        if (siegeMovement == null)
        {
            Debug.LogWarning(
                "SiegeEnemyAttack: SiegeEnemyMovement was not found."
            );
        }

        if (attackAnimation == null)
        {
            Debug.LogWarning(
                "SiegeEnemyAttack: Attack Animation is not assigned."
            );
        }
    }

    private void Update()
    {
        if (siegeMovement == null)
        {
            return;
        }

        if (!siegeMovement.IsAtTarget)
        {
            ResetAttackCycle();
            UpdateDebugInfo(false);
            return;
        }

        if (!CanAttackCurrentTarget())
        {
            ResetAttackCycle();
            UpdateDebugInfo(false);
            return;
        }

        attackTimer -= Time.deltaTime;

        if (!attackAnimationStarted &&
            attackTimer <= attackWindup)
        {
            StartAttackAnimation();
        }

        if (attackTimer <= 0f)
        {
            AttackCurrentTarget();

            attackTimer = attackInterval;
            attackAnimationStarted = false;
        }

        UpdateDebugInfo(true);
    }

    private bool CanAttackCurrentTarget()
    {
        if (siegeMovement.IsTargetingDefenseTower)
        {
            DefenseTowerHealth defenseTower =
                siegeMovement.CurrentDefenseTower;

            if (defenseTower == null)
            {
                return false;
            }

            if (defenseTower.IsDestroyed)
            {
                return false;
            }

            if (siegeMovement.CurrentTargetCollider == null)
            {
                return false;
            }

            return true;
        }

        CoreTowerHealth coreTower =
            siegeMovement.CurrentCoreTower;

        if (coreTower == null)
        {
            return false;
        }

        if (coreTower.IsDestroyed)
        {
            return false;
        }

        if (siegeMovement.CurrentTargetCollider == null)
        {
            return false;
        }

        return true;
    }

    private void AttackCurrentTarget()
    {
        if (siegeMovement.IsTargetingDefenseTower)
        {
            AttackDefenseTower();
        }
        else
        {
            AttackCoreTower();
        }
    }

    private void AttackDefenseTower()
    {
        DefenseTowerHealth defenseTower =
            siegeMovement.CurrentDefenseTower;

        Collider targetCollider =
            siegeMovement.CurrentTargetCollider;

        if (defenseTower == null ||
            defenseTower.IsDestroyed ||
            targetCollider == null)
        {
            return;
        }

        Vector3 hitPosition =
            GetHitPosition(targetCollider);

        Debug.Log(
            gameObject.name +
            " attacks " +
            defenseTower.gameObject.name +
            " for " +
            attackDamage +
            " damage."
        );

        defenseTower.TakeDamage(
            attackDamage
        );

        SpawnHitEffect(
            hitPosition
        );
    }

    private void AttackCoreTower()
    {
        CoreTowerHealth coreTower =
            siegeMovement.CurrentCoreTower;

        Collider targetCollider =
            siegeMovement.CurrentTargetCollider;

        if (coreTower == null ||
            coreTower.IsDestroyed ||
            targetCollider == null)
        {
            return;
        }

        Vector3 hitPosition =
            GetHitPosition(targetCollider);

        Debug.Log(
            gameObject.name +
            " attacks Core Tower for " +
            attackDamage +
            " damage."
        );

        coreTower.TakeDamage(
            attackDamage
        );

        SpawnHitEffect(
            hitPosition
        );
    }

    private Vector3 GetHitPosition(
        Collider targetCollider
    )
    {
        Vector3 hitPosition =
            targetCollider.ClosestPoint(
                transform.position
            );

        Vector3 outwardDirection =
            transform.position -
            hitPosition;

        outwardDirection.y = 0f;

        if (outwardDirection.sqrMagnitude >
            0.001f)
        {
            hitPosition +=
                outwardDirection.normalized *
                hitEffectOffset;
        }

        return hitPosition;
    }

    private void SpawnHitEffect(
        Vector3 hitPosition
    )
    {
        if (hitEffectPrefab == null)
        {
            return;
        }

        Instantiate(
            hitEffectPrefab,
            hitPosition,
            Quaternion.identity
        );
    }

    private void StartAttackAnimation()
    {
        attackAnimationStarted = true;

        if (attackAnimation != null)
        {
            attackAnimation.Play(
                attackWindup
            );
        }
    }

    private void ResetAttackCycle()
    {
        attackTimer = attackWindup;
        attackAnimationStarted = false;

        if (attackAnimation != null)
        {
            attackAnimation.ResetAnimation();
        }
    }

    private void UpdateDebugInfo(
        bool canAttack
    )
    {
        debugCanAttack = canAttack;
        debugAttackTimer = attackTimer;
        debugAnimationStarted =
            attackAnimationStarted;

        if (siegeMovement == null)
        {
            debugAttackTarget = "None";
            return;
        }

        if (siegeMovement.IsTargetingDefenseTower)
        {
            DefenseTowerHealth defenseTower =
                siegeMovement.CurrentDefenseTower;

            if (defenseTower != null)
            {
                debugAttackTarget =
                    defenseTower.gameObject.name;
            }
            else
            {
                debugAttackTarget = "None";
            }

            return;
        }

        CoreTowerHealth coreTower =
            siegeMovement.CurrentCoreTower;

        if (coreTower != null &&
            !coreTower.IsDestroyed)
        {
            debugAttackTarget =
                coreTower.gameObject.name;
        }
        else
        {
            debugAttackTarget = "None";
        }
    }

    private void OnDisable()
    {
        if (attackAnimation != null)
        {
            attackAnimation.ResetAnimation();
        }
    }
}