using UnityEngine;

namespace ElectricalWorkshop.Components
{
    /// <summary>
    /// Scenario 2's second phase: simulates powering the motor on/off once wiring is
    /// complete, driven by the disjoncteur, the MAR/ARR push buttons and the contactor.
    ///
    /// State model:
    ///   Idle    — wiring done, ARR-Voy on, contactor off, motor off.
    ///   Running — MAR-Voy on, contactor on, motor on.
    ///
    /// Idle -> Running: the disjonctor is on AND MAR is pressed. MAR pressed while the
    ///                  disjonctor is off is ignored, not remembered.
    /// Running -> Idle: the disjonctor flips off, OR ARR is pressed. Either resets to Idle
    ///                  (MAR-Voy off, ARR-Voy on, contactor off, motor off); the disjonctor
    ///                  flip additionally leaves the disjonctor off, while ARR leaves it
    ///                  untouched.
    ///
    /// Call BeginActivationPhase() once the wiring phase is complete (e.g. from
    /// WiringSequenceManager.onWiringPhaseComplete via ScenarioFlowManager).
    /// </summary>
    public class ActivationPhaseManager : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private DisjonctorController _disjonctor;
        [SerializeField] private ContactorController _contactor;
        [SerializeField] private VoyantController _marVoy;
        [SerializeField] private VoyantController _arrVoy;
        [SerializeField] private PushButtonController _marButton;
        [SerializeField] private PushButtonController _arrButton;
        [SerializeField] private MotorRunController _motor;

        private bool _isActive;
        private bool _isRunning;

        private void OnDisable()
        {
            if (!_isActive) return;

            if (_disjonctor != null) _disjonctor.onStateChanged.RemoveListener(OnDisjonctorChanged);
            if (_marButton != null) _marButton.onPushedChanged.RemoveListener(OnMarPushed);
            if (_arrButton != null) _arrButton.onPushedChanged.RemoveListener(OnArrPushed);
        }

        /// <summary>Call once the wiring phase is complete. Sets the idle state and starts listening.</summary>
        public void BeginActivationPhase()
        {
            if (_isActive) return;
            _isActive = true;

            GoIdle();

            if (_disjonctor != null) _disjonctor.onStateChanged.AddListener(OnDisjonctorChanged);
            if (_marButton != null) _marButton.onPushedChanged.AddListener(OnMarPushed);
            if (_arrButton != null) _arrButton.onPushedChanged.AddListener(OnArrPushed);
        }

        private void OnDisjonctorChanged(bool isOn)
        {
            if (!isOn && _isRunning)
            {
                GoIdle();
            }
            // Flipping the disjonctor on does not, by itself, start the motor — MAR
            // pressed earlier while it was off is ignored, not remembered.
        }

        private void OnMarPushed(bool pushed)
        {
            if (!pushed) return; // ignore release
            if (_isRunning) return;
            if (_disjonctor == null || !_disjonctor.isOn) return; // ignored while disjonctor is off

            GoRunning();
        }

        private void OnArrPushed(bool pushed)
        {
            if (!pushed) return; // ignore release
            if (!_isRunning) return;

            GoIdle();
        }

        private void GoRunning()
        {
            _isRunning = true;

            if (_contactor != null) _contactor.isOn = true;
            if (_marVoy != null) _marVoy.isOn = true;
            if (_arrVoy != null) _arrVoy.isOn = false;
            if (_motor != null) _motor.isOn = true;
        }

        private void GoIdle()
        {
            _isRunning = false;

            if (_contactor != null) _contactor.isOn = false;
            if (_marVoy != null) _marVoy.isOn = false;
            if (_arrVoy != null) _arrVoy.isOn = true;
            if (_motor != null) _motor.isOn = false;
        }
    }
}
