using UnityEngine;
using UnityEngine.Audio;

namespace _Project.Develop.Runtime.Utilities.Audio
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "_Project.Platform.Audio.Runtime", sourceAssembly: "GameJamKit.Audio.Unity", sourceClassName: "AudioService")]
    public class AudioService : MonoBehaviour
    {
        private const float MutedMixerVolume = -80f;
        private const float EnabledMixerVolume = 0f;
        private const float MinimumNormalizedVolume = 0.0001f;

        private const string MasterVolumeKey = "MVolume";
        private const string MusicVolumeKey = "MusVolume";
        private const string SoundVolumeKey = "SFXVolume";

        [SerializeField] private AudioMixer _audioMixer;
        [SerializeField] private AudioSource _soundsSource;

        public AudioMixerGroup SoundGroup => _soundsSource != null ? _soundsSource.outputAudioMixerGroup : null;

        public AudioMixerGroup MusicGroup
        {
            get
            {
                if (_audioMixer == null) return SoundGroup;
                AudioMixerGroup[] groups = _audioMixer.FindMatchingGroups("Music");
                return groups.Length > 0 ? groups[0] : SoundGroup;
            }
        }

        public bool IsSoundOn => IsVolumeOn(SoundVolumeKey);

        public bool IsMusicOn => IsVolumeOn(MusicVolumeKey);

        public float MasterVolume => GetNormalizedVolume(MasterVolumeKey);
        public float MusicVolume => GetNormalizedVolume(MusicVolumeKey);
        public float SoundVolume => GetNormalizedVolume(SoundVolumeKey);

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            EnsureAudioSources();
        }

        public void SoundPlay(AudioClip audioClip)
        {
            if (audioClip == null)
                return;

            _soundsSource.PlayOneShot(audioClip);
        }

        public void SoundPlay(AudioClip audioClip, float minimumPitch, float maximumPitch)
        {
            if (audioClip == null)
                return;

            _soundsSource.pitch = Random.Range(minimumPitch, maximumPitch);
            _soundsSource.PlayOneShot(audioClip);
            _soundsSource.pitch = 1f;
        }
        
        public void SoundPlayAtPoint(AudioClip audioClip, Vector3 position)
        {
            if (audioClip == null)
                return;

            var sound = new GameObject($"Spatial sound: {audioClip.name}");
            sound.transform.SetParent(transform);
            sound.transform.position = position;
            AudioSource source = sound.AddComponent<AudioSource>();
            source.outputAudioMixerGroup = _soundsSource.outputAudioMixerGroup;
            source.spatialBlend = 1f;
            source.clip = audioClip;
            source.Play();
            Destroy(sound, audioClip.length);
        }

        public void SoundsOn() => SetVolume(SoundVolumeKey, EnabledMixerVolume);

        public void SoundsOff() => SetVolume(SoundVolumeKey, MutedMixerVolume);

        public void MusicsOn() => SetVolume(MusicVolumeKey, EnabledMixerVolume);

        public void MusicsOff() => SetVolume(MusicVolumeKey, MutedMixerVolume);

        public void SetMasterVolume(float volume) => SetNormalizedVolume(MasterVolumeKey, volume);

        public void SetMusicVolume(float volume) => SetNormalizedVolume(MusicVolumeKey, volume);

        public void SetSoundVolume(float volume) => SetNormalizedVolume(SoundVolumeKey, volume);

        private void EnsureAudioSources()
        {
            if (_soundsSource == null)
                _soundsSource = gameObject.AddComponent<AudioSource>();
        }

        private bool IsVolumeOn(string volumeKey)
        {
            if (_audioMixer == null)
                return true;

            return _audioMixer.GetFloat(volumeKey, out float volume)
                   && volume > MutedMixerVolume;
        }

        private float GetNormalizedVolume(string volumeKey)
        {
            if (_audioMixer == null || _audioMixer.GetFloat(volumeKey, out float volume) == false)
                return 1f;

            return Mathf.Pow(10f, volume / 20f);
        }

        private void SetNormalizedVolume(string volumeKey, float volume)
        {
            float clampedVolume = Mathf.Clamp01(volume);
            float mixerVolume = clampedVolume <= MinimumNormalizedVolume
                ? MutedMixerVolume
                : Mathf.Log10(clampedVolume) * 20f;

            SetVolume(volumeKey, mixerVolume);
        }

        private void SetVolume(string volumeKey, float volume)
        {
            if (_audioMixer == null)
                return;

            _audioMixer.SetFloat(volumeKey, volume);
        }
    }
}
