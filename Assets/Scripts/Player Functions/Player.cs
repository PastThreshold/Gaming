using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Player Data")]
    public float movementSpeed = 5f;
    [SerializeField] float movementSpeedIncrease = 2f;
    public float health = 100;
    [SerializeField] float maxHealth = 100;
    [SerializeField] float groundFindDistance = 0.2f;
    Vector3 previousPosition;
    Vector3 currentPosition;

    public bool increasedSpeed = false;
    [SerializeField] float addFactor = -1.2265f;
    bool invulnerable = false;  

    public Vector3 mouseLocationConverted; // The base location of the mouse that the player uses for direction
    public Vector3 autoTargetLocation; // The location of the nearest enemy while auto target it active
    bool autoTargetActive = false;
    AutoTarget autoTarget;

    [Header("Script References")]
    Enrage enrageAbility;
    public bool enraged = false;

    HeadsUpDisplay hud;
    Animator anim;
    [SerializeField] Collider[] colliders;
    Vector3 movement;
    Rigidbody rb;
    public bool lookingAtMouse;

    public float originalSpeed;

    public bool canMove = true;

    public List<Clone> allActiveClones;

    [SerializeField] public LayerMask enemyLayerMask;

    void Start()
    {
        increasedSpeed = false;
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        hud = FindObjectOfType<HeadsUpDisplay>();
        enrageAbility = GetComponent<Enrage>();
        autoTarget = GetComponent<AutoTarget>();
        allActiveClones = new List<Clone>();
        DontDestroyOnLoad(gameObject);

        health = maxHealth;
    }

    private void Update()
    {
        if (AutoTargetActive())
        {
            if (HasTarget())
            {
                autoTargetLocation = autoTarget.GetTargetPosition();
            }
        }
    }
    public bool AutoTargetActive() { return autoTargetActive; }
    public bool HasTarget() { return autoTarget.HasTarget(); }
    public Transform GetTargetTransform() { return autoTarget.GetTargetTransform(); }
    public void EnableAutoTarget() { autoTargetActive = true; }
    public void DisableAutoTarget() 
    {
        autoTargetActive = false;
        hud.UpdateAutoTargetCrosshair();
    }


    void FixedUpdate()
    {
        previousPosition = currentPosition;
        currentPosition = transform.position;
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        if (canMove)
            Move(h, v);

        if (lookingAtMouse)
            LookAtMouse();

        float rotationY = transform.rotation.eulerAngles.y;
        Vector3 Val = new Vector3(h, 0f, v);
        Val = Quaternion.Euler(0f, -rotationY, 0f) * Val;
        anim.SetFloat("Forward", Val.z);
        anim.SetFloat("Turn", Val.x);

        if (allActiveClones.Count > 0)
        {
            if (Input.GetKeyDown(KeyCode.H))
            {
                if (Input.GetKey(KeyCode.LeftShift))
                    Clone.MouseDirectionOrPlayer();
                else
                    Clone.ControlledByPlayer();
            }
        }
    }

    private void LookAtMouse()
    {
        Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, addFactor);
        float rayLength;

        if (groundPlane.Raycast(cameraRay, out rayLength))
        {
            Vector3 pointToLook = cameraRay.GetPoint(rayLength);
            Debug.DrawLine(cameraRay.origin, pointToLook, Color.blue);
            mouseLocationConverted = new Vector3(pointToLook.x, transform.position.y, pointToLook.z);
            transform.LookAt(mouseLocationConverted);
        }
    }

    private void Move(float h, float v)
    {
        movement.x = h;
        movement.z = v;
        movement = movement.normalized * movementSpeed * Time.deltaTime;
        rb.MovePosition(transform.position + movement);
    }

    public void TakeDamage(float damage)
    {
        if (!invulnerable)
        {
            health -= damage;
            hud.UpdateHealth(health);
            if (health <= 0)
            {
                Destroy(gameObject);
            }
            else
            {
                if (enrageAbility.isHealing())
                {
                    enrageAbility.Deactivate();
                }
            }
        }
    }

    public void Heal(float heal)
    {
        health += heal;
        if (health >= maxHealth)
        {
            health = maxHealth;
        }
        hud.UpdateHealth(health);
    }


    public void DealtDamage(float damage)
    {
        enrageAbility.DealtDamage(damage);
    }

    IEnumerator IncreaseSpeed()
    {    
        yield return new WaitForSeconds(5f);
        movementSpeed -= movementSpeedIncrease;
        increasedSpeed = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == GlobalClass.SPECIAL_TAG && other.GetComponent<SphereWeapon>())
        {
            if (!increasedSpeed)
            {
                increasedSpeed = true;
                movementSpeed += movementSpeedIncrease;
                StartCoroutine("IncreaseSpeed");
            }
            else
            {
                StopCoroutine("IncreaseSpeed");
                StartCoroutine("IncreaseSpeed");
            }
        }

        if (other.GetComponent<Door>())
        {
            FindObjectOfType<LevelController>().NextLevel();        
        }
    }

    public void SetInvulnerable(bool invul)
    {
        invulnerable = invul;
        foreach(Collider col in colliders)
            col.enabled = !invul;
    }

    public void SpeedUp(float factor)
    {
        originalSpeed = movementSpeed;
        movementSpeed = movementSpeed * factor;
    }

    public void SpeedDown()
    { 
        movementSpeed = originalSpeed;
    }

    public bool WeaponFired(Projectile proj)
    {
        if (enraged)
        {
            proj.ChangeDamage(enrageAbility.GetDamageFactor());
            print(proj.name + " New damage: " + proj.GetDamage());
        }
        return enraged;
    }

    public Vector3 GetPreviousPos()
    {
        return previousPosition;
    }

    public Vector3 GetCurrentPos()
    {
        return currentPosition;
    }
}
