using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHealth
{
    float maxHealth {get; set;}
    float currentHealth {get; set;}
    bool isDead {get; set;}
    bool isDying {get; set;}
    void TakeDamage(float amount);
    void Die();
}