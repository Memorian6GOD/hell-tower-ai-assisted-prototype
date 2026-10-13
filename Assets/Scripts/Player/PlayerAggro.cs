using UnityEngine;

public class PlayerAggro : MonoBehaviour
{
    [Header("Master Balance Source")]

    [SerializeField]
    private GameBalanceConfig balanceConfig;


    public float AggroRadius
    {
        get
        {
            if (balanceConfig == null)
            {
                return 0f;
            }

            return balanceConfig.EnemyAggroRadius;
        }
    }


    public float DisengageRadius
    {
        get
        {
            if (balanceConfig == null)
            {
                return 0f;
            }

            return balanceConfig.EnemyDisengageRadius;
        }
    }


    public float AggroConfirmationTime
    {
        get
        {
            if (balanceConfig == null)
            {
                return 0f;
            }

            return balanceConfig.EnemyAggroConfirmationTime;
        }
    }


    private void Awake()
    {
        if (balanceConfig == null)
        {
            Debug.LogError(
                "PlayerAggro: GameBalanceConfig is not assigned.",
                this
            );
        }
    }


    private void OnDrawGizmosSelected()
    {
        if (balanceConfig == null)
        {
            return;
        }

        Gizmos.DrawWireSphere(
            transform.position,
            AggroRadius
        );

        Gizmos.DrawWireSphere(
            transform.position,
            DisengageRadius
        );
    }
}