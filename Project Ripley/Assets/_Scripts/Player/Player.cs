﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerState
{
    Idle,
    Walking,
    Dashing,
    Attacking,
    PickingUp,
    Dropping,
    Damaged,
    Dead,
    Count
}

[System.Serializable]
struct Health
{
    [SerializeField] int myHealth;
    [SerializeField] int myMaxHealth;
    [SerializeField] bool myIsDead;

    public void AddHealth(int aAmountToAdd)
    {
        myHealth += aAmountToAdd;
        myHealth = Mathf.Clamp(myHealth, 0, myMaxHealth);

        if (IsDead())
        {
            myIsDead = true;
            Player.Instance.UpdateStateTo(PlayerState.Dead);
            GameManager.Instance.FadeOut();
        }
    }

    public void SetHealth(int aAmountToAdd)
    {
        myHealth = aAmountToAdd;
        myHealth = Mathf.Clamp(myHealth, 0, myMaxHealth);
    }

    public void Reset()
    {
        myHealth = myMaxHealth;
        myIsDead = false;
    }

    public bool IsDead()
    {
        return myHealth <= 0;
    }
}

public class Player : MonoBehaviour
{
    public static Player Instance;
    [SerializeField] private Health myHealth;
    [SerializeField] private PlayerState myPlayerState;

    [HideInInspector] public Inventory inventory;
    [HideInInspector] public Equipment equipment;
    [HideInInspector] public InteractReceiver interact;
    private Animator myAnim;
    Vector3 mySpawnPosition;
    float myDeathTimer = 0;
    bool myResetPlayer = false;
    [SerializeField] float myDeathRate;

    void Awake()
    {
        if (Instance == null)
        {
            //DontDestroyOnLoad(gameObject);
            Instance = this;
        }

        ScreenTransition.OnScreenFadedIn += ActivatePlayer;

        inventory = GetComponent<Inventory>();
        equipment = GetComponent<Equipment>();
        interact = GetComponent<InteractReceiver>();

        myAnim = GetComponentInChildren<Animator>();

        //GameData.OnSavePlayer += OnSave;
        //GameData.OnLoadPlayer += OnLoad;
    }

    void ActivatePlayer()
    {
        myResetPlayer = true;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            myHealth.AddHealth(-1);
        }
        else if (Input.GetKeyDown(KeyCode.G))
        {
            myHealth.AddHealth(1);
        }

        if(myHealth.IsDead() && myResetPlayer)
        {
            myDeathTimer += Time.deltaTime;

            if(myDeathTimer >= myDeathRate)
            {
                myDeathTimer = 0;
                myHealth.Reset();
                myAnim.SetBool("Dead", false);
                transform.position = mySpawnPosition;
                myPlayerState = PlayerState.Idle;
                GameManager.Instance.FadeIn();
                myResetPlayer = false;
            }
        }
    }

    public void SetPlayerState(PlayerState aPlayerState)
    {
        myPlayerState = aPlayerState;
    }

    public PlayerState GetPlayerState()
    {
        return myPlayerState;
    }

    public bool CanChangeState(PlayerState aState)
    {
        return CheckState(aState);
    }

    public void UpdateStateTo(PlayerState aState)
    {
        bool canUpdate = CheckState(aState);

        if (canUpdate)
        {
            SetPlayerState(aState);
        }
    }

    public PlayerState GetState()
    {
        return myPlayerState;
    }

    bool CheckState(PlayerState aState)
    {
        bool aCan = true;
        switch (aState)
        {
            case PlayerState.Idle:
                if (myPlayerState == PlayerState.Dead)
                {
                    aCan = false;
                }
                break;
            case PlayerState.Walking:
                if (myPlayerState == PlayerState.Dashing || myPlayerState == PlayerState.Attacking || myPlayerState == PlayerState.Damaged || myPlayerState == PlayerState.PickingUp || myPlayerState == PlayerState.Dropping || myPlayerState == PlayerState.Dead)
                {
                    aCan = false;
                }
                break;
            case PlayerState.Dashing:
                if (myPlayerState == PlayerState.Attacking || myPlayerState == PlayerState.Damaged || myPlayerState == PlayerState.PickingUp || myPlayerState == PlayerState.Dropping || myPlayerState == PlayerState.Dead)
                {
                    aCan = false;
                }
                break;
            case PlayerState.Attacking:
                if (myPlayerState == PlayerState.Dashing || myPlayerState == PlayerState.Damaged || myPlayerState == PlayerState.PickingUp || myPlayerState == PlayerState.Dropping || myPlayerState == PlayerState.Dead)
                {
                    aCan = false;
                }
                break;
            case PlayerState.PickingUp:
                if (myPlayerState == PlayerState.Dashing || myPlayerState == PlayerState.Attacking || myPlayerState == PlayerState.Dropping || myPlayerState == PlayerState.Damaged || myPlayerState == PlayerState.Dead)
                {
                    aCan = false;
                }
                break;
            case PlayerState.Dropping:
                if (myPlayerState == PlayerState.Dashing || myPlayerState == PlayerState.Attacking || myPlayerState == PlayerState.PickingUp || myPlayerState == PlayerState.Damaged || myPlayerState == PlayerState.Dead)
                {
                    aCan = false;
                }
                break;
        }

        return aCan;
    }
}