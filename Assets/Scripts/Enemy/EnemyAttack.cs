using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private int attackDamage = 50;
    [SerializeField] private float attackInterval = 1f;
    [SerializeField] private float attackWindup = 0.5f;

    [Header("Animation Settings")]
    [SerializeField] private EnemyAttackAnimation attackAnimation;

    [Header("Hit Effect")]
    [SerializeField] private HitEffect hitEffectPrefab;
    [SerializeField] private float hitEffectOffset = 0.15f;

    private EnemyMovement enemyMovement;

    private CoreTowerHealth coreTower;
    private Collider coreTowerCollider;

    private PlayerHealth playerHealth;
    private Collider playerCollider;

    private float attackTimer;
    private bool attackAnimationStarted;
    private bool damageMultiplierApplied;

    void Start()
    {
        enemyMovement = GetComponent<EnemyMovement>();

        coreTower = FindAnyObjectByType<CoreTowerHealth>();

        if (coreTower != null)
        {
            coreTowerCollider = coreTower.GetComponent<Collider>();
        }

        playerHealth = FindAnyObjectByType<PlayerHealth>();

        if (playerHealth != null)
        {
            playerCollider = playerHealth.GetComponent<Collider>();
        }

        attackTimer = attackWindup;

        if (enemyMovement == null)
        {
            Debug.LogWarning(
                "EnemyAttack: EnemyMovement was not found."
            );
        }

        if (coreTower == null)
        {
            Debug.LogWarning(
                "EnemyAttack: CoreTower was not found."
            );
        }

        if (coreTowerCollider == null)
        {
            Debug.LogWarning(
                "EnemyAttack: CoreTower Collider was not found."
            );
        }

        if (playerHealth == null)
        {
            Debug.LogWarning(
                "EnemyAttack: PlayerHealth was not found."
            );
        }

        if (playerCollider == null)
        {
            Debug.LogWarning(
                "EnemyAttack: Player Collider was not found."
            );
        }
    }

    void Update()
    {
        if (enemyMovement == null)
        {
            return;
        }

        if (!enemyMovement.IsAtTarget)
        {
            ResetAttackCycle();
            return;
        }

        if (!CanAttackCurrentTarget())
        {
            ResetAttackCycle();
            return;
        }

        attackTimer -= Time.deltaTime;

        if (!attackAnimationStarted &&
            attackTimer <= attackWindup)
        {
            StartAttackAnimation();
        }

        if (attackTimer > 0f)
        {
            return;
        }

        AttackCurrentTarget();

        attackTimer = attackInterval;
        attackAnimationStarted = false;
    }

    public void ApplyDamageMultiplier(float damageMultiplier)
    {
        if (damageMultiplierApplied)
        {
            Debug.LogWarning(
                "EnemyAttack: Damage multiplier was already applied."
            );

            return;
        }

        if (damageMultiplier <= 0f)
        {
            Debug.LogWarning(
                "EnemyAttack: Damage multiplier must be greater than 0."
            );

            return;
        }

        attackDamage = Mathf.Max(
            1,
            Mathf.CeilToInt(
                attackDamage * damageMultiplier
            )
        );

        damageMultiplierApplied = true;
    }

    private bool CanAttackCurrentTarget()
    {
        if (enemyMovement.IsTargetingPlayer)
        {
            if (playerHealth == null ||
                playerCollider == null)
            {
                return false;
            }

            if (playerHealth.IsDead)
            {
                return false;
            }

            return true;
        }

        if (coreTower == null ||
            coreTowerCollider == null)
        {
            return false;
        }

        return true;
    }

    private void AttackCurrentTarget()
    {
        if (enemyMovement.IsTargetingPlayer)
        {
            AttackPlayer();
        }
        else
        {
            AttackCoreTower();
        }
    }

    private void AttackPlayer()
    {
        if (playerHealth == null ||
            playerCollider == null ||
            playerHealth.IsDead)
        {
            return;
        }

        Vector3 hitPosition =
            GetHitPosition(playerCollider);

        Debug.Log(
            "Enemy attacks Player for " +
            attackDamage +
            " damage."
        );

        playerHealth.TakeDamage(attackDamage);

        SpawnHitEffect(hitPosition);
    }

    private void AttackCoreTower()
    {
        if (coreTower == null ||
            coreTowerCollider == null)
        {
            return;
        }

        Vector3 hitPosition =
            GetHitPosition(coreTowerCollider);

        Debug.Log(
            "Enemy attacks Core Tower for " +
            attackDamage +
            " damage."
        );

        coreTower.TakeDamage(attackDamage);

        SpawnHitEffect(hitPosition);
    }

    private Vector3 GetHitPosition(Collider targetCollider)
    {
        Vector3 hitPosition =
            targetCollider.ClosestPoint(transform.position);

        Vector3 outwardDirection =
            transform.position - hitPosition;

        outwardDirection.y = 0f;

        if (outwardDirection.sqrMagnitude > 0.001f)
        {
            hitPosition +=
                outwardDirection.normalized *
                hitEffectOffset;
        }

        return hitPosition;
    }

    private void SpawnHitEffect(Vector3 hitPosition)
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
            attackAnimation.Play(attackWindup);
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
}