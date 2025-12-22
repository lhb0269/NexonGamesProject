using UnityEngine;

namespace NexonGame.Managers
{
    /// <summary>
    /// 오디오 관리 인터페이스
    /// </summary>
    public interface IAudioManager
    {
        void PlayMusic(AudioClip clip, bool loop = true);
        void StopMusic();
        void PlaySFX(AudioClip clip, float volume = 1f);
        void PlaySFXAtPosition(AudioClip clip, Vector3 position, float volume = 1f);
        void SetMusicVolume(float volume);
        void SetSFXVolume(float volume);
        float GetMusicVolume();
        float GetSFXVolume();
    }
}
