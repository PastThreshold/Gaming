using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{
    [SerializeField] CapsuleCollider capsuleCollider;
    [SerializeField] LineRenderer mainLaser;
    [SerializeField] float damagePerSecond = 5f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == GlobalClass.ENEMY_TAG)
            other.GetComponentInParent<Enemy>().TakeDamage(damagePerSecond * Time.deltaTime);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == GlobalClass.ENEMY_TAG)
            other.GetComponentInParent<Enemy>().TakeDamage(damagePerSecond * Time.deltaTime);
    }

    public void SetColliderSize(float length)
    {
        transform.localPosition = Vector3.zero;
        capsuleCollider.center = new Vector3(capsuleCollider.center.x, capsuleCollider.center.y, length / 2);
        capsuleCollider.height = length;
        if (mainLaser.positionCount > 0)
        {
            mainLaser.SetPosition(1, new Vector3(0, 0, length));
            mainLaser.transform.localPosition = Vector3.zero;
        }
    }
}
