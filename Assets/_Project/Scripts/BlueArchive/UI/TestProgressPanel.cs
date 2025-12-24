using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace NexonGame.BlueArchive.UI
{
    /// <summary>
    /// 테스트 진행 상황 패널
    /// - 5개 체크포인트 상태 표시
    /// - 진행률 바
    /// - 현재 테스트 메시지
    /// </summary>
    public class TestProgressPanel : MonoBehaviour
    {
        [Header("Colors")]
        [SerializeField] private Color _pendingColor = new Color(0.5f, 0.5f, 0.5f);
        [SerializeField] private Color _inProgressColor = new Color(1f, 0.8f, 0.3f);
        [SerializeField] private Color _completedColor = new Color(0.3f, 1f, 0.3f);
        [SerializeField] private Color _failedColor = new Color(1f, 0.3f, 0.3f);

        // UI 컴포넌트
        private Canvas _canvas;
        private Text _titleText;
        private Slider _progressBar;
        private Text _progressText;
        private Text _currentMessageText;
        private List<CheckpointEntry> _checkpointEntries;

        private const float UI_WIDTH = 500f;
        private const float UI_HEIGHT = 400f;
        private const float CHECKPOINT_HEIGHT = 30f;

        // 체크포인트 정의
        private readonly string[] _checkpointNames = new string[]
        {
            "체크포인트 #1: 플랫폼 이동 검증",
            "체크포인트 #2: 전투 진입 검증",
            "체크포인트 #3: EX 스킬 사용 (코스트 소모 포함)",
            "체크포인트 #4: 전투별 데미지 추적",
            "체크포인트 #5: 보상 획득 검증"
        };

        private void Awake()
        {
            _checkpointEntries = new List<CheckpointEntry>();
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
            _canvas.sortingOrder = 100; // 최상위 표시

            var canvasScaler = gameObject.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920, 1080);

            gameObject.AddComponent<GraphicRaycaster>();

            // 배경 패널
            var bgPanel = CreatePanel("BackgroundPanel", new Vector2(UI_WIDTH, UI_HEIGHT), Vector2.zero);
            var bgImage = bgPanel.GetComponent<Image>();
            bgImage.sprite = CreateWhiteSprite();
            bgImage.color = new Color(0.05f, 0.05f, 0.1f, 0.95f);

            // Anchored Position (화면 중앙 상단)
            var bgRect = bgPanel.GetComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0.5f, 1f);
            bgRect.anchorMax = new Vector2(0.5f, 1f);
            bgRect.pivot = new Vector2(0.5f, 1f);
            bgRect.anchoredPosition = new Vector2(0, -20);

            // 제목
            var titleObj = new GameObject("Title");
            titleObj.transform.SetParent(bgPanel.transform, false);

            var titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.sizeDelta = new Vector2(UI_WIDTH - 40, 40);
            titleRect.anchoredPosition = new Vector2(0, -25 + 180);

            _titleText = titleObj.AddComponent<Text>();
            _titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _titleText.fontSize = 24;
            _titleText.alignment = TextAnchor.MiddleCenter;
            _titleText.color = new Color(0.9f, 0.9f, 1f);
            _titleText.text = "블루 아카이브 자동화 테스트";
            _titleText.fontStyle = FontStyle.Bold;

            // 체크포인트 목록
            CreateCheckpointList(bgPanel.transform);

            // 진행률 바
            CreateProgressBar(bgPanel.transform);

            // 현재 메시지
            var messageObj = new GameObject("CurrentMessage");
            messageObj.transform.SetParent(bgPanel.transform, false);

            var messageRect = messageObj.AddComponent<RectTransform>();
            messageRect.sizeDelta = new Vector2(UI_WIDTH - 40, 40);
            messageRect.anchoredPosition = new Vector2(0, -UI_HEIGHT + 30 + 180);

            _currentMessageText = messageObj.AddComponent<Text>();
            _currentMessageText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _currentMessageText.fontSize = 16;
            _currentMessageText.alignment = TextAnchor.MiddleCenter;
            _currentMessageText.color = Color.white;
            _currentMessageText.text = "테스트 준비 중...";

            Debug.Log("[TestProgressPanel] UI 생성 완료");
        }

        /// <summary>
        /// 체크포인트 목록 생성
        /// </summary>
        private void CreateCheckpointList(Transform parent)
        {
            float startY = -80 + 180;

            for (int i = 0; i < _checkpointNames.Length; i++)
            {
                var entry = CreateCheckpointEntry(parent, i, startY - (i * (CHECKPOINT_HEIGHT + 5)));
                _checkpointEntries.Add(entry);
            }
        }

        /// <summary>
        /// 체크포인트 엔트리 생성
        /// </summary>
        private CheckpointEntry CreateCheckpointEntry(Transform parent, int index, float yPos)
        {
            var entry = new CheckpointEntry();

            // 배경
            var entryObj = new GameObject($"Checkpoint_{index + 1}");
            entryObj.transform.SetParent(parent, false);

            var entryRect = entryObj.AddComponent<RectTransform>();
            entryRect.sizeDelta = new Vector2(UI_WIDTH - 40, CHECKPOINT_HEIGHT);
            entryRect.anchoredPosition = new Vector2(0, yPos);

            var entryImage = entryObj.AddComponent<Image>();
            entryImage.sprite = CreateWhiteSprite();
            entryImage.color = new Color(0.1f, 0.1f, 0.15f, 0.8f);

            entry.BackgroundImage = entryImage;

            // 상태 아이콘
            var iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(entryObj.transform, false);

            var iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.sizeDelta = new Vector2(30, 30);
            iconRect.anchoredPosition = new Vector2(-UI_WIDTH / 2 + 35, 0);

            entry.IconText = iconObj.AddComponent<Text>();
            entry.IconText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            entry.IconText.fontSize = 18;
            entry.IconText.alignment = TextAnchor.MiddleCenter;
            entry.IconText.color = _pendingColor;
            entry.IconText.text = "⏳";
            entry.IconText.fontStyle = FontStyle.Bold;

            // 체크포인트 이름
            var nameObj = new GameObject("Name");
            nameObj.transform.SetParent(entryObj.transform, false);

            var nameRect = nameObj.AddComponent<RectTransform>();
            nameRect.sizeDelta = new Vector2(UI_WIDTH - 100, 30);
            nameRect.anchoredPosition = new Vector2(20, 0);

            entry.NameText = nameObj.AddComponent<Text>();
            entry.NameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            entry.NameText.fontSize = 14;
            entry.NameText.alignment = TextAnchor.MiddleLeft;
            entry.NameText.color = Color.white;
            entry.NameText.text = _checkpointNames[index];

            entry.Status = CheckpointStatus.Pending;

            return entry;
        }

        /// <summary>
        /// 진행률 바 생성
        /// </summary>
        private void CreateProgressBar(Transform parent)
        {
            var progressObj = new GameObject("ProgressBar");
            progressObj.transform.SetParent(parent, false);

            var progressRect = progressObj.AddComponent<RectTransform>();
            progressRect.sizeDelta = new Vector2(UI_WIDTH - 80, 25);
            progressRect.anchoredPosition = new Vector2(0, -UI_HEIGHT + 80 + 180);

            _progressBar = progressObj.AddComponent<Slider>();
            _progressBar.interactable = false;
            _progressBar.minValue = 0f;
            _progressBar.maxValue = 1f;
            _progressBar.value = 0f;

            // Background
            var bgObj = new GameObject("Background");
            bgObj.transform.SetParent(progressObj.transform, false);

            var bgRect = bgObj.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;

            var bgImage = bgObj.AddComponent<Image>();
            bgImage.sprite = CreateWhiteSprite();
            bgImage.color = new Color(0.2f, 0.2f, 0.3f);

            // Fill Area
            var fillAreaObj = new GameObject("Fill Area");
            fillAreaObj.transform.SetParent(progressObj.transform, false);

            var fillAreaRect = fillAreaObj.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.sizeDelta = Vector2.zero;

            // Fill
            var fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(fillAreaObj.transform, false);

            var fillRect = fillObj.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.sizeDelta = Vector2.zero;

            var fillImage = fillObj.AddComponent<Image>();
            fillImage.sprite = CreateWhiteSprite();
            fillImage.color = _inProgressColor;

            _progressBar.fillRect = fillRect;

            // 진행률 텍스트
            var progressTextObj = new GameObject("ProgressText");
            progressTextObj.transform.SetParent(progressObj.transform, false);

            var progressTextRect = progressTextObj.AddComponent<RectTransform>();
            progressTextRect.sizeDelta = new Vector2(UI_WIDTH - 80, 25);
            progressTextRect.anchoredPosition = Vector2.zero;

            _progressText = progressTextObj.AddComponent<Text>();
            _progressText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _progressText.fontSize = 14;
            _progressText.alignment = TextAnchor.MiddleCenter;
            _progressText.color = Color.white;
            _progressText.text = "0 / 5";
            _progressText.fontStyle = FontStyle.Bold;
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
        /// 체크포인트 상태 업데이트
        /// </summary>
        public void UpdateCheckpoint(int checkpointIndex, CheckpointStatus status)
        {
            if (checkpointIndex < 1 || checkpointIndex > _checkpointEntries.Count)
            {
                Debug.LogError($"[TestProgressPanel] 잘못된 체크포인트 인덱스: {checkpointIndex}");
                return;
            }

            var entry = _checkpointEntries[checkpointIndex - 1];
            entry.Status = status;

            // 아이콘 및 색상 업데이트
            switch (status)
            {
                case CheckpointStatus.Pending:
                    entry.IconText.text = "⏳";
                    entry.IconText.color = _pendingColor;
                    entry.BackgroundImage.color = new Color(0.1f, 0.1f, 0.15f, 0.8f);
                    break;

                case CheckpointStatus.InProgress:
                    entry.IconText.text = "▶";
                    entry.IconText.color = _inProgressColor;
                    entry.BackgroundImage.color = new Color(0.2f, 0.15f, 0.05f, 0.9f);
                    break;

                case CheckpointStatus.Completed:
                    entry.IconText.text = "✅";
                    entry.IconText.color = _completedColor;
                    entry.BackgroundImage.color = new Color(0.05f, 0.15f, 0.05f, 0.9f);
                    break;

                case CheckpointStatus.Failed:
                    entry.IconText.text = "❌";
                    entry.IconText.color = _failedColor;
                    entry.BackgroundImage.color = new Color(0.15f, 0.05f, 0.05f, 0.9f);
                    break;
            }

            // 진행률 업데이트
            UpdateProgress();

            Debug.Log($"[TestProgressPanel] 체크포인트 #{checkpointIndex} 상태 업데이트: {status}");
        }

        /// <summary>
        /// 진행률 업데이트
        /// </summary>
        private void UpdateProgress()
        {
            int completedCount = 0;
            int totalCount = _checkpointEntries.Count;

            foreach (var entry in _checkpointEntries)
            {
                if (entry.Status == CheckpointStatus.Completed)
                {
                    completedCount++;
                }
            }

            float progress = (float)completedCount / totalCount;
            _progressBar.value = progress;
            _progressText.text = $"{completedCount} / {totalCount}";

            // 완료 시 색상 변경
            if (completedCount == totalCount)
            {
                var fillImage = _progressBar.fillRect.GetComponent<Image>();
                fillImage.color = _completedColor;
            }
        }

        /// <summary>
        /// 현재 메시지 업데이트
        /// </summary>
        public void UpdateMessage(string message)
        {
            if (_currentMessageText != null)
            {
                _currentMessageText.text = message;
                Debug.Log($"[TestProgressPanel] 메시지 업데이트: {message}");
            }
        }

        /// <summary>
        /// 모든 체크포인트 리셋
        /// </summary>
        public void ResetAll()
        {
            foreach (var entry in _checkpointEntries)
            {
                entry.Status = CheckpointStatus.Pending;
                entry.IconText.text = "⏳";
                entry.IconText.color = _pendingColor;
                entry.BackgroundImage.color = new Color(0.1f, 0.1f, 0.15f, 0.8f);
            }

            _progressBar.value = 0f;
            _progressText.text = "0 / 5";
            _currentMessageText.text = "테스트 준비 중...";

            var fillImage = _progressBar.fillRect.GetComponent<Image>();
            fillImage.color = _inProgressColor;

            Debug.Log("[TestProgressPanel] 모든 체크포인트 리셋");
        }

        /// <summary>
        /// 체크포인트 엔트리
        /// </summary>
        private class CheckpointEntry
        {
            public Image BackgroundImage;
            public Text IconText;
            public Text NameText;
            public CheckpointStatus Status;
        }
    }

    /// <summary>
    /// 체크포인트 상태
    /// </summary>
    public enum CheckpointStatus
    {
        Pending,      // 대기 중
        InProgress,   // 진행 중
        Completed,    // 완료
        Failed        // 실패
    }
}
