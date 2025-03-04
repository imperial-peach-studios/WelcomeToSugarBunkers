﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance;
    public GameObject[] inventory = new GameObject[10];
    public bool IsInventoryFull => GetFirstEmptySlot() == -1;

    public event Action<int> OnInventoryChanged;

    public GameObject GetInventorySlot(int index) => inventory[index];

    [SerializeField] GameObject player;
    [SerializeField] Vector2 dropSize = new Vector2(0.6f, 0.6f);
    [SerializeField] LayerMask collideWithLayer;
    [SerializeField] GameObject pickUpObject;

    [SerializeField] List<GameObject> allItems = new List<GameObject>();

    void Awake()
    {
        inventory = new GameObject[10];

        if (Instance == null)
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        GameData.OnSavePlayer += OnSavePlayerInventory;
        GameData.OnLoadPlayer += OnLoadPlayerInventory;
    }

    void OnSavePlayerInventory()
    {
        List<string> itemNames = new List<string>();

        for(int i = 0; i < inventory.Length; i++)
        {
            if(inventory[i] != null)
            {
                itemNames.Add(inventory[i].name);
            }
            else
            {
                itemNames.Add("");
            }
        }

        GameData.aData.pData.SaveInventoriesData(itemNames);
    }
    void OnLoadPlayerInventory()
    {
        GameData.aData.pData.LoadInventoriesData(ref inventory, allItems);
        for(int i = 0; i < inventory.Length; i++)
        {
            if(inventory[i] != null)
            {
                //inventory[i].GetComponent<ItemInfo>().UpdateI();
            }
            OnInventoryChanged?.Invoke(i);
        }
    }

    public int GetFirstEmptySlot()
    {
        for (int i = 0; i < inventory.Length; i++)
        {
            if (inventory[i] == null)
            {
                return i;
            }
        }
        return -1;
    }

    public bool CanDrop()
    { //Can't Drop if Looting
        Collider2D[] hit = Physics2D.OverlapBoxAll(player.transform.position, dropSize, 0f, collideWithLayer);
        return hit.Length == 0;
    }

    public bool AddItem(GameObject pickUpItem)
    {
        int slot = GetFirstEmptySlot();
        pickUpItem.GetComponent<ItemInfo>().UpdateI();
        return AddItemToSlot(pickUpItem, slot);
    }

    public bool AddItemToSlot(GameObject pickUpItem, int slot)
    {
        if (slot <= -1)
            return false;

        if (inventory[slot] != null)
            return false;

        inventory[slot] = pickUpItem;
        OnInventoryChanged?.Invoke(slot);
        return true;
    }

    public void TryToRemoveItem(int slotIndex)
    {
        if (inventory[slotIndex] != null)
        {
            GameObject newPickUp = (Instantiate(pickUpObject, player.transform.position, Quaternion.identity) as GameObject);

            //newPickUp.GetComponent<InteractionGiver>().AddItem(inventory[slotIndex]);
            newPickUp.GetComponent<PickUpGiver>().StoreItem(inventory[slotIndex]);
            newPickUp.GetComponent<SpriteRenderer>().sprite = inventory[slotIndex].GetComponent<ItemInfo>().GetUISprite();

            inventory[slotIndex] = null;

            OnInventoryChanged?.Invoke(slotIndex);
        }
    }

    public void DestroyItem(int slotIndex)
    {
        inventory[slotIndex] = null;
        OnInventoryChanged?.Invoke(slotIndex);
    }
}