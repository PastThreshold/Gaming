using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    Transform cam;

    private void Start()
    {
        cam = Camera.main.transform;
    }

    private void LateUpdate()
    {
        transform.forward = cam.forward; ;
        //transform.LookAt(transform.position + cam.transform.forward);
    }
}
