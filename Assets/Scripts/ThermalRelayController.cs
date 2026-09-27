using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace ElectricalWorkshop.Components
{
    /// <summary>
    /// Controls a virtual thermal overload relay ("relais thermique") component for the
    /// electrical workshop simulation. Currently exposes only isSnaped for the scene manager
    /// to read; no functional state (e.g. isOn / isTripped) is wired up yet — this is
    /// grab/snap-to-socket support only, to be extended later once the relay's tripping
    /// behavior is specified.
    ///
    /// Expected prefab setup:
    /// - Root GameObject: an XRGrabInteractable, used to pick up and place the relay into its
    ///   slot on the DIN rail / control panel. Assign the matching XRSocketInteractor to
    ///   _correctSocket so only that specific slot can set isSnaped to true.
    /// </summary>
    [DisallowMultipleComponent]
    public class ThermalRelayController : MonoBehaviour
    {
        [System.Serializable]
        public class BoolUnityEvent : UnityEvent<bool> { }

        [Header("Interaction sources")]
        [Tooltip("Grabbed to place this thermal relay into its socket on the panel/DIN rail.")]
        [SerializeField] private XRGrabInteractable _grabInteractable;

        [Tooltip("The one XRSocketInteractor this relay must be placed into for isSnaped to be true.")]
        [SerializeField] private XRSocketInteractor _correctSocket;

        [Header("Events (optional — for event-driven manager integration)")]
        public BoolUnityEvent onSnappedChanged = new BoolUnityEvent();

        private bool _isSnaped;

        /// <summary>True while this thermal relay is placed in its correct circuit slot.</summary>
        public bool isSnaped => _isSnaped;

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
