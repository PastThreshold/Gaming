using UnityEngine;

public class Hook : MonoBehaviour
{
    public int canGrab = 1;
    public int held = 0;
    float speed = 10f;
    CollisionHandler colHandler;
    Grapple grapple = null;

    Vector3 directionToMove = Vector3.zero;
    Vector3 endPosition = Vector3.zero;
    bool moving = false;
    bool returning = false;
    [SerializeField] float closeEnoughToPosition = 0.1f;

    void Start()
    {
        colHandler = gameObject.AddComponent<CollisionHandler>();
    }

    /// <summary>
    /// If hook has been called to Fire() by grapple, move to the passed position, once close enough
    /// set returning flag to true and move back to player until close enough then stop moving
    /// </summary>
    void Update()
    {
        if (moving)
        {
            if (returning)
            {
                directionToMove = (GlobalClass.player.transform.position - transform.position).normalized;
                transform.position += directionToMove * speed * Time.deltaTime;

                if (Extra.SquaredDistanceWithoutY(transform.position, GlobalClass.player.transform.position) < closeEnoughToPosition)
                    Returned();

            }
            else
            {
                transform.position += directionToMove * speed * Time.deltaTime;

                if (Extra.SquaredDistanceWithoutY(transform.position, endPosition) < closeEnoughToPosition)
                    IsNowReturning();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        switch (other.tag)
        {
            case GlobalClass.PICKUP_TAG:
                if (held < canGrab)
                {
                    Pickup pickup = other.GetComponent<Pickup>();
                    pickup.transform.parent = transform;
                    pickup.PauseTimer();
                    held++;
                }
                break;
            default:
                Debug.Log("Different String Collision");
                break;
        }
    }

    public void UpdateLevelValues(int canGrab, float speed)
    {
        this.canGrab = canGrab;
        this.speed = speed;
    }

    /// <summary>
    /// Recieves the position to move to which is the current mouse location coverted and gets that direction and sets update
    /// Called by: Grapple
    /// </summary>
    /// <param name="position">Position to move (mouse location)</param>
    public void Fire(Vector3 position)
    {
        endPosition = position;
        directionToMove = (Extra.SetYToTransform(position, transform) - transform.position).normalized;
        moving = true;
        transform.parent = null;
    }

    private void IsNowReturning()
    {
        returning = true;
    }

    private void Returned()
    {
        held = 0;
        moving = false;
        returning = false;
        transform.parent = grapple.transform;
        grapple.HookReturned();
    }

    public void SetToAlternate()
    {

    }

    public void SetGrapple(Grapple grapple)
    {
        this.grapple = grapple;
    }
}
