using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventoryLoader : MonoBehaviour
{
    [SerializeField] private PlayerInventorySO _inventory;
    private void Awake()
    {
        _inventory.LoadInventory();
    }
}