using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField] private PlayerInventorySO _inventory;

    // PlayerPrefs keys
    private const string MONEY_KEY = "INV_MONEY";
    private static string BaitKey(BaitTypeSO bait) => $"INV_BAIT_{bait.name}";
    private static string FishKey(FishTypeSO fish) => $"INV_FISH_{fish.name}";

    private void Awake()
    {
        LoadInventory();
    }

    private void Start()
    {
        EventManager.AddListener<int>("OnAddToPlayerMoney", AddToPlayerMoney);
        EventManager.AddListener<BaitTypeSO, int>("OnAddToPlayerBaits", AddToPlayerBaits);
        EventManager.AddListener<FishTypeSO, int>("OnAddToPlayerFishes", AddToPlayerFishes);
    }

    private void OnDestroy()
    {
        EventManager.RemoveListener<int>("OnAddToPlayerMoney", AddToPlayerMoney);
        EventManager.RemoveListener<BaitTypeSO, int>("OnAddToPlayerBaits", AddToPlayerBaits);
        EventManager.RemoveListener<FishTypeSO, int>("OnAddToPlayerFishes", AddToPlayerFishes);
    }

    private void AddToPlayerMoney(int amount)
    {
        _inventory.playerMoney += amount;

        // saving to player prefs
        PlayerPrefs.SetInt(MONEY_KEY, _inventory.playerMoney);
        PlayerPrefs.Save();

        EventManager.TriggerEvent("OnUpdateMoneyUI");
    }

    private void AddToPlayerBaits(BaitTypeSO bait, int num)
    {
        for (int i = 0; i < _inventory.playerBaits.Length; i++)
        {
            if (_inventory.playerBaits[i].baitType == bait)
            {
                _inventory.playerBaits[i].baitNum += num;

                // saving to player prefs

                PlayerPrefs.SetInt(BaitKey(bait), _inventory.playerBaits[i].baitNum);
                PlayerPrefs.Save();
                return;
            }
        }
    }

    private void AddToPlayerFishes(FishTypeSO fish, int num)
    {
        for (int i = 0; i < _inventory.playerFishes.Length; i++)
        {
            if (_inventory.playerFishes[i].fishType == fish)
            {
                _inventory.playerFishes[i].fishNum += num;

                // saving to player prefs

                PlayerPrefs.SetInt(FishKey(fish), _inventory.playerFishes[i].fishNum);
                PlayerPrefs.Save();
                return;
            }
        }
    }

    private void LoadInventory()
    {
        // money
        _inventory.playerMoney = PlayerPrefs.GetInt(MONEY_KEY, _inventory.playerMoney);

        // baits (PlayerBait is struct, write back to array)
        for (int i = 0; i < _inventory.playerBaits.Length; i++)
        {
            var baitSO = _inventory.playerBaits[i].baitType;
            var pb = _inventory.playerBaits[i];
            pb.baitNum = PlayerPrefs.GetInt(BaitKey(baitSO), pb.baitNum);
            _inventory.playerBaits[i] = pb;
        }

        // fishes (PlayerFishes is class, can set directly)
        for (int i = 0; i < _inventory.playerFishes.Length; i++)
        {
            var fishSO = _inventory.playerFishes[i].fishType;
            _inventory.playerFishes[i].fishNum =
                PlayerPrefs.GetInt(FishKey(fishSO), _inventory.playerFishes[i].fishNum);
        }
    }
}
