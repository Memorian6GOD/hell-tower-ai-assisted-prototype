using UnityEngine;

public class BossMediumLineAttack : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private int playerDamage = 45;
    [SerializeField] private int coreTowerDamage = 120;

    [Header("Area Settings")]
    [SerializeField] private float lineLength = 8f;
    [SerializeField] private float lineWidth = 2.5f;
    [SerializeField] private float attackHeight = 3f;

    [Header("Visual Settings")]
    [SerializeField] private GameObject attackIndicator;
    [SerializeField] private float indicatorYOffset = 0.03f;
    [SerializeField] private float indicatorThickness = 0.02f;

    private PlayerHealth playerHealth;
    private Collider playerCollider;

    private CoreTowerHealth coreTower;
    private Collider coreTowerCollider;

    private Vector3 lockedAttackCenter;
    private Quaternion lockedAttackRotation;

    private void Start()
    {
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
                "BossMediumLineAttack: PlayerHealth was not found."
            );
        }

        if (playerCollider == null)
        {
            Debug.LogWarning(
                "BossMediumLineAttack: Player Collider was not found."
            );
        }

        if (coreTower == null)
        {
            Debug.LogWarning(
                "BossMediumLineAttack: CoreTowerHealth was not found."
            );
        }

        if (coreTowerCollider == null)
        {
            Debug.LogWarning(
                "BossMediumLineAttack: CoreTower Collider was not found."
            );
        }

        if (attackIndicator == null)
        {
            Debug.LogWarning(
                "BossMediumLineAttack: Attack Indicator is not assigned."
            );
        }
    }

    public void PrepareAttack()
    {
        if (playerHealth == null ||
            playerHealth.IsDead)
        {
            HideIndicator();
            return;
        }

        Vector3 directionToPlayer =
            playerHealth.transform.position -
            transform.position;

        directionToPlayer.y = 0f;

        if (directionToPlayer.sqrMagnitude <= 0.001f)
        {
            directionToPlayer =
                transform.forward;
        }

        directionToPlayer.Normalize();

        lockedAttackRotation =
            Quaternion.LookRotation(
                directionToPlayer
            );

        transform.rotation =
            lockedAttackRotation;

        Vector3 groundCenter =
            transform.position +
            directionToPlayer *
            (lineLength * 0.5f);

        lockedAttackCenter =
            new Vector3(
                groundCenter.x,
                transform.position.y +
                attackHeight * 0.5f,
                groundCenter.z
            );

        ShowIndicator();
    }

    public void ExecuteAttack()
    {
        HideIndicator();

        Collider[] hitColliders =
            Physics.OverlapBox(
                lockedAttackCenter,
                new Vector3(
                    lineWidth * 0.5f,
                    attackHeight * 0.5f,
                    lineLength * 0.5f
                ),
                lockedAttackRotation,
                Physics.AllLayers,
                QueryTriggerInteraction.Collide
            );

        bool playerWasHit =
            TryDamagePlayer(hitColliders);

        bool coreTowerWasHit =
            TryDamageCoreTower(
                hitColliders
            );

        if (!playerWasHit &&
            !coreTowerWasHit)
        {
            Debug.Log(
                "BossMediumLineAttack: Attack missed."
            );
        }
    }

    private bool TryDamagePlayer(
        Collider[] hitColliders
    )
    {
        if (playerHealth == null ||
            playerCollider == null ||
            playerHealth.IsDead)
        {
            return false;
        }

        if (!ContainsTargetCollider(
                hitColliders,
                playerCollider
            ))
        {
            return false;
        }

        Debug.Log(
            "BossMediumLineAttack: Player takes " +
            playerDamage +
            " damage."
        );

        playerHealth.TakeDamage(
            playerDamage
        );

        return true;
    }

    private bool TryDamageCoreTower(
        Collider[] hitColliders
    )
    {
        if (coreTower == null ||
            coreTowerCollider == null)
        {
            return false;
        }

        if (!ContainsTargetCollider(
                hitColliders,
                coreTowerCollider
            ))
        {
            return false;
        }

        Debug.Log(
            "BossMediumLineAttack: Core Tower takes " +
            coreTowerDamage +
            " damage."
        );

        coreTower.TakeDamage(
            coreTowerDamage
        );

        return true;
    }

    private bool ContainsTargetCollider(
        Collider[] hitColliders,
        Collider targetCollider
    )
    {
        foreach (Collider hitCollider in hitColliders)
        {
            if (hitCollider == null)
            {
                continue;
            }

            if (hitCollider == targetCollider)
            {
                return true;
            }

            if (hitCollider.transform.IsChildOf(
                    targetCollider.transform
                ))
            {
                return true;
            }

            if (targetCollider.transform.IsChildOf(
                    hitCollider.transform
                ))
            {
                return true;
            }
        }

        return false;
    }

    private void ShowIndicator()
    {
        if (attackIndicator == null)
        {
            return;
        }

        attackIndicator.transform.localPosition =
            new Vector3(
                0f,
                indicatorYOffset,
                lineLength * 0.5f
            );

        attackIndicator.transform.localRotation =
            Quaternion.identity;

        attackIndicator.transform.localScale =
            new Vector3(
                lineWidth,
                indicatorThickness,
                lineLength
            );

        attackIndicator.SetActive(true);
    }

    public void HideIndicator()
    {
        if (attackIndicator != null)
        {
            attackIndicator.SetActive(false);
        }
    }

    private void OnDisable()
    {
        HideIndicator();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color =
            new Color(1f, 0.5f, 0f, 0.35f);

        Vector3 center =
            transform.position +
            transform.forward *
            (lineLength * 0.5f);

        Vector3 size =
            new Vector3(
                lineWidth,
                0.05f,
                lineLength
            );

        Matrix4x4 previousMatrix =
            Gizmos.matrix;

        Gizmos.matrix =
            Matrix4x4.TRS(
                center,
                transform.rotation,
                Vector3.one
            );

        Gizmos.DrawWireCube(
            Vector3.zero,
            size
        );

        Gizmos.matrix = previousMatrix;
    }
}