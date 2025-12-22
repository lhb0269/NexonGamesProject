using UnityEngine;
using NexonGame.Core;

namespace NexonGame.Managers
{
    /// <summary>
    /// 오디오 시스템 관리자 (DI 패턴)
    /// </summary>
    public class AudioManager : IAudioManager, IService
    {
        private AudioSource _musicSource;
        private GameObject _audioRoot;
        private float _musicVolume = 1f;
        private float _sfxVolume = 1f;

        public void Initialize()
        {
            _audioRoot = new GameObject("AudioManager");
            UnityEngine.Object.DontDestroyOnLoad(_audioRoot);

            _musicSource = _audioRoot.AddComponent<AudioSource>();
            _musicSource.playOnAwake = false;
            _musicSource.loop = true;

            Debug.Log("AudioManager 초기화 완료");
        }

        public void Cleanup()
        {
            if (_audioRoot != null)
            {
                UnityEngine.Object.Destroy(_audioRoot);
            }
        }

        public void PlayMusic(AudioClip clip, bool loop = true)
        {
            if (_musicSource == null || clip == null) return;

            _musicSource.clip = clip;
            _musicSource.loop = loop;
            _musicSource.volume = _musicVolume;
            _musicSource.Play();
        }

        public void StopMusic()
        {
            if (_musicSource != null)
            {
                _musicSource.Stop();
            }
        }

        public void PlaySFX(AudioClip clip, float volume = 1f)
        {
            if (clip == null) return;

            var sfxObject = new GameObject("SFX_OneShot");
            var audioSource = sfxObject.AddComponent<AudioSource>();
            audioSource.clip = clip;
            audioSource.volume = volume * _sfxVolume;
            audioSource.Play();

            UnityEngine.Object.Destroy(sfxObject, clip.length + 0.1f);
        }

        public void PlaySFXAtPosition(AudioClip clip, Vector3 position, float volume = 1f)
        {
            if (clip == null) return;

            var sfxObject = new GameObject("SFX_3D");
            sfxObject.transform.position = position;
            var audioSource = sfxObject.AddComponent<AudioSource>();
            audioSource.clip = clip;
            audioSource.volume = volume * _sfxVolume;
            audioSource.spatialBlend = 1f; // 3D 사운드
            audioSource.Play();

            UnityEngine.Object.Destroy(sfxObject, clip.length + 0.1f);
        }

        public void SetMusicVolume(float volume)
        {
            _musicVolume = Mathf.Clamp01(volume);
            if (_musicSource != null)
            {
                _musicSource.volume = _musicVolume;
            }
        }

        public void SetSFXVolume(float volume)
        {
            _sfxVolume = Mathf.Clamp01(volume);
        }

        public float GetMusicVolume() => _musicVolume;
        public float GetSFXVolume() => _sfxVolume;
    }
}
