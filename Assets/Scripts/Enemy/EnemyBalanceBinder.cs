using UnityEngine;

public enum EnemyBalanceType
{
    Melee,
    Strong,
    Ranged,
    Siege
}

[DefaultExecutionOrder(-1000)]
[DisallowMultipleComponent]
public class EnemyBalanceBinder : MonoBehaviour
{
    [Header("Enemy Identity")]

    [SerializeField]
    private EnemyBalanceType enemyType =
        EnemyBalanceType.Melee;

    [Header("Master Balance Source")]

    [SerializeField]
    private GameBalanceConfig balanceConfig;


    public EnemyBalanceType EnemyType =>
        enemyType;


    private void Awake()
    {
        if (balanceConfig == null)
        {
            Debug.LogError(
                "EnemyBalanceBinder: GameBalanceConfig " +
                "is not assigned.",
                this
            );

            enabled = false;
            return;
        }

        EnemyBalanceValues balance =
            GetBalanceValues();

        if (balance == null)
        {
            Debug.LogError(
                "EnemyBalanceBinder: Enemy balance values " +
                $"were not found for {enemyType}.",
                this
            );

            enabled = false;
            return;
        }

        switch (enemyType)
        {
            case EnemyBalanceType.Melee:
            case EnemyBalanceType.Strong:

                ApplyHealthBalance(
                    balance
                );

                ApplyMeleeAttackBalance(
                    balance
                );

                ApplyMeleeMovementBalance(
                    balance
                );

                break;


            case EnemyBalanceType.Ranged:

                ApplyHealthBalance(
                    balance
                );

                ApplyRangedAttackBalance(
                    balance
                );

                ApplyRangedMovementBalance(
                    balance
                );

                break;


            case EnemyBalanceType.Siege:

                ApplyHealthBalance(
                    balance
                );

                ApplySiegeAttackBalance(
                    balance
                );

                ApplySiegeMovementBalance(
                    balance
                );

                break;
        }
    }


    private EnemyBalanceValues GetBalanceValues()
    {
        switch (enemyType)
        {
            case EnemyBalanceType.Melee:
                return balanceConfig.MeleeEnemy;

            case EnemyBalanceType.Strong:
                return balanceConfig.StrongEnemy;

            case EnemyBalanceType.Ranged:
                return balanceConfig.RangedEnemy;

            case EnemyBalanceType.Siege:
                return balanceConfig.SiegeEnemy;

            default:
                return null;
        }
    }


    // =========================================================
    // SHARED HEALTH
    // =========================================================

    private void ApplyHealthBalance(
        EnemyBalanceValues balance
    )
    {
        EnemyHealth enemyHealth =
            GetComponent<EnemyHealth>();

        if (enemyHealth == null)
        {
            Debug.LogError(
                "EnemyBalanceBinder: EnemyHealth " +
                "component was not found.",
                this
            );

            return;
        }

        bool configured =
            enemyHealth.ConfigureBaseMaxHealth(
                balance.BaseMaxHealth
            );

        if (!configured)
        {
            Debug.LogError(
                "EnemyBalanceBinder: Could not apply " +
                "base health.",
                this
            );
        }
    }


    // =========================================================
    // MELEE / STRONG
    // =========================================================

    private void ApplyMeleeAttackBalance(
        EnemyBalanceValues balance
    )
    {
        EnemyAttack enemyAttack =
            GetComponent<EnemyAttack>();

        if (enemyAttack == null)
        {
            Debug.LogError(
                "EnemyBalanceBinder: EnemyAttack " +
                "component was not found.",
                this
            );

            return;
        }

        bool configured =
            enemyAttack.ConfigureBaseAttack(
                balance.BaseDamage,
                balance.AttackInterval,
                balance.AttackWindup
            );

        if (!configured)
        {
            Debug.LogError(
                "EnemyBalanceBinder: Could not apply " +
                "melee base attack settings.",
                this
            );
        }
    }


    private void ApplyMeleeMovementBalance(
        EnemyBalanceValues balance
    )
    {
        EnemyMovement enemyMovement =
            GetComponent<EnemyMovement>();

        if (enemyMovement == null)
        {
            Debug.LogError(
                "EnemyBalanceBinder: EnemyMovement " +
                "component was not found.",
                this
            );

            return;
        }

        bool configured =
            enemyMovement.ConfigureMoveSpeed(
                balance.MoveSpeed
            );

        if (!configured)
        {
            Debug.LogError(
                "EnemyBalanceBinder: Could not apply " +
                "melee move speed.",
                this
            );
        }
    }


    // =========================================================
    // RANGED
    // =========================================================

    private void ApplyRangedAttackBalance(
        EnemyBalanceValues balance
    )
    {
        RangedEnemyAttack rangedAttack =
            GetComponent<RangedEnemyAttack>();

        if (rangedAttack == null)
        {
            Debug.LogError(
                "EnemyBalanceBinder: RangedEnemyAttack " +
                "component was not found.",
                this
            );

            return;
        }

        bool configured =
            rangedAttack.ConfigureBaseAttack(
                balance.BaseDamage,
                balance.AttackInterval,
                balance.AttackWindup,
                balanceConfig.RangedEnemyProjectileSpeed
            );

        if (!configured)
        {
            Debug.LogError(
                "EnemyBalanceBinder: Could not apply " +
                "ranged base attack settings.",
                this
            );
        }
    }


    private void ApplyRangedMovementBalance(
        EnemyBalanceValues balance
    )
    {
        RangedEnemyMovement rangedMovement =
            GetComponent<RangedEnemyMovement>();

        if (rangedMovement == null)
        {
            Debug.LogError(
                "EnemyBalanceBinder: RangedEnemyMovement " +
                "component was not found.",
                this
            );

            return;
        }

        bool configured =
            rangedMovement.ConfigureMoveSpeed(
                balance.MoveSpeed
            );

        if (!configured)
        {
            Debug.LogError(
                "EnemyBalanceBinder: Could not apply " +
                "ranged move speed.",
                this
            );
        }
    }


    // =========================================================
    // SIEGE
    // =========================================================

    private void ApplySiegeAttackBalance(
        EnemyBalanceValues balance
    )
    {
        SiegeEnemyAttack siegeAttack =
            GetComponent<SiegeEnemyAttack>();

        if (siegeAttack == null)
        {
            Debug.LogError(
                "EnemyBalanceBinder: SiegeEnemyAttack " +
                "component was not found.",
                this
            );

            return;
        }

        bool configured =
            siegeAttack.ConfigureBaseAttack(
                balance.BaseDamage,
                balance.AttackInterval,
                balance.AttackWindup
            );

        if (!configured)
        {
            Debug.LogError(
                "EnemyBalanceBinder: Could not apply " +
                "siege base attack settings.",
                this
            );
        }

        bool multipliersConfigured =
            siegeAttack.ConfigureTargetDamageMultipliers(
                balanceConfig.SiegeDefenseTowerDamageMultiplier,
                balanceConfig.SiegeCoreTowerDamageMultiplier
            );

        if (!multipliersConfigured)
        {
            Debug.LogError(
                "EnemyBalanceBinder: Could not apply " +
                "siege target damage multipliers.",
                this
            );
        }
    }


    private void ApplySiegeMovementBalance(
        EnemyBalanceValues balance
    )
    {
        SiegeEnemyMovement siegeMovement =
            GetComponent<SiegeEnemyMovement>();

        if (siegeMovement == null)
        {
            Debug.LogError(
                "EnemyBalanceBinder: SiegeEnemyMovement " +
                "component was not found.",
                this
            );

            return;
        }

        bool configured =
            siegeMovement.ConfigureMoveSpeed(
                balance.MoveSpeed
            );

        if (!configured)
        {
            Debug.LogError(
                "EnemyBalanceBinder: Could not apply " +
                "siege move speed.",
                this
            );
        }
    }
}