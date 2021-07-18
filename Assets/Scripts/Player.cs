using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Camera;
using static UnityEngine.InputSystem.InputAction;

public class Player : MonoBehaviour, IHealth
{
    [SerializeField] private Camera cam;
    [SerializeField] private LaserPooler laserPooler;
    [SerializeField] private BombPooler bombPooler;
    [SerializeField] private GameObject endGamePanel;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private ParticleSystemPooler sparksPooler;
    [SerializeField] private ParticleSystemPooler explosionPooler;
    [SerializeField] private AudioSource laserSource;
    [SerializeField] private AudioSource batterySource;
    [SerializeField] public AudioSource fuelSource;
    [SerializeField] public AudioSource alarmSource;
    [SerializeField] private AudioSource shieldsSource;
    [SerializeField] private AudioSource damagedSource;
    [SerializeField] private AudioSource screenWrapSource;
    [SerializeField] private AudioSource powerupSource;
    public AudioClip outOfEnergy;
    public AudioClip chargeFromEmpty;
    public AudioClip chargeClip;
    public List<AudioClip> damagedClips = new List<AudioClip>();
    public AudioClip pickupPowerupClip;
    public AudioClip activatePowerupClip;
    public AudioClip losePowerupClip;
    Vector2 movementInput = new Vector2();
    Vector2 lastMovementDir = new Vector2();
    public float maxSpeed;
    public float acceleration;
    public float decceleration;
    public float speed {get; private set;}

    public float maxHealth {get => _maxHP; set{}}
    [SerializeField] private float _maxHP;
    [ReadOnly, ShowInInspector] public float currentHealth {get => _hp; set{_hp = value;}}
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
    public Transform secondaryFirePoint;

    public event OnHealthChanged onHealthChanged;

    public float maxFuel;
    [ReadOnly] public float currentFuel;
    public float fuelEfficiency;
    public delegate void OnFuelChanged(float newAmount);
    public OnFuelChanged onFuelChanged;

    public float maxCharge;
    [ReadOnly] public float currentCharge;
    public delegate void OnBatteryChanged(float newAmount);
    public OnBatteryChanged onBatteryChanged;

    public Bomb activeBomb;

    bool flashing = false;
    public float flashTime;
    float ellapsedFlashTime;

    public float screenWrapCost;
    public bool canScreenWrap => currentCharge >= screenWrapCost;

    [SerializeField] private ParticleSystem enginePlume;

    public PowerupType mostRecentlyPickedUpPowerup {get; private set;}
    // public PowerupType activePowerup {get; private set;}
    public Dictionary<PowerupType, float> activePowerups = new Dictionary<PowerupType, float>();

    public float powerupDuration;
    // float powerupFallOffAtTime;

    public delegate void OnPickedUpPowerup(PowerupType pickedUp);
    public OnPickedUpPowerup onPickedUpPowerup;
    public delegate void OnActivatePowerup(PowerupType activated);
    public OnActivatePowerup onActivatePowerup;
    public delegate void OnLosePowerup(PowerupType lost);
    public OnLosePowerup onLosePowerup;

    private void Start()
    {
        currentFuel = maxFuel;
        currentHealth = maxHealth;
        currentCharge = maxCharge;
    } 

    private void Update()
    {
        if(primaryFire && Time.timeSinceLevelLoad >= nextPrimaryFireTime)
            PrimaryFire();
        
        if(flashing)
        {
            ellapsedFlashTime += Time.deltaTime;
            spriteRenderer.material.SetFloat("_HighlightAmount", Mathf.Lerp(0.1f, 0f, Mathf.Clamp01(ellapsedFlashTime/flashTime)));
            if(ellapsedFlashTime >= flashTime)
                flashing = false;
        }

        List<PowerupType> typesToRemove = new List<PowerupType>();

        foreach(KeyValuePair<PowerupType, float> activePowerup in activePowerups)
        {
            if(Time.timeSinceLevelLoad >= activePowerup.Value)
            {
                powerupSource.PlayOneShot(losePowerupClip);
                onLosePowerup?.Invoke(activePowerup.Key);
                typesToRemove.Add(activePowerup.Key);
            }
        }

        foreach(PowerupType typeToRemove in typesToRemove)
            activePowerups.Remove(typeToRemove);

        // if(activePowerup != PowerupType.None && Time.timeSinceLevelLoad >= powerupFallOffAtTime)
        // {
        //     powerupSource.PlayOneShot(losePowerupClip);
        //     activePowerup = PowerupType.None;
        //     onLosePowerup?.Invoke();
        // }
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
        Vector3 wrappedPos = screenPos.GetScreenWrapPosition();
        if(screenPos == wrappedPos)
            return;

        screenWrapSource.Play();

        Vector3 newPos = SpendCharge(screenWrapCost) ? wrappedPos : screenPos.ClampPositionToScreen();
        transform.position = cam.ScreenToWorldPoint(newPos, MonoOrStereoscopicEye.Mono);
    }

    private void LookAtMouse()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPos = cam.ScreenToWorldPoint((Vector3)mousePos + new Vector3(0, 0, 10), MonoOrStereoscopicEye.Mono);
        transform.rotation = Quaternion.LookRotation(transform.forward, (mouseWorldPos - transform.position).normalized);
    }

    private void Move()
    {
        float scaledFuelEfficiency = fuelEfficiency * (activePowerups.ContainsKey(PowerupType.Movespeed) ? 1.5f : 1f);
        if(currentFuel < speed * scaledFuelEfficiency)
        {
            if(!alarmSource.isPlaying)
                alarmSource.Play();
            // If engine jet isn't off, turn it off.
            if(enginePlume.isPlaying)
                enginePlume.Stop();

            speed = 0;
            lastMovementDir = Vector2.zero;
            return;
        }

        if(!SpendFuel(speed * scaledFuelEfficiency))
        {
            if(!alarmSource.isPlaying)
                alarmSource.Play();
            // If engine jet isn't off, turn it off.
            if(enginePlume.isPlaying)
                enginePlume.Stop();

            speed = 0;
            lastMovementDir = Vector2.zero;
            return;
        }

        Vector2 dir = new Vector2();

        float scaledMaxSpeed = maxSpeed * (activePowerups.ContainsKey(PowerupType.Movespeed) ? 2f : 1f);

        if(movementInput.magnitude == 0 && speed > 0)
        {
            if(speed > 0)
                speed = Mathf.Clamp(speed - decceleration, 0, scaledMaxSpeed);
            else if(speed == 0)
                lastMovementDir = Vector2.zero;

            dir = lastMovementDir;
        }
        else if(movementInput.magnitude > 0)
        {
            if(speed < scaledMaxSpeed)
                speed = Mathf.Clamp(speed + acceleration, 0, scaledMaxSpeed);

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
        nextPrimaryFireTime = Time.timeSinceLevelLoad + primaryFireSpeed * (activePowerups.ContainsKey(PowerupType.AttackSpeed) ? 0.25f : 1f);

        if(currentCharge == 0)
            return;        

        float damageMod = activePowerups.ContainsKey(PowerupType.AttackDamage) ? 3f : 1f;
        float chargeMod = activePowerups.ContainsKey(PowerupType.AttackDamage) ? 2f : 1f;
        
        Vector3 mouseWorldPos = cam.ScreenToWorldPoint((Vector3)Mouse.current.position.ReadValue() + Vector3.forward * 10, MonoOrStereoscopicEye.Mono);
        laserSource.Play();
        
        if(activePowerups.ContainsKey(PowerupType.TwinShot))
        {
            if(!SpendCharge(2 * chargeMod))
                return;

            LaserProjectile laser1 = laserPooler.pool.Get();
            LaserProjectile laser2 = laserPooler.pool.Get();
            Transform laser1Pos = primaryFirePoints[0];
            Transform laser2Pos = primaryFirePoints[1];
            laser1.transform.SetPositionAndRotation(
                position: laser1Pos.position,
                rotation: Quaternion.LookRotation(transform.forward, (mouseWorldPos - laser1Pos.transform.position).normalized)
            );
            laser2.transform.SetPositionAndRotation(
                position: laser2Pos.position,
                rotation: Quaternion.LookRotation(transform.forward, (mouseWorldPos - laser2Pos.transform.position).normalized)
            );
            laser1.Fire(transform, damageMod, true);
            laser2.Fire(transform, damageMod, true);
            
        }
        else
        {
            if(!SpendCharge(chargeMod))
                return;
                
            int firePointIndex = lastFirePointIndex + 1 > primaryFirePoints.Count - 1 ? 0 : lastFirePointIndex + 1;
            Transform firePoint = primaryFirePoints[firePointIndex];
            lastFirePointIndex = firePointIndex;
            
            LaserProjectile proj = laserPooler.pool.Get();
            proj.transform.SetPositionAndRotation(firePoint.position, Quaternion.LookRotation(transform.forward, (mouseWorldPos - firePoint.transform.position).normalized));
            proj.Fire(transform, damageMod, true);
        }
    }

    public void SecondaryFireInput(CallbackContext context)
    {
        if(isDead || isDying || !context.performed)
            return;

        if(activeBomb != null)
            activeBomb.Collide();
        else
        {
            if(currentCharge == 0)
                return;
            
            if(!SpendCharge(45))
                return;
            
            Bomb bomb = bombPooler.pool.Get();
            activeBomb = bomb;
            bomb.gameObject.transform.SetPositionAndRotation(secondaryFirePoint.position, transform.rotation);
            bomb.Fire(transform, 1, true);
        }
        
    }

    public void TakeDamage(float amount, Transform damagedBy)
    {
        if(isDying || isDead)
            return;

        amount = Mathf.Abs(amount);

        if(currentCharge > 0 )
        {
            flashing = true;
            ellapsedFlashTime = 0;
        }
        else
        {
            PooledParticleSystem sparks = sparksPooler.pool.Get();
            sparks.transform.position = transform.position;
            sparks.Play();
        }

        if(currentCharge >= amount)
        {
            shieldsSource.Play();
            SpendCharge(amount);
            return;
        }

        float remainingAfterShields = amount - currentCharge;
        SpendCharge(currentCharge);

        float oldHP = currentHealth;
        currentHealth = Mathf.Clamp(currentHealth - remainingAfterShields, 0, maxHealth);

        if(oldHP != currentHealth)
        {
            damagedSource.PlayOneShot(damagedClips[UnityEngine.Random.Range(0, damagedClips.Count)]);
            onHealthChanged?.Invoke(currentHealth);
        }

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

        PooledParticleSystem explosion = explosionPooler.pool.Get();
        explosion.transform.position = transform.position;
        explosion.Play();

        gameObject.SetActive(false);
        endGamePanel.SetActive(true);
    }

    public bool SpendFuel(float amount)
    {
        if(amount == 0)
            return true;
        else if(amount > currentFuel || currentFuel == 0)
            return false;
        
        currentFuel = Mathf.Clamp(currentFuel - amount, 0, maxFuel);
        onFuelChanged?.Invoke(currentFuel);

        return true;
    }

    public void Refuel(float amount)
    {
        if(amount == 0)
            return;

        if(alarmSource.isPlaying)
            alarmSource.Stop();

        if(enginePlume.isStopped)
            enginePlume.Play();

        fuelSource.Play();
        float oldFuel = currentFuel;
        currentFuel = Mathf.Clamp(currentFuel + amount, 0, maxFuel);

        if(oldFuel != currentFuel)
            onFuelChanged?.Invoke(currentFuel);
    }

    public bool SpendCharge(float amount)
    {
        if(amount == 0)
            return true;
        else if(amount > currentCharge || currentCharge == 0)
            return false;
        
        currentCharge = Mathf.Clamp(currentCharge - amount, 0, maxCharge);

        if(currentCharge == 0)
            batterySource.PlayOneShot(outOfEnergy);

        onBatteryChanged?.Invoke(currentCharge);
        
        return true;
    }

    public void ChargeBattery(float chargeAmount)
    {
        if(chargeAmount == 0)
            return;

        if(currentCharge == 0)
            batterySource.PlayOneShot(chargeFromEmpty);
        else
            batterySource.PlayOneShot(chargeClip);
        
        float oldCharge = currentCharge;
        currentCharge = Mathf.Clamp(currentCharge + chargeAmount, 0, maxCharge);

        if(oldCharge != currentCharge)
            onBatteryChanged?.Invoke(currentCharge);
    }

    public void ActivatePowerup(CallbackContext context)
    {
        if(!context.performed || mostRecentlyPickedUpPowerup == PowerupType.None)
            return;

        if(!SpendCharge(10))
            return;

        powerupSource.PlayOneShot(activatePowerupClip);
        PowerupType toUse = mostRecentlyPickedUpPowerup;
        mostRecentlyPickedUpPowerup = PowerupType.None;

        if(activePowerups.ContainsKey(toUse))
            activePowerups[toUse] = Time.timeSinceLevelLoad + powerupDuration;
        else
            activePowerups.Add(toUse, Time.timeSinceLevelLoad + powerupDuration);

        onActivatePowerup?.Invoke(toUse);
    }
    public void CollectPowerup(PowerupType type)
    {
        powerupSource.PlayOneShot(pickupPowerupClip);
        mostRecentlyPickedUpPowerup = type;
        onPickedUpPowerup?.Invoke(mostRecentlyPickedUpPowerup);
    }
}