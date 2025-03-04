﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    MovementDatabase movementDatabase;
    [SerializeField] float followMouseRate;
    [SerializeField] float clickRate;

    float mouseTrajectoryLength = 15;
    float followMouseTimer = -1;

    private float attackTimer = 0;
    private float attackRate = 1;
    private float consumeTimer = 0;
    private float clickTimer = 0;

    bool stopMoving = false;

    public bool hasAttacked = false;

    PlayerMovement playerMovement;
    Animator myAnim;

    [SerializeField] bool followMouse = false;

    [SerializeField] float attackCombo = 0;
    [SerializeField] float maxCombos = 3;
    float comboWaitingTimer = 0;
    [SerializeField] float comboWaitingLength;

    void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        myAnim = GetComponent<Animator>();
        movementDatabase = GetComponent<PlayersMovementData>().movementDatabaseSO;
    }

    void Update()
    {
        MouseDatabase.UpdateMousePosition();

        //AnimationController();
        Action();
    }

    void AnimationController()
    {
        int x = (int)MouseDatabase.CalculateDirectionNonDisplay(MouseDatabase.mousePosition, transform).x;
        int y = (int)MouseDatabase.CalculateDirectionNonDisplay(MouseDatabase.mousePosition, transform).y;

        float value = myAnim.GetFloat("ItemAttackID");

        if (value > 8 && value < 13) //Melee Weapons
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (attackTimer == 0)
                {
                    myAnim.SetBool("Click Attack", true);
                    followMouse = true;
                    playerMovement.StopMoving = true;
                    //stopMoving = true;
                }
            }

            if (myAnim.GetBool("Click Attack") == true)
            {
                attackTimer += Time.deltaTime;
                if (attackTimer > myAnim.GetCurrentAnimatorStateInfo(0).length)
                {
                    myAnim.SetBool("Click Attack", false);
                    playerMovement.StopMoving = false;
                    //stopMoving = false;
                    attackTimer = 0;
                }
            }
        }
        else if (value > 4 && value < 9) //Range Weapons
        {
            if (Input.GetMouseButton(0))
            {
                myAnim.SetBool("Hold Attack", true);
            }

            if (myAnim.GetBool("Hold Attack") == true)
            {
                followMouse = true;
                playerMovement.StopMoving = true;
            }
            if (Input.GetMouseButtonUp(0))
            {
                myAnim.SetBool("Hold Attack", false);
                playerMovement.StopMoving = false;
            }
        }
        else if (value > 0 && value < 5) //Consumable
        {
            if (Input.GetMouseButton(0))
            {
                if (consumeTimer == 0)
                {
                    myAnim.SetBool("Hold Attack", true);
                }
            }

            if (myAnim.GetBool("Hold Attack") == true)
            {
                playerMovement.StopMoving = true;
                //stopMoving = true;
                myAnim.speed = (GetComponent<PlayerInventory>().myPrimary.GetComponent<ConsumableItemManager>().consumeTimeRate / 2) *
                    (0.1f * GetComponent<PlayerInventory>().myPrimary.GetComponent<ConsumableItemManager>().consumeTimeRate);

                consumeTimer += Time.deltaTime;

                if (consumeTimer > myAnim.GetCurrentAnimatorStateInfo(0).length && myAnim.GetCurrentAnimatorStateInfo(0).IsName("Hold Attack") == true)
                {
                    myAnim.SetBool("Hold Attack", false);

                    playerMovement.StopMoving = false;
                    //stopMoving = false;
                    consumeTimer = 0;
                    myAnim.speed = 1;
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                myAnim.SetBool("Hold Attack", false);
                consumeTimer = 0;
                playerMovement.StopMoving = false;
                //stopMoving = false;
                myAnim.speed = 1;
            }
        }
        else
        {
            myAnim.SetBool("Hold Attack", false);
            myAnim.SetBool("Click Attack", false);
            playerMovement.StopMoving = false;
            //stopMoving = false;
        }

        if (followMouse == true)
        {
            movementDatabase.SetAnim(new Vector2(x, y));
            //animHorizontal = (int)CalculateDirectionNonDisplay(mousePosition).x;
            //animVertical = (int)CalculateDirectionNonDisplay(mousePosition).y;

            followMouseTimer = 0;
            followMouse = false;
        }

        if (followMouseTimer >= 0 && movementDatabase.GetMoving() == 0)
        {
            followMouseTimer += Time.deltaTime;
            if (followMouseTimer > followMouseRate)
            {
                followMouseTimer = -1;
            }
            else
            {
                if (playerMovement.StopMoving == false) // || stopMoving == false
                {
                    //Debug.Log("Count");
                    movementDatabase.SetAnim(new Vector2(x, y));
                    //animHorizontal = (int)CalculateDirectionNonDisplay(mousePosition).x;
                    //animVertical = (int)CalculateDirectionNonDisplay(mousePosition).y;
                }
            }
        }
    }

    void Action()
    {
        int selectedIndex = (int)Equipment.Instance.SelectedEQ;

        int itemIndex = -1;
        if (selectedIndex == 0)
            itemIndex = Equipment.Instance.Primary;
        else
            itemIndex = Equipment.Instance.Secondary;

        GameObject item = Inventory.Instance?.GetInventorySlot(itemIndex);

        //ItemInfo info = item?.GetComponent<ItemInfo>();

        //if (!GetComponent<PlayerDash>().HasDashed && info?.noDurability == false)
        //{
        //    if (info?.typeOfItem == ItemInfo.TypeOfItem.Melee)
        //    {
        //        Melee(info);
        //    }
        //    else if(info?.typeOfItem == ItemInfo.TypeOfItem.Range)
        //    {
        //        Range(info);
        //    }
        //}

        //if (info?.noDurability == true)
        //{
        //    Inventory.Instance.DestroyItem(itemIndex);

        //    attackCombo = 0;
        //    comboWaitingTimer = 0;
        //    myAnim.SetBool("ExitRecovery", true);
        //    playerMovement.StopMoving = false;
        //    GetComponent<PlayerDash>().enabled = true;
        //}

        CharacterFollowMouse();
    }

    void Melee(ItemInfo info)
    {
        if (Input.GetMouseButtonDown(0) && !myAnim.GetCurrentAnimatorStateInfo(0).IsName("Click Attacks") && !myAnim.GetCurrentAnimatorStateInfo(0).IsName("Recover")) //When We Get Input
        {
            myAnim.SetFloat("ItemAttackID", info.GetAnimationID());
            myAnim.Play("Click Attacks");
            attackCombo += 1;
            myAnim.SetFloat("AttackCombo", attackCombo);
            myAnim.SetBool("ExitRecovery", false);

            comboWaitingTimer = 0;

            playerMovement.StopMoving = true;
            followMouse = true;
            GetComponent<PlayerDash>().enabled = false;
        }

        //if(myAnim.GetCurrentAnimatorStateInfo(0).IsName("Attack Before Recovery"))
        //{
        //    if (Input.GetMouseButtonDown(0))
        //    {
        //        myAnim.Play("Click Attacks");
        //        attackCombo += 1;
        //        myAnim.SetFloat("AttackCombo", attackCombo);

        //        comboWaitingTimer = 0;
        //        return;
        //    }
        //}

        if (myAnim.GetCurrentAnimatorStateInfo(0).IsName("Recover"))
        {
            //attackCombo = 0;
            //comboWaitingTimer = 0;

            comboWaitingTimer += Time.deltaTime;

            if(comboWaitingTimer < comboWaitingLength && attackCombo < 3)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    myAnim.Play("Click Attacks");
                    attackCombo += 1;
                    myAnim.SetFloat("AttackCombo", attackCombo);
                    comboWaitingTimer = 0;
                }
            }
            else
            {
                attackCombo = 0;
                comboWaitingTimer = 0;
                myAnim.SetBool("ExitRecovery", true);
                playerMovement.StopMoving = false;
                GetComponent<PlayerDash>().enabled = true;
            }
        }

        //if (attackCombo > 0)
        //{
        //    comboWaitingTimer += Time.deltaTime;

        //    if (comboWaitingTimer > comboWaitingLength || attackCombo >= 4)
        //    {
        //        attackCombo = 0;
        //        comboWaitingTimer = 0;
        //    }
        //}
    }

    void Range(ItemInfo info)
    {
        //if (Input.GetMouseButton(0) && !myAnim.GetCurrentAnimatorStateInfo(0).IsName("Hold & Click"))
        //{
        //    myAnim.SetFloat("ItemAttackID", info.GetAnimationID());
        //    myAnim.Play("Hold & Click");
        //    followMouse = true;
        //    playerMovement.StopMoving = true;
        //}

        if(Input.GetMouseButton(0))
        {
            followMouse = true;
            playerMovement.StopMoving = true;
            GetComponent<PlayerDash>().enabled = false;

            if (!myAnim.GetCurrentAnimatorStateInfo(0).IsName("Hold & Click"))
            {
                myAnim.SetFloat("ItemAttackID", info.GetAnimationID());
                myAnim.Play("Hold & Click");
            }
        }

        if(myAnim.GetCurrentAnimatorStateInfo(0).IsName("Recover") && Input.GetMouseButtonUp(0) || !myAnim.GetCurrentAnimatorStateInfo(0).IsName("Recover") && !myAnim.GetCurrentAnimatorStateInfo(0).IsName("Hold & Click"))
        {
            playerMovement.StopMoving = false;
            GetComponent<PlayerDash>().enabled = true;
            //Debug.Log("HEGJ");

        }
           

    }

    void CharacterFollowMouse()
    {
        int x = (int)MouseDatabase.CalculateDirectionNonDisplay(MouseDatabase.mousePosition, transform).x;
        int y = (int)MouseDatabase.CalculateDirectionNonDisplay(MouseDatabase.mousePosition, transform).y;

        if (followMouse == true)
        {
            movementDatabase.SetAnim(new Vector2(x, y));

            followMouseTimer = 0;
            followMouse = false;
        }

        if (followMouseTimer >= 0 && movementDatabase.GetMoving() == 0)
        {
            followMouseTimer += Time.deltaTime;
            if (followMouseTimer > followMouseRate)
            {
                followMouseTimer = -1;
            }
            else if (playerMovement.StopMoving == false)
            {
                movementDatabase.SetAnim(new Vector2(x, y));
            }
        }
    }
}