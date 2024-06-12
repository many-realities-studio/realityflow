using UnityEngine;
using UnityEngine.InputSystem;

public class RfPlayerController : MonoBehaviour
{
    public float speed = 5.0f;  // Movement speed
    public float turnSpeed = 100.0f;  // Turning speed

    private Vector2 moveInput;  // Stores the movement input
    private float turnInput;    // Stores the turning input

    private void OnEnable()
    {
        // Enable the move action
        var playerInput = GetComponent<PlayerInput>();
        var moveAction = playerInput.actions.FindAction("Move", true);
        moveAction.Enable();
        moveAction.performed += OnMove;
        moveAction.canceled += OnMove;

        // Enable the turn action
        var turnAction = playerInput.actions.FindAction("Turn", true);
        turnAction.Enable();
        turnAction.performed += OnTurn;
        turnAction.canceled += OnTurn;
    }

    private void OnDisable()
    {
        // Disable the move action
        var playerInput = GetComponent<PlayerInput>();
        var moveAction = playerInput.actions.FindAction("Move", true);
        moveAction.Disable();
        moveAction.performed -= OnMove;
        moveAction.canceled -= OnMove;

        // Disable the turn action
        var turnAction = playerInput.actions.FindAction("Turn", true);
        turnAction.Disable();
        turnAction.performed -= OnTurn;
        turnAction.canceled -= OnTurn;
    }

    // Update is called once per frame
    void Update()
    {
        // Move the player based on thumbstick input
        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y) * speed * Time.deltaTime;
        transform.Translate(move, Space.Self);

        // Turn the player based on thumbstick input
        float turn = turnInput * turnSpeed * Time.deltaTime;
        transform.Rotate(0, turn, 0);
    }

    // Input System callback for movement
    private void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    // Input System callback for turning
    private void OnTurn(InputAction.CallbackContext context)
    {
        turnInput = context.ReadValue<Vector2>().x;
    }
}