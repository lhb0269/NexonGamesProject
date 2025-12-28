using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NexonGame.BlueArchive.Data;
using NexonGame.BlueArchive.Reward;

namespace NexonGame.BlueArchive.UI
{
    /// <summary>
    /// 인벤토리 패널
    /// - RewardSystem의 인벤토리 시각화
    /// - 아이템 추가 애니메이션
    /// - RewardSystem.OnRewardGranted 이벤트 구독
    /// </summary>
    public class InventoryPanel : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private RectTransform _contentArea;
        [SerializeField] private Text _titleText;
        [SerializeField] private Text _totalItemsText;

        [Header("Settings")]
        [SerializeField] private Vector2 _panelPosition = new Vector2(-400, 0);
        [SerializeField] private Vector2 _panelSize = new Vector2(350, 600);
        [SerializeField] private float _slotHeight = 60f;
        [SerializeField] private float _slotSpacing = 10f;

        private Canvas _canvas;
        private Dictionary<RewardItemType, InventoryItemSlot> _itemSlots;
        private RewardSystem _rewardSystem;
        private int _totalItems;

        private void Awake()
        {
            _itemSlots = new Dictionary<RewardItemType, InventoryItemSlot>();
            _totalItems = 0;
        }

        /// <summary>
        /// 초기화 - RewardSystem과 연결
        /// </summary>
        public void Initialize(RewardSystem rewardSystem)
        {
            _rewardSystem = rewardSystem;

            // Canvas 설정
            SetupCanvas();

            // UI 생성
            CreatePanelUI();

            // RewardSystem 이벤트 구독
            _rewardSystem.OnRewardGranted += OnRewardAdded;

            Debug.Log("[InventoryPanel] 초기화 완료");
        }

        /// <summary>
        /// Canvas 설정 (Screen Space Overlay)
        /// </summary>
        private void SetupCanvas()
        {
            _canvas = gameObject.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 210; // RewardResultPanel(200)보다 높게

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
            rectTransform.anchorMin = new Vector2(1, 0.5f); // 오른쪽 중앙
            rectTransform.anchorMax = new Vector2(1, 0.5f);
            rectTransform.pivot = new Vector2(1, 0.5f);
            rectTransform.anchoredPosition = _panelPosition;
            rectTransform.sizeDelta = _panelSize;

            var panelImage = panelObj.AddComponent<Image>();
            panelImage.color = new Color(0.1f, 0.1f, 0.15f, 0.95f);

            // 타이틀
            CreateTitle(panelObj.transform);

            // 총 아이템 수 표시
            CreateTotalItemsText(panelObj.transform);

            // 스크롤 영역
            CreateScrollArea(panelObj.transform);
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
            _titleText.text = "인벤토리";
            _titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _titleText.fontSize = 24;
            _titleText.alignment = TextAnchor.MiddleCenter;
            _titleText.color = Color.white;
        }

        /// <summary>
        /// 총 아이템 수 텍스트 생성
        /// </summary>
        private void CreateTotalItemsText(Transform parent)
        {
            var totalObj = new GameObject("TotalItems");
            totalObj.transform.SetParent(parent, false);

            var rectTransform = totalObj.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.pivot = new Vector2(0.5f, 1);
            rectTransform.anchoredPosition = new Vector2(0, -55);
            rectTransform.sizeDelta = new Vector2(-20, 30);

            _totalItemsText = totalObj.AddComponent<Text>();
            _totalItemsText.text = "보유 아이템: 0개";
            _totalItemsText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _totalItemsText.fontSize = 16;
            _totalItemsText.alignment = TextAnchor.MiddleCenter;
            _totalItemsText.color = new Color(0.8f, 0.8f, 0.8f);
        }

        /// <summary>
        /// 스크롤 영역 생성
        /// </summary>
        private void CreateScrollArea(Transform parent)
        {
            // 스크롤 뷰 컨테이너
            var scrollObj = new GameObject("ScrollView");
            scrollObj.transform.SetParent(parent, false);

            var scrollRect = scrollObj.AddComponent<RectTransform>();
            scrollRect.anchorMin = new Vector2(0, 0);
            scrollRect.anchorMax = new Vector2(1, 1);
            scrollRect.pivot = new Vector2(0.5f, 0.5f);
            scrollRect.anchoredPosition = new Vector2(0, -50);
            scrollRect.sizeDelta = new Vector2(-20, -110); // 타이틀 + 총 아이템 수 공간

            // Content 영역
            var contentObj = new GameObject("Content");
            contentObj.transform.SetParent(scrollObj.transform, false);

            _contentArea = contentObj.AddComponent<RectTransform>();
            _contentArea.anchorMin = new Vector2(0, 1);
            _contentArea.anchorMax = new Vector2(1, 1);
            _contentArea.pivot = new Vector2(0.5f, 1);
            _contentArea.anchoredPosition = Vector2.zero;
            _contentArea.sizeDelta = new Vector2(0, 0); // 동적으로 조정

            // VerticalLayoutGroup 추가
            var layoutGroup = contentObj.AddComponent<VerticalLayoutGroup>();
            layoutGroup.childAlignment = TextAnchor.UpperCenter;
            layoutGroup.spacing = _slotSpacing;
            layoutGroup.padding = new RectOffset(10, 10, 10, 10);
            layoutGroup.childControlHeight = false;
            layoutGroup.childControlWidth = true;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.childForceExpandWidth = true;

            // ContentSizeFitter 추가
            var contentSizeFitter = contentObj.AddComponent<ContentSizeFitter>();
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        /// <summary>
        /// 보상 추가 이벤트 핸들러 (애니메이션 포함)
        /// </summary>
        private void OnRewardAdded(RewardItemData reward)
        {
            StartCoroutine(AddItemWithAnimation(reward));
        }

        /// <summary>
        /// 아이템 추가 애니메이션
        /// </summary>
        private IEnumerator AddItemWithAnimation(RewardItemData reward)
        {
            Debug.Log($"[InventoryPanel] 아이템 추가: {reward.itemName} x{reward.quantity}");

            // 기존 슬롯이 있으면 업데이트, 없으면 생성
            if (_itemSlots.ContainsKey(reward.itemType))
            {
                var slot = _itemSlots[reward.itemType];
                int currentQuantity = _rewardSystem.GetInventoryCount(reward.itemType);
                slot.UpdateQuantity(currentQuantity);
                slot.PlayAddAnimation();
            }
            else
            {
                // 새 슬롯 생성
                var slotObj = new GameObject($"Slot_{reward.itemType}");
                slotObj.transform.SetParent(_contentArea.transform, false);

                var slotRectTransform = slotObj.AddComponent<RectTransform>();
                slotRectTransform.sizeDelta = new Vector2(0, _slotHeight);

                var slot = slotObj.AddComponent<InventoryItemSlot>();
                slot.Initialize(reward.itemType, reward.itemName, reward.quantity);

                _itemSlots[reward.itemType] = slot;

                // 생성 애니메이션
                slot.PlayAddAnimation();
            }

            // 총 아이템 수 업데이트
            _totalItems++;
            UpdateTotalItemsDisplay();

            yield return null;
        }

        /// <summary>
        /// 총 아이템 수 표시 업데이트
        /// </summary>
        private void UpdateTotalItemsDisplay()
        {
            if (_totalItemsText != null)
            {
                int uniqueItems = _itemSlots.Count;
                _totalItemsText.text = $"보유 아이템: {uniqueItems}종류 (총 {_totalItems}개)";
            }
        }

        /// <summary>
        /// 인벤토리 데이터 가져오기
        /// </summary>
        public Dictionary<RewardItemType, int> GetInventoryData()
        {
            return _rewardSystem?.GetInventory();
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

        /// <summary>
        /// 정리
        /// </summary>
        private void OnDestroy()
        {
            if (_rewardSystem != null)
            {
                _rewardSystem.OnRewardGranted -= OnRewardAdded;
            }
        }
    }
}
