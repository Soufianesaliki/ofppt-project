using UnityEngine;

namespace ElectricalWorkshop.Components
{
    /// <summary>
    /// Controls the Scenario 2 motor's running state — a rotating part plus looping audio.
    /// This is a separate motor object from Scenario 1's animated one. Driven externally
    /// (by ActivationPhaseManager) via the isOn property; no learner interaction here.
    /// </summary>
    public class MotorRunController : MonoBehaviour
    {
        [Header("Rotation")]
        [Tooltip("The part that spins while the motor is running (e.g. the rotor/shaft).")]
        [SerializeField] private Transform _rotatingPart;

        [Tooltip("Local-space axis to rotate around.")]
        [SerializeField] private Vector3 _rotationAxis = Vector3.forward;

        [Tooltip("Rotation speed in degrees per second while running.")]
        [SerializeField] private float _rotationSpeed = 360f;

        [Header("Audio")]
        [Tooltip("Looping audio source played while running. Set its own Loop flag in the Inspector.")]
        [SerializeField] private AudioSource _audioSource;

        private bool _isOn;

        /// <summary>True while the motor is running. Setting this starts/stops rotation and audio.</summary>
        public bool isOn
        {
            get => _isOn;
            set
            {
                if (_isOn == value) return;
                _isOn = value;

                if (_isOn)
                {
                    if (_audioSource != null && !_audioSource.isPlaying)
                        _audioSource.Play();
                }
                else
                {
                    if (_audioSource != null)
                        _audioSource.Stop();
                }
            }
        }

        private void Update()
        {
            if (!_isOn || _rotatingPart == null) return;

            _rotatingPart.Rotate(_rotationAxis, _rotationSpeed * Time.deltaTime, Space.Self);
        }
    }
}
