using UnityEngine;

public class LookAtMouse : MonoBehaviour
{
    [SerializeField] Transform firePointRotation;
    [SerializeField] Transform deagleTest;
    [SerializeField] Transform deagleTest2;
    public Vector3 pointToLook;

    void Update()
    {
        RotateTransforms();
    }


    private void RotateTransforms()
    {
        pointToLook = GlobalClass.player.mouseLocationConverted;
        deagleTest.LookAt(new Vector3(pointToLook.x, deagleTest.position.y, pointToLook.z));
        deagleTest2.LookAt(new Vector3(pointToLook.x, deagleTest2.position.y, pointToLook.z));
        firePointRotation.LookAt(new Vector3(pointToLook.x, transform.position.y, pointToLook.z));
    }
}
