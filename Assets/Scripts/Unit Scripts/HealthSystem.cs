using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Stores unit health
public class HealthSystem : MonoBehaviour
{
    [SerializeField] private int health = 10;
    private int maxHealth;

    public event EventHandler OnDeath;
    public event EventHandler OnHealthChanged;

    private void Awake()
    {
        maxHealth = health;
    }

    public void TakeDamage(int damage)
    {
        health -= damage;

        OnHealthChanged?.Invoke(this, EventArgs.Empty);

        if (health <= 0)
        {
            // Clamp health to 0
            health = 0;

            // Die
            Death();
        }
    }

    private void Death()
    {
        OnDeath?.Invoke(this, EventArgs.Empty);
    }

    public float GetHealthNormalized()
    {
        return (float)health / maxHealth; // cast to float for action weight calculations and similar (1 - health normalized)
    }
}
