using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Camera;
using static UnityEngine.InputSystem.InputAction;

public class Player : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private LaserPooler laserPooler;
    // [SerializeField] private GameObject primaryProjectilePrefab;
    Vector2 movementInput = new Vector2();
    Vector2 lastMovementDir = new Vector2();
    public float maxSpeed;
    public float acceleration;
    public float decceleration;
    public float speed {get; private set;}

    bool primaryFire;
    public float primaryFireSpeed;
    float nextPrimaryFireTime;
    public List<Transform> primaryFirePoints = new List<Transform>();
    int lastFirePointIndex;

    bool secondaryFire;

    private void Update()
    {
        // LookAtMouse();

        if(primaryFire && Time.timeSinceLevelLoad >= nextPrimaryFireTime)
            PrimaryFire();
    }

    private void FixedUpdate()
    {
        Move();
        ScreenWrap();
        LookAtMouse();
    }

    private void ScreenWrap()
    {
        Vector3 screenPos = cam.WorldToScreenPoint(transform.position, MonoOrStereoscopicEye.Mono);

        // Check horizontal wrapping
        if(screenPos.x < 0)
            screenPos = new Vector3(Screen.width + screenPos.x, screenPos.y, screenPos.z);
        else if(screenPos.x > Screen.width)
            screenPos = new Vector3(screenPos.x - Screen.width, screenPos.y, screenPos.z);

        // Check vertical wrapping
        if(screenPos.y < 0)
            screenPos = new Vector3(screenPos.x, Screen.height + screenPos.y, screenPos.z);
        else if(screenPos.y > Screen.height)
            screenPos = new Vector3(screenPos.x, screenPos.y - Screen.height, screenPos.z);

        transform.position = cam.ScreenToWorldPoint(screenPos, MonoOrStereoscopicEye.Mono);
    }

    private void LookAtMouse()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPos = cam.ScreenToWorldPoint((Vector3)mousePos + new Vector3(0, 0, 10), MonoOrStereoscopicEye.Mono);
        transform.rotation = Quaternion.LookRotation(transform.forward, (mouseWorldPos - transform.position).normalized);
    }

    private void Move()
    {
        Vector2 dir = new Vector2();

        if(movementInput.magnitude == 0 && speed > 0)
        {
            if(speed > 0)
                speed = Mathf.Clamp(speed - decceleration, 0, maxSpeed);
            else if(speed == 0)
                lastMovementDir = Vector2.zero;

            dir = lastMovementDir;
        }
        else if(movementInput.magnitude > 0)
        {
            if(speed < maxSpeed)
                speed = Mathf.Clamp(speed + acceleration, 0, maxSpeed);

            lastMovementDir = movementInput;
            dir = movementInput;
        }

        if(speed > 0 && dir.magnitude > 0)
            transform.position = transform.position + (Vector3)dir * speed * Time.fixedDeltaTime;
    }

    public void Horizontal(CallbackContext context)
    {
        movementInput = new Vector2(context.ReadValue<float>(), movementInput.y);
    }
    public void Vertical(CallbackContext context)
    {
        movementInput = new Vector2(movementInput.x, context.ReadValue<float>());
    }

    public void PrimaryFireInput(CallbackContext context)
    {
        if(context.started)
            primaryFire = true;
        else if(context.canceled)
            primaryFire = false;
    }
    private void PrimaryFire()
    {
        nextPrimaryFireTime = Time.timeSinceLevelLoad + primaryFireSpeed;
        int firePointIndex = lastFirePointIndex + 1 > primaryFirePoints.Count - 1 ? 0 : lastFirePointIndex + 1;
        Transform firePoint = primaryFirePoints[firePointIndex];
        lastFirePointIndex = firePointIndex;
        
        LaserProjectile proj = laserPooler.pool.Get();
        proj.gameObject.transform.SetPositionAndRotation(firePoint.position, transform.rotation);
        proj.Fire();
    }

    public void SecondaryFireInput(CallbackContext context)
    {
        
    }
}