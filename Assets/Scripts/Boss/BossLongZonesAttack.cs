using UnityEngine;
using UnityEngine.AI;

public class BossLongZonesAttack : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private int playerDamage = 55;

    [Header("Zone Settings")]
    [SerializeField] private float zoneRadius = 1.75f;
    [SerializeField] private float randomPlacementRadius = 4f;
    [SerializeField] private float minimumZoneSpacing = 2.5f;
    [SerializeField] private int randomPositionAttempts = 20;

    [Header("Navigation Settings")]
    [SerializeField] private float navMeshSampleDistance = 2f;

    [Header("Visual Settings")]
    [SerializeField]
    private GameObject[] attackIndicators =
        new GameObject[3];

    [SerializeField] private float indicatorYOffset = 0.03f;
    [SerializeField] private float indicatorThickness = 0.02f;

    [Header("Debug - Do Not Edit")]
    [SerializeField] private Vector3 debugZone01Position;
    [SerializeField] private Vector3 debugZone02Position;
    [SerializeField] private Vector3 debugZone03Position;
    [SerializeField] private int debugLastOverlapCount;
    [SerializeField] private bool debugLastAttackHitPlayer;

    private PlayerHealth playerHealth;
    private Collider playerCollider;

    private readonly Vector3[] zonePositions =
        new Vector3[3];

    private void Start()
    {
        playerHealth =
            FindAnyObjectByType<PlayerHealth>();

        if (playerHealth != null)
        {
            playerCollider =
                playerHealth.GetComponent<Collider>();
        }

        HideIndicators();

        if (playerHealth == null)
        {
            Debug.LogWarning(
                "BossLongZonesAttack: PlayerHealth was not found."
            );
        }

        if (playerCollider == null)
        {
            Debug.LogWarning(
                "BossLongZonesAttack: Player Collider was not found."
            );
        }

        if (attackIndicators == null ||
            attackIndicators.Length != 3)
        {
            Debug.LogWarning(
                "BossLongZonesAttack: Exactly 3 Attack Indicators are required."
            );

            return;
        }

        for (int i = 0;
             i < attackIndicators.Length;
             i++)
        {
            if (attackIndicators[i] == null)
            {
                Debug.LogWarning(
                    "BossLongZonesAttack: Attack Indicator " +
                    (i + 1) +
                    " is not assigned."
                );
            }
        }
    }

    public void PrepareAttack()
    {
        if (playerHealth == null ||
            playerHealth.IsDead)
        {
            HideIndicators();
            return;
        }

        RotateTowardsPlayer();

        zonePositions[0] =
            GetPlayerGroundPosition();

        zonePositions[1] =
            GenerateRandomZonePosition(1);

        zonePositions[2] =
            GenerateRandomZonePosition(2);

        UpdateDebugPositions();
        ShowIndicators();
    }

    public void ExecuteAttack()
    {
        HideIndicators();

        debugLastOverlapCount =
            CountPlayerZoneOverlaps();

        debugLastAttackHitPlayer =
            debugLastOverlapCount > 0;

        if (!debugLastAttackHitPlayer)
        {
            Debug.Log(
                "BossLongZonesAttack: Attack missed."
            );

            return;
        }

        Debug.Log(
            "BossLongZonesAttack: Player overlaps " +
            debugLastOverlapCount +
            " zone(s). Damage is applied once."
        );

        Debug.Log(
            "BossLongZonesAttack: Player takes " +
            playerDamage +
            " damage."
        );

        playerHealth.TakeDamage(
            playerDamage
        );
    }

    private Vector3 GetPlayerGroundPosition()
    {
        Vector3 playerPosition =
            playerHealth.transform.position;

        if (NavMesh.SamplePosition(
                playerPosition,
                out NavMeshHit hit,
                navMeshSampleDistance,
                NavMesh.AllAreas
            ))
        {
            return hit.position;
        }

        return new Vector3(
            playerPosition.x,
            transform.position.y,
            playerPosition.z
        );
    }

    private Vector3 GenerateRandomZonePosition(
        int zoneIndex
    )
    {
        Vector3 centerPosition =
            zonePositions[0];

        for (int attempt = 0;
             attempt < randomPositionAttempts;
             attempt++)
        {
            Vector2 randomOffset =
                Random.insideUnitCircle *
                randomPlacementRadius;

            Vector3 candidatePosition =
                centerPosition +
                new Vector3(
                    randomOffset.x,
                    0f,
                    randomOffset.y
                );

            if (!NavMesh.SamplePosition(
                    candidatePosition,
                    out NavMeshHit hit,
                    navMeshSampleDistance,
                    NavMesh.AllAreas
                ))
            {
                continue;
            }

            candidatePosition =
                hit.position;

            if (IsFarEnoughFromOtherZones(
                    candidatePosition,
                    zoneIndex
                ))
            {
                return candidatePosition;
            }
        }

        return GetFallbackZonePosition(
            zoneIndex
        );
    }

    private bool IsFarEnoughFromOtherZones(
        Vector3 candidatePosition,
        int zoneIndex
    )
    {
        for (int i = 0;
             i < zoneIndex;
             i++)
        {
            Vector3 direction =
                candidatePosition -
                zonePositions[i];

            direction.y = 0f;

            if (direction.magnitude <
                minimumZoneSpacing)
            {
                return false;
            }
        }

        return true;
    }

    private Vector3 GetFallbackZonePosition(
        int zoneIndex
    )
    {
        float angle;

        if (zoneIndex == 1)
        {
            angle = 120f;
        }
        else
        {
            angle = -120f;
        }

        Vector3 direction =
            Quaternion.Euler(
                0f,
                angle,
                0f
            ) *
            transform.forward;

        Vector3 fallbackPosition =
            zonePositions[0] +
            direction.normalized *
            minimumZoneSpacing;

        if (NavMesh.SamplePosition(
                fallbackPosition,
                out NavMeshHit hit,
                navMeshSampleDistance,
                NavMesh.AllAreas
            ))
        {
            return hit.position;
        }

        return fallbackPosition;
    }

    private int CountPlayerZoneOverlaps()
    {
        if (playerHealth == null ||
            playerCollider == null ||
            playerHealth.IsDead)
        {
            return 0;
        }

        int overlapCount = 0;

        for (int i = 0;
             i < zonePositions.Length;
             i++)
        {
            if (IsPlayerInsideZone(
                    zonePositions[i]
                ))
            {
                overlapCount++;
            }
        }

        return overlapCount;
    }

    private bool IsPlayerInsideZone(
        Vector3 zonePosition
    )
    {
        Vector3 closestPoint =
            playerCollider.ClosestPoint(
                zonePosition
            );

        Vector3 directionToPlayer =
            closestPoint -
            zonePosition;

        directionToPlayer.y = 0f;

        return directionToPlayer.sqrMagnitude <=
               zoneRadius * zoneRadius;
    }

    private void RotateTowardsPlayer()
    {
        Vector3 directionToPlayer =
            playerHealth.transform.position -
            transform.position;

        directionToPlayer.y = 0f;

        if (directionToPlayer.sqrMagnitude <= 0.001f)
        {
            return;
        }

        transform.rotation =
            Quaternion.LookRotation(
                directionToPlayer.normalized
            );
    }

    private void ShowIndicators()
    {
        if (attackIndicators == null)
        {
            return;
        }

        int indicatorCount =
            Mathf.Min(
                attackIndicators.Length,
                zonePositions.Length
            );

        for (int i = 0;
             i < indicatorCount;
             i++)
        {
            GameObject indicator =
                attackIndicators[i];

            if (indicator == null)
            {
                continue;
            }

            indicator.transform.position =
                zonePositions[i] +
                Vector3.up *
                indicatorYOffset;

            indicator.transform.rotation =
                Quaternion.identity;

            indicator.transform.localScale =
                new Vector3(
                    zoneRadius * 2f,
                    indicatorThickness,
                    zoneRadius * 2f
                );

            indicator.SetActive(true);
        }
    }

    public void HideIndicators()
    {
        if (attackIndicators == null)
        {
            return;
        }

        foreach (GameObject indicator
                 in attackIndicators)
        {
            if (indicator != null)
            {
                indicator.SetActive(false);
            }
        }
    }

    private void UpdateDebugPositions()
    {
        debugZone01Position =
            zonePositions[0];

        debugZone02Position =
            zonePositions[1];

        debugZone03Position =
            zonePositions[2];
    }

    private void OnDisable()
    {
        HideIndicators();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color =
            new Color(0.7f, 0f, 1f, 0.35f);

        foreach (Vector3 zonePosition
                 in zonePositions)
        {
            Gizmos.DrawWireSphere(
                zonePosition,
                zoneRadius
            );
        }
    }
}