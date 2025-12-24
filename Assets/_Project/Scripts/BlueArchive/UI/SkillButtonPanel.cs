using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NexonGame.BlueArchive.Character;
using NexonGame.BlueArchive.Combat;

namespace NexonGame.BlueArchive.UI
{
    /// <summary>
    /// 스킬 버튼 패널
    /// - 코스트바 위에 학생 스킬 버튼들을 배치
    /// - 버튼 클릭 이벤트를 CombatManager에 전달
    /// </summary>
    public class SkillButtonPanel : MonoBehaviour
    {
        private Canvas _canvas;
        private List<StudentSkillButton> _skillButtons;
        private CombatManager _combatManager;
        private CostSystem _costSystem;

        private const float BUTTON_WIDTH = 80f;
        private const float BUTTON_HEIGHT = 70f;
        private const float BUTTON_SPACING = 10f;
        private const float PANEL_Y_OFFSET = 120f; // 코스트바 위 위치

        private void Awake()
        {
            _skillButtons = new List<StudentSkillButton>();
            CreateCanvas();
        }

        /// <summary>
        /// Canvas 생성
        /// </summary>
        private void CreateCanvas()
        {
            var canvasObj = new GameObject("Canvas");
            canvasObj.transform.SetParent(transform, false);

            _canvas = canvasObj.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 100;

            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            canvasObj.AddComponent<GraphicRaycaster>();
        }

        /// <summary>
        /// 초기화
        /// </summary>
        public void Initialize(List<Student> students, CombatManager combatManager, CostSystem costSystem)
        {
            _combatManager = combatManager;
            _costSystem = costSystem;

            // 기존 버튼 제거
            foreach (var button in _skillButtons)
            {
                if (button != null)
                    Destroy(button.gameObject);
            }
            _skillButtons.Clear();

            // 버튼 생성
            for (int i = 0; i < students.Count; i++)
            {
                CreateSkillButton(students[i], i, students.Count);
            }

            Debug.Log($"[SkillButtonPanel] {students.Count}개 스킬 버튼 생성 완료");
        }

        /// <summary>
        /// 스킬 버튼 생성
        /// </summary>
        private void CreateSkillButton(Student student, int index, int totalCount)
        {
            var buttonObj = new GameObject($"SkillButton_{student.Data.studentName}");
            buttonObj.transform.SetParent(_canvas.transform, false);

            var rectTransform = buttonObj.AddComponent<RectTransform>();

            // 화면 하단 중앙에 가로로 배치
            float totalWidth = (BUTTON_WIDTH * totalCount) + (BUTTON_SPACING * (totalCount - 1));
            float startX = -totalWidth / 2f + (BUTTON_WIDTH / 2f);
            float xPos = startX + index * (BUTTON_WIDTH + BUTTON_SPACING);

            rectTransform.anchorMin = new Vector2(0.5f, 0f);
            rectTransform.anchorMax = new Vector2(0.5f, 0f);
            rectTransform.pivot = new Vector2(0.5f, 0f);
            rectTransform.anchoredPosition = new Vector2(xPos, PANEL_Y_OFFSET);
            rectTransform.sizeDelta = new Vector2(BUTTON_WIDTH, BUTTON_HEIGHT);

            // StudentSkillButton 컴포넌트 추가
            var skillButton = buttonObj.AddComponent<StudentSkillButton>();
            skillButton.Initialize(student, OnSkillButtonClicked);

            _skillButtons.Add(skillButton);
        }

        /// <summary>
        /// 스킬 버튼 클릭 이벤트
        /// </summary>
        private void OnSkillButtonClicked(Student student)
        {
            if (_combatManager == null) return;

            // 학생 인덱스 찾기
            int studentIndex = FindStudentIndex(student);
            if (studentIndex < 0)
            {
                Debug.LogError($"[SkillButtonPanel] 학생을 찾을 수 없음: {student.Data.studentName}");
                return;
            }

            // CombatManager를 통해 스킬 사용
            var result = _combatManager.UseStudentSkill(studentIndex);

            if (result != null && !result.Success)
            {
                Debug.Log($"[SkillButtonPanel] 스킬 사용 실패: {result.FailureReason}");
            }
        }

        /// <summary>
        /// 학생 인덱스 찾기
        /// </summary>
        private int FindStudentIndex(Student student)
        {
            for (int i = 0; i < _combatManager.StudentCount; i++)
            {
                if (_combatManager.GetStudent(i) == student)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// 매 프레임 업데이트 (코스트 부족 상태 체크)
        /// </summary>
        private void Update()
        {
            if (_costSystem == null) return;

            // 각 버튼의 코스트 부족 상태 업데이트
            foreach (var button in _skillButtons)
            {
                if (button == null) continue;

                // 버튼 내부에서 Student를 통해 필요 코스트 가져오기
                // StudentSkillButton이 자동으로 상태를 업데이트하므로
                // 여기서는 코스트 정보만 전달
                UpdateButtonCostState(button);
            }
        }

        /// <summary>
        /// 버튼 코스트 상태 업데이트
        /// </summary>
        private void UpdateButtonCostState(StudentSkillButton button)
        {
            // StudentSkillButton이 자체적으로 상태를 관리하므로
            // 필요시 코스트 부족 여부만 전달
            // 현재는 StudentSkillButton.Update()에서 자동 처리
        }

        /// <summary>
        /// 프로그래매틱 스킬 버튼 클릭 (테스트용)
        /// </summary>
        public void SimulateButtonClick(int studentIndex)
        {
            if (studentIndex >= 0 && studentIndex < _skillButtons.Count)
            {
                var button = _skillButtons[studentIndex];
                var student = _combatManager.GetStudent(studentIndex);
                if (student != null)
                {
                    OnSkillButtonClicked(student);
                }
            }
        }
    }
}
