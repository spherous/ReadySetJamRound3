using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void OnHealthChanged(float newHP);
public interface IHealth
{
    float maxHealth {get; set;}
    float currentHealth {get; set;}
    bool isDead {get; set;}
    bool isDying {get; set;}
    void HealToFull();
    void TakeDamage(float amount, Transform damgedBy);
    void Die();
    event OnHealthChanged onHealthChanged;
}