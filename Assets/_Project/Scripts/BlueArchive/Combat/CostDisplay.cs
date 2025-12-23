using UnityEngine;
using UnityEngine.UI;

namespace NexonGame.BlueArchive.Combat
{
    /// <summary>
    /// 코스트 UI 표시
    /// - 현재/최대 코스트 게이지 바
    /// - 텍스트 라벨 (숫자)
    /// - 회복 중 시각 효과
    /// </summary>
    public class CostDisplay : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private RectTransform _fillBar;
        [SerializeField] private Text _costText;
        [SerializeField] private Image _fillImage;

        [Header("Colors")]
        [SerializeField] private Color _fullColor = new Color(0.3f, 0.8f, 1f); // 파란색
        [SerializeField] private Color _emptyColor = new Color(0.2f, 0.2f, 0.3f); // 어두운 회색
        [SerializeField] private Color _regeneratingColor = new Color(0.5f, 1f, 0.8f); // 밝은 청록색

        [Header("Animation")]
        [SerializeField] private float _smoothTime = 0.2f;

        // 상태
        private int _currentCost;
        private int _maxCost;
        private float _targetFillAmount;
        private float _currentFillAmount;
        private float _fillVelocity;
        private bool _isRegenerating;

        // Canvas 관련
        private Canvas _canvas;
        private const float UI_WIDTH = 300f;
        private const float UI_HEIGHT = 40f;

        private void Awake()
        {
            CreateUIElements();
        }

        /// <summary>
        /// UI 요소 생성 (Canvas 기반)
        /// </summary>
        private void CreateUIElements()
        {
            // Canvas 추가 (Screen Space - Overlay)
            _canvas = gameObject.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 10;

            var canvasScaler = gameObject.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920, 1080);

            gameObject.AddComponent<GraphicRaycaster>();

            // 배경 패널
            var bgPanel = CreatePanel("BackgroundPanel", new Vector2(UI_WIDTH, UI_HEIGHT), Vector2.zero);
            bgPanel.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.15f, 0.9f);

            // Anchored Position (화면 상단 중앙)
            var bgRect = bgPanel.GetComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0.5f, 1f);
            bgRect.anchorMax = new Vector2(0.5f, 1f);
            bgRect.pivot = new Vector2(0.5f, 1f);
            bgRect.anchoredPosition = new Vector2(0, -20);

            // 게이지 바 배경
            var barBg = CreatePanel("BarBackground", new Vector2(UI_WIDTH - 120, 20), new Vector2(-30, 0));
            barBg.transform.SetParent(bgPanel.transform, false);
            barBg.GetComponent<Image>().color = _emptyColor;

            // 게이지 바 Fill
            var fillObj = CreatePanel("Fill", new Vector2(UI_WIDTH - 120, 20), new Vector2(-30, 0));
            fillObj.transform.SetParent(bgPanel.transform, false);
            _fillBar = fillObj.GetComponent<RectTransform>();
            _fillImage = fillObj.GetComponent<Image>();
            _fillImage.color = _fullColor;
            _fillImage.type = Image.Type.Filled;
            _fillImage.fillMethod = Image.FillMethod.Horizontal;
            _fillImage.fillAmount = 1f;

            // Fill Bar의 Anchor 설정 (왼쪽 정렬)
            _fillBar.anchorMin = new Vector2(0f, 0.5f);
            _fillBar.anchorMax = new Vector2(0f, 0.5f);
            _fillBar.pivot = new Vector2(0f, 0.5f);

            // 텍스트 라벨
            var textObj = new GameObject("CostText");
            textObj.transform.SetParent(bgPanel.transform, false);

            var textRect = textObj.AddComponent<RectTransform>();
            textRect.sizeDelta = new Vector2(100, 30);
            textRect.anchoredPosition = new Vector2(UI_WIDTH / 2 - 50, 0);

            _costText = textObj.AddComponent<Text>();
            _costText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _costText.fontSize = 20;
            _costText.alignment = TextAnchor.MiddleCenter;
            _costText.color = Color.white;
            _costText.text = "0/10";

            // 라벨 (COST)
            var labelObj = new GameObject("Label");
            labelObj.transform.SetParent(bgPanel.transform, false);

            var labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.sizeDelta = new Vector2(60, 30);
            labelRect.anchoredPosition = new Vector2(-UI_WIDTH / 2 + 35, 0);

            var labelText = labelObj.AddComponent<Text>();
            labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            labelText.fontSize = 16;
            labelText.alignment = TextAnchor.MiddleCenter;
            labelText.color = new Color(0.7f, 0.7f, 0.8f);
            labelText.text = "COST";
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
            image.color = Color.white;

            return obj;
        }

        /// <summary>
        /// 코스트 업데이트
        /// </summary>
        public void UpdateCost(int currentCost, int maxCost, bool isRegenerating = false)
        {
            int prevCost = _currentCost;
            _currentCost = currentCost;
            _maxCost = maxCost;
            _isRegenerating = isRegenerating;

            // Fill amount 계산
            _targetFillAmount = maxCost > 0 ? (float)currentCost / maxCost : 0f;

            // 텍스트 업데이트
            if (_costText != null)
            {
                _costText.text = $"{currentCost}/{maxCost}";
            }

            // 색상 업데이트
            UpdateColor();

            // 즉시 변화 (코스트 소모 시)
            if (currentCost < prevCost)
            {
                _currentFillAmount = _targetFillAmount;
                if (_fillImage != null)
                {
                    _fillImage.fillAmount = _currentFillAmount;
                }
            }
        }

        /// <summary>
        /// 색상 업데이트
        /// </summary>
        private void UpdateColor()
        {
            if (_fillImage == null) return;

            if (_isRegenerating)
            {
                _fillImage.color = _regeneratingColor;
            }
            else if (_currentCost >= _maxCost)
            {
                _fillImage.color = _fullColor;
            }
            else
            {
                _fillImage.color = Color.Lerp(_emptyColor, _fullColor, _targetFillAmount);
            }
        }

        /// <summary>
        /// 부드러운 Fill 애니메이션
        /// </summary>
        private void Update()
        {
            if (_fillImage == null) return;

            // 회복 중일 때만 부드럽게 애니메이션
            if (_isRegenerating && Mathf.Abs(_currentFillAmount - _targetFillAmount) > 0.01f)
            {
                _currentFillAmount = Mathf.SmoothDamp(
                    _currentFillAmount,
                    _targetFillAmount,
                    ref _fillVelocity,
                    _smoothTime
                );

                _fillImage.fillAmount = _currentFillAmount;
            }
            else if (!_isRegenerating)
            {
                _currentFillAmount = _targetFillAmount;
                _fillImage.fillAmount = _currentFillAmount;
            }
        }

        /// <summary>
        /// 디버그 정보
        /// </summary>
        public string GetDebugInfo()
        {
            return $"Cost: {_currentCost}/{_maxCost} (Fill: {_currentFillAmount:F2}, Regenerating: {_isRegenerating})";
        }
    }
}
