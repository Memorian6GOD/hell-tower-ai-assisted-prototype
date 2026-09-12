using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float attackRadius = 4f;
    [SerializeField] private int attackDamage = 20;
    [SerializeField] private float attackInterval = 1f;
    [SerializeField] private float attackWindup = 0.2f;
    [SerializeField] private float rotationSpeed = 720f;

    [Header("Projectile Settings")]
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private Transform attackPoint;

    [Header("Animation Settings")]
    [SerializeField]
    private PlayerAttackAnimation attackAnimation;

    [Header("Target Settings")]
    [SerializeField] private LayerMask enemyLayer;

    [Header("Debug - Do Not Edit")]
    [SerializeField] private bool debugAttackDelayed;
    [SerializeField] private float debugAttackDelayTimer;

    private PlayerMovement playerMovement;
    private PlayerHealth playerHealth;

    private float attackTimer;
    private float attackDelayTimer;

    private bool attackAnimationStarted;

    public bool IsAttackDelayed =>
        attackDelayTimer > 0f;

    private void Start()
    {
        playerMovement =
            GetComponent<PlayerMovement>();

        playerHealth =
            GetComponent<PlayerHealth>();

        attackTimer = attackWindup;
    }

    private void Update()
    {
        if (UpdateAttackDelay())
        {
            ResetAttackCycle();
            return;
        }

        if (playerHealth != null &&
            playerHealth.IsDead)
        {
            ResetAttackCycle();
            return;
        }

        if (playerMovement != null &&
            playerMovement.IsMoving)
        {
            ResetAttackCycle();
            return;
        }

        Collider nearestEnemy =
            FindNearestEnemy();

        if (nearestEnemy == null)
        {
            ResetAttackCycle();
            return;
        }

        EnemyHealth nearestEnemyHealth =
            nearestEnemy.GetComponent<EnemyHealth>();

        if (nearestEnemyHealth == null)
        {
            ResetAttackCycle();
            return;
        }

        RotateTowardsEnemy(nearestEnemy);

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

        AttackEnemy(nearestEnemyHealth);

        attackTimer = attackInterval;
        attackAnimationStarted = false;
    }

    private void LateUpdate()
    {
        debugAttackDelayed =
            IsAttackDelayed;

        debugAttackDelayTimer =
            attackDelayTimer;
    }

    public void StartAttackDelay(
        float duration
    )
    {
        float safeDuration =
            Mathf.Max(
                0f,
                duration
            );

        attackDelayTimer =
            Mathf.Max(
                attackDelayTimer,
                safeDuration
            );

        ResetAttackCycle();

        Debug.Log(
            "PlayerAttack: Attacks blocked for " +
            safeDuration.ToString("F1") +
            " seconds."
        );
    }

    private bool UpdateAttackDelay()
    {
        if (attackDelayTimer <= 0f)
        {
            return false;
        }

        attackDelayTimer =
            Mathf.Max(
                0f,
                attackDelayTimer -
                Time.deltaTime
            );

        return true;
    }

    private Collider FindNearestEnemy()
    {
        Collider[] enemiesInRange =
            Physics.OverlapSphere(
                transform.position,
                attackRadius,
                enemyLayer
            );

        Collider nearestEnemy = null;
        float shortestDistance =
            float.MaxValue;

        foreach (Collider enemy
                 in enemiesInRange)
        {
            float distance =
                Vector3.Distance(
                    transform.position,
                    enemy.transform.position
                );

            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                nearestEnemy = enemy;
            }
        }

        return nearestEnemy;
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

    private void RotateTowardsEnemy(
        Collider enemy
    )
    {
        Vector3 direction =
            enemy.transform.position -
            transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
        {
            return;
        }

        Quaternion targetRotation =
            Quaternion.LookRotation(
                direction
            );

        transform.rotation =
            Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed *
                Time.deltaTime
            );
    }

    private void AttackEnemy(
        EnemyHealth enemyHealth
    )
    {
        if (enemyHealth == null)
        {
            return;
        }

        if (projectilePrefab == null ||
            attackPoint == null)
        {
            return;
        }

        Projectile projectile =
            Instantiate(
                projectilePrefab,
                attackPoint.position,
                attackPoint.rotation
            );

        projectile.Initialize(
            enemyHealth,
            attackDamage
        );
    }
}