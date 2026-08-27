using UnityEngine;

public class WorldSpaceBillboard : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;

    private Quaternion rotationOffset;

    private void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (targetCamera == null)
        {
            Debug.LogError("WorldSpaceBillboard: камера не найдена.");
            enabled = false;
            return;
        }

        rotationOffset =
            Quaternion.Inverse(targetCamera.transform.rotation) *
            transform.rotation;
    }

    private void LateUpdate()
    {
        transform.rotation =
            targetCamera.transform.rotation *
            rotationOffset;
    }
}