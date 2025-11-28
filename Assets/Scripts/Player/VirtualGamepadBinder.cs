using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

public class VirtualGamepadBinder : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    private static bool boundGamepad;

    private void Start()
    {
        if (boundGamepad) return;
        if (!playerInput)
            playerInput = GetComponent<PlayerInput>();

        InputSystem.onAfterUpdate += Setup;
    }

    private void Setup()
    {
        InputSystem.onAfterUpdate -= Setup;

        UnityEngine.InputSystem.EnhancedTouch.EnhancedTouchSupport.Enable();

        var onScreenGamepad = InputSystem.devices
            .FirstOrDefault(d => d is Gamepad && d.description.interfaceName == "OnScreen");

        if (onScreenGamepad == null)
            return;

        if (!playerInput.user.pairedDevices.Contains(onScreenGamepad))
        {
            InputUser.PerformPairingWithDevice(onScreenGamepad, playerInput.user);
        }

        playerInput.SwitchCurrentControlScheme(
            "DefaultScheme",
            playerInput.user.pairedDevices.ToArray()
        );

        boundGamepad = true;

        Debug.Log("Paired virtual gamepad: " + onScreenGamepad);
    }
}
