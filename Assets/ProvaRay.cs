using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProvaRay : MonoBehaviour
{

    private GameObject ball;
    void Start()
    {
        ball = GameObject.Find("Sphere");
        StartCoroutine(MyCoroutine());
    }

    IEnumerator MyCoroutine()
    {
        yield return new WaitForSeconds(3);
        bool ray = Physics.Raycast(gameObject.transform.position, ball.transform.position - gameObject.transform.position, out RaycastHit hit, Mathf.Infinity);
        Debug.Log(hit.transform.name);
    }
}
