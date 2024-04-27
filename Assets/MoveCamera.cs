using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        gameObject.transform.position += new Vector3(20, 0, 20) * Time.deltaTime;
    }
}
