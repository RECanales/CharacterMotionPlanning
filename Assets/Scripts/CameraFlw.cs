using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFlw : MonoBehaviour
{
    GameObject follow_object;
    Vector3 offset;
    void Start()
    {
        follow_object = GameObject.FindGameObjectWithTag("Player");
        if (follow_object)
            offset = transform.position - follow_object.transform.position;
        else
            enabled = false;
    }

    void Update()
    {
        if (follow_object)
            transform.position = Vector3.Lerp(transform.position, follow_object.transform.position + offset, 2.5f * Time.deltaTime);
    }
}
