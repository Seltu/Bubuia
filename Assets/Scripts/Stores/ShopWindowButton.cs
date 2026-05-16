using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ShopWindowButton : MonoBehaviour
{
    [SerializeField] private Color _baseColor;
    [SerializeField] private Color _selectedColor;
    [SerializeField] private Image _buttonImg;
    [SerializeField] private GameObject _scrollList;

    private void OnEnable()
    {
        EventManager.AddListener<GameObject, GameObject>("ShowItemShoListing", ShowItensList);
    }

    private void OnDestroy()
    {
        EventManager.RemoveListener<GameObject, GameObject>("ShowItemShoListing", ShowItensList);
    }

    public void CallList()
    {
        EventManager.TriggerEvent("ShowItemShoListing", _scrollList, gameObject);
    }

    private void ShowItensList(GameObject list, GameObject button)
    {
        if (_scrollList != list)
            _scrollList.SetActive(false);
        else
            _scrollList.SetActive(true);

        if(gameObject != button)
            _buttonImg.color = _baseColor;
        else
            _buttonImg.color = _selectedColor;
    }
}
