using UnityEngine;
using UnityEngine.InputSystem;

namespace ElectricalWorkshop.UI
{
    public class ControllerInputController : MonoBehaviour
    {
        [SerializeField] private AppController _appController;
        [SerializeField] private InputActionReference _menuButtonAction;   // X
        [SerializeField] private InputActionReference _replayVoiceAction;  // A

        private void OnEnable()
        {
            _menuButtonAction.action.Enable();
            _replayVoiceAction.action.Enable();
            _menuButtonAction.action.performed += OnMenuButtonPerformed;
            _replayVoiceAction.action.performed += OnReplayVoicePerformed;
        }

        private void OnDisable()
        {
            _menuButtonAction.action.performed -= OnMenuButtonPerformed;
            _replayVoiceAction.action.performed -= OnReplayVoicePerformed;
        }

        private void OnMenuButtonPerformed(InputAction.CallbackContext ctx) => _appController.TogglePauseMenu();
        private void OnReplayVoicePerformed(InputAction.CallbackContext ctx) => _appController.ReplayVoice();
    }
}