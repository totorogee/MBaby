using System;
using UnityEngine;
using System.Collections;


public class SimpleContral : MonoBehaviour
{
    public bool circular = false;
    private Vector3 center = Vector3.zero;

    public float speed = 5;
    public float rotateSpeed = 5;
    private Rigidbody2D m_Rigidbody;
    private Vector3 move = Vector3.zero;
    private float z = 0;

    private void Update()
    {
        float v = Input.GetAxis("Vertical");
        float h = Input.GetAxis("Horizontal");

        z = 0;
        if (Input.GetKey("z")) z += rotateSpeed;
        if (Input.GetKey("x")) z -= rotateSpeed;

        if (!circular)
        move = v * Vector3.up + h * Vector3.right;

        if (circular)
        {
            move = v * (center - transform.position).normalized;
            move += h * (Vector3) V2Rotate((transform.position - center), 90f).normalized;
        }
    }

    private void FixedUpdate()
    {
        transform.position = transform.position + move * Time.fixedDeltaTime * speed;
        transform.eulerAngles = new Vector3(
            transform.eulerAngles.x,
            transform.eulerAngles.y,
            transform.eulerAngles.z + z);
    }

    Vector2 V2Rotate(Vector2 aPoint, float a)
    {
        float rad = a * Mathf.Deg2Rad;
        float s = Mathf.Sin(rad);
        float c = Mathf.Cos(rad);
        return new Vector2(
            aPoint.x * c - aPoint.y * s,
            aPoint.y * c + aPoint.x * s);
    }
}

