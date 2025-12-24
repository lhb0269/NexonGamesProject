using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using NexonGame.BlueArchive.Character;
using NexonGame.BlueArchive.Combat;

namespace NexonGame.BlueArchive.UI
{
    /// <summary>
    /// 전투 상태 패널
    /// - 학생들의 HP, 스킬 쿨다운 표시
    /// - 학생별 데미지 통계 스크롤
    /// - 실시간 업데이트
    /// </summary>
    public class CombatStatusPanel : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private Color _hpFullColor = new Color(0.3f, 1f, 0.3f);
        [SerializeField] private Color _hpLowColor = new Color(1f, 0.3f, 0.3f);
        [SerializeField] private Color _skillReadyColor = new Color(0.3f, 0.8f, 1f);
        [SerializeField] private Color _skillCooldownColor = new Color(0.3f, 0.3f, 0.4f);

        // UI 컴포넌트
        private Canvas _canvas;
        private List<StudentStatusEntry> _studentEntries;
        private GameObject _damageStatsPanel;
        private GameObject _damageStatsContent;
        private ScrollRect _damageStatsScrollRect;
        private CombatLogSystem _combatLog;
        private Dictionary<string, DamageStatEntry> _damageStatEntries;
        private int _lastDamageUpdateCount;
        private float _lastUpdateLogTime;

        private const float UI_WIDTH = 350f;
        private const float ENTRY_HEIGHT = 60f;
        private const float SPACING = 5f;
        private const float DAMAGE_PANEL_HEIGHT = 200f;

        private void Awake()
        {
            _studentEntries = new List<StudentStatusEntry>();
            _damageStatEntries = new Dictionary<string, DamageStatEntry>();
            _lastDamageUpdateCount = 0;
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

            Debug.Log("[CombatStatusPanel] UI 생성 완료");
        }

        /// <summary>
        /// 학생 목록 초기화
        /// </summary>
        public void InitializeStudents(List<Student> students)
        {
            // 기존 엔트리 제거
            foreach (var entry in _studentEntries)
            {
                if (entry.RootObject != null)
                {
                    Destroy(entry.RootObject);
                }
            }
            _studentEntries.Clear();

            // 새 엔트리 생성
            for (int i = 0; i < students.Count; i++)
            {
                var entry = CreateStudentEntry(students[i], i);
                _studentEntries.Add(entry);
            }

            Debug.Log($"[CombatStatusPanel] {students.Count}명 학생 상태 UI 생성");
        }

        /// <summary>
        /// CombatLogSystem 설정 (데미지 통계용)
        /// </summary>
        public void SetCombatLog(CombatLogSystem combatLog)
        {
            _combatLog = combatLog;
            Debug.Log($"[CombatStatusPanel] CombatLog 설정: {(combatLog != null ? "성공" : "실패 (null)")}");
            CreateDamageStatsPanel();
        }

        /// <summary>
        /// 데미지 통계 패널 생성
        /// </summary>
        private void CreateDamageStatsPanel()
        {
            Debug.Log("[CombatStatusPanel] CreateDamageStatsPanel 시작");

            // 데미지 통계 패널 (학생 상태 아래에 위치)
            _damageStatsPanel = CreatePanel("DamageStatsPanel", new Vector2(UI_WIDTH, DAMAGE_PANEL_HEIGHT), Vector2.zero);
            Debug.Log($"[CombatStatusPanel] _damageStatsPanel 생성: {_damageStatsPanel != null}");
            var panelRect = _damageStatsPanel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(1f, 1f);
            panelRect.anchorMax = new Vector2(1f, 1f);
            panelRect.pivot = new Vector2(1f, 1f);

            // 학생 엔트리 개수에 따라 위치 조정 (학생 패널 아래)
            float yOffset = -80 - (_studentEntries.Count * (ENTRY_HEIGHT + SPACING)) - 10;
            panelRect.anchoredPosition = new Vector2(-20, yOffset);

            var panelBg = _damageStatsPanel.GetComponent<Image>();
            panelBg.sprite = CreateWhiteSprite();
            panelBg.color = new Color(0.1f, 0.1f, 0.15f, 1f); // Alpha를 1로 변경
            panelBg.raycastTarget = false; // 레이캐스트 비활성화

            Debug.Log($"[CombatStatusPanel] 패널 배경 색상: {panelBg.color}, 위치: {panelRect.anchoredPosition}");

            // 타이틀
            var titleObj = new GameObject("Title");
            titleObj.transform.SetParent(_damageStatsPanel.transform, false);
            var titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.sizeDelta = new Vector2(UI_WIDTH, 25);
            titleRect.anchoredPosition = new Vector2(0, DAMAGE_PANEL_HEIGHT / 2 - 15);

            var titleText = titleObj.AddComponent<Text>();
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleText.material = null; // Material을 null로 설정하여 기본 UI Shader 사용
            titleText.fontSize = 14;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.color = new Color(1f, 0.9f, 0.3f, 1f); // Alpha를 명시적으로 1로 설정
            titleText.fontStyle = FontStyle.Bold;
            titleText.text = "학생별 데미지 통계";
            titleText.raycastTarget = false;
            titleText.maskable = false;

            Debug.Log($"[CombatStatusPanel] 타이틀 텍스트 색상: {titleText.color}, 텍스트: '{titleText.text}'");

            // ScrollRect 생성
            _damageStatsScrollRect = _damageStatsPanel.AddComponent<ScrollRect>();

            // Viewport
            var viewportObj = new GameObject("Viewport");
            viewportObj.transform.SetParent(_damageStatsPanel.transform, false);
            var viewportRect = viewportObj.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = new Vector2(5, 5);
            viewportRect.offsetMax = new Vector2(-5, -30);

            var viewportMask = viewportObj.AddComponent<Mask>();
            viewportMask.showMaskGraphic = false;
            var viewportImage = viewportObj.AddComponent<Image>();
            viewportImage.color = Color.clear;

            // Content
            var contentObj = new GameObject("Content");
            contentObj.transform.SetParent(viewportObj.transform, false);
            var contentRect = contentObj.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.sizeDelta = new Vector2(0, 0);

            _damageStatsContent = contentObj;
            Debug.Log($"[CombatStatusPanel] _damageStatsContent 할당: {_damageStatsContent != null}");

            // ScrollRect 설정
            _damageStatsScrollRect.content = contentRect;
            _damageStatsScrollRect.viewport = viewportRect;
            _damageStatsScrollRect.horizontal = false;
            _damageStatsScrollRect.vertical = true;
            _damageStatsScrollRect.scrollSensitivity = 20f;

            Debug.Log("[CombatStatusPanel] 데미지 통계 패널 생성 완료");
        }

        /// <summary>
        /// 데미지 통계 업데이트
        /// </summary>
        private void UpdateDamageStats()
        {
            if (_combatLog == null)
            {
                Debug.LogWarning("[CombatStatusPanel] UpdateDamageStats: _combatLog가 null입니다!");
                return;
            }

            if (_damageStatsContent == null)
            {
                Debug.LogWarning("[CombatStatusPanel] UpdateDamageStats: _damageStatsContent가 null입니다!");
                return;
            }

            var stats = _combatLog.StudentDamageStats;

            // 데미지가 변경되지 않았으면 업데이트 하지 않음
            int currentTotalDamage = _combatLog.TotalDamageDealt;
            if (currentTotalDamage == _lastDamageUpdateCount && _damageStatEntries.Count > 0)
            {
                return;
            }

            // 디버그: 데미지 변경 감지
            if (currentTotalDamage != _lastDamageUpdateCount)
            {
                Debug.Log($"[CombatStatusPanel] 데미지 통계 업데이트: {_lastDamageUpdateCount} → {currentTotalDamage}, 학생 수: {stats.Count}");
                foreach (var kvp in stats)
                {
                    Debug.Log($"  - {kvp.Key}: {kvp.Value} DMG");
                }
            }

            _lastDamageUpdateCount = currentTotalDamage;

            if (stats.Count == 0)
            {
                // 모든 엔트리 숨기기
                foreach (var entry in _damageStatEntries.Values)
                {
                    entry.RootObject.SetActive(false);
                }

                // "아직 데이터가 없습니다" 메시지는 첫 생성시에만 표시
                if (_damageStatsContent.transform.childCount == 0)
                {
                    var noDataObj = new GameObject("NoData");
                    noDataObj.transform.SetParent(_damageStatsContent.transform, false);
                    var noDataRect = noDataObj.AddComponent<RectTransform>();
                    noDataRect.sizeDelta = new Vector2(UI_WIDTH - 20, 30);
                    noDataRect.anchoredPosition = new Vector2(0, -15);

                    var noDataText = noDataObj.AddComponent<Text>();
                    noDataText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                    noDataText.material = null; // Material을 null로 설정하여 기본 UI Shader 사용
                    noDataText.fontSize = 12;
                    noDataText.alignment = TextAnchor.MiddleCenter;
                    noDataText.color = new Color(0.6f, 0.6f, 0.6f, 1f); // Alpha를 1로 명시
                    noDataText.text = "아직 데미지 기록이 없습니다";
                    noDataText.raycastTarget = false;
                    noDataText.maskable = false;
                }

                _damageStatsContent.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 30);
                return;
            }

            // "아직 데이터가 없습니다" 메시지 제거
            var noDataMsg = _damageStatsContent.transform.Find("NoData");
            if (noDataMsg != null)
            {
                Destroy(noDataMsg.gameObject);
            }

            // 데미지 순으로 정렬
            var sortedStats = new List<KeyValuePair<string, int>>(stats);
            sortedStats.Sort((a, b) => b.Value.CompareTo(a.Value));

            // 각 학생별 데미지 엔트리 업데이트 또는 생성
            float yPos = 0;
            var processedStudents = new HashSet<string>();

            foreach (var kvp in sortedStats)
            {
                processedStudents.Add(kvp.Key);

                // 기존 엔트리가 있으면 재사용
                if (_damageStatEntries.TryGetValue(kvp.Key, out var entry))
                {
                    entry.RootObject.SetActive(true);
                    entry.DamageText.text = $"{kvp.Value:N0} DMG";
                    entry.RootObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, yPos);
                }
                else
                {
                    // 새 엔트리 생성
                    entry = CreateDamageStatEntry(kvp.Key, kvp.Value, yPos);
                    _damageStatEntries[kvp.Key] = entry;
                }

                yPos -= 35; // 엔트리 높이(30) + 간격(5)
            }

            // 더 이상 사용하지 않는 엔트리 숨기기
            foreach (var kvp in _damageStatEntries)
            {
                if (!processedStudents.Contains(kvp.Key))
                {
                    kvp.Value.RootObject.SetActive(false);
                }
            }

            // Content 높이 조정
            _damageStatsContent.GetComponent<RectTransform>().sizeDelta = new Vector2(0, Mathf.Abs(yPos));
        }

        /// <summary>
        /// 데미지 통계 엔트리 생성 (StudentEntry 구조 참고)
        /// </summary>
        private DamageStatEntry CreateDamageStatEntry(string studentName, int damage, float yPos)
        {
            var entry = new DamageStatEntry();

            // 배경 패널
            var entryObj = new GameObject($"DamageEntry_{studentName}");
            entryObj.transform.SetParent(_damageStatsContent.transform, false);
            var entryRect = entryObj.AddComponent<RectTransform>();
            entryRect.sizeDelta = new Vector2(UI_WIDTH - 20, 30);
            entryRect.anchoredPosition = new Vector2(0, yPos);

            var entryBg = entryObj.AddComponent<Image>();
            entryBg.sprite = CreateWhiteSprite();
            entryBg.color = new Color(0.15f, 0.15f, 0.2f, 1f);
            entryBg.raycastTarget = false;

            entry.RootObject = entryObj;

            // 학생 이름 (StudentEntry의 Name 텍스트와 동일한 방식)
            var nameTextObj = new GameObject("Name");
            nameTextObj.transform.SetParent(entryObj.transform, false);

            var nameTextRect = nameTextObj.AddComponent<RectTransform>();
            nameTextRect.sizeDelta = new Vector2(200, 25);
            nameTextRect.anchoredPosition = new Vector2(-60, 0);

            entry.NameText = nameTextObj.AddComponent<Text>();
            entry.NameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            entry.NameText.material = null; // Material을 null로 설정하여 기본 UI Shader 사용
            entry.NameText.fontSize = 14;
            entry.NameText.alignment = TextAnchor.MiddleLeft;
            entry.NameText.color = Color.white;
            entry.NameText.text = $"  {studentName}";
            entry.NameText.raycastTarget = false;
            entry.NameText.maskable = false; // Mask 영향 받지 않음

            // 데미지 (StudentEntry의 HP 텍스트와 동일한 방식)
            var damageTextObj = new GameObject("Damage");
            damageTextObj.transform.SetParent(entryObj.transform, false);

            var damageTextRect = damageTextObj.AddComponent<RectTransform>();
            damageTextRect.sizeDelta = new Vector2(100, 25);
            damageTextRect.anchoredPosition = new Vector2(120, 0);

            entry.DamageText = damageTextObj.AddComponent<Text>();
            entry.DamageText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            entry.DamageText.material = null; // Material을 null로 설정하여 기본 UI Shader 사용
            entry.DamageText.fontSize = 14;
            entry.DamageText.alignment = TextAnchor.MiddleRight;
            entry.DamageText.color = new Color(1f, 0.6f, 0.2f, 1f);
            entry.DamageText.fontStyle = FontStyle.Bold;
            entry.DamageText.text = $"{damage:N0} DMG";
            entry.DamageText.raycastTarget = false;
            entry.DamageText.maskable = false; // Mask 영향 받지 않음

            Debug.Log($"[CombatStatusPanel] 데미지 엔트리 생성: {studentName} - {damage} DMG (Y: {yPos})");

            return entry;
        }

        /// <summary>
        /// 학생 상태 엔트리 생성
        /// </summary>
        private StudentStatusEntry CreateStudentEntry(Student student, int index)
        {
            var entry = new StudentStatusEntry();

            // 배경 패널
            var bgPanel = CreatePanel($"StudentEntry_{index}", new Vector2(UI_WIDTH, ENTRY_HEIGHT), Vector2.zero);
            var bgImage = bgPanel.GetComponent<Image>();
            bgImage.sprite = CreateWhiteSprite();
            bgImage.color = new Color(0.1f, 0.1f, 0.15f, 0.9f);

            // Anchored Position (화면 오른쪽 상단)
            var bgRect = bgPanel.GetComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(1f, 1f);
            bgRect.anchorMax = new Vector2(1f, 1f);
            bgRect.pivot = new Vector2(1f, 1f);
            bgRect.anchoredPosition = new Vector2(-20, -80 - (index * (ENTRY_HEIGHT + SPACING)));

            entry.RootObject = bgPanel;

            // 이름 텍스트
            var nameObj = new GameObject("Name");
            nameObj.transform.SetParent(bgPanel.transform, false);

            var nameRect = nameObj.AddComponent<RectTransform>();
            nameRect.sizeDelta = new Vector2(UI_WIDTH - 20, 20);
            nameRect.anchoredPosition = new Vector2(0, 15);

            entry.NameText = nameObj.AddComponent<Text>();
            entry.NameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            entry.NameText.fontSize = 16;
            entry.NameText.alignment = TextAnchor.MiddleLeft;
            entry.NameText.color = Color.white;
            entry.NameText.fontStyle = FontStyle.Bold;
            entry.NameText.text = $"  {student.Data.studentName}";

            // HP 슬라이더
            entry.HPSlider = CreateSlider(bgPanel.transform, "HPSlider", new Vector2(-10, -10), UI_WIDTH - 140);
            entry.HPFillImage = entry.HPSlider.fillRect.GetComponent<Image>();

            // HP 텍스트
            var hpTextObj = new GameObject("HPText");
            hpTextObj.transform.SetParent(bgPanel.transform, false);

            var hpTextRect = hpTextObj.AddComponent<RectTransform>();
            hpTextRect.sizeDelta = new Vector2(60, 20);
            hpTextRect.anchoredPosition = new Vector2(UI_WIDTH / 2 - 40, -10);

            entry.HPText = hpTextObj.AddComponent<Text>();
            entry.HPText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            entry.HPText.fontSize = 12;
            entry.HPText.alignment = TextAnchor.MiddleRight;
            entry.HPText.color = Color.white;

            // 스킬 쿨다운 텍스트
            var skillTextObj = new GameObject("SkillText");
            skillTextObj.transform.SetParent(bgPanel.transform, false);

            var skillTextRect = skillTextObj.AddComponent<RectTransform>();
            skillTextRect.sizeDelta = new Vector2(UI_WIDTH - 20, 15);
            skillTextRect.anchoredPosition = new Vector2(0, -13);

            entry.SkillText = skillTextObj.AddComponent<Text>();
            entry.SkillText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            entry.SkillText.fontSize = 11;
            entry.SkillText.alignment = TextAnchor.MiddleLeft;
            entry.SkillText.color = _skillReadyColor;
            entry.SkillText.text = "  스킬: 준비";

            entry.Student = student;

            return entry;
        }

        /// <summary>
        /// Slider 생성
        /// </summary>
        private Slider CreateSlider(Transform parent, string name, Vector2 position, float width)
        {
            var sliderObj = new GameObject(name);
            sliderObj.transform.SetParent(parent, false);

            var sliderRect = sliderObj.AddComponent<RectTransform>();
            sliderRect.sizeDelta = new Vector2(width, 15);
            sliderRect.anchoredPosition = position;

            var slider = sliderObj.AddComponent<Slider>();
            slider.interactable = false;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 1f;

            // Background
            var bgObj = new GameObject("Background");
            bgObj.transform.SetParent(sliderObj.transform, false);

            var bgRect = bgObj.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;

            var bgImage = bgObj.AddComponent<Image>();
            bgImage.sprite = CreateWhiteSprite();
            bgImage.color = new Color(0.2f, 0.2f, 0.3f);

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

            var fillImage = fillObj.AddComponent<Image>();
            fillImage.sprite = CreateWhiteSprite();
            fillImage.color = _hpFullColor;

            slider.fillRect = fillRect;

            return slider;
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
        /// 매 프레임 업데이트 (스킬 쿨다운 + 데미지 통계 실시간 갱신)
        /// </summary>
        private void Update()
        {
            // 1초마다 Update 호출 확인 로그
            if (Time.time - _lastUpdateLogTime > 1f)
            {
                _lastUpdateLogTime = Time.time;
                Debug.Log($"[CombatStatusPanel] Update 호출 중 (CombatLog: {_combatLog != null}, Content: {_damageStatsContent != null})");
            }

            UpdateAllStudents();
            UpdateDamageStats();
        }

        private void OnEnable()
        {
            Debug.Log("[CombatStatusPanel] OnEnable 호출됨");
        }

        private void OnDisable()
        {
            Debug.Log("[CombatStatusPanel] OnDisable 호출됨");
        }

        /// <summary>
        /// 모든 학생 상태 업데이트
        /// </summary>
        public void UpdateAllStudents()
        {
            foreach (var entry in _studentEntries)
            {
                UpdateStudentEntry(entry);
            }
        }

        /// <summary>
        /// 학생 상태 엔트리 업데이트
        /// </summary>
        private void UpdateStudentEntry(StudentStatusEntry entry)
        {
            if (entry.Student == null) return;

            // HP 업데이트
            float hpRatio = (float)entry.Student.CurrentHP / entry.Student.Data.maxHP;
            entry.HPSlider.value = hpRatio;
            entry.HPText.text = $"{entry.Student.CurrentHP}/{entry.Student.Data.maxHP}";

            // HP 색상
            entry.HPFillImage.color = Color.Lerp(_hpLowColor, _hpFullColor, hpRatio);

            // 스킬 쿨다운 업데이트
            if (entry.Student.SkillCooldownRemaining > 0)
            {
                entry.SkillText.text = $"  스킬: {entry.Student.SkillCooldownRemaining:F1}s";
                entry.SkillText.color = _skillCooldownColor;
            }
            else
            {
                entry.SkillText.text = "  스킬: 준비";
                entry.SkillText.color = _skillReadyColor;
            }
        }

        /// <summary>
        /// 학생 상태 엔트리
        /// </summary>
        private class StudentStatusEntry
        {
            public GameObject RootObject;
            public Student Student;
            public Text NameText;
            public Slider HPSlider;
            public Image HPFillImage;
            public Text HPText;
            public Text SkillText;
        }

        /// <summary>
        /// 데미지 통계 엔트리
        /// </summary>
        private class DamageStatEntry
        {
            public GameObject RootObject;
            public Text NameText;
            public Text DamageText;
        }
    }
}
