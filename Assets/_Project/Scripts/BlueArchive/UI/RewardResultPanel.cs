using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using NexonGame.BlueArchive.Reward;

namespace NexonGame.BlueArchive.UI
{
    /// <summary>
    /// 보상 결과 패널
    /// - 스테이지 클리어 메시지
    /// - 획득한 보상 목록
    /// - 전투 통계 표시
    /// </summary>
    public class RewardResultPanel : MonoBehaviour
    {
        [Header("Colors")]
        [SerializeField] private Color _titleColor = new Color(1f, 0.9f, 0.3f);
        [SerializeField] private Color _rewardColor = new Color(0.3f, 1f, 0.3f);
        [SerializeField] private Color _statsColor = new Color(0.7f, 0.7f, 0.7f);

        // UI 컴포넌트
        private Canvas _canvas;
        private GameObject _panelRoot;
        private Text _titleText;
        private Text _stageNameText;
        private Transform _rewardListContainer;
        private Text _statisticsText;
        private List<GameObject> _rewardEntries;

        private const float PANEL_WIDTH = 600f;
        private const float PANEL_HEIGHT = 500f;
        private const float REWARD_ENTRY_HEIGHT = 30f;

        private void Awake()
        {
            _rewardEntries = new List<GameObject>();
            CreateUIElements();
            HidePanel(); // 초기에는 숨김
        }

        /// <summary>
        /// UI 요소 생성
        /// </summary>
        private void CreateUIElements()
        {
            // Canvas 추가 (Screen Space - Overlay)
            _canvas = gameObject.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 200; // 최상위 표시

            var canvasScaler = gameObject.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920, 1080);

            gameObject.AddComponent<GraphicRaycaster>();

            // 반투명 배경
            var bgOverlay = CreatePanel("BackgroundOverlay", new Vector2(1920, 1080), Vector2.zero);
            var bgOverlayRect = bgOverlay.GetComponent<RectTransform>();
            bgOverlayRect.anchorMin = Vector2.zero;
            bgOverlayRect.anchorMax = Vector2.one;
            bgOverlayRect.sizeDelta = Vector2.zero;

            var bgOverlayImage = bgOverlay.GetComponent<Image>();
            bgOverlayImage.sprite = CreateWhiteSprite();
            bgOverlayImage.color = new Color(0f, 0f, 0f, 0.7f);

            // 메인 패널
            _panelRoot = CreatePanel("MainPanel", new Vector2(PANEL_WIDTH, PANEL_HEIGHT), Vector2.zero);
            var panelRect = _panelRoot.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;

            var panelImage = _panelRoot.GetComponent<Image>();
            panelImage.sprite = CreateWhiteSprite();
            panelImage.color = new Color(0.05f, 0.05f, 0.1f, 0.95f);

            // 제목
            var titleObj = new GameObject("Title");
            titleObj.transform.SetParent(_panelRoot.transform, false);

            var titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.sizeDelta = new Vector2(PANEL_WIDTH - 40, 50);
            titleRect.anchoredPosition = new Vector2(0, PANEL_HEIGHT / 2 - 40);

            _titleText = titleObj.AddComponent<Text>();
            _titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _titleText.fontSize = 28;
            _titleText.alignment = TextAnchor.MiddleCenter;
            _titleText.color = _titleColor;
            _titleText.text = "🎉 스테이지 클리어!";
            _titleText.fontStyle = FontStyle.Bold;

            // 스테이지 이름
            var stageNameObj = new GameObject("StageName");
            stageNameObj.transform.SetParent(_panelRoot.transform, false);

            var stageNameRect = stageNameObj.AddComponent<RectTransform>();
            stageNameRect.sizeDelta = new Vector2(PANEL_WIDTH - 40, 30);
            stageNameRect.anchoredPosition = new Vector2(0, PANEL_HEIGHT / 2 - 80);

            _stageNameText = stageNameObj.AddComponent<Text>();
            _stageNameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _stageNameText.fontSize = 20;
            _stageNameText.alignment = TextAnchor.MiddleCenter;
            _stageNameText.color = Color.white;
            _stageNameText.text = "Normal 1-4";

            // 보상 목록 라벨
            var rewardLabelObj = new GameObject("RewardLabel");
            rewardLabelObj.transform.SetParent(_panelRoot.transform, false);

            var rewardLabelRect = rewardLabelObj.AddComponent<RectTransform>();
            rewardLabelRect.sizeDelta = new Vector2(PANEL_WIDTH - 40, 30);
            rewardLabelRect.anchoredPosition = new Vector2(0, PANEL_HEIGHT / 2 - 130);

            var rewardLabel = rewardLabelObj.AddComponent<Text>();
            rewardLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            rewardLabel.fontSize = 18;
            rewardLabel.alignment = TextAnchor.MiddleLeft;
            rewardLabel.color = new Color(0.9f, 0.9f, 1f);
            rewardLabel.text = "  획득 보상:";
            rewardLabel.fontStyle = FontStyle.Bold;

            // 보상 목록 컨테이너
            var rewardContainerObj = new GameObject("RewardContainer");
            rewardContainerObj.transform.SetParent(_panelRoot.transform, false);

            var rewardContainerRect = rewardContainerObj.AddComponent<RectTransform>();
            rewardContainerRect.sizeDelta = new Vector2(PANEL_WIDTH - 60, 150);
            rewardContainerRect.anchoredPosition = new Vector2(0, PANEL_HEIGHT / 2 - 220);

            _rewardListContainer = rewardContainerObj.transform;

            // 통계 라벨
            var statsLabelObj = new GameObject("StatsLabel");
            statsLabelObj.transform.SetParent(_panelRoot.transform, false);

            var statsLabelRect = statsLabelObj.AddComponent<RectTransform>();
            statsLabelRect.sizeDelta = new Vector2(PANEL_WIDTH - 40, 30);
            statsLabelRect.anchoredPosition = new Vector2(0, -PANEL_HEIGHT / 2 + 140);

            var statsLabel = statsLabelObj.AddComponent<Text>();
            statsLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            statsLabel.fontSize = 18;
            statsLabel.alignment = TextAnchor.MiddleLeft;
            statsLabel.color = new Color(0.9f, 0.9f, 1f);
            statsLabel.text = "  전투 통계:";
            statsLabel.fontStyle = FontStyle.Bold;

            // 통계 텍스트
            var statsTextObj = new GameObject("StatsText");
            statsTextObj.transform.SetParent(_panelRoot.transform, false);

            var statsTextRect = statsTextObj.AddComponent<RectTransform>();
            statsTextRect.sizeDelta = new Vector2(PANEL_WIDTH - 60, 80);
            statsTextRect.anchoredPosition = new Vector2(0, -PANEL_HEIGHT / 2 + 70);

            _statisticsText = statsTextObj.AddComponent<Text>();
            _statisticsText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _statisticsText.fontSize = 14;
            _statisticsText.alignment = TextAnchor.UpperLeft;
            _statisticsText.color = _statsColor;
            _statisticsText.text = "";

            Debug.Log("[RewardResultPanel] UI 생성 완료");
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
        /// 보상 결과 표시
        /// </summary>
        public void ShowRewards(string stageName, RewardGrantResult rewardResult, string statistics)
        {
            // 스테이지 이름 설정
            _stageNameText.text = stageName;

            // 기존 보상 엔트리 제거
            ClearRewardEntries();

            // 보상 목록 생성
            for (int i = 0; i < rewardResult.GrantedRewards.Count; i++)
            {
                var reward = rewardResult.GrantedRewards[i];
                CreateRewardEntry(reward, i);
            }

            // 통계 설정
            _statisticsText.text = statistics;

            // 패널 표시
            ShowPanel();

            Debug.Log($"[RewardResultPanel] 보상 결과 표시: {rewardResult.GrantedRewards.Count}개 보상");
        }

        /// <summary>
        /// 보상 엔트리 생성
        /// </summary>
        private void CreateRewardEntry(RewardItemData reward, int index)
        {
            var entryObj = new GameObject($"RewardEntry_{index}");
            entryObj.transform.SetParent(_rewardListContainer, false);

            var entryRect = entryObj.AddComponent<RectTransform>();
            entryRect.sizeDelta = new Vector2(PANEL_WIDTH - 60, REWARD_ENTRY_HEIGHT);
            entryRect.anchoredPosition = new Vector2(0, -index * (REWARD_ENTRY_HEIGHT + 5));

            // 배경
            var entryBg = entryObj.AddComponent<Image>();
            entryBg.sprite = CreateWhiteSprite();
            entryBg.color = new Color(0.1f, 0.1f, 0.15f, 0.8f);

            // 아이콘 (간단한 텍스트)
            var iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(entryObj.transform, false);

            var iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.sizeDelta = new Vector2(30, 30);
            iconRect.anchoredPosition = new Vector2(-PANEL_WIDTH / 2 + 50, 0);

            var iconText = iconObj.AddComponent<Text>();
            iconText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            iconText.fontSize = 18;
            iconText.alignment = TextAnchor.MiddleCenter;
            iconText.color = _rewardColor;
            iconText.text = GetRewardIcon(reward.itemType);
            iconText.fontStyle = FontStyle.Bold;

            // 보상 이름
            var nameObj = new GameObject("Name");
            nameObj.transform.SetParent(entryObj.transform, false);

            var nameRect = nameObj.AddComponent<RectTransform>();
            nameRect.sizeDelta = new Vector2(PANEL_WIDTH - 200, 30);
            nameRect.anchoredPosition = new Vector2(-20, 0);

            var nameText = nameObj.AddComponent<Text>();
            nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            nameText.fontSize = 16;
            nameText.alignment = TextAnchor.MiddleLeft;
            nameText.color = Color.white;
            nameText.text = reward.itemName;

            // 수량
            var quantityObj = new GameObject("Quantity");
            quantityObj.transform.SetParent(entryObj.transform, false);

            var quantityRect = quantityObj.AddComponent<RectTransform>();
            quantityRect.sizeDelta = new Vector2(100, 30);
            quantityRect.anchoredPosition = new Vector2(PANEL_WIDTH / 2 - 80, 0);

            var quantityText = quantityObj.AddComponent<Text>();
            quantityText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            quantityText.fontSize = 16;
            quantityText.alignment = TextAnchor.MiddleRight;
            quantityText.color = _rewardColor;
            quantityText.text = $"x{reward.quantity}";
            quantityText.fontStyle = FontStyle.Bold;

            _rewardEntries.Add(entryObj);
        }

        /// <summary>
        /// 보상 타입별 아이콘
        /// </summary>
        private string GetRewardIcon(RewardItemType itemType)
        {
            return itemType switch
            {
                RewardItemType.Currency => "💰",
                RewardItemType.Material => "📦",
                RewardItemType.Equipment => "🎒",
                RewardItemType.Exp => "⭐",
                _ => "❓"
            };
        }

        /// <summary>
        /// 기존 보상 엔트리 제거
        /// </summary>
        private void ClearRewardEntries()
        {
            foreach (var entry in _rewardEntries)
            {
                if (entry != null)
                {
                    Destroy(entry);
                }
            }
            _rewardEntries.Clear();
        }

        /// <summary>
        /// 패널 표시
        /// </summary>
        public void ShowPanel()
        {
            gameObject.SetActive(true);
            Debug.Log("[RewardResultPanel] 패널 표시");
        }

        /// <summary>
        /// 패널 숨김
        /// </summary>
        public void HidePanel()
        {
            gameObject.SetActive(false);
            Debug.Log("[RewardResultPanel] 패널 숨김");
        }

        /// <summary>
        /// 패널 초기화
        /// </summary>
        public void ResetPanel()
        {
            ClearRewardEntries();
            _stageNameText.text = "";
            _statisticsText.text = "";
            HidePanel();

            Debug.Log("[RewardResultPanel] 패널 초기화");
        }
    }
}
