using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using NexonGame.BlueArchive.Data;
using NexonGame.BlueArchive.Reward;

namespace NexonGame.BlueArchive.UI
{
    /// <summary>
    /// 검증 결과 패널
    /// - 예상 보상 vs 실제 인벤토리 비교
    /// - RewardValidator 결과 시각화
    /// - ✅/❌ 표시
    /// </summary>
    public class ValidationResultPanel : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private Vector2 _panelPosition = new Vector2(0, -250);
        [SerializeField] private Vector2 _panelSize = new Vector2(800, 400);
        [SerializeField] private float _rowHeight = 40f;

        private Canvas _canvas;
        private Text _titleText;
        private Text _validationStatusText;
        private RectTransform _tableContentArea;
        private List<GameObject> _tableRows;

        private void Awake()
        {
            _tableRows = new List<GameObject>();
        }

        /// <summary>
        /// 초기화 및 검증 결과 표시
        /// </summary>
        public void ShowValidationResult(
            RewardValidationResult validationResult,
            RewardGrantResult rewardResult,
            Dictionary<RewardItemType, int> inventory)
        {
            // Canvas 설정
            SetupCanvas();

            // UI 생성
            CreatePanelUI();

            // 검증 상태 표시
            DisplayValidationStatus(validationResult);

            // 비교 테이블 생성
            CreateComparisonTable(rewardResult, inventory);

            Debug.Log($"[ValidationResultPanel] 검증 결과 표시: {(validationResult.IsValid ? "성공" : "실패")}");
        }

        /// <summary>
        /// Canvas 설정
        /// </summary>
        private void SetupCanvas()
        {
            _canvas = gameObject.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 220; // InventoryPanel(210)보다 높게

            var canvasScaler = gameObject.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920, 1080);

            gameObject.AddComponent<GraphicRaycaster>();
        }

        /// <summary>
        /// 패널 UI 생성
        /// </summary>
        private void CreatePanelUI()
        {
            // 배경 패널
            var panelObj = new GameObject("Panel");
            panelObj.transform.SetParent(transform, false);

            var rectTransform = panelObj.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = _panelPosition;
            rectTransform.sizeDelta = _panelSize;

            var panelImage = panelObj.AddComponent<Image>();
            panelImage.color = new Color(0.15f, 0.15f, 0.2f, 0.95f);

            // 타이틀
            CreateTitle(panelObj.transform);

            // 검증 상태 텍스트
            CreateValidationStatusText(panelObj.transform);

            // 테이블 헤더
            CreateTableHeader(panelObj.transform);

            // 테이블 컨텐츠 영역
            CreateTableContent(panelObj.transform);
        }

        /// <summary>
        /// 타이틀 생성
        /// </summary>
        private void CreateTitle(Transform parent)
        {
            var titleObj = new GameObject("Title");
            titleObj.transform.SetParent(parent, false);

            var rectTransform = titleObj.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.pivot = new Vector2(0.5f, 1);
            rectTransform.anchoredPosition = new Vector2(0, -10);
            rectTransform.sizeDelta = new Vector2(-20, 40);

            _titleText = titleObj.AddComponent<Text>();
            _titleText.text = "보상 검증 결과";
            _titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _titleText.fontSize = 24;
            _titleText.fontStyle = FontStyle.Bold;
            _titleText.alignment = TextAnchor.MiddleCenter;
            _titleText.color = Color.white;
        }

        /// <summary>
        /// 검증 상태 텍스트 생성
        /// </summary>
        private void CreateValidationStatusText(Transform parent)
        {
            var statusObj = new GameObject("ValidationStatus");
            statusObj.transform.SetParent(parent, false);

            var rectTransform = statusObj.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.pivot = new Vector2(0.5f, 1);
            rectTransform.anchoredPosition = new Vector2(0, -55);
            rectTransform.sizeDelta = new Vector2(-20, 30);

            _validationStatusText = statusObj.AddComponent<Text>();
            _validationStatusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _validationStatusText.fontSize = 18;
            _validationStatusText.alignment = TextAnchor.MiddleCenter;
        }

        /// <summary>
        /// 테이블 헤더 생성
        /// </summary>
        private void CreateTableHeader(Transform parent)
        {
            var headerObj = new GameObject("TableHeader");
            headerObj.transform.SetParent(parent, false);

            var headerRect = headerObj.AddComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0, 1);
            headerRect.anchorMax = new Vector2(1, 1);
            headerRect.pivot = new Vector2(0.5f, 1);
            headerRect.anchoredPosition = new Vector2(0, -95);
            headerRect.sizeDelta = new Vector2(-40, _rowHeight);

            var headerBg = headerObj.AddComponent<Image>();
            headerBg.color = new Color(0.25f, 0.25f, 0.3f, 1f);

            // 컬럼 헤더들
            CreateHeaderColumn(headerObj.transform, "아이템 이름", 0, 0.4f);
            CreateHeaderColumn(headerObj.transform, "예상 수량", 0.4f, 0.6f);
            CreateHeaderColumn(headerObj.transform, "실제 수량", 0.6f, 0.8f);
            CreateHeaderColumn(headerObj.transform, "검증", 0.8f, 1f);
        }

        /// <summary>
        /// 헤더 컬럼 생성
        /// </summary>
        private void CreateHeaderColumn(Transform parent, string headerText, float anchorMinX, float anchorMaxX)
        {
            var colObj = new GameObject($"Header_{headerText}");
            colObj.transform.SetParent(parent, false);

            var colRect = colObj.AddComponent<RectTransform>();
            colRect.anchorMin = new Vector2(anchorMinX, 0);
            colRect.anchorMax = new Vector2(anchorMaxX, 1);
            colRect.sizeDelta = Vector2.zero;

            var colText = colObj.AddComponent<Text>();
            colText.text = headerText;
            colText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            colText.fontSize = 16;
            colText.fontStyle = FontStyle.Bold;
            colText.alignment = TextAnchor.MiddleCenter;
            colText.color = new Color(0.9f, 0.9f, 0.9f);
        }

        /// <summary>
        /// 테이블 컨텐츠 영역 생성
        /// </summary>
        private void CreateTableContent(Transform parent)
        {
            var contentObj = new GameObject("TableContent");
            contentObj.transform.SetParent(parent, false);

            _tableContentArea = contentObj.AddComponent<RectTransform>();
            _tableContentArea.anchorMin = new Vector2(0, 0);
            _tableContentArea.anchorMax = new Vector2(1, 1);
            _tableContentArea.pivot = new Vector2(0.5f, 1);
            _tableContentArea.anchoredPosition = new Vector2(0, -95 - _rowHeight - 10);
            _tableContentArea.sizeDelta = new Vector2(-40, -150); // 헤더 + 타이틀 공간

            // VerticalLayoutGroup 추가
            var layoutGroup = contentObj.AddComponent<VerticalLayoutGroup>();
            layoutGroup.childAlignment = TextAnchor.UpperCenter;
            layoutGroup.spacing = 5f;
            layoutGroup.childControlHeight = false;
            layoutGroup.childControlWidth = true;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.childForceExpandWidth = true;
        }

        /// <summary>
        /// 검증 상태 표시
        /// </summary>
        private void DisplayValidationStatus(RewardValidationResult validationResult)
        {
            if (_validationStatusText == null) return;

            if (validationResult.IsValid)
            {
                _validationStatusText.text = "✅ 검증 성공 - 모든 보상이 정상적으로 인벤토리에 추가되었습니다";
                _validationStatusText.color = new Color(0.3f, 1f, 0.3f); // 녹색
            }
            else
            {
                _validationStatusText.text = $"❌ 검증 실패 - {validationResult.FailureReason}";
                _validationStatusText.color = new Color(1f, 0.3f, 0.3f); // 빨간색
            }
        }

        /// <summary>
        /// 비교 테이블 생성
        /// </summary>
        private void CreateComparisonTable(RewardGrantResult rewardResult, Dictionary<RewardItemType, int> inventory)
        {
            if (_tableContentArea == null) return;

            // 각 보상 아이템에 대해 행 생성
            foreach (var reward in rewardResult.GrantedRewards)
            {
                int expectedQuantity = reward.quantity;
                int actualQuantity = inventory.ContainsKey(reward.itemType) ? inventory[reward.itemType] : 0;
                bool isValid = actualQuantity >= expectedQuantity;

                CreateTableRow(reward.itemName, expectedQuantity, actualQuantity, isValid);
            }
        }

        /// <summary>
        /// 테이블 행 생성
        /// </summary>
        private void CreateTableRow(string itemName, int expected, int actual, bool isValid)
        {
            var rowObj = new GameObject($"Row_{itemName}");
            rowObj.transform.SetParent(_tableContentArea, false);

            var rowRect = rowObj.AddComponent<RectTransform>();
            rowRect.sizeDelta = new Vector2(0, _rowHeight);

            // 배경
            var rowBg = rowObj.AddComponent<Image>();
            rowBg.color = isValid
                ? new Color(0.2f, 0.3f, 0.2f, 0.5f)  // 녹색 배경
                : new Color(0.3f, 0.2f, 0.2f, 0.5f); // 빨간색 배경

            // 컬럼들
            CreateRowColumn(rowObj.transform, itemName, 0, 0.4f, TextAnchor.MiddleLeft);
            CreateRowColumn(rowObj.transform, expected.ToString(), 0.4f, 0.6f, TextAnchor.MiddleCenter);
            CreateRowColumn(rowObj.transform, actual.ToString(), 0.6f, 0.8f, TextAnchor.MiddleCenter);
            CreateRowColumn(rowObj.transform, isValid ? "✅" : "❌", 0.8f, 1f, TextAnchor.MiddleCenter);

            _tableRows.Add(rowObj);
        }

        /// <summary>
        /// 행 컬럼 생성
        /// </summary>
        private void CreateRowColumn(Transform parent, string text, float anchorMinX, float anchorMaxX, TextAnchor alignment)
        {
            var colObj = new GameObject($"Column_{text}");
            colObj.transform.SetParent(parent, false);

            var colRect = colObj.AddComponent<RectTransform>();
            colRect.anchorMin = new Vector2(anchorMinX, 0);
            colRect.anchorMax = new Vector2(anchorMaxX, 1);
            colRect.sizeDelta = Vector2.zero;
            colRect.offsetMin = new Vector2(5, 0);  // 왼쪽 패딩
            colRect.offsetMax = new Vector2(-5, 0); // 오른쪽 패딩

            var colText = colObj.AddComponent<Text>();
            colText.text = text;
            colText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            colText.fontSize = 14;
            colText.alignment = alignment;
            colText.color = Color.white;
        }

        /// <summary>
        /// 패널 숨기기
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// 패널 표시
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
        }
    }
}
