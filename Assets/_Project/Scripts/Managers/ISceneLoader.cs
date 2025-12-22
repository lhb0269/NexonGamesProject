using System;
using UnityEngine.SceneManagement;

namespace NexonGame.Managers
{
    /// <summary>
    /// 씬 로딩 관리 인터페이스
    /// </summary>
    public interface ISceneLoader
    {
        void LoadScene(string sceneName, Action onComplete = null);
        void LoadSceneAsync(string sceneName, Action<float> onProgress = null, Action onComplete = null);
        void ReloadCurrentScene();
        string GetCurrentSceneName();
    }
}
