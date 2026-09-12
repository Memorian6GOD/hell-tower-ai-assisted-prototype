using System;
using UnityEngine;

public class BossFightManager : MonoBehaviour
{
    public enum BossFightState
    {
        NotStarted,
        Active,
        WaitingForContinue,
        Victory,
        Defeat
    }

    [Header("Fight References")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerAttack playerAttack;
    [SerializeField] private CoreTowerHealth coreTowerHealth;
    [SerializeField] private EnemyHealth bossHealth;

    [SerializeField]
    private BossAttackController bossAttackController;

    [SerializeField]
    private BossDialogue bossDialogue;

    [SerializeField] private GameManager gameManager;

    [Header("Fight Start Settings")]
    [SerializeField] private bool startFightOnStart = true;

    [Header("Continue Settings")]
    [SerializeField]
    [Range(1, 100)]
    private int continueHealthPercent = 100;

    [Header("Post Continue Delay Settings")]
    [SerializeField]
    private float playerAttackDelay = 3f;

    [SerializeField]
    private float bossMovementDelay = 2f;

    [SerializeField]
    private float bossAttackDelay = 3f;

    [Header("Debug - Do Not Edit")]
    [SerializeField]
    private BossFightState debugFightState;

    [SerializeField] private bool debugContinueUsed;
    [SerializeField] private bool debugContinueAvailable;

    private bool hasUsedContinue;
    private bool sharedEventsSubscribed;
    private bool bossEventSubscribed;

    public event Action ContinueRequested;
    public event Action FightWon;
    public event Action FightLost;

    public BossFightState FightState
    {
        get;
        private set;
    } = BossFightState.NotStarted;

    public bool HasUsedContinue =>
        hasUsedContinue;

    public bool IsContinueAvailable =>
        FightState ==
        BossFightState.WaitingForContinue &&
        !hasUsedContinue;

    private void Awake()
    {
        FindMissingReferences();
    }

    private void OnEnable()
    {
        SubscribeToSharedEvents();
        SubscribeToBossEvent();
    }

    private void Start()
    {
        ValidateSharedReferences();

        if (startFightOnStart)
        {
            ValidateBossReferences();
            StartBossFight();
        }
    }

    private void LateUpdate()
    {
        UpdateDebugInfo();
    }

    [ContextMenu("Start Boss Fight")]
    public void StartBossFight()
    {
        TryStartBossFight();
    }

    public bool StartBossFight(
        EnemyHealth spawnedBossHealth
    )
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning(
                "BossFightManager: Fight can only " +
                "start in Play Mode."
            );

            return false;
        }

        if (spawnedBossHealth == null)
        {
            Debug.LogWarning(
                "BossFightManager: Spawned Boss Health " +
                "is missing."
            );

            return false;
        }

        if (FightState !=
            BossFightState.NotStarted)
        {
            Debug.LogWarning(
                "BossFightManager: Boss fight " +
                "has already started."
            );

            return false;
        }

        AssignBoss(
            spawnedBossHealth
        );

        return TryStartBossFight();
    }

    [ContextMenu("Use Test Continue")]
    public void UseContinue()
    {
        if (!IsContinueAvailable)
        {
            Debug.LogWarning(
                "BossFightManager: Continue " +
                "is not available."
            );

            return;
        }

        if (playerHealth == null)
        {
            Debug.LogWarning(
                "BossFightManager: PlayerHealth " +
                "is missing."
            );

            return;
        }

        bool reviveSucceeded =
            playerHealth.TryRevive(
                continueHealthPercent
            );

        if (!reviveSucceeded)
        {
            Debug.LogWarning(
                "BossFightManager: Player " +
                "could not be revived."
            );

            return;
        }

        if (playerAttack != null)
        {
            playerAttack.StartAttackDelay(
                playerAttackDelay
            );
        }
        else
        {
            Debug.LogWarning(
                "BossFightManager: PlayerAttack " +
                "is missing."
            );
        }

        if (bossAttackController != null)
        {
            bossAttackController
                .StartPostContinueDelays(
                    bossMovementDelay,
                    bossAttackDelay
                );
        }
        else
        {
            Debug.LogWarning(
                "BossFightManager: " +
                "BossAttackController is missing."
            );
        }

        if (bossDialogue != null)
        {
            bossDialogue.ShowReviveLine();
        }
        else
        {
            Debug.LogWarning(
                "BossFightManager: " +
                "BossDialogue is missing."
            );
        }

        hasUsedContinue = true;
        FightState =
            BossFightState.Active;

        Debug.Log(
            "BossFightManager: Continue used. " +
            "Temporary combat delays started."
        );
    }

    [ContextMenu("Decline Continue")]
    public void DeclineContinue()
    {
        if (!IsContinueAvailable)
        {
            Debug.LogWarning(
                "BossFightManager: There is no " +
                "continue offer to decline."
            );

            return;
        }

        FinishWithDefeat(
            "Player declined the continue."
        );
    }

    private bool TryStartBossFight()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning(
                "BossFightManager: Fight can only " +
                "start in Play Mode."
            );

            return false;
        }

        if (FightState !=
            BossFightState.NotStarted)
        {
            Debug.LogWarning(
                "BossFightManager: Boss fight " +
                "has already started."
            );

            return false;
        }

        if (playerHealth == null ||
            coreTowerHealth == null ||
            bossHealth == null ||
            gameManager == null)
        {
            Debug.LogWarning(
                "BossFightManager: Fight cannot start " +
                "because required references are missing."
            );

            return false;
        }

        if (playerHealth.IsDead ||
            coreTowerHealth.IsDestroyed ||
            bossHealth.IsDead ||
            gameManager.IsGameOver)
        {
            Debug.LogWarning(
                "BossFightManager: Fight cannot start " +
                "because one of its participants " +
                "is unavailable."
            );

            return false;
        }

        hasUsedContinue = false;
        FightState =
            BossFightState.Active;

        Debug.Log(
            "BossFightManager: Boss fight started."
        );

        return true;
    }

    private void AssignBoss(
        EnemyHealth spawnedBossHealth
    )
    {
        UnsubscribeFromBossEvent();

        bossHealth =
            spawnedBossHealth;

        bossAttackController =
            bossHealth.GetComponent<
                BossAttackController
            >();

        bossDialogue =
            bossHealth.GetComponent<
                BossDialogue
            >();

        SubscribeToBossEvent();
        ValidateBossReferences();

        Debug.Log(
            "BossFightManager: Spawned boss assigned."
        );
    }

    private void HandlePlayerDied(
        PlayerHealth deadPlayer
    )
    {
        if (FightState !=
            BossFightState.Active)
        {
            return;
        }

        if (hasUsedContinue)
        {
            FinishWithDefeat(
                "Player died after using the continue."
            );

            return;
        }

        FightState =
            BossFightState.WaitingForContinue;

        Debug.Log(
            "BossFightManager: Player died. " +
            "One continue is available."
        );

        ContinueRequested?.Invoke();
    }

    private void HandleCoreTowerDestroyed(
        CoreTowerHealth destroyedTower
    )
    {
        if (FightState !=
                BossFightState.Active &&
            FightState !=
                BossFightState.WaitingForContinue)
        {
            return;
        }

        FinishWithDefeat(
            "CoreTower was destroyed."
        );
    }

    private void HandleBossDied(
        EnemyHealth deadBoss
    )
    {
        if (FightState !=
                BossFightState.Active &&
            FightState !=
                BossFightState.WaitingForContinue)
        {
            return;
        }

        FightState =
            BossFightState.Victory;

        Debug.Log(
            "BossFightManager: Boss defeated. " +
            "Boss fight victory."
        );

        FightWon?.Invoke();
    }

    private void FinishWithDefeat(
        string reason
    )
    {
        if (FightState ==
                BossFightState.Victory ||
            FightState ==
                BossFightState.Defeat)
        {
            return;
        }

        FightState =
            BossFightState.Defeat;

        Debug.Log(
            "BossFightManager: Defeat. " +
            reason
        );

        FightLost?.Invoke();

        if (gameManager != null)
        {
            gameManager.GameOver();
        }
    }

    private void FindMissingReferences()
    {
        if (playerHealth == null)
        {
            playerHealth =
                FindAnyObjectByType<PlayerHealth>();
        }

        if (playerAttack == null)
        {
            playerAttack =
                FindAnyObjectByType<PlayerAttack>();
        }

        if (coreTowerHealth == null)
        {
            coreTowerHealth =
                FindAnyObjectByType<CoreTowerHealth>();
        }

        if (gameManager == null)
        {
            gameManager =
                FindAnyObjectByType<GameManager>();
        }

        if (bossAttackController == null &&
            bossHealth != null)
        {
            bossAttackController =
                bossHealth.GetComponent<
                    BossAttackController
                >();
        }

        if (bossDialogue == null &&
            bossHealth != null)
        {
            bossDialogue =
                bossHealth.GetComponent<
                    BossDialogue
                >();
        }
    }

    private void SubscribeToSharedEvents()
    {
        if (sharedEventsSubscribed)
        {
            return;
        }

        if (playerHealth != null)
        {
            playerHealth.Died +=
                HandlePlayerDied;
        }

        if (coreTowerHealth != null)
        {
            coreTowerHealth.Destroyed +=
                HandleCoreTowerDestroyed;
        }

        sharedEventsSubscribed = true;
    }

    private void UnsubscribeFromSharedEvents()
    {
        if (!sharedEventsSubscribed)
        {
            return;
        }

        if (playerHealth != null)
        {
            playerHealth.Died -=
                HandlePlayerDied;
        }

        if (coreTowerHealth != null)
        {
            coreTowerHealth.Destroyed -=
                HandleCoreTowerDestroyed;
        }

        sharedEventsSubscribed = false;
    }

    private void SubscribeToBossEvent()
    {
        if (bossEventSubscribed ||
            bossHealth == null)
        {
            return;
        }

        bossHealth.Died +=
            HandleBossDied;

        bossEventSubscribed = true;
    }

    private void UnsubscribeFromBossEvent()
    {
        if (!bossEventSubscribed)
        {
            return;
        }

        if (bossHealth != null)
        {
            bossHealth.Died -=
                HandleBossDied;
        }

        bossEventSubscribed = false;
    }

    private void ValidateSharedReferences()
    {
        if (playerHealth == null)
        {
            Debug.LogWarning(
                "BossFightManager: PlayerHealth " +
                "was not found."
            );
        }

        if (playerAttack == null)
        {
            Debug.LogWarning(
                "BossFightManager: PlayerAttack " +
                "was not found."
            );
        }

        if (coreTowerHealth == null)
        {
            Debug.LogWarning(
                "BossFightManager: CoreTowerHealth " +
                "was not found."
            );
        }

        if (gameManager == null)
        {
            Debug.LogWarning(
                "BossFightManager: GameManager " +
                "was not found."
            );
        }
    }

    private void ValidateBossReferences()
    {
        if (bossHealth == null)
        {
            Debug.LogWarning(
                "BossFightManager: Boss Health " +
                "is not assigned."
            );
        }

        if (bossAttackController == null)
        {
            Debug.LogWarning(
                "BossFightManager: " +
                "BossAttackController is not assigned."
            );
        }

        if (bossDialogue == null)
        {
            Debug.LogWarning(
                "BossFightManager: " +
                "BossDialogue is not assigned."
            );
        }
    }

    private void UpdateDebugInfo()
    {
        debugFightState =
            FightState;

        debugContinueUsed =
            hasUsedContinue;

        debugContinueAvailable =
            IsContinueAvailable;
    }

    private void OnDisable()
    {
        UnsubscribeFromSharedEvents();
        UnsubscribeFromBossEvent();
    }
}