using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class TestRotate : MonoBehaviour
{
    public Vector3 dir;

    private void Start()
    {
    }

    private void Update()
    {
        transform.right = dir;

        // transform.Translate(Vector3.left * 0.1f * Time.deltaTime);

        // transform.position = transform.position + Vector3.left * 0.1f * Time.deltaTime;

        transform.position = transform.position + ((Vector3)(math.left() * 0.1f * Time.deltaTime));
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(new Vector3(0, 1, 0), new Vector3(0, 1, 0) + dir.normalized * 4);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + transform.right * 4);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 4);
    }
}
