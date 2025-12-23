using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using NexonGame.BlueArchive.Character;

namespace NexonGame.BlueArchive.UI
{
    /// <summary>
    /// 전투 상태 패널
    /// - 학생들의 HP, 스킬 쿨다운 표시
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

        private const float UI_WIDTH = 350f;
        private const float ENTRY_HEIGHT = 60f;
        private const float SPACING = 5f;

        private void Awake()
        {
            _studentEntries = new List<StudentStatusEntry>();
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
            nameRect.anchoredPosition = new Vector2(0, -15);

            entry.NameText = nameObj.AddComponent<Text>();
            entry.NameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            entry.NameText.fontSize = 16;
            entry.NameText.alignment = TextAnchor.MiddleLeft;
            entry.NameText.color = Color.white;
            entry.NameText.fontStyle = FontStyle.Bold;
            entry.NameText.text = $"  {student.Data.studentName}";

            // HP 슬라이더
            entry.HPSlider = CreateSlider(bgPanel.transform, "HPSlider", new Vector2(-10, -35), UI_WIDTH - 140);
            entry.HPFillImage = entry.HPSlider.fillRect.GetComponent<Image>();

            // HP 텍스트
            var hpTextObj = new GameObject("HPText");
            hpTextObj.transform.SetParent(bgPanel.transform, false);

            var hpTextRect = hpTextObj.AddComponent<RectTransform>();
            hpTextRect.sizeDelta = new Vector2(60, 20);
            hpTextRect.anchoredPosition = new Vector2(UI_WIDTH / 2 - 40, -35);

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
            skillTextRect.anchoredPosition = new Vector2(0, -50);

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
    }
}
