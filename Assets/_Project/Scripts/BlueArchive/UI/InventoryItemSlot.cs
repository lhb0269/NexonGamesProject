using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using NexonGame.BlueArchive.Data;

namespace NexonGame.BlueArchive.UI
{
    /// <summary>
    /// 인벤토리 아이템 슬롯
    /// - 아이템 정보 표시 (타입, 이름, 수량)
    /// - 추가 애니메이션 효과
    /// </summary>
    public class InventoryItemSlot : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image _iconImage;
        [SerializeField] private Text _itemNameText;
        [SerializeField] private Text _quantityText;

        [Header("Animation Settings")]
        [SerializeField] private float _animationDuration = 0.3f;
        [SerializeField] private Color _highlightColor = Color.yellow;

        private RewardItemType _itemType;
        private string _itemName;
        private int _quantity;
        private Color _originalBackgroundColor;

        /// <summary>
        /// 초기화
        /// </summary>
        public void Initialize(RewardItemType itemType, string itemName, int quantity)
        {
            _itemType = itemType;
            _itemName = itemName;
            _quantity = quantity;

            CreateSlotUI();
            UpdateDisplay();
        }

        /// <summary>
        /// 슬롯 UI 생성
        /// </summary>
        private void CreateSlotUI()
        {
            // 배경
            var backgroundObj = new GameObject("Background");
            backgroundObj.transform.SetParent(transform, false);

            var bgRect = backgroundObj.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;

            _backgroundImage = backgroundObj.AddComponent<Image>();
            _backgroundImage.color = new Color(0.2f, 0.2f, 0.25f, 0.8f);
            _originalBackgroundColor = _backgroundImage.color;

            // 아이콘 (왼쪽)
            CreateIcon(transform);

            // 아이템 이름 (중앙)
            CreateItemNameText(transform);

            // 수량 (오른쪽)
            CreateQuantityText(transform);
        }

        /// <summary>
        /// 아이콘 생성
        /// </summary>
        private void CreateIcon(Transform parent)
        {
            var iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(parent, false);

            var iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0, 0.5f);
            iconRect.anchorMax = new Vector2(0, 0.5f);
            iconRect.pivot = new Vector2(0, 0.5f);
            iconRect.anchoredPosition = new Vector2(10, 0);
            iconRect.sizeDelta = new Vector2(40, 40);

            _iconImage = iconObj.AddComponent<Image>();
            _iconImage.color = GetColorForItemType(_itemType);
        }

        /// <summary>
        /// 아이템 이름 텍스트 생성
        /// </summary>
        private void CreateItemNameText(Transform parent)
        {
            var nameObj = new GameObject("ItemName");
            nameObj.transform.SetParent(parent, false);

            var nameRect = nameObj.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0, 0.5f);
            nameRect.anchorMax = new Vector2(1, 0.5f);
            nameRect.pivot = new Vector2(0, 0.5f);
            nameRect.anchoredPosition = new Vector2(60, 0);
            nameRect.sizeDelta = new Vector2(-140, 30);

            _itemNameText = nameObj.AddComponent<Text>();
            _itemNameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _itemNameText.fontSize = 16;
            _itemNameText.alignment = TextAnchor.MiddleLeft;
            _itemNameText.color = Color.white;
        }

        /// <summary>
        /// 수량 텍스트 생성
        /// </summary>
        private void CreateQuantityText(Transform parent)
        {
            var quantityObj = new GameObject("Quantity");
            quantityObj.transform.SetParent(parent, false);

            var quantityRect = quantityObj.AddComponent<RectTransform>();
            quantityRect.anchorMin = new Vector2(1, 0.5f);
            quantityRect.anchorMax = new Vector2(1, 0.5f);
            quantityRect.pivot = new Vector2(1, 0.5f);
            quantityRect.anchoredPosition = new Vector2(-10, 0);
            quantityRect.sizeDelta = new Vector2(60, 30);

            _quantityText = quantityObj.AddComponent<Text>();
            _quantityText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _quantityText.fontSize = 18;
            _quantityText.fontStyle = FontStyle.Bold;
            _quantityText.alignment = TextAnchor.MiddleRight;
            _quantityText.color = new Color(1f, 0.9f, 0.3f); // 금색
        }

        /// <summary>
        /// 표시 업데이트
        /// </summary>
        private void UpdateDisplay()
        {
            if (_itemNameText != null)
            {
                _itemNameText.text = _itemName;
            }

            if (_quantityText != null)
            {
                _quantityText.text = $"x{_quantity}";
            }
        }

        /// <summary>
        /// 수량 업데이트
        /// </summary>
        public void UpdateQuantity(int newQuantity)
        {
            _quantity = newQuantity;
            UpdateDisplay();
        }

        /// <summary>
        /// 추가 애니메이션 재생
        /// </summary>
        public void PlayAddAnimation()
        {
            StartCoroutine(AddAnimationCoroutine());
        }

        /// <summary>
        /// 추가 애니메이션 코루틴
        /// </summary>
        private IEnumerator AddAnimationCoroutine()
        {
            float elapsed = 0f;

            // 1. 배경 하이라이트
            while (elapsed < _animationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / _animationDuration;

                if (_backgroundImage != null)
                {
                    _backgroundImage.color = Color.Lerp(_highlightColor, _originalBackgroundColor, t);
                }

                yield return null;
            }

            // 2. 수량 텍스트 펄스 효과
            if (_quantityText != null)
            {
                Vector3 originalScale = _quantityText.transform.localScale;
                float pulseTime = 0.2f;
                elapsed = 0f;

                // 확대
                while (elapsed < pulseTime)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / pulseTime;
                    _quantityText.transform.localScale = Vector3.Lerp(originalScale, originalScale * 1.3f, t);
                    yield return null;
                }

                // 축소
                elapsed = 0f;
                while (elapsed < pulseTime)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / pulseTime;
                    _quantityText.transform.localScale = Vector3.Lerp(originalScale * 1.3f, originalScale, t);
                    yield return null;
                }

                _quantityText.transform.localScale = originalScale;
            }
        }

        /// <summary>
        /// 아이템 타입별 색상 반환
        /// </summary>
        private Color GetColorForItemType(RewardItemType itemType)
        {
            return itemType switch
            {
                RewardItemType.Credit => new Color(1f, 0.84f, 0f), // 골드
                RewardItemType.AP => new Color(0.3f, 0.8f, 1f),    // 블루
                RewardItemType.Equipment => new Color(0.8f, 0.3f, 1f), // 퍼플
                RewardItemType.Material => new Color(0.3f, 1f, 0.5f),  // 그린
                RewardItemType.Experience => new Color(1f, 0.5f, 0.2f), // 오렌지
                _ => Color.white
            };
        }
    }
}
