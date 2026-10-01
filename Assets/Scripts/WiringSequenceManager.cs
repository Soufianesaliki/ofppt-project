using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

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

            [Tooltip("This step's own socket. Kept inactive until this step's turn so only the required part can be snapped.")]
            public XRSocketInteractor socket;

            [System.NonSerialized] public XRSelectFilterDelegate lockFilter;

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

            for (int i = 0; i < StepsCount; i++)
            {
                SetSocketActive(i, false);
            }
        }

        /// <summary>Starts (or restarts) the sequence from step 0. Called by ScenarioFlowManager.</summary>
        public void BeginSequence()
        {
            UnsubscribeCurrent();
            HideAllVisuals();

            for (int i = 0; i < StepsCount; i++)
            {
                UnlockStep(i);
            }

            // Sockets are deliberately not re-deactivated here (Awake already did): setting
            // socketActive = false on a socket that already holds a piece is unverified.
            _currentStepIndex = -1;
            AdvanceStep(); // activates step 0's socket
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
            SetSocketActive(_currentStepIndex, true);

            ISnappable snappable = step.Snappable;
            if (snappable == null)
            {
                Debug.LogWarning($"WiringSequenceManager: step '{step.label}' has no component implementing ISnappable assigned.");
                return;
            }

            // Defensive: already snapped (e.g. pre-placed) -> no event will come, so complete now.
            if (snappable.isSnaped)
            {
                CompleteCurrentStep();
                return;
            }

            snappable.onSnappedChanged.AddListener(OnCurrentStepSnappedChanged);
        }

        private void OnCurrentStepSnappedChanged(bool snapped)
        {
            if (!snapped) return; // ignore unsnap — removing a piece mid-task doesn't rewind

            steps[_currentStepIndex].Snappable?.onSnappedChanged.RemoveListener(OnCurrentStepSnappedChanged);
            CompleteCurrentStep();
        }

        private void CompleteCurrentStep()
        {
            SetVisualActive(_currentStepIndex, false);
            LockStep(_currentStepIndex);

            AdvanceStep();
        }

        private void SetSocketActive(int index, bool active)
        {
            XRSocketInteractor s = steps[index]?.socket;
            if (s != null)
            {
                s.socketActive = active;
            }
        }

        /// <summary>Once placed, a part can only be selected by a socket — hands/rays are rejected.</summary>
        private void LockStep(int index)
        {
            WiringStep step = steps[index];
            if (step == null || step.lockFilter != null || step.componentController == null) return;

            XRGrabInteractable grab = step.componentController.GetComponent<XRGrabInteractable>();
            if (grab == null) return;

            step.lockFilter = new XRSelectFilterDelegate((interactor, interactable) => interactor is XRSocketInteractor);
            grab.selectFilters.Add(step.lockFilter);
        }

        private void UnlockStep(int index)
        {
            WiringStep step = steps[index];
            if (step == null || step.lockFilter == null) return;

            if (step.componentController != null)
            {
                XRGrabInteractable grab = step.componentController.GetComponent<XRGrabInteractable>();
                if (grab != null) grab.selectFilters.Remove(step.lockFilter);
            }

            step.lockFilter = null;
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
