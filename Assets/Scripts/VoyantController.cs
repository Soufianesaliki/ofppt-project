using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace ElectricalWorkshop.Components
{
    /// <summary>
    /// Controls a virtual indicator light ("voyant") component for the electrical
    /// workshop simulation. Exposes isOn / offColor / onIntensity / isSnaped for the
    /// scene manager to read and drive.
    ///
    /// Expected prefab setup:
    /// - Root GameObject: an XRGrabInteractable, used to pick up and place the voyant into its
    ///   slot on the DIN rail / control panel. Assign the matching XRSocketInteractor to
    ///   _correctSocket so only that specific slot can set isSnaped to true.
    /// - _targetRenderer: the Renderer whose material colour should follow the on/off state.
    ///
    /// Unlike PushButtonController, this component has no pressable child interactable —
    /// isOn is expected to be driven externally by the scene/circuit manager, not by the
    /// learner clicking the voyant itself.
    ///
    /// Color logic: the "on" color is derived from offColor by adding onIntensity to its
    /// HSV Value channel (clamped to [0,1]), recomputed on demand from isOn/offColor/onIntensity
    /// rather than incrementally mutated — so changing onIntensity or offColor at runtime,
    /// in either state, always produces a consistent result with no drift.
    /// </summary>
    [DisallowMultipleComponent]
    public class VoyantController : MonoBehaviour
    {
        [System.Serializable]
        public class BoolUnityEvent : UnityEvent<bool> { }

        [Header("Interaction sources")]
        [Tooltip("Grabbed to place this voyant into its socket on the panel/DIN rail.")]
        [SerializeField] private XRGrabInteractable _grabInteractable;

        [Tooltip("The one XRSocketInteractor this voyant must be placed into for isSnaped to be true.")]
        [SerializeField] private XRSocketInteractor _correctSocket;

        [Header("Visuals")]
        [SerializeField] private Renderer _targetRenderer;

        [Tooltip("Index into _targetRenderer.sharedMaterials to recolor. Use 0 if it only has one material.")]
        [SerializeField] private int _materialIndex = 0;

        [Tooltip("URP Lit / HDRP Lit: \"_BaseColor\". Built-in Standard: \"_Color\".")]
        [SerializeField] private string _colorPropertyName = "_BaseColor";

        [Tooltip("Color of the voyant when isOn = false.")]
        [SerializeField] private Color _offColor = Color.gray;

        [Tooltip("Added to offColor's HSV Value when isOn = true (clamped to [0,1]). Subtracted back out when isOn = false.")]
        [SerializeField] private float _onIntensity = 0.5f;

        [SerializeField] private bool _isOn;

        [Header("Events (optional — for event-driven manager integration)")]
        public BoolUnityEvent onStateChanged = new BoolUnityEvent();
        public BoolUnityEvent onSnappedChanged = new BoolUnityEvent();

        private bool _isSnaped;
        private MaterialPropertyBlock _mpb;
        private int _colorPropertyId;

        /// <summary>True while the voyant is lit.</summary>
        public bool isOn
        {
            get => _isOn;
            set
            {
                if (_isOn == value) return;
                _isOn = value;
                ApplyColor();
                onStateChanged.Invoke(_isOn);
            }
        }

        /// <summary>Base color used when isOn = false. Setting this updates the material at runtime.</summary>
        public Color offColor
        {
            get => _offColor;
            set
            {
                if (_offColor == value) return;
                _offColor = value;
                ApplyColor();
            }
        }

        /// <summary>Amount added to offColor's HSV Value when isOn = true.</summary>
        public float onIntensity
        {
            get => _onIntensity;
            set
            {
                if (Mathf.Approximately(_onIntensity, value)) return;
                _onIntensity = value;
                ApplyColor();
            }
        }

        /// <summary>Index into the renderer's material array being recolored.</summary>
        public int materialIndex
        {
            get => _materialIndex;
            set
            {
                if (_materialIndex == value) return;
                _materialIndex = value;
                ApplyColor();
            }
        }

        /// <summary>True while this voyant is placed in its correct circuit slot.</summary>
        public bool isSnaped => _isSnaped;

        private void Awake()
        {
            if (_grabInteractable == null)
                _grabInteractable = GetComponent<XRGrabInteractable>();

            _colorPropertyId = Shader.PropertyToID(_colorPropertyName);
            _mpb = new MaterialPropertyBlock();
        }

        private void OnEnable()
        {
            if (_grabInteractable != null)
            {
                _grabInteractable.selectEntered.AddListener(OnGrabSelectEntered);
                _grabInteractable.selectExited.AddListener(OnGrabSelectExited);
            }

            ApplyColor();
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

        /// <summary>
        /// Computes the current display color: offColor as-is when off, or offColor with
        /// onIntensity added to its HSV Value (clamped [0,1]) when on. Always derived fresh
        /// from _offColor/_onIntensity/_isOn — never incrementally mutated — so it stays
        /// correct even if onIntensity or offColor change while the voyant is on.
        /// </summary>
        private Color ComputeDisplayColor()
        {
            if (!_isOn) return _offColor;

            Color.RGBToHSV(_offColor, out float h, out float s, out float v);
            v = Mathf.Clamp01(v + _onIntensity);
            Color result = Color.HSVToRGB(h, s, v);
            result.a = _offColor.a;
            return result;
        }

        private void ApplyColor()
        {
            if (_targetRenderer == null || _mpb == null) return;

            int index = Mathf.Clamp(_materialIndex, 0, _targetRenderer.sharedMaterials.Length - 1);

            _targetRenderer.GetPropertyBlock(_mpb, index);
            _mpb.SetColor(_colorPropertyId, ComputeDisplayColor());
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
