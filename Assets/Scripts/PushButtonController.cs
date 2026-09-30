using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace ElectricalWorkshop.Components
{
    /// <summary>
    /// Controls a virtual push-button ("bouton poussoir") component for the electrical
    /// workshop simulation. Exposes isPushed / color / isSnaped for the scene manager to read.
    ///
    /// Expected prefab setup:
    /// - Root GameObject: an XRGrabInteractable, used to pick up and place the button into its
    ///   slot on the DIN rail / control panel. Assign the matching XRSocketInteractor to
    ///   _correctSocket so only that specific slot can set isSnaped to true.
    /// - A child GameObject (the button's physical cap) holding an XRSimpleInteractable that the
    ///   learner presses — either by poking it directly (XRPokeInteractor + Poke Filter, which
    ///   raises selectEntered) or by pointing at it with a controller and pulling its trigger
    ///   (which raises activated, since XRI's defaults bind Select to Grip and Activate to
    ///   Trigger). Both events are listened to here and treated as the same press.
    /// - _targetRenderer: the Renderer whose material colour should follow the `color` property.
    /// </summary>
    [DisallowMultipleComponent]
    public class PushButtonController : MonoBehaviour, ISnappable
    {
        [System.Serializable]
        public class BoolUnityEvent : UnityEvent<bool> { }

        [Header("Interaction sources")]
        [Tooltip("Grabbed to place this button into its socket on the panel/DIN rail.")]
        [SerializeField] private XRGrabInteractable _grabInteractable;

        [Tooltip("The pressable cap. Can be poked (finger) or selected via a controller trigger.")]
        [SerializeField] private XRSimpleInteractable _pushInteractable;

        [Tooltip("The one XRSocketInteractor this button must be placed into for isSnaped to be true.")]
        [SerializeField] private XRSocketInteractor _correctSocket;

        [Header("Visuals")]
        [SerializeField] private Renderer _targetRenderer;

        [Tooltip("Index into _targetRenderer.sharedMaterials to recolor. Use 0 if it only has one material.")]
        [SerializeField] private int _materialIndex = 0;

        [Tooltip("URP Lit / HDRP Lit: \"_BaseColor\". Built-in Standard: \"_Color\".")]
        [SerializeField] private string _colorPropertyName = "_BaseColor";

        [SerializeField] private Color _color = Color.red;

        [Header("Events (optional — for event-driven manager integration)")]
        public BoolUnityEvent onPushedChanged = new BoolUnityEvent();
        public BoolUnityEvent onSnappedChanged = new BoolUnityEvent();

        private bool _isPushed;
        private bool _isSnaped;
        private MaterialPropertyBlock _mpb;
        private int _colorPropertyId;

        /// <summary>True only while the button is physically held (pressed/poked). Momentary, not a toggle.</summary>
        public bool isPushed => _isPushed;

        /// <summary>True while this button is placed in its correct circuit slot.</summary>
        public bool isSnaped => _isSnaped;

        UnityEvent<bool> ISnappable.onSnappedChanged => onSnappedChanged;

        /// <summary>Button colour. Setting this updates the instance's material at runtime.</summary>
        public Color color
        {
            get => _color;
            set
            {
                if (_color == value) return;
                _color = value;
                ApplyColor();
            }
        }

        private void Awake()
        {
            if (_grabInteractable == null)
                _grabInteractable = GetComponent<XRGrabInteractable>();

            _colorPropertyId = Shader.PropertyToID(_colorPropertyName);
            _mpb = new MaterialPropertyBlock();
        }

        private void OnEnable()
        {
            if (_pushInteractable != null)
            {
                _pushInteractable.selectEntered.AddListener(OnPushSelectEntered);
                _pushInteractable.selectExited.AddListener(OnPushSelectExited);
                _pushInteractable.activated.AddListener(OnPushActivated);
                _pushInteractable.deactivated.AddListener(OnPushDeactivated);
            }

            if (_grabInteractable != null)
            {
                _grabInteractable.selectEntered.AddListener(OnGrabSelectEntered);
                _grabInteractable.selectExited.AddListener(OnGrabSelectExited);
            }

            ApplyColor();
        }

        private void OnDisable()
        {
            if (_pushInteractable != null)
            {
                _pushInteractable.selectEntered.RemoveListener(OnPushSelectEntered);
                _pushInteractable.selectExited.RemoveListener(OnPushSelectExited);
                _pushInteractable.activated.RemoveListener(OnPushActivated);
                _pushInteractable.deactivated.RemoveListener(OnPushDeactivated);
            }

            if (_grabInteractable != null)
            {
                _grabInteractable.selectEntered.RemoveListener(OnGrabSelectEntered);
                _grabInteractable.selectExited.RemoveListener(OnGrabSelectExited);
            }
        }

        // Poking fires selectEntered/selectExited (via the interactable's Poke Filter). A
        // controller pointing at the button and pulling/releasing its physical trigger fires
        // activated/deactivated instead — XRI's default input actions bind Select to Grip and
        // Activate to Trigger — so both paths are listened to and treated as the same
        // momentary press: isPushed is true only while physically held, false on release.
        private void OnPushSelectEntered(SelectEnterEventArgs args) => SetPushed(true);
        private void OnPushSelectExited(SelectExitEventArgs args) => SetPushed(false);
        private void OnPushActivated(ActivateEventArgs args) => SetPushed(true);
        private void OnPushDeactivated(DeactivateEventArgs args) => SetPushed(false);

        private void SetPushed(bool pushed)
        {
            if (_isPushed == pushed) return;

            _isPushed = pushed;
            Debug.Log($"Button Pressed: {_isPushed}");
            onPushedChanged.Invoke(_isPushed);
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

        private void ApplyColor()
        {
            if (_targetRenderer == null || _mpb == null) return;

            int index = Mathf.Clamp(_materialIndex, 0, _targetRenderer.sharedMaterials.Length - 1);

            _targetRenderer.GetPropertyBlock(_mpb, index);
            _mpb.SetColor(_colorPropertyId, _color);
            _targetRenderer.SetPropertyBlock(_mpb, index);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            _mpb ??= new MaterialPropertyBlock();
            _colorPropertyId = Shader.PropertyToID(_colorPropertyName);
            if (_targetRenderer != null)
                _materialIndex = Mathf.Clamp(_materialIndex, 0, Mathf.Max(0, _targetRenderer.sharedMaterials.Length - 1));
            ApplyColor();
        }
#endif
    }
}
