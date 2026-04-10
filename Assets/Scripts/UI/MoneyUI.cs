using DG.Tweening;
using TMPro;
using UnityEngine;

public class MoneyUI : MonoBehaviour
{
    [SerializeField] private PlayerInventorySO m_Inventory;
    [SerializeField] private TMP_Text _moneyTxt;
    [SerializeField] private Transform _moneyTxtTransform;

    private void Start()
    {
        EventManager.AddListener("OnUpdateMoneyUI", OnUpdateMoneyTxt);

        OnUpdateMoneyTxt();
    }

    private void OnDestroy()
    {
        EventManager.RemoveListener("OnUpdateMoneyUI", OnUpdateMoneyTxt);
    }

    private void OnUpdateMoneyTxt()
    {
        _moneyTxt.SetText(m_Inventory.playerMoney.ToString());

        _moneyTxtTransform.DOKill();
        _moneyTxtTransform.DOScale(1.3f, 1f).SetEase(Ease.OutCubic).SetUpdate(true).OnComplete(() =>
        {
            _moneyTxtTransform.DOScale(1f, 1f).SetEase(Ease.OutCubic).SetUpdate(true);
        });
    }
}
