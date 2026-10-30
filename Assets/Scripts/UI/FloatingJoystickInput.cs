using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;

using InputTouch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using InputTouchPhase = UnityEngine.InputSystem.TouchPhase;

[DefaultExecutionOrder(-200)]
[DisallowMultipleComponent]
public sealed class FloatingJoystickInput : MonoBehaviour
{
    [Header("Joystick Visuals")]
    [SerializeField] private RectTransform joystickBase;
    [SerializeField] private RectTransform joystickHandle;
    [SerializeField] private Canvas joystickCanvas;

    [Header("Player")]
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Feel")]
    [SerializeField, Range(0f, 0.5f)]
    private float deadZone = 0.12f;

    private PlayerHealth playerHealth;
    private int activeTouchId = -1;
    private bool mouseIsHeld;
    private bool enhancedTouchEnabled;

    private Vector2 startLocalPosition;
    private readonly List<RaycastResult> raycastResults =
        new List<RaycastResult>();

    public Vector2 Direction { get; private set; }

    // The attack should resume only when the controlling finger is released.
    public bool IsHeld => activeTouchId >= 0 || mouseIsHeld;

    private void Awake()
    {
        ResolveReferences();
        ResetJoystick();
    }

    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
        enhancedTouchEnabled = true;

        ResolveReferences();
        ResetJoystick();
    }

    private void Start()
    {
        if (joystickBase == null ||
            joystickHandle == null ||
            joystickCanvas == null ||
            playerMovement == null)
        {
            Debug.LogError(
                "FloatingJoystickInput: Assign Base, Handle, " +
                "HUDCanvas and PlayerMovement.",
                this
            );

            enabled = false;
        }
    }

    private void Update()
    {
        if (!CanReceiveInput())
        {
            ResetJoystick();
            return;
        }

        // Real phone touches have priority over the Editor mouse fallback.
        var activeTouches = InputTouch.activeTouches;

        if (activeTouches.Count > 0)
        {
            UpdateTouchInput();
            return;
        }

        if (activeTouchId >= 0)
        {
            ResetJoystick();
            return;
        }

#if UNITY_EDITOR
        UpdateEditorMouseInput();
#endif
    }

    private void UpdateTouchInput()
    {
        var touches = InputTouch.activeTouches;

        if (activeTouchId < 0)
        {
            if (mouseIsHeld)
            {
                ResetJoystick();
            }

            // Accept only a newly started touch. Touches that started
            // over UI cannot turn into movement by dragging away.
            foreach (InputTouch touch in touches)
            {
                if (touch.phase != InputTouchPhase.Began)
                {
                    continue;
                }

                if (IsOverInteractiveUI(touch.screenPosition))
                {
                    continue;
                }

                BeginJoystick(touch.screenPosition, touch.touchId);
                break;
            }

            return;
        }

        foreach (InputTouch touch in touches)
        {
            if (touch.touchId != activeTouchId)
            {
                continue;
            }

            if (touch.phase == InputTouchPhase.Ended ||
                touch.phase == InputTouchPhase.Canceled)
            {
                ResetJoystick();
                return;
            }

            UpdateJoystickPosition(touch.screenPosition);
            return;
        }

        // The controlling touch disappeared, but another finger is
        // still touching the display. Never transfer control to it.
        ResetJoystick();
    }

#if UNITY_EDITOR
    private void UpdateEditorMouseInput()
    {
        Mouse mouse = Mouse.current;

        if (mouse == null)
        {
            if (mouseIsHeld)
            {
                ResetJoystick();
            }

            return;
        }

        if (!mouseIsHeld)
        {
            if (mouse.leftButton.wasPressedThisFrame &&
                !IsOverInteractiveUI(mouse.position.ReadValue()))
            {
                BeginJoystick(mouse.position.ReadValue(), -1);
                mouseIsHeld = true;
            }

            return;
        }

        if (!mouse.leftButton.isPressed)
        {
            ResetJoystick();
            return;
        }

        UpdateJoystickPosition(mouse.position.ReadValue());
    }
#endif

    private void BeginJoystick(Vector2 screenPosition, int touchId)
    {
        if (!TryGetCanvasLocalPosition(
                screenPosition,
                out Vector2 localPosition))
        {
            return;
        }

        activeTouchId = touchId;
        startLocalPosition = localPosition;
        Direction = Vector2.zero;

        joystickBase.anchoredPosition = startLocalPosition;
        joystickHandle.anchoredPosition = Vector2.zero;
        joystickBase.gameObject.SetActive(true);
    }

    private void UpdateJoystickPosition(Vector2 screenPosition)
    {
        if (!TryGetCanvasLocalPosition(
                screenPosition,
                out Vector2 currentLocalPosition))
        {
            return;
        }

        float availableRadius = Mathf.Max(
            1f,
            (joystickBase.rect.width - joystickHandle.rect.width) * 0.5f
        );

        Vector2 offset = Vector2.ClampMagnitude(
            currentLocalPosition - startLocalPosition,
            availableRadius
        );

        joystickHandle.anchoredPosition = offset;

        float inputAmount = offset.magnitude / availableRadius;

        // A small finger movement should not move the character.
        Direction = inputAmount > deadZone
            ? offset.normalized
            : Vector2.zero;
    }

    private bool TryGetCanvasLocalPosition(
        Vector2 screenPosition,
        out Vector2 localPosition)
    {
        localPosition = Vector2.zero;

        if (joystickCanvas == null)
        {
            return false;
        }

        RectTransform canvasRect =
            joystickCanvas.transform as RectTransform;

        if (canvasRect == null)
        {
            return false;
        }

        Camera uiCamera =
            joystickCanvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : joystickCanvas.worldCamera;

        return RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPosition,
            uiCamera,
            out localPosition
        );
    }

    private bool IsOverInteractiveUI(Vector2 screenPosition)
    {
        EventSystem eventSystem = EventSystem.current;

        if (eventSystem == null)
        {
            return false;
        }

        PointerEventData pointer = new PointerEventData(eventSystem)
        {
            position = screenPosition
        };

        raycastResults.Clear();
        eventSystem.RaycastAll(pointer, raycastResults);

        foreach (RaycastResult result in raycastResults)
        {
            if (result.gameObject == null)
            {
                continue;
            }

            // Decorative Images should not block free movement.
            if (result.gameObject.GetComponentInParent<Selectable>() != null ||
                ExecuteEvents.GetEventHandler<IPointerClickHandler>(
                    result.gameObject) != null ||
                ExecuteEvents.GetEventHandler<IPointerDownHandler>(
                    result.gameObject) != null ||
                ExecuteEvents.GetEventHandler<IDragHandler>(
                    result.gameObject) != null)
            {
                return true;
            }
        }

        return false;
    }

    private bool CanReceiveInput()
    {
        if (playerMovement == null ||
            !playerMovement.isActiveAndEnabled ||
            Time.timeScale <= 0f)
        {
            return false;
        }

        return playerHealth == null || !playerHealth.IsDead;
    }

    private void ResolveReferences()
    {
        if (joystickCanvas == null)
        {
            joystickCanvas = GetComponentInParent<Canvas>();
        }

        if (playerMovement == null)
        {
            playerMovement = FindAnyObjectByType<PlayerMovement>(
                FindObjectsInactive.Include
            );
        }

        if (playerMovement != null && playerHealth == null)
        {
            playerHealth = playerMovement.GetComponent<PlayerHealth>();
        }
    }

    private void ResetJoystick()
    {
        activeTouchId = -1;
        mouseIsHeld = false;
        Direction = Vector2.zero;

        if (joystickHandle != null)
        {
            joystickHandle.anchoredPosition = Vector2.zero;
        }

        if (joystickBase != null)
        {
            joystickBase.gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        ResetJoystick();

        if (enhancedTouchEnabled)
        {
            EnhancedTouchSupport.Disable();
            enhancedTouchEnabled = false;
        }
    }
}
