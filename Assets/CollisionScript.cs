using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionScript : MonoBehaviour
{
    private Rigidbody bananaRigidBody;
    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "banana")
        {
            bananaRigidBody = collision.gameObject.GetComponent<Rigidbody>();


            

            //var direction = bananaRigidBody.position - gameObject.transform.position;
            //direction = direction.normalized;

            bananaRigidBody.AddForce(collision.GetContact(0).normal * 20);
        }
    }

    public void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.name == "banana")
        {
            //var direction = bananaRigidBody.position - gameObject.transform.position;
            //direction = direction.normalized;

            bananaRigidBody.AddForce(collision.GetContact(0).normal * 20);
        }
    }
}
