using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace ElectricalWorkshop.Components
{
    /// <summary>
    /// Controls a virtual contactor ("contacteur") component for the electrical
    /// workshop simulation. Exposes isOn / isSnaped for the scene manager to read and set.
    ///
    /// Expected prefab setup:
    /// - Root GameObject: an XRGrabInteractable, used to pick up and place the contactor into
    ///   its slot on the DIN rail / control panel. Assign the matching XRSocketInteractor to
    ///   _correctSocket so only that specific slot can set isSnaped to true.
    ///
    /// Unlike PushButtonController and DisjonctorController, this component has no pressable
    /// or switchable child interactable — a real contactor is coil-actuated (closed by energizing
    /// its control coil, not manually flipped by the learner), so isOn is expected to be driven
    /// externally by the scene/circuit manager, same convention as VoyantController.isOn.
    /// </summary>
    [DisallowMultipleComponent]
    public class ContactorController : MonoBehaviour, ISnappable
    {
        [System.Serializable]
        public class BoolUnityEvent : UnityEvent<bool> { }

        [Header("Interaction sources")]
        [Tooltip("Grabbed to place this contactor into its socket on the panel/DIN rail.")]
        [SerializeField] private XRGrabInteractable _grabInteractable;

        [Tooltip("The one XRSocketInteractor this contactor must be placed into for isSnaped to be true.")]
        [SerializeField] private XRSocketInteractor _correctSocket;

        [Header("Events (optional — for event-driven manager integration)")]
        public BoolUnityEvent onStateChanged = new BoolUnityEvent();
        public BoolUnityEvent onSnappedChanged = new BoolUnityEvent();

        private bool _isOn;
        private bool _isSnaped;

        /// <summary>
        /// True while the contactor's coil is energized and its contacts are closed. Set
        /// externally by the scene/circuit manager — there is no learner-pressable interactable
        /// for this, since a real contactor is coil-actuated rather than manually flipped.
        /// </summary>
        public bool isOn
        {
            get => _isOn;
            set
            {
                if (_isOn == value) return;
                _isOn = value;
                Debug.Log($"Contactor isOn: {_isOn}");
                onStateChanged.Invoke(_isOn);
            }
        }

        /// <summary>True while this contactor is placed in its correct circuit slot.</summary>
        public bool isSnaped => _isSnaped;

        UnityEvent<bool> ISnappable.onSnappedChanged => onSnappedChanged;

        private void Awake()
        {
            if (_grabInteractable == null)
                _grabInteractable = GetComponent<XRGrabInteractable>();
        }

        private void OnEnable()
        {
            if (_grabInteractable != null)
            {
                _grabInteractable.selectEntered.AddListener(OnGrabSelectEntered);
                _grabInteractable.selectExited.AddListener(OnGrabSelectExited);
            }
        }

        private void OnDisable()
        {
            if (_grabInteractable != null)
            {
                _grabInteractable.selectEntered.RemoveListener(OnGrabSelectEntered);
                _grabInteractable.selectExited.RemoveListener(OnGrabSelectExited);
            }
        }

        private void OnGrabSelectEntered(SelectEnterEventArgs args)
        {
            if (_correctSocket == null || !ReferenceEquals(args.interactorObject, _correctSocket))
                return;

            _isSnaped = true;
            onSnappedChanged.Invoke(true);
        }

        private void OnGrabSelectExited(SelectExitEventArgs args)
        {
            if (_correctSocket == null || !ReferenceEquals(args.interactorObject, _correctSocket))
                return;

            _isSnaped = false;
            onSnappedChanged.Invoke(false);
        }
    }
}
