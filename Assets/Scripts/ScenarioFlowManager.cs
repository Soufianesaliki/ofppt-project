using UnityEngine;
using ElectricalWorkshop.UI;

namespace ElectricalWorkshop.Components
{
    /// <summary>
    /// Sequences the two scenarios: Scenario 1 (motor batch reveal) then Scenario 2, which
    /// itself has two phases — wiring, then activation (motor on/off simulation).
    ///
    /// Subscribes to onScenario1Complete / onWiringPhaseComplete from code — leave those
    /// events empty in the Inspector, otherwise the handoff would fire twice.
    ///
    /// For now the handoff is immediate and only logs to the Console; the "move to the next
    /// bench" step can be gated here later.
    /// </summary>
    public class ScenarioFlowManager : MonoBehaviour
    {
        [Header("Scenario 1")]
        [SerializeField] private MotorAnimationManager _motorAnimationManager;
        [SerializeField] private ControllerInputController _controllerInput;

        [Tooltip("Active at start, deactivated when Scenario 2 begins. Leave empty to keep the first bench visible.")]
        [SerializeField] private GameObject[] _scenario1Objects;

        [Header("Scenario 2")]
        [SerializeField] private WiringSequenceManager _wiringSequenceManager;
        [SerializeField] private ActivationPhaseManager _activationPhaseManager;

        [Tooltip("Inactive at start, activated when Scenario 2 begins.")]
        [SerializeField] private GameObject[] _scenario2Objects;

        private void OnEnable()
        {
            if (_motorAnimationManager != null)
                _motorAnimationManager.onScenario1Complete.AddListener(StartScenario2);

            if (_wiringSequenceManager != null)
                _wiringSequenceManager.onWiringPhaseComplete.AddListener(OnWiringPhaseComplete);
        }

        private void OnDisable()
        {
            if (_motorAnimationManager != null)
                _motorAnimationManager.onScenario1Complete.RemoveListener(StartScenario2);

            if (_wiringSequenceManager != null)
                _wiringSequenceManager.onWiringPhaseComplete.RemoveListener(OnWiringPhaseComplete);
        }

        private void Start()
        {
            SetGroupActive(_scenario1Objects, true);
            SetGroupActive(_scenario2Objects, false);
            Debug.Log("ScenarioFlowManager: Scenario 1 started.");
        }

        private void StartScenario2()
        {
            Debug.Log("ScenarioFlowManager: Scenario 1 complete, starting Scenario 2.");

            if (_controllerInput != null)
                _controllerInput.SetNextBatchInputEnabled(false);

            SetGroupActive(_scenario1Objects, false);
            SetGroupActive(_scenario2Objects, true); // activate before BeginSequence so Awake has run

            if (_wiringSequenceManager != null)
                _wiringSequenceManager.BeginSequence();
        }

        private void OnWiringPhaseComplete()
        {
            Debug.Log("ScenarioFlowManager: Wiring phase complete, starting activation phase.");

            if (_activationPhaseManager != null)
                _activationPhaseManager.BeginActivationPhase();
        }

        private static void SetGroupActive(GameObject[] group, bool active)
        {
            if (group == null) return;

            foreach (GameObject go in group)
            {
                if (go != null) go.SetActive(active);
            }
        }
    }
}
