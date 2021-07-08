using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserProjectile : MonoBehaviour, IProjectile
{
    public delegate void OnCollision();
    public OnCollision onCollision;
    private float dieAtTime;
    public float lifetime {get => _lifetime; set{_lifetime = value;}}
    [SerializeField] private float _lifetime;

    public float speed {get => _speed; set{_speed = value;}}
    [SerializeField] private float _speed;

    public Rigidbody2D body {get => _body; set{}}
    [SerializeField] private Rigidbody2D _body;


    private void Update() {
        if(Time.timeSinceLevelLoad >= dieAtTime)
        {
            Collide();
        }
    }

    public void Collide()
    {
        onCollision?.Invoke();
    }

    public void Fire()
    {
        dieAtTime = Time.timeSinceLevelLoad + lifetime;
        body.velocity = transform.up * speed;
    }
}