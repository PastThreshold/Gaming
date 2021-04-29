using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class is for other classes to use as an object
// It will handle its colliders so that multi-collider objects only trigger
// Ontriggerenter and oncollision only once
public class CollisionHandler : MonoBehaviour
{
    public List<Collider> collidersHit;
    public List<GameObject> gameObjectsHit;
    const float NULL_CHECK = 7f;
    public bool checkingNulls = false;

    void Start()
    {
        collidersHit = new List<Collider>();
        gameObjectsHit = new List<GameObject>();
        checkingNulls = false;
    }

    void Update()
    {
        if (!checkingNulls)
            StartCoroutine("CheckNulls");
    }

    IEnumerator CheckNulls()
    {
        checkingNulls = true;
        yield return new WaitForSeconds(NULL_CHECK);
        if (collidersHit.Count > 0)
            RemoveNullsFromColliders();
        if (gameObjectsHit.Count > 0)
            RemoveNullsFromGameObjects();
        checkingNulls = false;
    }

    /// <summary>
    /// A collider is put in, if the collider's root parent matches one in gameObjectHit it returns true else return false
    /// True if it was already hit false otherwise</summary>
    /// <param name="other">collider to checdk against already hit colliders</param>
    /// <returns></returns>
    public bool CheckIfAlreadyBeenHitOnEnter(Collider other)
    {
        bool alreadyHit = true;
        collidersHit.Add(other);

        foreach (GameObject obj in gameObjectsHit)
        {
            if (obj != null)
            {
                if (other.transform.root.gameObject == obj)
                    alreadyHit = false;
            }
        }

        if (alreadyHit)
        {
            AddGameObject(other.gameObject);
            return false;
        }
        return true;
    }

    public bool CheckIfAlreadyHitOnExit(Collider other)
    {
        bool alreadyHit = true;

        print("Remove " +  other.name);
        collidersHit.Remove(other);

        foreach (Collider col in collidersHit)
        {
            if (col != null)
            {
                if (other.transform.root.gameObject == col.transform.root.gameObject)
                    alreadyHit = false;
            }
        }

        if (alreadyHit)
        {
            RemoveGameObject(other.gameObject);
            return false;
        }
        return true;
    }

    // These can get expensive if used every frame so they are used every five seconds
    private void RemoveNullsFromColliders()
    {
        print("rem");
        for (int i = collidersHit.Count - 1; i >= 0; i--)
        {
            if (collidersHit[i] == null)
                collidersHit.Remove(collidersHit[i]);
        }
    }

    private void RemoveNullsFromGameObjects()
    {
        for (int i = gameObjectsHit.Count - 1; i >= 0; i--)
        {
            if (gameObjectsHit[i] == null)
                gameObjectsHit.Remove(gameObjectsHit[i]);
        }
    }

    public void RemoveGameObject(GameObject obj)
    {
        gameObjectsHit.Remove(obj.transform.root.gameObject);
    }

    public void AddGameObject(GameObject obj)
    {
        gameObjectsHit.Add(obj.transform.root.gameObject);
    }

    // Static method intended for classes when using things like physics overlap sphere and box in order to do single
    // occurance damage

    public static List<GameObject> TestSingleTriggerArray(Collider[] colliders)
    {
        List<GameObject> gameObjects = new List<GameObject>();
        bool alreadyExists;
        foreach (Collider col in colliders)
        {
            
            GameObject rootObj = col.transform.root.gameObject;
            alreadyExists = false;
            foreach (GameObject obj in gameObjects)
            {
                if (rootObj == obj)
                {
                    alreadyExists = true;
                    break;
                }
            }

            if (!alreadyExists)
            {
                gameObjects.Add(rootObj);
            }
        }
        return gameObjects;
    }

    /// <summary>
    /// Tests a single array of RaycastsHits and returns the root object of every collider hit
    /// but that root will only appear once in the returned list
    /// Useful for RaycastAll()
    /// </summary>
    /// <param name="hits">Array of raycasts to test</param>
    /// <returns>List with root gameobjects</returns>
    public static List<GameObject> TestSingleTriggerArray(RaycastHit[] hits)
    {
        List<GameObject> gameObjects = new List<GameObject>();
        bool alreadyExists;
        foreach (RaycastHit hit in hits)
        {

            GameObject rootObj = hit.transform.root.gameObject;
            alreadyExists = false;
            foreach (GameObject obj in gameObjects)
            {
                if (rootObj == obj)
                {
                    alreadyExists = true;
                    break;
                }
            }

            if (!alreadyExists)
            {
                gameObjects.Add(rootObj);
            }
        }
        return gameObjects;
    }


    /// <summary>
    /// Tests a single collider if its root object has already been handled or hit
    /// Meant for use with Add and Remove GameObject rather than colliderAlreadyHitEnter and exit
    /// </summary>
    /// <param name="col"></param>
    /// <returns>Whether the list contains the col</returns>
    public bool TestCollider(Collider col)
    {
        GameObject rootObject = col.transform.root.gameObject;
        for(int i = 0; i < gameObjectsHit.Count; i++)
        {
            if (rootObject == gameObjectsHit[i])
            {
                return true;
            }
        }
        return false;
    }
}
