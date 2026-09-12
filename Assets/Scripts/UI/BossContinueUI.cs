using UnityEngine;
using UnityEngine.UI;

public class BossContinueUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private BossFightManager bossFightManager;

    [SerializeField]
    private GameObject continuePanel;

    [SerializeField]
    private Button reviveButton;

    [SerializeField]
    private Button surrenderButton;

    [Header("Debug - Do Not Edit")]
    [SerializeField]
    private bool debugChoiceVisible;

    [SerializeField]
    private bool debugEventsSubscribed;

    private bool isChoiceVisible;
    private bool eventsSubscribed;
    private bool buttonsSubscribed;

    private float previousTimeScale = 1f;

    private void Awake()
    {
        FindMissingReferences();

        if (continuePanel != null)
        {
            continuePanel.SetActive(false);
        }
    }

    private void OnEnable()
    {
        SubscribeToEvents();
        SubscribeToButtons();
    }

    private void Start()
    {
        ValidateReferences();
    }

    private void LateUpdate()
    {
        debugChoiceVisible =
            isChoiceVisible;

        debugEventsSubscribed =
            eventsSubscribed;
    }

    private void HandleContinueRequested()
    {
        if (continuePanel == null)
        {
            Debug.LogWarning(
                "BossContinueUI: Continue Panel " +
                "is missing."
            );

            return;
        }

        if (isChoiceVisible)
        {
            return;
        }

        previousTimeScale =
            Time.timeScale;

        isChoiceVisible = true;
        continuePanel.SetActive(true);

        Time.timeScale = 0f;

        Debug.Log(
            "BossContinueUI: Continue choice shown. " +
            "The game is paused."
        );
    }

    private void HandleReviveButtonClicked()
    {
        if (!isChoiceVisible ||
            bossFightManager == null)
        {
            return;
        }

        if (!bossFightManager.IsContinueAvailable)
        {
            Debug.LogWarning(
                "BossContinueUI: Continue " +
                "is not available."
            );

            return;
        }

        HideChoiceAndRestoreTime();

        bossFightManager.UseContinue();

        Debug.Log(
            "BossContinueUI: Revive selected."
        );
    }

    private void HandleSurrenderButtonClicked()
    {
        if (!isChoiceVisible ||
            bossFightManager == null)
        {
            return;
        }

        if (!bossFightManager.IsContinueAvailable)
        {
            Debug.LogWarning(
                "BossContinueUI: Surrender " +
                "is not available."
            );

            return;
        }

        HideChoiceAndRestoreTime();

        bossFightManager.DeclineContinue();

        Debug.Log(
            "BossContinueUI: Surrender selected."
        );
    }

    private void HandleFightFinished()
    {
        if (!isChoiceVisible)
        {
            return;
        }

        HideChoiceAndRestoreTime();
    }

    private void HideChoiceAndRestoreTime()
    {
        isChoiceVisible = false;

        if (continuePanel != null)
        {
            continuePanel.SetActive(false);
        }

        Time.timeScale =
            previousTimeScale;
    }

    private void FindMissingReferences()
    {
        if (bossFightManager == null)
        {
            bossFightManager =
                GetComponent<BossFightManager>();
        }

        if (bossFightManager == null)
        {
            bossFightManager =
                FindAnyObjectByType<
                    BossFightManager
                >();
        }
    }

    private void SubscribeToEvents()
    {
        if (eventsSubscribed ||
            bossFightManager == null)
        {
            return;
        }

        bossFightManager.ContinueRequested +=
            HandleContinueRequested;

        bossFightManager.FightWon +=
            HandleFightFinished;

        bossFightManager.FightLost +=
            HandleFightFinished;

        eventsSubscribed = true;
    }

    private void UnsubscribeFromEvents()
    {
        if (!eventsSubscribed ||
            bossFightManager == null)
        {
            return;
        }

        bossFightManager.ContinueRequested -=
            HandleContinueRequested;

        bossFightManager.FightWon -=
            HandleFightFinished;

        bossFightManager.FightLost -=
            HandleFightFinished;

        eventsSubscribed = false;
    }

    private void SubscribeToButtons()
    {
        if (buttonsSubscribed)
        {
            return;
        }

        if (reviveButton != null)
        {
            reviveButton.onClick.AddListener(
                HandleReviveButtonClicked
            );
        }

        if (surrenderButton != null)
        {
            surrenderButton.onClick.AddListener(
                HandleSurrenderButtonClicked
            );
        }

        buttonsSubscribed = true;
    }

    private void UnsubscribeFromButtons()
    {
        if (!buttonsSubscribed)
        {
            return;
        }

        if (reviveButton != null)
        {
            reviveButton.onClick.RemoveListener(
                HandleReviveButtonClicked
            );
        }

        if (surrenderButton != null)
        {
            surrenderButton.onClick.RemoveListener(
                HandleSurrenderButtonClicked
            );
        }

        buttonsSubscribed = false;
    }

    private void ValidateReferences()
    {
        if (bossFightManager == null)
        {
            Debug.LogWarning(
                "BossContinueUI: BossFightManager " +
                "is missing."
            );
        }

        if (continuePanel == null)
        {
            Debug.LogWarning(
                "BossContinueUI: Continue Panel " +
                "is missing."
            );
        }

        if (reviveButton == null)
        {
            Debug.LogWarning(
                "BossContinueUI: Revive Button " +
                "is missing."
            );
        }

        if (surrenderButton == null)
        {
            Debug.LogWarning(
                "BossContinueUI: Surrender Button " +
                "is missing."
            );
        }
    }

    private void OnDisable()
    {
        UnsubscribeFromEvents();
        UnsubscribeFromButtons();

        if (isChoiceVisible)
        {
            HideChoiceAndRestoreTime();
        }
    }
}