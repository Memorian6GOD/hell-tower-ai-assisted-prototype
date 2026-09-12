using UnityEngine;

public class RangedEnemyAttack : MonoBehaviour
{
    private enum AttackTarget
    {
        None,
        Player,
        CoreTower
    }

    [Header("Attack Settings")]
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float attackInterval = 1.5f;
    [SerializeField] private float attackWindup = 0.5f;

    [Header("Projectile Settings")]
    [SerializeField] private EnemyProjectile projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;

    [Header("Debug - Do Not Edit")]
    [SerializeField] private float debugAttackTimer;
    [SerializeField] private bool debugIsPreparingShot;
    [SerializeField] private string debugCurrentTarget;

    private RangedEnemyMovement rangedEnemyMovement;

    private CoreTowerHealth coreTower;
    private Collider coreTowerCollider;

    private PlayerHealth playerHealth;
    private Collider playerCollider;

    private AttackTarget currentAttackTarget;

    private float attackTimer;
    private bool isPreparingShot;
    private bool damageMultiplierApplied;

    private void Start()
    {
        rangedEnemyMovement =
            GetComponent<RangedEnemyMovement>();

        coreTower =
            FindAnyObjectByType<CoreTowerHealth>();

        if (coreTower != null)
        {
            coreTowerCollider =
                coreTower.GetComponent<Collider>();
        }

        playerHealth =
            FindAnyObjectByType<PlayerHealth>();

        if (playerHealth != null)
        {
            playerCollider =
                playerHealth.GetComponent<Collider>();
        }

        ResetAttackCycle();

        if (rangedEnemyMovement == null)
        {
            Debug.LogWarning(
                "RangedEnemyAttack: RangedEnemyMovement was not found."
            );
        }

        if (projectilePrefab == null)
        {
            Debug.LogWarning(
                "RangedEnemyAttack: Projectile Prefab is not assigned."
            );
        }

        if (projectileSpawnPoint == null)
        {
            Debug.LogWarning(
                "RangedEnemyAttack: Projectile Spawn Point is not assigned."
            );
        }

        if (coreTower == null)
        {
            Debug.LogWarning(
                "RangedEnemyAttack: CoreTower was not found."
            );
        }

        if (coreTowerCollider == null)
        {
            Debug.LogWarning(
                "RangedEnemyAttack: CoreTower Collider was not found."
            );
        }

        if (playerHealth == null)
        {
            Debug.LogWarning(
                "RangedEnemyAttack: PlayerHealth was not found."
            );
        }

        if (playerCollider == null)
        {
            Debug.LogWarning(
                "RangedEnemyAttack: Player Collider was not found."
            );
        }

        if (attackDamage <= 0)
        {
            Debug.LogWarning(
                "RangedEnemyAttack: Attack Damage must be greater than 0."
            );
        }

        if (attackInterval <= 0f)
        {
            Debug.LogWarning(
                "RangedEnemyAttack: Attack Interval must be greater than 0."
            );
        }

        if (attackWindup < 0f)
        {
            Debug.LogWarning(
                "RangedEnemyAttack: Attack Windup cannot be negative."
            );
        }
    }

    private void Update()
    {
        if (rangedEnemyMovement == null ||
            projectilePrefab == null ||
            projectileSpawnPoint == null)
        {
            return;
        }

        AttackTarget desiredTarget =
            GetDesiredAttackTarget();

        if (!rangedEnemyMovement.IsAtTarget ||
            desiredTarget == AttackTarget.None)
        {
            currentAttackTarget =
                AttackTarget.None;

            ResetAttackCycle();

            UpdateDebugInfo();

            return;
        }

        if (desiredTarget != currentAttackTarget)
        {
            currentAttackTarget =
                desiredTarget;

            ResetAttackCycle();
        }

        attackTimer -= Time.deltaTime;

        if (!isPreparingShot &&
            attackTimer <= attackWindup)
        {
            BeginShotPreparation();
        }

        if (attackTimer > 0f)
        {
            UpdateDebugInfo();

            return;
        }

        FireAtCurrentTarget();

        attackTimer =
            Mathf.Max(0.01f, attackInterval);

        isPreparingShot = false;

        UpdateDebugInfo();
    }

    public void ApplyDamageMultiplier(float damageMultiplier)
    {
        if (damageMultiplierApplied)
        {
            Debug.LogWarning(
                "RangedEnemyAttack: Damage multiplier was already applied."
            );

            return;
        }

        if (damageMultiplier <= 0f)
        {
            Debug.LogWarning(
                "RangedEnemyAttack: Damage multiplier must be greater than 0."
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

    private AttackTarget GetDesiredAttackTarget()
    {
        if (rangedEnemyMovement.IsTargetingPlayer)
        {
            if (playerHealth == null ||
                playerCollider == null ||
                playerHealth.IsDead)
            {
                return AttackTarget.None;
            }

            return AttackTarget.Player;
        }

        if (coreTower == null ||
            coreTowerCollider == null)
        {
            return AttackTarget.None;
        }

        return AttackTarget.CoreTower;
    }

    private void BeginShotPreparation()
    {
        isPreparingShot = true;

        if (currentAttackTarget ==
            AttackTarget.Player)
        {
            Debug.Log(
                "RangedEnemy prepares a shot at Player."
            );
        }
        else if (currentAttackTarget ==
                 AttackTarget.CoreTower)
        {
            Debug.Log(
                "RangedEnemy prepares a shot at Core Tower."
            );
        }
    }

    private void FireAtCurrentTarget()
    {
        if (currentAttackTarget ==
            AttackTarget.Player)
        {
            FireAtPlayer();

            return;
        }

        if (currentAttackTarget ==
            AttackTarget.CoreTower)
        {
            FireAtCoreTower();
        }
    }

    private void FireAtPlayer()
    {
        if (playerHealth == null ||
            playerCollider == null ||
            playerHealth.IsDead)
        {
            return;
        }

        FireProjectile(
            playerCollider.bounds.center
        );

        Debug.Log(
            "RangedEnemy fires at Player."
        );
    }

    private void FireAtCoreTower()
    {
        if (coreTower == null ||
            coreTowerCollider == null)
        {
            return;
        }

        FireProjectile(
            coreTowerCollider.bounds.center
        );

        Debug.Log(
            "RangedEnemy fires at Core Tower."
        );
    }

    private void FireProjectile(
        Vector3 targetPosition
    )
    {
        Vector3 direction =
            targetPosition -
            projectileSpawnPoint.position;

        if (direction.sqrMagnitude < 0.001f)
        {
            Debug.LogWarning(
                "RangedEnemyAttack: Projectile direction is too small."
            );

            return;
        }

        Quaternion projectileRotation =
            Quaternion.LookRotation(
                direction.normalized
            );

        EnemyProjectile projectile =
            Instantiate(
                projectilePrefab,
                projectileSpawnPoint.position,
                projectileRotation
            );

        projectile.Initialize(
            direction,
            attackDamage
        );
    }

    private void ResetAttackCycle()
    {
        attackTimer =
            Mathf.Max(0f, attackWindup);

        isPreparingShot = false;
    }

    private void UpdateDebugInfo()
    {
        debugAttackTimer =
            attackTimer;

        debugIsPreparingShot =
            isPreparingShot;

        debugCurrentTarget =
            currentAttackTarget.ToString();
    }
}
