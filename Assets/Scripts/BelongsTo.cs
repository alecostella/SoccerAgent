using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BelongsTo : MonoBehaviour
{
    //private Rigidbody rigidBody;
    public GameObject player = null;
    private Rigidbody rb;

    public void SetPlayer(GameObject Player)
    {
        player = Player;
        if (Player != null) transform.position = player.transform.position + new Vector3(0, 0.9f, 0);
    }

    public void Start()
    {
        rb = GetComponent<Rigidbody>();
        player = null;
    }

    public void Update()
    {
        if (player != null) 
        {
            Vector3 newPos = player.transform.position + new Vector3(0, 0.9f, 0);
            rb.MovePosition(newPos);
        }
    }
}
