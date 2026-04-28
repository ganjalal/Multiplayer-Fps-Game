using UnityEngine;
using Mirror;

public class FPSController : NetworkBehaviour
{   
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -13f;

    [Header("MouseLook")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private Transform playerBody;
    [SerializeField] private Camera camera;

    [Header("GroundCheck")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.4f;
    [SerializeField] private LayerMask groundMask = 1;

    private CharacterController controller;
    private Vector3 velocity;
    private float xRotation = 0f;
    private bool isGrounded;

    private bool canMove = true;

    public override void OnStartLocalPlayer()
    {
        controller = GetComponent<CharacterController>();
        camera.gameObject.SetActive(true);
        // Cursor.lockState = CursorLockMode.Locked;
        // Cursor.visible = false;

    }
    public void SetCanMove(bool move)
    {
        canMove = move;
        if(!move) velocity = Vector3.zero;
    }
    void Update()
    {   
        if(!isLocalPlayer || !canMove || GameManager.Instance.gameEnded) return;
        HandleMovement();
        HandleMouseLook();
    }

    void HandleMovement()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position,groundDistance,groundMask);
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * moveSpeed * Time.deltaTime);

        //Handle Jump
        if(Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        //Gravity
        if(isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        else
        {
            velocity.y += gravity * Time.deltaTime; 
        }

        controller.Move(velocity * Time.deltaTime);

    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation,-90f,90f);

        camera.transform.localRotation = Quaternion.Euler(xRotation,0f,0f);

        playerBody.Rotate(Vector3.up * mouseX);
    }

    void OnDrawGizmosSelected()
    {   
        if(groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
        }

        Gizmos.DrawWireSphere(groundCheck.position,groundDistance);

    }
}
