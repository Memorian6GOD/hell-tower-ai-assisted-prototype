using UnityEngine;

public class CoreTowerAttackSlots : MonoBehaviour
{
    [Header("Melee Attack Slots")]
    [SerializeField] private Transform[] attackSlots;

    [Header("Editor Layout Settings")]

    [Tooltip("ќбъект, относительно которого располагаютс€ слоты.")]
    [SerializeField] private Transform slotsRoot;

    [Tooltip("Ќеобходимое количество ближних слотов.")]
    [SerializeField, Min(1)] private int desiredSlotCount = 14;

    [Tooltip("ќсновное рассто€ние всех ближних слотов от башни.")]
    [SerializeField, Min(0.1f)] private float slotRadius = 4f;

    [Tooltip("ќбщий угол дуги со слотами.")]
    [SerializeField, Range(10f, 360f)] private float arcAngle = 180f;

    [Tooltip("Ќаправление центра дуги относительно поворота Slots Root.")]
    [SerializeField, Range(-180f, 180f)] private float arcCenterAngle = 180f;

    [Tooltip("¬ысота слотов относительно Slots Root.")]
    [SerializeField] private float slotHeight = 0f;

    [Header("Additional Radius Settings")]

    [Tooltip("ѕервый слот, который нужно дополнительно отодвинуть.")]
    [SerializeField, Min(1)] private int extraRadiusStartSlot = 5;

    [Tooltip("ѕоследний слот, который нужно дополнительно отодвинуть.")]
    [SerializeField, Min(1)] private int extraRadiusEndSlot = 12;

    [Tooltip("Ќа сколько дополнительно отодвигаютс€ выбранные слоты.")]
    [SerializeField, Min(0f)] private float extraRadius = 0.6f;

    private EnemyMovement[] occupants;

    private void Awake()
    {
        int slotCount = 0;

        if (attackSlots != null)
        {
            slotCount = attackSlots.Length;
        }

        occupants = new EnemyMovement[slotCount];
    }

    [ContextMenu("Create And Arrange Melee Slots")]
    private void CreateAndArrangeMeleeSlots()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning(
                "CoreTowerAttackSlots: Slots can only be created " +
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
                    $"MeleeAttackSlot_{i + 1:00}"
                );

            slotObject.transform.SetParent(
                root,
                false
            );

            newSlots[i] = slotObject.transform;
        }

        attackSlots = newSlots;

        ArrangeMeleeSlots();

        Debug.Log(
            $"CoreTowerAttackSlots: {attackSlots.Length} " +
            "melee slots created and arranged."
        );
    }

    [ContextMenu("Arrange Melee Slots")]
    private void ArrangeMeleeSlots()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning(
                "CoreTowerAttackSlots: Slots can only be arranged " +
                "outside Play Mode."
            );

            return;
        }

        if (attackSlots == null ||
            attackSlots.Length == 0)
        {
            Debug.LogWarning(
                "CoreTowerAttackSlots: There are no melee slots to arrange."
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

            // ¬ массиве нумераци€ начинаетс€ с нул€,
            // а в Inspector и названи€х слотов Ч с единицы.
            int slotNumber = i + 1;

            float currentRadius = slotRadius;

            // “олько выбранный диапазон слотов
            // получает дополнительное рассто€ние от башни.
            if (slotNumber >= extraRadiusStartSlot &&
                slotNumber <= extraRadiusEndSlot)
            {
                currentRadius += extraRadius;
            }

            Vector3 localPosition =
                new Vector3(
                    Mathf.Sin(angleInRadians) * currentRadius,
                    slotHeight,
                    Mathf.Cos(angleInRadians) * currentRadius
                );

            // TransformPoint переводит локальную позицию
            // в мировую с учЄтом положени€ и поворота Slots Root.
            attackSlots[i].position =
                root.TransformPoint(localPosition);
        }
    }

    public Transform ClaimClosestSlot(EnemyMovement enemy)
    {
        if (enemy == null ||
            attackSlots == null ||
            occupants == null ||
            attackSlots.Length == 0)
        {
            return null;
        }

        // ≈сли этот враг уже занимал слот,
        // возвращаем ему тот же самый слот.
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

    public void ReleaseSlot(EnemyMovement enemy)
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