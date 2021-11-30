using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public abstract class Damageable : MonoBehaviour
{
    public float maxHealth;
    public float currentHealth;
    public void TakeDamage(float amount) 
    {
        currentHealth -= amount;
        if (currentHealth <= 0f) 
        {
            Death();
        }
    }
    public void Heal(float amount) 
    {
        currentHealth += amount;
        if (currentHealth > maxHealth) 
        {
            currentHealth = maxHealth;
        }
    }

    public abstract void Death();

}