using UnityEngine;
using UnityEngine.AI;

public class CoreTowerAttackSlots : MonoBehaviour
{
    [Header("Melee Attack Slots")]
    [SerializeField]
    private Transform[] attackSlots;

    [Header("Editor Layout Settings")]

    [Tooltip(
        "Object relative to which melee slots are arranged."
    )]
    [SerializeField]
    private Transform slotsRoot;

    [Tooltip(
        "Required number of melee attack slots."
    )]
    [SerializeField, Min(1)]
    private int desiredSlotCount = 14;

    [Tooltip(
        "Base distance of melee slots from the CoreTower."
    )]
    [SerializeField, Min(0.1f)]
    private float slotRadius = 4f;

    [Tooltip(
        "Total angle of the melee slot arc."
    )]
    [SerializeField, Range(10f, 360f)]
    private float arcAngle = 180f;

    [Tooltip(
        "Direction of the arc center relative to Slots Root."
    )]
    [SerializeField, Range(-180f, 180f)]
    private float arcCenterAngle = 180f;

    [Tooltip(
        "Vertical position of slots relative to Slots Root."
    )]
    [SerializeField]
    private float slotHeight = 0f;

    [Header("Additional Radius Settings")]

    [Tooltip(
        "First slot that receives additional radius."
    )]
    [SerializeField, Min(1)]
    private int extraRadiusStartSlot = 5;

    [Tooltip(
        "Last slot that receives additional radius."
    )]
    [SerializeField, Min(1)]
    private int extraRadiusEndSlot = 12;

    [Tooltip(
        "Additional distance applied to selected slots."
    )]
    [SerializeField, Min(0f)]
    private float extraRadius = 0.6f;

    [Header("NavMesh Slot Selection")]

    [Tooltip(
        "Maximum distance used to find a NavMesh point " +
        "near an attack slot."
    )]
    [SerializeField, Min(0.1f)]
    private float slotNavMeshSampleDistance = 1.5f;

    [Header("Debug - Do Not Edit")]

    [SerializeField]
    private int debugTotalSlotCount;

    [SerializeField]
    private int debugFreeSlotCount;

    [SerializeField]
    private int debugReachableSlotCount;

    [SerializeField]
    private int debugRejectedSlotCount;

    [SerializeField]
    private string debugLastClaimedSlot = "None";

    [SerializeField]
    private float debugLastPathLength;

    private EnemyMovement[] occupants;

    private void Awake()
    {
        int slotCount = 0;

        if (attackSlots != null)
        {
            slotCount = attackSlots.Length;
        }

        occupants =
            new EnemyMovement[slotCount];

        RefreshDebugSlotCounts();
    }

    [ContextMenu("Create And Arrange Melee Slots")]
    private void CreateAndArrangeMeleeSlots()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning(
                "CoreTowerAttackSlots: Slots can only be " +
                "created outside Play Mode."
            );

            return;
        }

        Transform root =
            slotsRoot != null
                ? slotsRoot
                : transform;

        Transform[] previousSlots =
            attackSlots ??
            new Transform[0];

        Transform[] newSlots =
            new Transform[desiredSlotCount];

        for (int i = 0;
             i < desiredSlotCount;
             i++)
        {
            if (i < previousSlots.Length &&
                previousSlots[i] != null)
            {
                newSlots[i] =
                    previousSlots[i];

                continue;
            }

            GameObject slotObject =
                new GameObject(
                    $"MeleeAttackSlot_{i + 1:00}"
                );

            slotObject.transform.SetParent(
                root,
                false
            );

            newSlots[i] =
                slotObject.transform;
        }

        attackSlots = newSlots;

        ArrangeMeleeSlots();

        Debug.Log(
            $"CoreTowerAttackSlots: " +
            $"{attackSlots.Length} melee slots " +
            "created and arranged."
        );
    }

    [ContextMenu("Arrange Melee Slots")]
    private void ArrangeMeleeSlots()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning(
                "CoreTowerAttackSlots: Slots can only be " +
                "arranged outside Play Mode."
            );

            return;
        }

        if (attackSlots == null ||
            attackSlots.Length == 0)
        {
            Debug.LogWarning(
                "CoreTowerAttackSlots: There are no " +
                "melee slots to arrange."
            );

            return;
        }

        Transform root =
            slotsRoot != null
                ? slotsRoot
                : transform;

        for (int i = 0;
             i < attackSlots.Length;
             i++)
        {
            if (attackSlots[i] == null)
            {
                continue;
            }

            float normalizedPosition =
                attackSlots.Length == 1
                    ? 0.5f
                    : (float)i /
                      (attackSlots.Length - 1);

            float angle =
                arcCenterAngle +
                Mathf.Lerp(
                    -arcAngle * 0.5f,
                    arcAngle * 0.5f,
                    normalizedPosition
                );

            float angleInRadians =
                angle * Mathf.Deg2Rad;

            int slotNumber =
                i + 1;

            float currentRadius =
                slotRadius;

            if (slotNumber >=
                    extraRadiusStartSlot &&
                slotNumber <=
                    extraRadiusEndSlot)
            {
                currentRadius +=
                    extraRadius;
            }

            Vector3 localPosition =
                new Vector3(
                    Mathf.Sin(
                        angleInRadians
                    ) * currentRadius,
                    slotHeight,
                    Mathf.Cos(
                        angleInRadians
                    ) * currentRadius
                );

            attackSlots[i].position =
                root.TransformPoint(
                    localPosition
                );
        }
    }

    public Transform ClaimClosestSlot(
        EnemyMovement enemy
    )
    {
        if (enemy == null ||
            attackSlots == null ||
            occupants == null ||
            attackSlots.Length == 0)
        {
            return null;
        }

        // If this enemy already owns a slot,
        // keep returning the same slot.
        for (int i = 0;
             i < occupants.Length;
             i++)
        {
            if (occupants[i] == enemy)
            {
                return attackSlots[i];
            }
        }

        NavMeshAgent enemyAgent =
            enemy.GetComponent<NavMeshAgent>();

        if (enemyAgent == null ||
            !enemyAgent.isOnNavMesh)
        {
            Debug.LogWarning(
                "CoreTowerAttackSlots: Enemy has no " +
                "usable NavMeshAgent.",
                enemy
            );

            return null;
        }

        int freeSlotCount = 0;
        int reachableSlotCount = 0;
        int rejectedSlotCount = 0;

        int bestSlotIndex = -1;

        float bestPathLength =
            float.MaxValue;

        NavMeshPath path =
            new NavMeshPath();

        for (int i = 0;
             i < attackSlots.Length;
             i++)
        {
            Transform slot =
                attackSlots[i];

            if (slot == null)
            {
                continue;
            }

            if (occupants[i] != null)
            {
                continue;
            }

            freeSlotCount++;

            bool sampleSucceeded =
                NavMesh.SamplePosition(
                    slot.position,
                    out NavMeshHit slotHit,
                    slotNavMeshSampleDistance,
                    enemyAgent.areaMask
                );

            if (!sampleSucceeded)
            {
                rejectedSlotCount++;
                continue;
            }

            bool pathCalculated =
                NavMesh.CalculatePath(
                    enemyAgent.transform.position,
                    slotHit.position,
                    enemyAgent.areaMask,
                    path
                );

            if (!pathCalculated ||
                path.status !=
                NavMeshPathStatus.PathComplete)
            {
                rejectedSlotCount++;
                continue;
            }

            float pathLength =
                CalculatePathLength(path);

            if (float.IsInfinity(pathLength) ||
                float.IsNaN(pathLength))
            {
                rejectedSlotCount++;
                continue;
            }

            reachableSlotCount++;

            if (pathLength <
                bestPathLength)
            {
                bestPathLength =
                    pathLength;

                bestSlotIndex =
                    i;
            }
        }

        debugTotalSlotCount =
            attackSlots.Length;

        debugFreeSlotCount =
            freeSlotCount;

        debugReachableSlotCount =
            reachableSlotCount;

        debugRejectedSlotCount =
            rejectedSlotCount;

        if (bestSlotIndex == -1)
        {
            debugLastClaimedSlot =
                "None";

            debugLastPathLength =
                0f;

            Debug.LogWarning(
                "CoreTowerAttackSlots: No reachable " +
                $"free melee slot found for {enemy.name}. " +
                $"Free: {freeSlotCount}, " +
                $"reachable: {reachableSlotCount}, " +
                $"rejected: {rejectedSlotCount}.",
                enemy
            );

            return null;
        }

        occupants[bestSlotIndex] =
            enemy;

        debugLastClaimedSlot =
            attackSlots[
                bestSlotIndex
            ].name;

        debugLastPathLength =
            bestPathLength;

        RefreshDebugSlotCounts();

        Debug.Log(
            "CoreTowerAttackSlots: " +
            $"{enemy.name} claimed " +
            $"'{debugLastClaimedSlot}'. " +
            $"NavMesh path length: " +
            $"{bestPathLength:F2}. " +
            $"Reachable free slots checked: " +
            $"{reachableSlotCount}.",
            enemy
        );

        return attackSlots[
            bestSlotIndex
        ];
    }

    public void ReleaseSlot(
        EnemyMovement enemy
    )
    {
        if (enemy == null ||
            occupants == null)
        {
            return;
        }

        for (int i = 0;
             i < occupants.Length;
             i++)
        {
            if (occupants[i] != enemy)
            {
                continue;
            }

            occupants[i] = null;

            RefreshDebugSlotCounts();

            return;
        }
    }

    private float CalculatePathLength(
        NavMeshPath path
    )
    {
        if (path == null ||
            path.corners == null ||
            path.corners.Length < 2)
        {
            return 0f;
        }

        float totalLength = 0f;

        for (int i = 1;
             i < path.corners.Length;
             i++)
        {
            totalLength +=
                Vector3.Distance(
                    path.corners[i - 1],
                    path.corners[i]
                );
        }

        return totalLength;
    }

    private void RefreshDebugSlotCounts()
    {
        if (attackSlots == null)
        {
            debugTotalSlotCount = 0;
            debugFreeSlotCount = 0;
            return;
        }

        debugTotalSlotCount =
            attackSlots.Length;

        if (occupants == null)
        {
            debugFreeSlotCount =
                attackSlots.Length;

            return;
        }

        int freeCount = 0;

        for (int i = 0;
             i < occupants.Length;
             i++)
        {
            if (occupants[i] == null)
            {
                freeCount++;
            }
        }

        debugFreeSlotCount =
            freeCount;
    }

    private void OnValidate()
    {
        desiredSlotCount =
            Mathf.Max(
                1,
                desiredSlotCount
            );

        slotRadius =
            Mathf.Max(
                0.1f,
                slotRadius
            );

        extraRadiusStartSlot =
            Mathf.Max(
                1,
                extraRadiusStartSlot
            );

        extraRadiusEndSlot =
            Mathf.Max(
                extraRadiusStartSlot,
                extraRadiusEndSlot
            );

        extraRadius =
            Mathf.Max(
                0f,
                extraRadius
            );

        slotNavMeshSampleDistance =
            Mathf.Max(
                0.1f,
                slotNavMeshSampleDistance
            );
    }
}