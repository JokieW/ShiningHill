using UnityEngine;
using UnityEditor;
using System.Collections;

public class Test2 : MonoBehaviour
{
    public Vector3 axis = Vector3.up;
    void Start()
    {
        Debug.Log("tes " + Vector3.Project(Vector3.one, Vector3.forward));
    }

    float angle = 0;

	void Update () 
    {
        /*transform.Rotate(Vector3.up * 180.0f * Time.deltaTime);
        angle++;
        if (angle == 361)
        {
            angle = 0;
        }*/
	}

    void OnDrawGizmos()
    {
        //Gizmos.DrawWireSphere(Vector3.Project(), Vector3.Cross(Vector3.up, Vector3.up));

    }

}
