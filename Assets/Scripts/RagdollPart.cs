using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollPart : MonoBehaviour
{
    Rigidbody rb;
    [Tooltip("The root parent of the entire ragdoll")] 
    [SerializeField] RagdollPart containerParent = null;
    [Tooltip("The join component of this object")] 
    [SerializeField] ConfigurableJoint joint;
    [SerializeField] float time = 3.5f;
    [SerializeField] float childBreakChance = 60f;


    public void StartRagdoll(Vector3 direction, float force)
    {
        DirectionAndForce value = new DirectionAndForce(direction, force);
        BroadcastMessage("TakeImpact", value, SendMessageOptions.DontRequireReceiver);
        Destroy(gameObject, time);
    }

    public void TakeImpact(DirectionAndForce value)
    {
        if (containerParent == this)
            return;
        if (GetComponent<Rigidbody>())
            rb = GetComponent<Rigidbody>();
        
        if (!Extra.RollChance(childBreakChance))
        {
            if (transform.parent.GetComponent<RagdollPart>() == containerParent)
            {
                rb.AddForce(value.GetDirection() * value.GetForce() * Random.Range(1f, 2.5f), ForceMode.Impulse);
            }
            else
            {
                //rb.isKinematic = true;
            }
        }
        else
        {
            if (joint)
                Destroy(joint);
            transform.parent = containerParent.transform;
            rb.AddForce(value.GetDirection() * value.GetForce() * Random.Range(2.5f, 1f), ForceMode.Impulse);
        }
    }

    public class DirectionAndForce
    {
        // The purpose of this class is to hold a differenceVector and force so that it can pass multiple 
        // parameters into a broadcast message becuase unity bad.
        Vector3 differenceVector;
        float force;
        
        public DirectionAndForce(Vector3 direction, float force)
        {
            differenceVector = direction;
            this.force = force;
        }

        public Vector3 GetDirection()
        {
            return differenceVector;
        }
        public float GetForce()
        {
            return force;
        }
    }
}
