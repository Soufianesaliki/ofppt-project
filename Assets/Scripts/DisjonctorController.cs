using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace ElectricalWorkshop.Components
{
    /// <summary>
    /// Controls a virtual circuit breaker ("disjoncteur") component for the electrical
    /// workshop simulation. Exposes isOn / isSnaped for the scene manager to read and set.
    ///
    /// Expected prefab setup:
    /// - Root GameObject: an XRGrabInteractable, used to pick up and place the breaker into its
    ///   slot on the DIN rail / control panel. Assign the matching XRSocketInteractor to
    ///   _correctSocket so only that specific slot can set isSnaped to true.
    /// - switchObj: the physical lever/switch that rotates between Off and On. It must hold an
    ///   XRSimpleInteractable and a Collider so the learner can toggle it directly — either by
    ///   poking it (XRPokeInteractor + Poke Filter, which raises selectEntered) or by pointing at
    ///   it with a controller and pulling its trigger (which raises activated, since XRI's
    ///   defaults bind Select to Grip and Activate to Trigger). Both events are listened to here
    ///   and treated as the same toggle, same convention as PushButtonController.
    /// - The rotation is driven by animCurve over animDuration seconds: switchObj rotates
    ///   rotationDegree around the world X axis relative to its initial orientation when isOn
    ///   becomes true, and back to its initial orientation when isOn becomes false.
    /// </summary>
    [DisallowMultipleComponent]
    public class DisjonctorController : MonoBehaviour
    {
        [System.Serializable]
        public class BoolUnityEvent : UnityEvent<bool> { }

        [Header("Interaction sources")]
        [Tooltip("Grabbed to place this breaker into its socket on the panel/DIN rail.")]
        [SerializeField] private XRGrabInteractable _grabInteractable;

        [Tooltip("The one XRSocketInteractor this breaker must be placed into for isSnaped to be true.")]
        [SerializeField] private XRSocketInteractor _correctSocket;

        [Header("Switch")]
        [Tooltip("The lever/switch object. Needs a Collider and an XRSimpleInteractable so it can be poked/selected to toggle isOn. See OnValidate warning re: trigger.")]
        public GameObject switchObj;

        [Tooltip("Degrees added to switchObj's world-space X rotation when isOn is true, relative to its initial orientation at Awake.")]
        public float rotationDegree = 30f;

        [Tooltip("Eases the rotation over time. Evaluated with t in [0,1] (elapsed / animDuration); keep output in [0,1] too.")]
        public AnimationCurve animCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Tooltip("Seconds the switch takes to rotate fully from Off to On (or On to Off).")]
        public float animDuration = 0.25f;

        [Header("Events (optional — for event-driven manager integration)")]
        public BoolUnityEvent onStateChanged = new BoolUnityEvent();
        public BoolUnityEvent onSnappedChanged = new BoolUnityEvent();

        private bool _isOn;
        private bool _isSnaped;
        private XRSimpleInteractable _switchInteractable;
        private Quaternion _initialSwitchRotation;
        private float _currentOffset;
        private Coroutine _rotateCoroutine;

        /// <summary>
        /// True while the breaker is toggled On. Settable directly by the scene manager as well
        /// as by the learner poking/selecting switchObj — both paths go through this setter, so
        /// the rotation animation and onStateChanged event always fire consistently regardless
        /// of who changed the state.
        /// </summary>
        public bool isOn
        {
            get => _isOn;
            set
            {
                if (_isOn == value) return;
                _isOn = value;
                Debug.Log($"Disjonctor isOn: {_isOn}");
                onStateChanged.Invoke(_isOn);
                AnimateTo(_isOn ? rotationDegree : 0f);
            }
        }

        /// <summary>True while this breaker is placed in its correct circuit slot.</summary>
        public bool isSnaped => _isSnaped;

        private void Awake()
        {
            if (_grabInteractable == null)
                _grabInteractable = GetComponent<XRGrabInteractable>();

            if (switchObj != null)
            {
                _switchInteractable = switchObj.GetComponent<XRSimpleInteractable>();
                _initialSwitchRotation = switchObj.transform.rotation;
            }
        }

        private void OnEnable()
        {
            if (_switchInteractable != null)
            {
                _switchInteractable.selectEntered.AddListener(OnSwitchSelectEntered);
                _switchInteractable.activated.AddListener(OnSwitchActivated);
            }

            if (_grabInteractable != null)
            {
                _grabInteractable.selectEntered.AddListener(OnGrabSelectEntered);
                _grabInteractable.selectExited.AddListener(OnGrabSelectExited);
            }
        }

        private void OnDisable()
        {
            if (_switchInteractable != null)
            {
                _switchInteractable.selectEntered.RemoveListener(OnSwitchSelectEntered);
                _switchInteractable.activated.RemoveListener(OnSwitchActivated);
            }

            if (_grabInteractable != null)
            {
                _grabInteractable.selectEntered.RemoveListener(OnGrabSelectEntered);
                _grabInteractable.selectExited.RemoveListener(OnGrabSelectExited);
            }

            if (_rotateCoroutine != null)
            {
                StopCoroutine(_rotateCoroutine);
                _rotateCoroutine = null;
            }
        }

        // Poking fires selectEntered (via the interactable's Poke Filter). A controller
        // pointing at the switch and pulling its physical trigger fires "activated" instead —
        // same convention as PushButtonController. Both simply flip isOn.
        private void OnSwitchSelectEntered(SelectEnterEventArgs args) => isOn = !isOn;

        private void OnSwitchActivated(ActivateEventArgs args) => isOn = !isOn;

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

        private void AnimateTo(float targetOffset)
        {
            if (switchObj == null) return;

            if (_rotateCoroutine != null)
                StopCoroutine(_rotateCoroutine);

            _rotateCoroutine = StartCoroutine(RotateRoutine(_currentOffset, targetOffset));
        }

        private IEnumerator RotateRoutine(float fromOffset, float toOffset)
        {
            float duration = Mathf.Max(animDuration, 0.0001f);
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float curveT = animCurve.Evaluate(t);
                _currentOffset = Mathf.LerpUnclamped(fromOffset, toOffset, curveT);
                ApplyRotation();
                yield return null;
            }

            _currentOffset = toOffset;
            ApplyRotation();
            _rotateCoroutine = null;
        }

        private void ApplyRotation()
        {
            // Rotates switchObj about the world X axis, relative to its ORIGINAL orientation
            // (cached at Awake) rather than compounding onto its current rotation frame over
            // frame. That guarantees isOn=false always lands back exactly on the starting pose,
            // even if animations are interrupted mid-way and re-triggered.
            switchObj.transform.rotation = Quaternion.AngleAxis(_currentOffset, Vector3.right) * _initialSwitchRotation;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (switchObj != null)
            {
                var col = switchObj.GetComponent<Collider>();
                if (col != null && !col.isTrigger)
                {
                    Debug.LogWarning(
                        $"[DisjonctorController] '{switchObj.name}' collider is not a trigger. " +
                        "For an XRSimpleInteractable toggled by poke/select (no physical grab), " +
                        "a trigger collider is the usual XRI recommendation so it doesn't add " +
                        "unwanted physical collision response — set isTrigger = true unless you " +
                        "specifically need physical collisions on this object.",
                        this);
                }
            }
        }
#endif
    }
}
