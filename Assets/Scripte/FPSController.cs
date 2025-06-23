using UnityEngine;

public class FPSController : MonoBehaviour
{
    public CharacterController controller;
    public Transform playerCamera;

    public float walkSpeed = 4f;
    public float sprintSpeed = 7f;
    public float crouchSpeed = 2f;
    public float gravity = -9.81f;

    public float jumpHeight = 1.2f;
    public float crouchHeight = 1f;
    private float standingHeight;

    // Entferne oder ignoriere die alte public mouseSensitivity, 
    // wir nutzen künftig PauseMenuController.MouseSensitivity:
    // public float mouseSensitivity = 100f;

    private float xRotation = 0f;
    private Vector3 velocity;
    private bool isGrounded;

    // Wenn du Dialog-Zustände hast, setze FPSController.dialogue = true, um Mausbewegung zu sperren
    static public bool dialogue = false;

    void Start()
    {
        standingHeight = controller.height;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Mausbewegung und Bewegung nur, wenn nicht in Dialog oder nicht im Pause-Menü.
        // Time.timeScale wird beim Pause auf 0 gesetzt, sodass Time.deltaTime == 0 ist und Rotation automatisch ausbleibt.
        // Zusätzlich prüfen wir dialogue-Flag:
        if (!dialogue)
        {
            HandleMouseLook();
        }

        HandleMovement();
    }

    private void HandleMouseLook()
    {
        // Lies globale Empfindlichkeit:
        float sens = PauseMenuController.MouseSensitivity;
        // Multipliziere Input mit Time.deltaTime: Wenn pausiert (timeScale=0), ist Time.deltaTime 0 -> keine Rotation.
        float mouseX = Input.GetAxis("Mouse X") * sens * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sens * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);
    }

    private void HandleMovement()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        Vector3 move = transform.right * moveX + transform.forward * moveZ;

        float speed = walkSpeed;
        if (Input.GetKey(KeyCode.LeftShift)) speed = sprintSpeed;
        if (Input.GetKey(KeyCode.LeftControl)) speed = crouchSpeed;

        controller.Move(move * speed * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        if (Input.GetKey(KeyCode.LeftControl))
        {
            controller.height = crouchHeight;
        }
        else
        {
            controller.height = standingHeight;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
