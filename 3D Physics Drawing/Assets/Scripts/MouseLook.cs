using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public float mouseSensitivity = 100.0f;

    public Transform playerBody;

    float xRotation = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90.0f, 90.0f);

        transform.localRotation = Quaternion.Euler(xRotation, 0.0f, 0.0f);
        playerBody.Rotate(Vector3.up * mouseX);
    }

    //void OnDrawGizmos()
    //{
    //    // Draw a yellow sphere in front of the object
    //    Gizmos.color = Color.yellow;
    //    Gizmos.DrawSphere(transform.position + (transform.TransformDirection(Vector3.forward) * 2.0f), 0.25f);

    //    // Draws a 1.75 unit long red line in front of the object
    //    Vector3 direction = transform.TransformDirection(Vector3.forward) * 1.75f;
    //    Gizmos.DrawRay(transform.position, direction);
    //}
}
