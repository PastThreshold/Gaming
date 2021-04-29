using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityWell : MonoBehaviour
{
    Rigidbody rb;
    const float G = 66.74f;
    public static List<Rigidbody> projectilesInScene;
    [SerializeField] float maxRange;
    bool waitingToRemove = true;


    private void FixedUpdate()
    {
        for (int i = 0; i < projectilesInScene.Count; i++)
        {
            if (projectilesInScene[i] == null)
            {
                if (!waitingToRemove)
                StartCoroutine("WaitBetweenRemoval");
            }
            else
                Attract(projectilesInScene[i]);
        }
    }

    IEnumerator WaitBetweenRemoval()
    {
        waitingToRemove = true;
        yield return new WaitForSeconds(10f);
        for (int i = projectilesInScene.Count; i >= 0; i--)
        {
            if (projectilesInScene[i] == null)
            {
                projectilesInScene.Remove(projectilesInScene[i]);
            }
        }
        waitingToRemove = false;
    }

    void Attract(Rigidbody objToAttract)
    {
        Vector3 direction = rb.position - objToAttract.position;
        float distance = direction.magnitude;
        if (distance < maxRange)
        {
            float forceMagnitude = G * (rb.mass * objToAttract.mass) / (distance * distance);
            Vector3 force = direction.normalized * forceMagnitude;
            force.y = 0;
            objToAttract.AddForce(force);
        }
    }

    public static void AddProjectile(Rigidbody rb)
    {
        if (projectilesInScene == null)
            projectilesInScene = new List<Rigidbody>();
        
        projectilesInScene.Add(rb);
    }
}
