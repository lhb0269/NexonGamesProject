using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace NexonGame.BlueArchive.UI
{
    /// <summary>
    /// 전투 로그 패널
    /// - 스킬 사용, 데미지, 격파 등의 전투 이벤트 표시
    /// - 스크롤 가능한 로그 목록
    /// - 자동 스크롤 (최신 로그가 항상 보임)
    /// </summary>
    public class CombatLogPanel : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private Transform _logContainer;
        [SerializeField] private Text _logTextPrefab;

        [Header("Settings")]
        [SerializeField] private int _maxLogEntries = 50;
        [SerializeField] private Color _skillColor = new Color(0.3f, 0.8f, 1f);
        [SerializeField] private Color _damageColor = new Color(1f, 0.6f, 0.3f);
        [SerializeField] private Color _defeatColor = new Color(1f, 0.3f, 0.3f);
        [SerializeField] private Color _systemColor = new Color(0.7f, 0.7f, 0.7f);

        // 로그 엔트리
        private Queue<GameObject> _logEntries;
        private Canvas _canvas;

        private const float UI_WIDTH = 400f;
        private const float UI_HEIGHT = 300f;
        private const float LOG_ENTRY_HEIGHT = 25f;

        private void Awake()
        {
            _logEntries = new Queue<GameObject>();
            CreateUIElements();
        }

        /// <summary>
        /// UI 요소 생성
        /// </summary>
        private void CreateUIElements()
        {
            // Canvas 추가 (Screen Space - Overlay)
            _canvas = gameObject.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 5;

            var canvasScaler = gameObject.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920, 1080);

            gameObject.AddComponent<GraphicRaycaster>();

            // 배경 패널
            var bgPanel = CreatePanel("BackgroundPanel", new Vector2(UI_WIDTH, UI_HEIGHT), Vector2.zero);
            var bgImage = bgPanel.GetComponent<Image>();
            bgImage.sprite = CreateWhiteSprite();
            bgImage.color = new Color(0.05f, 0.05f, 0.1f, 0.95f);

            // Anchored Position (화면 왼쪽 상단)
            var bgRect = bgPanel.GetComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0f, 1f);
            bgRect.anchorMax = new Vector2(0f, 1f);
            bgRect.pivot = new Vector2(0f, 1f);
            bgRect.anchoredPosition = new Vector2(20, -80);

            // 제목
            var titleObj = new GameObject("Title");
            titleObj.transform.SetParent(bgPanel.transform, false);

            var titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.sizeDelta = new Vector2(UI_WIDTH - 20, 30);
            titleRect.anchoredPosition = new Vector2(0, 130);

            var titleText = titleObj.AddComponent<Text>();
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleText.fontSize = 18;
            titleText.alignment = TextAnchor.MiddleLeft;
            titleText.color = new Color(0.9f, 0.9f, 1f);
            titleText.text = "  전투 로그";
            titleText.fontStyle = FontStyle.Bold;

            // ScrollRect 생성
            CreateScrollView(bgPanel.transform);

            Debug.Log("[CombatLogPanel] UI 생성 완료");
        }

        /// <summary>
        /// ScrollView 생성
        /// </summary>
        private void CreateScrollView(Transform parent)
        {
            var scrollViewObj = new GameObject("ScrollView");
            scrollViewObj.transform.SetParent(parent, false);

            var scrollViewRect = scrollViewObj.AddComponent<RectTransform>();
            scrollViewRect.sizeDelta = new Vector2(UI_WIDTH - 20, UI_HEIGHT - 60);
            scrollViewRect.anchoredPosition = new Vector2(0, -13);

            _scrollRect = scrollViewObj.AddComponent<ScrollRect>();
            _scrollRect.horizontal = false;
            _scrollRect.vertical = true;
            _scrollRect.movementType = ScrollRect.MovementType.Clamped;
            _scrollRect.scrollSensitivity = 20f;

            // Viewport
            var viewportObj = new GameObject("Viewport");
            viewportObj.transform.SetParent(scrollViewObj.transform, false);

            var viewportRect = viewportObj.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.sizeDelta = Vector2.zero;

            var viewportMask = viewportObj.AddComponent<Mask>();
            viewportMask.showMaskGraphic = false;

            var viewportImage = viewportObj.AddComponent<Image>();
            viewportImage.sprite = CreateWhiteSprite();
            viewportImage.color = new Color(0.1f, 0.1f, 0.15f, 0.5f);

            _scrollRect.viewport = viewportRect;

            // Content
            var contentObj = new GameObject("Content");
            contentObj.transform.SetParent(viewportObj.transform, false);

            var contentRect = contentObj.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.sizeDelta = new Vector2(0, 0);

            var contentLayout = contentObj.AddComponent<VerticalLayoutGroup>();
            contentLayout.childControlWidth = true;
            contentLayout.childControlHeight = false;
            contentLayout.childForceExpandWidth = true;
            contentLayout.childForceExpandHeight = false;
            contentLayout.spacing = 2;
            contentLayout.padding = new RectOffset(5, 5, 5, 5);

            var contentFitter = contentObj.AddComponent<ContentSizeFitter>();
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            _scrollRect.content = contentRect;
            _logContainer = contentObj.transform;
        }

        /// <summary>
        /// 화이트 스프라이트 생성
        /// </summary>
        private Sprite CreateWhiteSprite()
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();

            return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        }

        /// <summary>
        /// 패널 생성 헬퍼
        /// </summary>
        private GameObject CreatePanel(string name, Vector2 size, Vector2 position)
        {
            var obj = new GameObject(name);
            var rect = obj.AddComponent<RectTransform>();
            rect.SetParent(transform, false);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;

            var image = obj.AddComponent<Image>();

            return obj;
        }

        /// <summary>
        /// 로그 추가
        /// </summary>
        public void AddLog(string message, LogType logType = LogType.System)
        {
            // 로그 엔트리 생성
            var logEntryObj = new GameObject($"LogEntry_{_logEntries.Count}");
            logEntryObj.transform.SetParent(_logContainer, false);

            var logRect = logEntryObj.AddComponent<RectTransform>();
            logRect.sizeDelta = new Vector2(0, LOG_ENTRY_HEIGHT);

            var logText = logEntryObj.AddComponent<Text>();
            logText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            logText.fontSize = 14;
            logText.alignment = TextAnchor.MiddleLeft;
            logText.text = $"  {message}";
            logText.color = GetColorForType(logType);

            _logEntries.Enqueue(logEntryObj);

            // 최대 로그 개수 제한
            while (_logEntries.Count > _maxLogEntries)
            {
                var oldEntry = _logEntries.Dequeue();
                Destroy(oldEntry);
            }

            // 자동 스크롤 (최신 로그로)
            Canvas.ForceUpdateCanvases();
            _scrollRect.verticalNormalizedPosition = 0f;

            Debug.Log($"[CombatLogPanel] {logType}: {message}");
        }

        /// <summary>
        /// 로그 타입별 색상
        /// </summary>
        private Color GetColorForType(LogType logType)
        {
            return logType switch
            {
                LogType.Skill => _skillColor,
                LogType.Damage => _damageColor,
                LogType.Defeat => _defeatColor,
                LogType.System => _systemColor,
                _ => Color.white
            };
        }

        /// <summary>
        /// 로그 초기화
        /// </summary>
        public void ClearLogs()
        {
            while (_logEntries.Count > 0)
            {
                var entry = _logEntries.Dequeue();
                Destroy(entry);
            }

            AddLog("전투 로그 초기화", LogType.System);
        }

        /// <summary>
        /// 로그 타입
        /// </summary>
        public enum LogType
        {
            System,   // 시스템 메시지
            Skill,    // 스킬 사용
            Damage,   // 데미지
            Defeat    // 격파
        }
    }
}
