using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkerLeg : MonoBehaviour
{
    [Header("Leg Movement")]
    [SerializeField] Walker thisWalker;
    [SerializeField] Transform footTarget;
    Vector3 footTargetSavedPosition;
    [SerializeField] Transform elbowTarget;
    [SerializeField] Transform distanceCheckTarget;
    bool moving = false;
    [SerializeField] float distance;
    Vector3 directionToMove;
    [SerializeField] float timeToMove = 1f;
    [SerializeField] float upwardMovment = 0.5f;
    float speed;

    [Header("Spike")]
    [SerializeField] Transform spike;
    [SerializeField] Transform spikeTarget;
    [SerializeField] Transform elbowBone;
    [SerializeField] Vector3 spikeRotationOffset;
    //[SerializeField] Transform endOfLegBone;

    static bool isActive = true;


    private void Start()
    {
        moving = false;
        footTargetSavedPosition = footTarget.position;
    }

    private void Update()
    {
        spikeTarget.position = footTarget.position + elbowBone.transform.up; // Spike target is always parallel to leg direction
        spike.position = footTarget.position; // spike always at the end of leg
        spike.transform.LookAt(spikeTarget); // spike looks at the spike target (parallel to leg direction)
        spike.eulerAngles += spikeRotationOffset;

        if (moving)
        {
            footTarget.position += directionToMove * speed * Time.deltaTime;
        }
        else
        {
            footTarget.position = footTargetSavedPosition;

            if ((footTarget.position - distanceCheckTarget.position).sqrMagnitude > distance)
            {
                CalculateMovement(false);
            }
        }
    }

    /// <summary>
    /// Calculate where to move the leg target
    /// </summary>
    /// <param name="resetFlag">Bool flag to reset the leg to original position or to move just a bit further</param>
    public void CalculateMovement(bool resetFlag)
    {
        float distance = (footTarget.position - distanceCheckTarget.position).magnitude;
        if (!resetFlag)
            distance += 0.65f;
        directionToMove = (distanceCheckTarget.position - footTarget.position).normalized;
        speed = distance / timeToMove;
        moving = true;
        StartCoroutine("MoveTimer");
    }

    /// <summary>
    /// Timer for how long the leg should be moving, the first moves slightly upwards, and 
    /// the second half move the same downwards
    /// </summary>
    IEnumerator MoveTimer()
    {
        float halfTime = timeToMove / 2;
        directionToMove.y += upwardMovment;
        yield return new WaitForSeconds(halfTime);
        directionToMove.y -= upwardMovment * 2;
        yield return new WaitForSeconds(halfTime);
        FinishedMovement();
    }

    private void FinishedMovement()
    {
        moving = false;
        spike.position = footTarget.position;
        footTargetSavedPosition = footTarget.position;
    }

    public void StopMovement()
    {
        moving = false;
        StopCoroutine("MoveTimer");
    }

    public bool IsMoving() { return moving; }
    public static void Enable() { isActive = true; }
    public static void Disable() { isActive = false; }
}
