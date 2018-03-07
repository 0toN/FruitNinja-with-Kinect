using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderUtil : MonoBehaviour {
    private int colliderCount;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    //当碰撞发生的时候
    void OnCollisionEnter2D(Collision2D collision)
    {
        colliderCount++;
        Destroy(collision.gameObject);
        if (colliderCount == 3)
        {

        }
    }
}
