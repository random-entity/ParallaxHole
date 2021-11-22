using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FreeMovement : MonoBehaviour
{
    [SerializeField] private float speed = 2f;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float lookSpeed = 2.0f;
    [SerializeField] private float lookXLimit = 90f;

    private CharacterController characterController;
    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);
        Vector3 up = transform.TransformDirection(Vector3.up);

        float curSpeedZ = speed * Input.GetAxis("Vertical");
        float curSpeedX = speed * Input.GetAxis("Horizontal");

        float curSpeedY = 0;
        if (Input.GetKey(KeyCode.Q)) curSpeedY += -1f;
        if (Input.GetKey(KeyCode.E)) curSpeedY += 1f;
        curSpeedY *= speed;

        moveDirection = (forward * curSpeedZ) + (right * curSpeedX) + (up * curSpeedY);

        // Move the controller
        characterController.Move(moveDirection * Time.deltaTime);

        // Player and Camera rotation
        rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
    }
}