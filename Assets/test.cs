using UnityEngine;
using System.Collections;

public class test : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Debug.Log("el " + Quaternion.Euler(25.0f, 90.0f , 0));
        Debug.Log("AAZ " + Quaternion.AngleAxis(0.0f, Vector3.forward));
        Debug.Log("AAX " + Quaternion.AngleAxis(25.0f, Vector3.right));
        Debug.Log("AAY " + Quaternion.AngleAxis(90.0f, Vector3.up));
        Debug.Log("ZX " + (Quaternion.AngleAxis(0.0f, Vector3.forward) * Quaternion.AngleAxis(25.0f, Vector3.right)));
        Debug.Log("(ZX)Y " + 
            Quaternion.AngleAxis(90.0f, Vector3.up)  *
            (Quaternion.AngleAxis(25.0f, Vector3.right) * Quaternion.AngleAxis(0.0f, Vector3.forward))
            );
	}

    Vector2 accRotation = Vector2.zero;
    
	void Update () {
        
        if (Input.GetKey(KeyCode.W))
        {
            transform.Translate(Vector3.forward * Time.deltaTime * 5.0f, Space.Self);
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.Translate(Vector3.right * Time.deltaTime * 5.0f, Space.Self);
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.Translate(Vector3.back * Time.deltaTime * 5.0f, Space.Self);
        }
        if (Input.GetKey(KeyCode.A))
        {
            transform.Translate(Vector3.left * Time.deltaTime * 5.0f, Space.Self);
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            //transform.Rotate(transform.up, -5.0f, Space.Self);
            Cursor.lockState = CursorLockMode.Locked;        
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            //transform.Rotate(transform.up, 5.0f, Space.Self);
            Cursor.lockState = CursorLockMode.None;
        }
        accRotation.x += Input.GetAxis("Mouse X");
        accRotation.y -= Input.GetAxis("Mouse Y");

        if (accRotation.x > 360.0f)
        {
            accRotation.x -= 360.0f;
        }
        else if (accRotation.x < -360.0f)
        {
            accRotation.x += 360.0f;
        }

        if (accRotation.y > 65.0f)
        {
            accRotation.y = 65.0f;
        }
        else if (accRotation.y < -65.0f)
        {
            accRotation.y = -65.0f;
        }

        //transform.localRotation = Quaternion.Euler(accRotation.y, accRotation.x, 0.0f);

        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            Debug.Log("LtoW: " + transform.localToWorldMatrix);
            Debug.Log("WtoL: " + transform.worldToLocalMatrix);
            Debug.Log("WtoC: " + GetComponent<Camera>().worldToCameraMatrix);
            Debug.Log("up: " + transform.up);
            Debug.Log("right: " + transform.right);
            Debug.Log("forward: " + transform.forward);
            Debug.Log("WtoC: " + GetComponent<Camera>().projectionMatrix);
        }
	}
}
