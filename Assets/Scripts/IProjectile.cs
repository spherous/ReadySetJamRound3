using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IProjectile
{
    float lifetime {get; set;}
    float speed {get; set;}
    Rigidbody2D body{get; set;}
    void Fire(Transform owner, float damage, bool isPlayer = false, bool isJuiced = false);
    void Collide();
}
