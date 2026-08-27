using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Collision Settings")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float enemyCheckDistance = 0.05f;

    private CharacterController characterController;
    private PlayerHealth playerHealth;

    private float verticalVelocity;

    public bool IsMoving { get; private set; }

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        playerHealth = GetComponent<PlayerHealth>();
    }

    void Update()
    {
        if (playerHealth != null && playerHealth.IsDead)
        {
            IsMoving = false;
            return;
        }

        if (Keyboard.current == null)
        {
            return;
        }

        Vector3 moveDirection = Vector3.zero;

        if (Keyboard.current.wKey.isPressed)
            moveDirection.z += 1f;

        if (Keyboard.current.sKey.isPressed)
            moveDirection.z -= 1f;

        if (Keyboard.current.aKey.isPressed)
            moveDirection.x -= 1f;

        if (Keyboard.current.dKey.isPressed)
            moveDirection.x += 1f;

        moveDirection = moveDirection.normalized;

        bool wantsToMove =
            moveDirection.sqrMagnitude > 0.01f;

        if (wantsToMove &&
            IsEnemyBlockingMovement(moveDirection))
        {
            moveDirection = Vector3.zero;
        }

        IsMoving =
            moveDirection.sqrMagnitude > 0.01f;

        if (characterController.isGrounded &&
            verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        verticalVelocity +=
            gravity * Time.deltaTime;

        Vector3 finalMovement =
            moveDirection * moveSpeed
            + Vector3.up * verticalVelocity;

        characterController.Move(
            finalMovement * Time.deltaTime
        );
    }

    private bool IsEnemyBlockingMovement(
        Vector3 moveDirection
    )
    {
        if (characterController == null)
        {
            return false;
        }

        float radius =
            characterController.radius;

        float height =
            Mathf.Max(
                characterController.height,
                radius * 2f
            );

        Vector3 worldCenter =
            transform.TransformPoint(
                characterController.center
            );

        float halfCapsule =
            Mathf.Max(
                0f,
                height * 0.5f - radius
            );

        Vector3 point1 =
            worldCenter +
            Vector3.up * halfCapsule;

        Vector3 point2 =
            worldCenter -
            Vector3.up * halfCapsule;

        float movementDistance =
            moveSpeed * Time.deltaTime
            + enemyCheckDistance;

        return Physics.CapsuleCast(
            point1,
            point2,
            radius,
            moveDirection,
            movementDistance,
            enemyLayer,
            QueryTriggerInteraction.Collide
        );
    }
}