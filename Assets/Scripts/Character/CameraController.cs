using Environment;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float sensitivity = 5.0f;
    [SerializeField] private float height = 2.0f;

    private Transform m_cameraTransform;
    private Vector2 m_mouseLook;
    private Vector2 m_input;


    private void Awake()
    {
        m_cameraTransform = transform;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = true;
    }

    private void Update()
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

    private void LateUpdate()
    {
        m_input = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
        m_input *= sensitivity;

        m_mouseLook += m_input;
        m_mouseLook.y = Mathf.Clamp(m_mouseLook.y, -90f, 90f);

        Quaternion targetRotation = Quaternion.AngleAxis(m_mouseLook.x, target.transform.up);
        Quaternion cameraRotation = targetRotation * Quaternion.AngleAxis(-m_mouseLook.y, Vector3.right);

        target.rotation = targetRotation;
        m_cameraTransform.position = target.transform.position + Vector3.up * height;
        m_cameraTransform.rotation = cameraRotation;
    }
}