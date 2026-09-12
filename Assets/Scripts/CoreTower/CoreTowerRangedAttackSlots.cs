using UnityEngine;

public class CoreTowerRangedAttackSlots : MonoBehaviour
{
    [Header("Ranged Attack Slots")]
    [SerializeField] private Transform[] attackSlots;

    [Header("Editor Layout Settings")]
    [SerializeField] private Transform slotsRoot;
    [SerializeField, Min(1)] private int desiredSlotCount = 26;
    [SerializeField, Min(0.1f)] private float slotRadius = 7f;
    [SerializeField, Range(10f, 360f)] private float arcAngle = 220f;
    [SerializeField, Range(-180f, 180f)] private float arcCenterAngle = 180f;
    [SerializeField] private float slotHeight = 0f;

    private GameObject[] occupants;

    private void Awake()
    {
        int slotCount = 0;

        if (attackSlots != null)
        {
            slotCount = attackSlots.Length;
        }

        occupants = new GameObject[slotCount];
    }

    [ContextMenu("Create And Arrange Ranged Slots")]
    private void CreateAndArrangeRangedSlots()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning(
                "CoreTowerRangedAttackSlots: Slots can only be created " +
                "outside Play Mode."
            );

            return;
        }

        Transform root =
            slotsRoot != null
                ? slotsRoot
                : transform;

        Transform[] previousSlots =
            attackSlots ?? new Transform[0];

        Transform[] newSlots =
            new Transform[desiredSlotCount];

        for (int i = 0; i < desiredSlotCount; i++)
        {
            if (i < previousSlots.Length &&
                previousSlots[i] != null)
            {
                newSlots[i] = previousSlots[i];
                continue;
            }

            GameObject slotObject =
                new GameObject(
                    $"RangedAttackSlot_{i + 1:00}"
                );

            slotObject.transform.SetParent(
                root,
                false
            );

            newSlots[i] = slotObject.transform;
        }

        attackSlots = newSlots;

        ArrangeRangedSlots();

        Debug.Log(
            $"CoreTowerRangedAttackSlots: {attackSlots.Length} " +
            "ranged slots created and arranged."
        );
    }

    [ContextMenu("Arrange Ranged Slots")]
    private void ArrangeRangedSlots()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning(
                "CoreTowerRangedAttackSlots: Slots can only be arranged " +
                "outside Play Mode."
            );

            return;
        }

        if (attackSlots == null ||
            attackSlots.Length == 0)
        {
            Debug.LogWarning(
                "CoreTowerRangedAttackSlots: There are no ranged slots " +
                "to arrange."
            );

            return;
        }

        Transform root =
            slotsRoot != null
                ? slotsRoot
                : transform;

        for (int i = 0; i < attackSlots.Length; i++)
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

            Vector3 localPosition =
                new Vector3(
                    Mathf.Sin(angleInRadians) * slotRadius,
                    slotHeight,
                    Mathf.Cos(angleInRadians) * slotRadius
                );

            attackSlots[i].position =
                root.TransformPoint(localPosition);
        }
    }

    public Transform ClaimClosestSlot(GameObject enemy)
    {
        if (enemy == null ||
            attackSlots == null ||
            occupants == null ||
            attackSlots.Length == 0)
        {
            return null;
        }

        for (int i = 0; i < occupants.Length; i++)
        {
            if (occupants[i] == enemy)
            {
                return attackSlots[i];
            }
        }

        int closestSlotIndex = -1;
        float closestDistance = float.MaxValue;

        for (int i = 0; i < attackSlots.Length; i++)
        {
            if (attackSlots[i] == null)
            {
                continue;
            }

            if (occupants[i] != null)
            {
                continue;
            }

            Vector3 direction =
                attackSlots[i].position -
                enemy.transform.position;

            direction.y = 0f;

            float distance =
                direction.sqrMagnitude;

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestSlotIndex = i;
            }
        }

        if (closestSlotIndex == -1)
        {
            return null;
        }

        occupants[closestSlotIndex] = enemy;

        return attackSlots[closestSlotIndex];
    }

    public void ReleaseSlot(GameObject enemy)
    {
        if (enemy == null ||
            occupants == null)
        {
            return;
        }

        for (int i = 0; i < occupants.Length; i++)
        {
            if (occupants[i] == enemy)
            {
                occupants[i] = null;

                return;
            }
        }
    }
}