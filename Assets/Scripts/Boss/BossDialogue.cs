using System.Collections;
using TMPro;
using UnityEngine;

public class BossDialogue : MonoBehaviour
{
    [Header("Dialogue References")]
    [SerializeField]
    private GameObject dialogueObject;

    [SerializeField]
    private TMP_Text dialogueText;

    [Header("Dialogue Settings")]
    [SerializeField]
    [TextArea]
    private string introLine =
        "У тебя всего одна жизнь";

    [SerializeField]
    [TextArea]
    private string reviveLine =
        "Ах ты жулик!";

    [SerializeField]
    private float dialogueDuration = 3f;

    [Header("Debug - Do Not Edit")]
    [SerializeField]
    private bool debugDialogueVisible;

    [SerializeField]
    private bool debugIntroLineShown;

    [SerializeField]
    private bool debugReviveLineShown;

    private Camera mainCamera;
    private Coroutine hideCoroutine;

    private bool introLineShown;
    private bool reviveLineShown;

    private void Awake()
    {
        mainCamera = Camera.main;

        HideDialogueImmediately();
    }

    private void Start()
    {
        ShowIntroLine();
    }

    private void LateUpdate()
    {
        RotateTowardsCamera();

        debugDialogueVisible =
            dialogueObject != null &&
            dialogueObject.activeSelf;

        debugIntroLineShown =
            introLineShown;

        debugReviveLineShown =
            reviveLineShown;
    }

    public void ShowIntroLine()
    {
        if (introLineShown)
        {
            return;
        }

        introLineShown = true;

        ShowLine(
            introLine
        );
    }

    public void ShowReviveLine()
    {
        if (reviveLineShown)
        {
            return;
        }

        reviveLineShown = true;

        ShowLine(
            reviveLine
        );
    }

    private void ShowLine(
        string line
    )
    {
        if (dialogueObject == null ||
            dialogueText == null)
        {
            Debug.LogWarning(
                "BossDialogue: Dialogue references " +
                "are missing."
            );

            return;
        }

        if (hideCoroutine != null)
        {
            StopCoroutine(
                hideCoroutine
            );
        }

        dialogueText.text = line;
        dialogueObject.SetActive(true);

        hideCoroutine =
            StartCoroutine(
                HideDialogueAfterDelay()
            );
    }

    private IEnumerator HideDialogueAfterDelay()
    {
        yield return new WaitForSeconds(
            dialogueDuration
        );

        if (dialogueObject != null)
        {
            dialogueObject.SetActive(false);
        }

        hideCoroutine = null;
    }

    private void RotateTowardsCamera()
    {
        if (dialogueObject == null)
        {
            return;
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera == null)
        {
            return;
        }

        dialogueObject.transform.rotation =
            mainCamera.transform.rotation;
    }

    private void HideDialogueImmediately()
    {
        if (dialogueObject != null)
        {
            dialogueObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        if (hideCoroutine != null)
        {
            StopCoroutine(
                hideCoroutine
            );

            hideCoroutine = null;
        }

        HideDialogueImmediately();
    }
}