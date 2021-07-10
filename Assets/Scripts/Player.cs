using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Camera;
using static UnityEngine.InputSystem.InputAction;

public class Player : MonoBehaviour, IHealth
{
    [SerializeField] private Camera cam;
    [SerializeField] private LaserPooler laserPooler;
    [SerializeField] private GameObject endGamePanel;
    // [SerializeField] private GameObject primaryProjectilePrefab;
    Vector2 movementInput = new Vector2();
    Vector2 lastMovementDir = new Vector2();
    public float maxSpeed;
    public float acceleration;
    public float decceleration;
    public float speed {get; private set;}

    public float maxHealth {get => _maxHP; set{}}
    [SerializeField] private float _maxHP;
    public float currentHealth {get => _hp; set{_hp = value;}}
    float _hp;
    public bool isDead {get => _isDead; set{}}
    bool _isDead;
    public bool isDying {get => _isDying; set{}}
    bool _isDying;

    bool primaryFire;
    public float primaryFireSpeed;
    float nextPrimaryFireTime;
    public List<Transform> primaryFirePoints = new List<Transform>();
    int lastFirePointIndex;

    bool secondaryFire;

    public event OnHealthChanged onHealthChanged;

    private void Start() => currentHealth = maxHealth;

    private void Update()
    {
        if(primaryFire && Time.timeSinceLevelLoad >= nextPrimaryFireTime)
            PrimaryFire();
    }

    private void FixedUpdate()
    {
        if(isDead || isDying)
            return;

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
        if(isDead || isDying)
            return;

        movementInput = new Vector2(context.ReadValue<float>(), movementInput.y);
    }
    public void Vertical(CallbackContext context)
    {
        if(isDead || isDying)
            return;

        movementInput = new Vector2(movementInput.x, context.ReadValue<float>());
    }

    public void PrimaryFireInput(CallbackContext context)
    {
        if(isDead || isDying)
            return;

        if(context.canceled)
            primaryFire = false;
        else if(context.started)
            primaryFire = true;
    }
    private void PrimaryFire()
    {
        nextPrimaryFireTime = Time.timeSinceLevelLoad + primaryFireSpeed;
        int firePointIndex = lastFirePointIndex + 1 > primaryFirePoints.Count - 1 ? 0 : lastFirePointIndex + 1;
        Transform firePoint = primaryFirePoints[firePointIndex];
        lastFirePointIndex = firePointIndex;
        
        LaserProjectile proj = laserPooler.pool.Get();
        proj.gameObject.transform.SetPositionAndRotation(firePoint.position, transform.rotation);
        proj.Fire(transform, true);
    }

    public void SecondaryFireInput(CallbackContext context)
    {
        if(isDead || isDying)
            return;
    }

    public void TakeDamage(float amount, Transform damagedBy)
    {
        if(isDying || isDead)
            return;

        float oldHP = currentHealth;
        currentHealth = Mathf.Clamp(currentHealth - Mathf.Abs(amount), 0, maxHealth);

        if(oldHP != currentHealth)
            onHealthChanged?.Invoke(currentHealth);

        if(currentHealth == 0)
            Die();
    }
    
    public void HealToFull()
    {
        currentHealth = maxHealth;
        onHealthChanged?.Invoke(currentHealth);
    }

    public void Die()
    {
        if(primaryFire)
            primaryFire = false;
        _isDead = true;

        endGamePanel.SetActive(true);
    }
}