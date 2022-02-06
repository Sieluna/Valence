using Environment;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] float sensitivity = 5.0f;
    [SerializeField] float height = 2.0f;

    Transform cameraTransform;
    Vector2 mouseLook;
    Vector2 input;
    

    void Awake()
    {
        cameraTransform = transform;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = true;
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, 1 << LayerMask.NameToLayer("Block")))
            {
                World.Instance.SetBlock(hit.point - ray.direction * 0.01f, BlockType.Glowstone);
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, 1 << LayerMask.NameToLayer("Block")))
            {
                World.Instance.SetBlock(hit.point + ray.direction * 0.01f, BlockType.Air);
            }
        }
    }

    void LateUpdate()
    {
        input = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
        input *= sensitivity;

        mouseLook += input;
        mouseLook.y = Mathf.Clamp(mouseLook.y, -90f, 90f);

        Quaternion targetRotation = Quaternion.AngleAxis(mouseLook.x, target.transform.up);
        Quaternion cameraRotation = targetRotation * Quaternion.AngleAxis(-mouseLook.y, Vector3.right);

        target.rotation = targetRotation;
        cameraTransform.position = target.transform.position + Vector3.up * height;
        cameraTransform.rotation = cameraRotation;
    }
}