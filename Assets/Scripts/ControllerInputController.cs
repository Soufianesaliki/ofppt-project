using UnityEngine;
using UnityEngine.InputSystem;

namespace ElectricalWorkshop.UI
{
    public class ControllerInputController : MonoBehaviour
    {
        [SerializeField] private AppController _appController;
        [SerializeField] private MotorAnimationManager _motorAnimationManager;
        [SerializeField] private InputActionReference _menuButtonAction;   // X
        [SerializeField] private InputActionReference _replayVoiceAction;  // A
        [SerializeField] private InputActionReference _nextBatchAction;    // "Next" (B)

        private void OnEnable()
        {
            _menuButtonAction.action.Enable();
            _replayVoiceAction.action.Enable();
            _nextBatchAction.action.Enable();
            _menuButtonAction.action.performed += OnMenuButtonPerformed;
            _replayVoiceAction.action.performed += OnReplayVoicePerformed;
            _nextBatchAction.action.performed += OnNextBatchPerformed;
        }

        private void OnDisable()
        {
            _menuButtonAction.action.performed -= OnMenuButtonPerformed;
            _replayVoiceAction.action.performed -= OnReplayVoicePerformed;
            _nextBatchAction.action.performed -= OnNextBatchPerformed;
        }

        private bool _nextBatchEnabled = true;

        /// <summary>Enables/disables the Next (B) action's effect without touching the X/A actions.</summary>
        public void SetNextBatchInputEnabled(bool enabled) => _nextBatchEnabled = enabled;

        private void OnMenuButtonPerformed(InputAction.CallbackContext ctx) => _appController.TogglePauseMenu();
        private void OnReplayVoicePerformed(InputAction.CallbackContext ctx) => _appController.ReplayVoice();
        private void OnNextBatchPerformed(InputAction.CallbackContext ctx)
        {
            if (_nextBatchEnabled) _motorAnimationManager.AdvanceBatch();
        }
    }
}