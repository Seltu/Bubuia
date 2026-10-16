using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private InputActionReference _interactAction;
    [SerializeField] private float _interactionRadius;
    private GameObject _selectedInteractable;
    private bool _hasSelection = false; // Equivale a _selectedInteractable != null, mas comparação com nulo é mais pesado no processamento do que comparar um bool.


    private void Awake()
    {
        _interactAction.action.performed += Interact;
    }

    private void Interact(InputAction.CallbackContext context)
    {
        if (!isActiveAndEnabled) return;
        if (_hasSelection)
            EventManager.TriggerEvent("CheckInteract", _selectedInteractable); // Evento de clicar E pra interagir com um objeto
    }

    private void OnDisable()
    {
        _hasSelection = false;
        EventManager.TriggerEvent<GameObject>("InteractorSelect", null);
    }

    private void Update()
    {
        var aroundPlayer =
                Physics.OverlapSphere(
                    transform.position,
                    _interactionRadius
                );
        bool foundInteractable = false;  // flag que guarda se o raycast achou uma interactable
        foreach (var interactable in aroundPlayer)
        {
            if (!interactable.CompareTag("Interactable")) continue; // Ele usa a tag pra saber se o objeto é um interactable ou não.
            foundInteractable = true;
            if (_hasSelection)
            {
                if (interactable.gameObject == _selectedInteractable) continue;
                if(!(Vector3.Distance(interactable.transform.position, transform.position) < Vector3.Distance(_selectedInteractable.transform.position, transform.position))) continue;
                _hasSelection = true;
                _selectedInteractable = interactable.gameObject;
                EventManager.TriggerEvent("InteractorSelect", _selectedInteractable);
                // Evento que avisa qual interactable está sendo selecionado (geralmente usado pra feedback visual)
                // (selecionado: jogador se aproximou do interactable, e ele é o mais próximo dos interactables dentro do raio de interação)
            }
            else
            {
                _selectedInteractable = interactable.gameObject;
                _hasSelection = true;
                EventManager.TriggerEvent("InteractorSelect", _selectedInteractable);
            }
        }
        if (!foundInteractable) // Se o raycast não encontrou nenhum interactable
            if (_hasSelection) // Se ele tiver uma seleção ele deseleciona ela, e lança o evento pra avisar
            {
                _selectedInteractable = null;
                _hasSelection = false;
                EventManager.TriggerEvent<GameObject>("InteractorSelect", null);
            }
    }
}
