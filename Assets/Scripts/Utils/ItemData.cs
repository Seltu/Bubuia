using UnityEngine;

public class ItemData : MonoBehaviour
{
    [SerializeField] private string _objectId;

    public string GetObjectId()
    {
        return _objectId;
    }
}
