using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

namespace ElectricalWorkshop.UI
{
    public class AppController : MonoBehaviour
    {
        [Header("Audio")]
        [SerializeField] private AudioMixer _mixer;
        [SerializeField] private string _sfxParam = "SFXVolume";
        [SerializeField] private string _speechParam = "SpeechVolume";
        private const float MutedDb = -80f;
        private const float UnmutedDb = 0f;

        [Header("Controls Guide")]
        [SerializeField] private GameObject _controlsGuideCanvas;

        private bool _soundOn;
        private bool _speechOn;

        [Header("Pause Menu")]
        [SerializeField] private GameObject _menuCanvas;
        [Header("Speech Replay")]
        [SerializeField] private AudioSource _speechSource;

        private bool _isPaused;

        public void TogglePauseMenu()
        {
            if (_isPaused) Resume();
            else Pause();
        }

        public void Pause()
        {
            _isPaused = true;
            Time.timeScale = 0f;
            AudioListener.pause = true;
            _menuCanvas.SetActive(true);
        }

        public void Resume()
        {
            _isPaused = false;
            Time.timeScale = 1f;
            AudioListener.pause = false;
            _menuCanvas.SetActive(false);
        }

        public void ReplayVoice()
        {
            if (_speechSource == null) return;
            _speechSource.Stop();
            _speechSource.Play();
        }

        private void Start()
        {
            _mixer.GetFloat(_sfxParam, out float sfxDb);
            _mixer.GetFloat(_speechParam, out float speechDb);
            _soundOn = sfxDb > MutedDb;
            _speechOn = speechDb > MutedDb;
            // TODO: sync button visual state (icon/highlight) to _soundOn/_speechOn here
        }

        public void RestartScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void QuitApplication()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void ToggleControlsGuide()
        {
            if (_controlsGuideCanvas == null) return;
            _controlsGuideCanvas.SetActive(!_controlsGuideCanvas.activeSelf);
        }

        public void ToggleSound()
        {
            _soundOn = !_soundOn;
            _mixer.SetFloat(_sfxParam, _soundOn ? UnmutedDb : MutedDb);
        }

        public void ToggleSpeech()
        {
            _speechOn = !_speechOn;
            _mixer.SetFloat(_speechParam, _speechOn ? UnmutedDb : MutedDb);
        }
    }
}