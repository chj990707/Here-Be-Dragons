using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    public Camera mainCamera;
    // Update is called once per frame
    void Update()
    {
        mainCamera.transform.rotation = Quaternion.Euler(new Vector3(15, -Time.realtimeSinceStartup / Mathf.PI * 12f, 0));
    }
}
