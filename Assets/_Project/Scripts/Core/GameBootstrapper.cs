using UnityEngine;

namespace NexonGame.Core
{
    /// <summary>
    /// 게임 시작 시 모든 서비스를 초기화하고 의존성을 설정합니다
    /// 이 스크립트는 첫 번째 씬에서 실행되어야 합니다
    /// </summary>
    public class GameBootstrapper : MonoBehaviour
    {
        [Header("서비스 초기화 순서")]
        [SerializeField] private bool _dontDestroyOnLoad = true;

        private static bool _isInitialized = false;

        private void Awake()
        {
            if (_isInitialized)
            {
                Destroy(gameObject);
                return;
            }

            _isInitialized = true;

            if (_dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }

            InitializeServices();
        }

        private void InitializeServices()
        {
            Debug.Log("=== 게임 서비스 초기화 시작 ===");

            // AudioManager 초기화
            var audioManager = new Managers.AudioManager();
            ServiceLocator.Instance.Register<Managers.IAudioManager>(audioManager);
            audioManager.Initialize();

            // SceneLoader 초기화
            var sceneLoader = new Managers.SceneLoader();
            ServiceLocator.Instance.Register<Managers.ISceneLoader>(sceneLoader);
            sceneLoader.Initialize();

            // InputManager 초기화
            var inputManager = new Managers.InputManager();
            ServiceLocator.Instance.Register<Managers.IInputManager>(inputManager);
            inputManager.Initialize();

            // UIManager 초기화
            var uiManager = new Managers.UIManager();
            ServiceLocator.Instance.Register<Managers.IUIManager>(uiManager);
            uiManager.Initialize();

            Debug.Log("=== 게임 서비스 초기화 완료 ===");
        }

        private void OnApplicationQuit()
        {
            CleanupServices();
        }

        private void OnDestroy()
        {
            if (_isInitialized)
            {
                CleanupServices();
            }
        }

        private void CleanupServices()
        {
            Debug.Log("=== 게임 서비스 정리 시작 ===");

            ServiceLocator.Instance.Clear();

            Debug.Log("=== 게임 서비스 정리 완료 ===");
        }
    }
}
