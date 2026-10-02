using System.Collections.Generic;
using UnityEngine;

namespace ElectricalWorkshop.Components
{
    /// <summary>
    /// Plays the one-time voice instructions at fixed stages of the simulation.
    ///
    /// Playback policy: clips are QUEUED (FIFO) — a new clip waits until the current one has
    /// finished, nothing is ever cut off. Each clip plays at most once per scene load; a scene
    /// restart reloads this component, so everything becomes playable again.
    ///
    /// Plays through the same AudioSource AppController uses for ReplayVoice(): the manager
    /// assigns .clip and calls Play() (not PlayOneShot), so the A button replays the last clip.
    ///
    /// All sources are subscribed to in code — leave the matching events empty in the Inspector,
    /// otherwise a clip would be queued twice.
    ///
    /// Gating:
    ///   - Disjonctor grab: only while the disjonctor's wiring step is the active one.
    ///   - Disjonctor on / MAR / ARR: only during the activation phase (MAR/ARR only when the
    ///     press takes effect, per ActivationPhaseManager's rules).
    /// A gated trigger that is rejected does not consume the clip.
    /// </summary>
    public class VoiceoverManager : MonoBehaviour
    {
        [Header("Audio")]
        [Tooltip("The SAME AudioSource as AppController's Speech Source (routed to the SpeechVolume mixer group). Play On Awake and Loop must be off.")]
        [SerializeField] private AudioSource _speechSource;

        [Header("Scene start")]
        [SerializeField] private AudioClip _startClip;

        [Tooltip("Seconds before the start clip is queued. Uses scaled time, so it freezes while paused.")]
        [SerializeField, Min(0f)] private float _startDelay = 1f;

        [Header("Scenario 1")]
        [Tooltip("Element i plays when batch i starts revealing (element 0 = batch 1). Extra batches without a clip are skipped.")]
        [SerializeField] private AudioClip[] _batchClips;

        [Tooltip("Plays when Scenario 1 completes — the transition to Scenario 2.")]
        [SerializeField] private AudioClip _scenario1EndClip;

        [Header("Scenario 2")]
        [SerializeField] private AudioClip _disjonctorGrabClip;
        [SerializeField] private AudioClip _wiringCompleteClip;
        [SerializeField] private AudioClip _disjonctorOnClip;
        [SerializeField] private AudioClip _marPressedClip;
        [SerializeField] private AudioClip _arrPressedClip;

        [Header("Event sources (subscribed in code — leave their Inspector events empty)")]
        [SerializeField] private MotorAnimationManager _motorAnimationManager;
        [SerializeField] private WiringSequenceManager _wiringSequenceManager;
        [SerializeField] private ActivationPhaseManager _activationPhaseManager;
        [SerializeField] private DisjonctorController _disjonctor;

        private readonly Queue<AudioClip> _queue = new Queue<AudioClip>();
        private readonly HashSet<string> _played = new HashSet<string>();

        private float _startTimer;
        private bool _startQueued;
        private bool _activationPhase;

        private void Start()
        {
            _startTimer = _startDelay;
        }

        private void OnEnable()
        {
            if (_motorAnimationManager != null)
            {
                _motorAnimationManager.onBatchRevealStarted.AddListener(OnBatchRevealStarted);
                _motorAnimationManager.onScenario1Complete.AddListener(OnScenario1Complete);
            }

            if (_wiringSequenceManager != null)
                _wiringSequenceManager.onWiringPhaseComplete.AddListener(OnWiringPhaseComplete);

            if (_disjonctor != null)
            {
                _disjonctor.onGrabbed.AddListener(OnDisjonctorGrabbed);
                _disjonctor.onStateChanged.AddListener(OnDisjonctorStateChanged);
            }

            if (_activationPhaseManager != null)
            {
                _activationPhaseManager.onMarAccepted.AddListener(OnMarAccepted);
                _activationPhaseManager.onArrAccepted.AddListener(OnArrAccepted);
            }
        }

        private void OnDisable()
        {
            if (_motorAnimationManager != null)
            {
                _motorAnimationManager.onBatchRevealStarted.RemoveListener(OnBatchRevealStarted);
                _motorAnimationManager.onScenario1Complete.RemoveListener(OnScenario1Complete);
            }

            if (_wiringSequenceManager != null)
                _wiringSequenceManager.onWiringPhaseComplete.RemoveListener(OnWiringPhaseComplete);

            if (_disjonctor != null)
            {
                _disjonctor.onGrabbed.RemoveListener(OnDisjonctorGrabbed);
                _disjonctor.onStateChanged.RemoveListener(OnDisjonctorStateChanged);
            }

            if (_activationPhaseManager != null)
            {
                _activationPhaseManager.onMarAccepted.RemoveListener(OnMarAccepted);
                _activationPhaseManager.onArrAccepted.RemoveListener(OnArrAccepted);
            }
        }

        private void Update()
        {
            if (!_startQueued)
            {
                _startTimer -= Time.deltaTime;
                if (_startTimer <= 0f)
                {
                    _startQueued = true;
                    PlayOnce("start", _startClip);
                }
            }

            if (_queue.Count == 0 || _speechSource == null) return;

            // AudioListener.pause (pause menu) must not let the next clip start over a paused one.
            if (AudioListener.pause || _speechSource.isPlaying) return;

            _speechSource.clip = _queue.Dequeue();
            _speechSource.Play();
        }

        // ---- Triggers ----

        private void OnBatchRevealStarted(int batchIndex)
        {
            if (_batchClips == null || batchIndex < 0 || batchIndex >= _batchClips.Length) return;
            PlayOnce($"batch{batchIndex}", _batchClips[batchIndex]);
        }

        private void OnScenario1Complete() => PlayOnce("scenario1End", _scenario1EndClip);

        private void OnDisjonctorGrabbed()
        {
            if (_wiringSequenceManager == null || !_wiringSequenceManager.IsStepActive(_disjonctor)) return;
            PlayOnce("disjonctorGrab", _disjonctorGrabClip);
        }

        private void OnWiringPhaseComplete()
        {
            _activationPhase = true;
            PlayOnce("wiringComplete", _wiringCompleteClip);
        }

        private void OnDisjonctorStateChanged(bool isOn)
        {
            if (!isOn || !_activationPhase) return;
            PlayOnce("disjonctorOn", _disjonctorOnClip);
        }

        private void OnMarAccepted() => PlayOnce("mar", _marPressedClip);

        private void OnArrAccepted() => PlayOnce("arr", _arrPressedClip);

        // ---- Queue ----

        private void PlayOnce(string key, AudioClip clip)
        {
            if (!_played.Add(key)) return;

            if (clip == null)
            {
                Debug.LogWarning($"VoiceoverManager: no clip assigned for '{key}'.", this);
                return;
            }

            _queue.Enqueue(clip);
        }
    }
}
