using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

public class VirtualGamepadBinder : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    private Gamepad _virtualGamepad;

    void Start()
    {
        if (playerInput == null)
            playerInput = GetComponent<PlayerInput>();

        // Garante que existe um gamepad virtual
        _virtualGamepad = Gamepad.current ?? InputSystem.AddDevice<Gamepad>();

        // Agora o user já é válido -> pode parear sem erro
        if (playerInput.user.valid)
        {
            playerInput.user.UnpairDevices();
            InputUser.PerformPairingWithDevice(_virtualGamepad, playerInput.user);
        }
    }
}
