using UnityEngine;

public class TutorialSpot : MonoBehaviour
{
    [SerializeField] private QuestSO _quest;
    [SerializeField] private LayerMask _bobberMask;

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Hook")
        {
            _quest.objectives[2].currentAmount += 1;
            gameObject.SetActive(false);
        }
    }
}

