using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using NexonGame.Core;

namespace NexonGame.Managers
{
    /// <summary>
    /// 씬 로딩 시스템 (DI 패턴)
    /// </summary>
    public class SceneLoader : ISceneLoader, IService
    {
        private MonoBehaviour _coroutineRunner;

        public void Initialize()
        {
            var go = new GameObject("SceneLoader");
            UnityEngine.Object.DontDestroyOnLoad(go);
            _coroutineRunner = go.AddComponent<CoroutineRunner>();

            Debug.Log("SceneLoader 초기화 완료");
        }

        public void Cleanup()
        {
            if (_coroutineRunner != null)
            {
                UnityEngine.Object.Destroy(_coroutineRunner.gameObject);
            }
        }

        public void LoadScene(string sceneName, Action onComplete = null)
        {
            SceneManager.LoadScene(sceneName);
            onComplete?.Invoke();
        }

        public void LoadSceneAsync(string sceneName, Action<float> onProgress = null, Action onComplete = null)
        {
            if (_coroutineRunner != null)
            {
                _coroutineRunner.StartCoroutine(LoadSceneAsyncCoroutine(sceneName, onProgress, onComplete));
            }
        }

        private IEnumerator LoadSceneAsyncCoroutine(string sceneName, Action<float> onProgress, Action onComplete)
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
            operation.allowSceneActivation = false;

            while (!operation.isDone)
            {
                float progress = Mathf.Clamp01(operation.progress / 0.9f);
                onProgress?.Invoke(progress);

                if (operation.progress >= 0.9f)
                {
                    operation.allowSceneActivation = true;
                }

                yield return null;
            }

            onComplete?.Invoke();
        }

        public void ReloadCurrentScene()
        {
            Scene currentScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(currentScene.name);
        }

        public string GetCurrentSceneName()
        {
            return SceneManager.GetActiveScene().name;
        }

        private class CoroutineRunner : MonoBehaviour { }
    }
}
