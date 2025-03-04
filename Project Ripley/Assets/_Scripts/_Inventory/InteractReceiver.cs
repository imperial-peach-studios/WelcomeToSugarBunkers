﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractReceiver : MonoBehaviour
{
    GameObject closestObject;
    [SerializeField] float radius;
    [SerializeField] bool showRadius = false;
    [SerializeField] LayerMask layersToCollide;
    [SerializeField] string interactableLayerName, pickUpLayerName, lookLayerName;
    [SerializeField] KeyCode pickUpKey;

    GameObject previousObject;

    bool updateLootShow = false;
    bool previousLootShow = false;

    [SerializeField] float offsetY = 2f;
    bool call = false;

    public delegate void OnLoot(GameObject item, Vector3 position);
    public static OnLoot OnStartLoot;
    public static OnLoot OnUpdateLoot;
    public static OnLoot OnExitLoot;

    void Update()
    {
        RaycastHit2D[] circleHit = Physics2D.CircleCastAll(transform.position + new Vector3(0f, 0.5f, 0f), radius, Vector2.zero, 0f, layersToCollide);

        Vector3 newTransform = transform.position;
        float destination = Mathf.Infinity;

        foreach (RaycastHit2D r in circleHit)
        {
            Vector3 diff = r.transform.position - newTransform;
            float newDistance = diff.sqrMagnitude;
            if (destination > newDistance && newDistance >= 0)
            {
                destination = newDistance;
                if (previousObject != closestObject)
                {
                    call = true;
                    OnExitLoot.Invoke(null, Vector3.zero);
                }
                previousObject = closestObject;
                closestObject = r.transform.gameObject;
            }
        }
        
        if (closestObject != null)
        {
            if (Vector3.Distance(transform.position, closestObject.transform.position) < radius)
            {
                if (Input.GetKeyDown(pickUpKey))
                {
                    if (closestObject.layer == LayerMask.NameToLayer(interactableLayerName))
                    {
                        closestObject.GetComponent<DialogueWaitInput>().FungusMessage();
                    }
                    else if (closestObject.layer == LayerMask.NameToLayer(pickUpLayerName)
                        || closestObject.layer == LayerMask.NameToLayer(lookLayerName))
                    {
                        closestObject.GetComponent<PickUpGiver>().TryToAddItemToInventory();
                        previousLootShow = closestObject;
                        closestObject = null;
                        return;
                    }
                }

                if (closestObject.layer == LayerMask.NameToLayer(lookLayerName))
                {
                    Vector3 newPos = new Vector3(closestObject.transform.position.x, closestObject.transform.position.y + offsetY, closestObject.transform.position.z);

                    if (call)
                    {
                        OnStartLoot.Invoke(closestObject, newPos);
                        call = false;
                    }
                    OnUpdateLoot.Invoke(closestObject, newPos);
                }
            }
            else if (Vector3.Distance(transform.position, closestObject.transform.position) > radius &&
                        previousObject != null && previousObject.layer == LayerMask.NameToLayer(lookLayerName))
            {
                OnExitLoot.Invoke(closestObject, Vector3.zero);
            }
        }

        if (closestObject == null && previousObject != null)
        {
            if(previousObject.layer == 0)
            {
                OnExitLoot.Invoke(null, Vector3.zero);
                previousObject = null;
            }
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (showRadius)
        {
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}