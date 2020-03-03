using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spostamento : MonoBehaviour
{

    public int magnitude;
    void Start()
    {
        Rigidbody rb;
        rb = gameObject.GetComponent<Rigidbody>();
        rb.AddForce(new Vector3(magnitude, 0, magnitude));
    }

}
