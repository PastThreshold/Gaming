using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] Collider doorCollider;
    [SerializeField] GameObject cube;
    [SerializeField] GameObject cube1;
    [SerializeField] GameObject cube2;
    bool active = false;

    private void Start()
    {
        active = false;
        cube.SetActive(false);
        cube1.SetActive(false);
        cube2.SetActive(false);
        doorCollider.enabled = false;
    }

    public void SetActive()
    {
        active = true;
        cube.SetActive(true);
        cube1.SetActive(true);
        cube2.SetActive(true);
        doorCollider.enabled = true;
    }
}
