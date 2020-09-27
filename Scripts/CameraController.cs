using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Look sensitivity")]
    public float sensX;
    public float sensY;

    [Header("Clamping")]
    public float minY;
    public float maxY;

    [Header("Spectator")]
    public float spectatorMoveSpeed;

    private float rotX;
    private float rotY;

    private bool isSpectator;

    private void Start()
    {
        //lock the cursor to the center
        Cursor.lockState = CursorLockMode.Locked;
    }

    //check and apply input to rotation here to prevent jittering
    private void LateUpdate()
    {
        //get mouse inputs
        rotX += Input.GetAxis("Mouse X") * sensX;
        rotY += Input.GetAxis("Mouse Y") * sensY;

        //clamp vertical rotation
        rotY = Mathf.Clamp(rotY, minY, maxY);

        //are we spectating?
        if (isSpectator)
        {
            //rotate camera vertically
            transform.rotation = Quaternion.Euler(-rotY, rotX, 0);

            //movement
            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");
            float y = 0;

            if (Input.GetKey(KeyCode.E))
                y = 1;
            else if (Input.GetKey(KeyCode.Q))
                y = -1;
            else
                y = 0;

            Vector3 dir = transform.right * x + transform.up * y + transform.forward * z;
            transform.position += dir * spectatorMoveSpeed * Time.deltaTime;
        }
        else
        {
            //rotate camera vertically
            transform.localRotation = Quaternion.Euler(-rotY, 0, 0);

            //rotate player horizontally
            transform.parent.rotation = Quaternion.Euler(transform.rotation.x, rotX, 0);
        }
    }

    public void SetAsSpectator()
    {
        isSpectator = true;
        transform.parent = null;
    }
}
