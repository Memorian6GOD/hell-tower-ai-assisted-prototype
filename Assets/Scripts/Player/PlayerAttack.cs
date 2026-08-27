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
    [SerializeField] private PlayerAttackAnimation attackAnimation;

    [Header("Target Settings")]
    [SerializeField] private LayerMask enemyLayer;

    private PlayerMovement playerMovement;
    private PlayerHealth playerHealth;

    private float attackTimer;

    private EnemyHealth currentTargetHealth;

    private bool attackAnimationStarted;

    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerHealth = GetComponent<PlayerHealth>();

        attackTimer = attackWindup;
    }

    void Update()
    {
        if (playerHealth != null && playerHealth.IsDead)
        {
            ClearTarget();
            ResetAttackCycle();
            return;
        }

        if (playerMovement.IsMoving)
        {
            ClearTarget();
            ResetAttackCycle();
            return;
        }

        Collider nearestEnemy = FindNearestEnemy();

        if (nearestEnemy == null)
        {
            ClearTarget();
            ResetAttackCycle();
            return;
        }

        EnemyHealth nearestEnemyHealth =
            nearestEnemy.GetComponent<EnemyHealth>();

        if (nearestEnemyHealth == null)
        {
            ClearTarget();
            ResetAttackCycle();
            return;
        }

        UpdateTarget(nearestEnemyHealth);

        RotateTowardsEnemy(nearestEnemy);

        attackTimer -= Time.deltaTime;

        if (!attackAnimationStarted &&
            attackTimer <= attackWindup)
        {
            StartAttackAnimation();
        }

        if (attackTimer > 0f)
            return;

        AttackEnemy(nearestEnemyHealth);

        attackTimer = attackInterval;
        attackAnimationStarted = false;
    }

    private Collider FindNearestEnemy()
    {
        Collider[] enemiesInRange = Physics.OverlapSphere(
            transform.position,
            attackRadius,
            enemyLayer
        );

        Collider nearestEnemy = null;
        float shortestDistance = float.MaxValue;

        foreach (Collider enemy in enemiesInRange)
        {
            float distance = Vector3.Distance(
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

    private void UpdateTarget(EnemyHealth newTargetHealth)
    {
        if (currentTargetHealth == newTargetHealth)
            return;

        if (currentTargetHealth != null)
        {
            currentTargetHealth.HideHealthBar();
        }

        currentTargetHealth = newTargetHealth;

        if (currentTargetHealth != null)
        {
            currentTargetHealth.ShowHealthBar();
        }
    }

    private void ClearTarget()
    {
        if (currentTargetHealth != null)
        {
            currentTargetHealth.HideHealthBar();
            currentTargetHealth = null;
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

    private void StartAttackAnimation()
    {
        attackAnimationStarted = true;

        if (attackAnimation != null)
        {
            attackAnimation.Play(attackWindup);
        }
    }

    private void RotateTowardsEnemy(Collider enemy)
    {
        Vector3 direction =
            enemy.transform.position - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    private void AttackEnemy(EnemyHealth enemyHealth)
    {
        if (enemyHealth == null)
            return;

        if (projectilePrefab == null || attackPoint == null)
            return;

        Projectile projectile = Instantiate(
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