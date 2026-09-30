using UnityEngine;
using UnityEngine.Events;

namespace ElectricalWorkshop.Components
{
    /// <summary>
    /// Sequences Scenario 2: reveals the snap zones (sockets) for the wiring components one
    /// at a time, in a fixed order, advancing only once the current step's component is
    /// snapped into its own correct socket. Sockets are typed — a wrong component physically
    /// can't fit — so this manager only needs to handle ordering, not validation.
    ///
    /// Unsnapping (removing a placed piece) does not rewind the sequence.
    /// </summary>
    public class WiringSequenceManager : MonoBehaviour
    {
        [System.Serializable]
        public class WiringStep
        {
            public string label;

            [Tooltip("Any component implementing ISnappable: PushButtonController, VoyantController, DisjonctorController, ContactorController or ThermalRelayController.")]
            public MonoBehaviour componentController;

            [Tooltip("Renderer(s) enabled while it's this step's turn, disabled once the step is complete. Toggling only the Renderer (not the GameObject) keeps the socket's own functionality — XRSocketInteractor, joints, etc. — untouched, so a piece already snapped in doesn't fall when its highlight is hidden.")]
            public Renderer[] socketRevealRenderers;

            public ISnappable Snappable => componentController as ISnappable;
        }

        [Header("Steps")]
        [Tooltip("The wiring steps, in the order they must be completed.")]
        [SerializeField] private WiringStep[] steps;

        [Header("Events")]
        [Tooltip("Invoked once the last step's component has been snapped in — i.e. the wiring phase is complete.")]
        public UnityEvent onWiringPhaseComplete = new UnityEvent();

        private int _currentStepIndex = -1;

        private int StepsCount => steps?.Length ?? 0;

        /// <summary>Index of the step currently awaiting a snap (-1 = not started, StepsCount = fully done).</summary>
        public int currentStepIndex => _currentStepIndex;

        private void Awake()
        {
            HideAllVisuals();
        }

        /// <summary>Starts (or restarts) the sequence from step 0. Called by ScenarioFlowManager.</summary>
        public void BeginSequence()
        {
            UnsubscribeCurrent();
            HideAllVisuals();

            _currentStepIndex = -1;
            AdvanceStep();
        }

        private void HideAllVisuals()
        {
            for (int i = 0; i < StepsCount; i++)
            {
                SetVisualActive(i, false);
            }
        }

        private void AdvanceStep()
        {
            _currentStepIndex++;

            if (_currentStepIndex >= StepsCount)
            {
                onWiringPhaseComplete.Invoke();
                return;
            }

            WiringStep step = steps[_currentStepIndex];
            SetVisualActive(_currentStepIndex, true);

            ISnappable snappable = step.Snappable;
            if (snappable == null)
            {
                Debug.LogWarning($"WiringSequenceManager: step '{step.label}' has no component implementing ISnappable assigned.");
                return;
            }

            snappable.onSnappedChanged.AddListener(OnCurrentStepSnappedChanged);
        }

        private void OnCurrentStepSnappedChanged(bool snapped)
        {
            if (!snapped) return; // ignore unsnap — removing a piece mid-task doesn't rewind

            steps[_currentStepIndex].Snappable?.onSnappedChanged.RemoveListener(OnCurrentStepSnappedChanged);
            SetVisualActive(_currentStepIndex, false);

            AdvanceStep();
        }

        private void UnsubscribeCurrent()
        {
            if (_currentStepIndex < 0 || _currentStepIndex >= StepsCount) return;

            steps[_currentStepIndex].Snappable?.onSnappedChanged.RemoveListener(OnCurrentStepSnappedChanged);
        }

        private void SetVisualActive(int index, bool active)
        {
            Renderer[] renderers = steps[index]?.socketRevealRenderers;
            if (renderers == null) return;

            foreach (Renderer r in renderers)
            {
                if (r != null)
                {
                    r.enabled = active;
                }
            }
        }
    }
}
