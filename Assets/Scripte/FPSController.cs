using UnityEngine;
using System.Collections;

public class FPSController : MonoBehaviour
{
    public CharacterController controller;
    public Transform playerCamera;

    [Header("Movement Settings")]
    public float walkSpeed = 4f;
    public float sprintSpeed = 7f;
    public float crouchSpeed = 2f;
    public float gravity = -9.81f;

    [Header("Jump & Crouch")]
    public float jumpHeight = 1.2f;
    public float crouchHeight = 1f;
    private float standingHeight;

    [Header("Mouse Look")]
    public float mouseSensitivity = 100f;
    private float xRotation = 0f;

    private Vector3 velocity;
    private bool isGrounded;

    void Start()
    {
        standingHeight = controller.height;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // Mouse Look
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);

        // Movement input
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        Vector3 move = transform.right * moveX + transform.forward * moveZ;

        float speed = walkSpeed;
        if (Input.GetKey(KeyCode.LeftShift)) speed = sprintSpeed;
        if (Input.GetKey(KeyCode.LeftControl)) speed = crouchSpeed;

        controller.Move(move * speed * Time.deltaTime);

        // Jump
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Crouch
        if (Input.GetKey(KeyCode.LeftControl))
        {
            controller.height = crouchHeight;
        }
        else
        {
            controller.height = standingHeight;
        }

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
