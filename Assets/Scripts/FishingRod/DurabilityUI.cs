using UnityEngine;

public class DurabilityUI : MonoBehaviour
{
    [SerializeField] private Animator _iconAnimator;
    public void PlayDestruction()
    {
        _iconAnimator.Play("DurabilityBreak");
    }

    public void DestroyIcon()
    {
        Destroy(gameObject);
    }
}
