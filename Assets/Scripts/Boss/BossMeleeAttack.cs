using UnityEngine;

public class BossMeleeAttack : MonoBehaviour
{
    // Runtime design values are supplied by BossBalanceBinder
    // from GameBalanceConfig before Start().
    private int playerDamage;
    private int coreTowerDamage;
    private float attackRadius;

    [Header("Visual Settings")]
    [SerializeField] private GameObject attackIndicator;

    private PlayerHealth playerHealth;
    private Collider playerCollider;

    private CoreTowerHealth coreTower;
    private Collider coreTowerCollider;

    private bool balanceConfigured;
    private bool hasStarted;

    public bool ConfigureBalance(
        BossMeleeAttackBalanceValues balance
    )
    {
        if (hasStarted ||
            balance == null)
        {
            return false;
        }

        playerDamage =
            balance.PlayerDamage;

        coreTowerDamage =
            balance.CoreTowerDamage;

        attackRadius =
            balance.AttackRadius;

        balanceConfigured = true;
        return true;
    }

    private void Start()
    {
        hasStarted = true;

        if (!balanceConfigured)
        {
            Debug.LogError(
                "BossMeleeAttack: Boss attack balance was not " +
                "configured before Start().",
                this
            );

            enabled = false;
            return;
        }

        playerHealth =
            FindAnyObjectByType<PlayerHealth>();

        if (playerHealth != null)
        {
            playerCollider =
                playerHealth.GetComponent<Collider>();
        }

        coreTower =
            FindAnyObjectByType<CoreTowerHealth>();

        if (coreTower != null)
        {
            coreTowerCollider =
                coreTower.GetComponent<Collider>();
        }

        HideIndicator();

        if (playerHealth == null)
        {
            Debug.LogWarning(
                "BossMeleeAttack: PlayerHealth was not found."
            );
        }

        if (playerCollider == null)
        {
            Debug.LogWarning(
                "BossMeleeAttack: Player Collider was not found."
            );
        }

        if (coreTower == null)
        {
            Debug.LogWarning(
                "BossMeleeAttack: CoreTowerHealth was not found."
            );
        }

        if (coreTowerCollider == null)
        {
            Debug.LogWarning(
                "BossMeleeAttack: CoreTower Collider was not found."
            );
        }

        if (attackIndicator == null)
        {
            Debug.LogWarning(
                "BossMeleeAttack: Attack Indicator is not assigned."
            );
        }
    }

    public void ShowIndicator()
    {
        if (attackIndicator != null)
        {
            attackIndicator.SetActive(true);
        }
    }

    public void HideIndicator()
    {
        if (attackIndicator != null)
        {
            attackIndicator.SetActive(false);
        }
    }

    public void ExecuteAttack()
    {
        HideIndicator();

        bool playerWasHit =
            TryDamagePlayer();

        bool coreTowerWasHit =
            TryDamageCoreTower();

        if (!playerWasHit &&
            !coreTowerWasHit)
        {
            Debug.Log(
                "BossMeleeAttack: Attack missed."
            );
        }
    }

    private bool TryDamagePlayer()
    {
        if (playerHealth == null ||
            playerCollider == null ||
            playerHealth.IsDead)
        {
            return false;
        }

        if (!IsInsideAttackArea(
                playerCollider
            ))
        {
            return false;
        }

        Debug.Log(
            "BossMeleeAttack: Player takes " +
            playerDamage +
            " damage."
        );

        playerHealth.TakeDamage(
            playerDamage
        );

        return true;
    }

    private bool TryDamageCoreTower()
    {
        if (coreTower == null ||
            coreTowerCollider == null)
        {
            return false;
        }

        if (!IsInsideAttackArea(
                coreTowerCollider
            ))
        {
            return false;
        }

        Debug.Log(
            "BossMeleeAttack: Core Tower takes " +
            coreTowerDamage +
            " damage."
        );

        coreTower.TakeDamage(
            coreTowerDamage
        );

        return true;
    }

    private bool IsInsideAttackArea(
        Collider targetCollider
    )
    {
        Vector3 closestPoint =
            targetCollider.ClosestPoint(
                transform.position
            );

        Vector3 directionToTarget =
            closestPoint -
            transform.position;

        directionToTarget.y = 0f;

        return directionToTarget.sqrMagnitude <=
               attackRadius * attackRadius;
    }

    private void OnDisable()
    {
        HideIndicator();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color =
            new Color(1f, 0f, 0f, 0.35f);

        Gizmos.DrawWireSphere(
            transform.position,
            attackRadius
        );
    }
}