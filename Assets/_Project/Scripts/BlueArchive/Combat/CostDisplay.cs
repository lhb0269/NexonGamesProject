using UnityEngine;
using UnityEngine.UI;

namespace NexonGame.BlueArchive.Combat
{
    /// <summary>
    /// 코스트 UI 표시
    /// - 현재/최대 코스트 게이지 바 (Slider 사용)
    /// - 텍스트 라벨 (숫자)
    /// - 회복 중 시각 효과
    /// </summary>
    public class CostDisplay : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private Slider _costSlider;
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
        private float _targetValue;
        private float _currentValue;
        private float _valueVelocity;
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
        /// UI 요소 생성 (Slider 기반)
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
            var bgImage = bgPanel.GetComponent<Image>();
            bgImage.sprite = CreateWhiteSprite();
            bgImage.color = new Color(0.1f, 0.1f, 0.15f, 0.9f);

            // Anchored Position (화면 상단 중앙)
            var bgRect = bgPanel.GetComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0.5f, 1f);
            bgRect.anchorMax = new Vector2(0.5f, 1f);
            bgRect.pivot = new Vector2(0.5f, 1f);
            bgRect.anchoredPosition = new Vector2(0, -20);

            // Slider 생성
            CreateSlider(bgPanel.transform);

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

            Debug.Log("[CostDisplay] Slider 기반 UI 생성 완료");
        }

        /// <summary>
        /// Slider 생성
        /// </summary>
        private void CreateSlider(Transform parent)
        {
            var sliderObj = new GameObject("CostSlider");
            sliderObj.transform.SetParent(parent, false);

            var sliderRect = sliderObj.AddComponent<RectTransform>();
            sliderRect.sizeDelta = new Vector2(UI_WIDTH - 120, 20);
            sliderRect.anchoredPosition = new Vector2(-30, 0);

            _costSlider = sliderObj.AddComponent<Slider>();
            _costSlider.interactable = false; // 사용자 조작 불가
            _costSlider.minValue = 0f;
            _costSlider.maxValue = 1f;
            _costSlider.value = 0f;

            // Background
            var bgObj = new GameObject("Background");
            bgObj.transform.SetParent(sliderObj.transform, false);

            var bgRect = bgObj.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;

            var bgImage = bgObj.AddComponent<Image>();
            bgImage.sprite = CreateWhiteSprite();
            bgImage.color = _emptyColor;

            // Fill Area
            var fillAreaObj = new GameObject("Fill Area");
            fillAreaObj.transform.SetParent(sliderObj.transform, false);

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

            _fillImage = fillObj.AddComponent<Image>();
            _fillImage.sprite = CreateWhiteSprite();
            _fillImage.color = _fullColor;
            _fillImage.type = Image.Type.Filled;
            _fillImage.fillMethod = Image.FillMethod.Horizontal;
            _fillImage.fillOrigin = 0;

            // Slider에 Fill Area와 Fill 연결
            _costSlider.fillRect = fillRect;
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
        /// 코스트 업데이트
        /// </summary>
        public void UpdateCost(int currentCost, int maxCost, bool isRegenerating = false)
        {
            int prevCost = _currentCost;
            _currentCost = currentCost;
            _maxCost = maxCost;
            _isRegenerating = isRegenerating;

            // Slider value 계산 (0~1 범위)
            _targetValue = maxCost > 0 ? (float)currentCost / maxCost : 0f;

            // 텍스트 업데이트
            if (_costText != null)
            {
                _costText.text = $"{currentCost}/{maxCost}";
            }

            // 색상 업데이트
            UpdateColor();

            // 코스트 소모 시 즉시 반영
            if (currentCost < prevCost)
            {
                _currentValue = _targetValue;
                if (_costSlider != null)
                {
                    _costSlider.value = _currentValue;
                }
            }

            Debug.Log($"[CostDisplay] UpdateCost: {currentCost}/{maxCost}, Value: {_targetValue:F2}, Regenerating: {isRegenerating}");
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
                _fillImage.color = Color.Lerp(_emptyColor, _fullColor, _targetValue);
            }
        }

        /// <summary>
        /// 부드러운 Slider 애니메이션
        /// </summary>
        private void Update()
        {
            if (_costSlider == null) return;

            // 목표값과 차이가 있으면 업데이트
            if (Mathf.Abs(_currentValue - _targetValue) > 0.001f)
            {
                if (_isRegenerating)
                {
                    // 회복 중일 때는 부드럽게 애니메이션
                    _currentValue = Mathf.SmoothDamp(
                        _currentValue,
                        _targetValue,
                        ref _valueVelocity,
                        _smoothTime
                    );
                }
                else
                {
                    // 회복 중이 아니면 즉시 반영
                    _currentValue = _targetValue;
                }

                _costSlider.value = _currentValue;
            }
        }

        /// <summary>
        /// 디버그 정보
        /// </summary>
        public string GetDebugInfo()
        {
            return $"Cost: {_currentCost}/{_maxCost} (Value: {_currentValue:F2}, Regenerating: {_isRegenerating})";
        }
    }
}
