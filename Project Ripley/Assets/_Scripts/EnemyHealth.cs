﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] float health;
    [SerializeField] float startHealth;
    [SerializeField] float maxHealth;
    [SerializeField] float destroyLength;
    bool isDead = false;
    bool secondaryDead = false;
    EnemyEvent eE;
    EnemyAttack eA;
    AIPath aPath;
    CircleCollider2D circle;

    void Awake()
    {
        health = startHealth;
        eE = GetComponent<EnemyEvent>();
        eA = GetComponent<EnemyAttack>();
        aPath = GetComponent<AIPath>();
        circle = GetComponent<CircleCollider2D>();
    }

    void Update()
    {
        if(!isDead)
        {
            if (health <= 0)
            {
                health = 0;
                isDead = true;
                secondaryDead = true;
            }
            else if (health >= maxHealth)
            {
                health = maxHealth;
            }
        }
        else if(isDead)
        {
            eE.enabled = false;
            eA.enabled = false;
            aPath.enabled = false;
            circle.isTrigger = true;
            Destroy(gameObject, destroyLength);
        }
    }

    public void DecreaseHealthWith(float value)
    {
        health -= value;
    }
    public bool IsDead()
    {
        bool newDead = secondaryDead;
        secondaryDead = false;
        return newDead;
    }
}
