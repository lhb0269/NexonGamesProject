using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NexonGame.Core;

namespace NexonGame.Managers
{
    /// <summary>
    /// UI 시스템 관리자 (DI 패턴)
    /// </summary>
    public class UIManager : IUIManager, IService
    {
        private Dictionary<string, GameObject> _panels = new Dictionary<string, GameObject>();
        private GameObject _loadingScreen;
        private MonoBehaviour _coroutineRunner;

        public void Initialize()
        {
            var go = new GameObject("UIManager");
            UnityEngine.Object.DontDestroyOnLoad(go);
            _coroutineRunner = go.AddComponent<UICoroutineRunner>();

            Debug.Log("UIManager 초기화 완료");
        }

        public void Cleanup()
        {
            _panels.Clear();

            if (_coroutineRunner != null)
            {
                UnityEngine.Object.Destroy(_coroutineRunner.gameObject);
            }
        }

        public void ShowPanel(string panelName)
        {
            if (_panels.TryGetValue(panelName, out GameObject panel))
            {
                panel.SetActive(true);
            }
            else
            {
                Debug.LogWarning($"패널을 찾을 수 없습니다: {panelName}");
            }
        }

        public void HidePanel(string panelName)
        {
            if (_panels.TryGetValue(panelName, out GameObject panel))
            {
                panel.SetActive(false);
            }
        }

        public void HideAllPanels()
        {
            foreach (var panel in _panels.Values)
            {
                panel.SetActive(false);
            }
        }

        public void ShowLoadingScreen(bool show)
        {
            if (_loadingScreen != null)
            {
                _loadingScreen.SetActive(show);
            }
        }

        public void ShowMessage(string message, float duration = 3f)
        {
            Debug.Log($"[UI 메시지] {message}");
            // TODO: 실제 UI 메시지 표시 구현
        }

        public void RegisterPanel(string panelName, GameObject panel)
        {
            if (!_panels.ContainsKey(panelName))
            {
                _panels.Add(panelName, panel);
            }
        }

        public void RegisterLoadingScreen(GameObject loadingScreen)
        {
            _loadingScreen = loadingScreen;
        }

        private class UICoroutineRunner : MonoBehaviour { }
    }
}
