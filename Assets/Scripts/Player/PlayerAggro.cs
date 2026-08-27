using UnityEngine;

public class PlayerAggro : MonoBehaviour
{
    [Header("Aggro Settings")]
    [SerializeField] private float aggroRadius = 4f;
    [SerializeField] private float disengageRadius = 5f;

    public float AggroRadius => aggroRadius;
    public float DisengageRadius => disengageRadius;

    private void Awake()
    {
        if (disengageRadius < aggroRadius)
        {
            disengageRadius = aggroRadius;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(
            transform.position,
            aggroRadius
        );

        Gizmos.DrawWireSphere(
            transform.position,
            disengageRadius
        );
    }
}