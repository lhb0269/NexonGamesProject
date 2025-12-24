using UnityEngine;
using UnityEngine.UI;
using NexonGame.BlueArchive.Character;

namespace NexonGame.BlueArchive.UI
{
    /// <summary>
    /// 학생 EX 스킬 버튼
    /// - 원작처럼 코스트바 위에 배치
    /// - 클릭으로 스킬 발동
    /// - 쿨타임 시각화
    /// </summary>
    public class StudentSkillButton : MonoBehaviour
    {
        // 참조
        private Student _student;
        private System.Action<Student> _onSkillButtonClicked;

        // UI 요소
        private Button _button;
        private Image _background;
        private Image _cooldownFill;
        private Text _nameText;
        private Text _costText;
        private Text _cooldownText;

        // 상태
        public enum ButtonState
        {
            Available,      // 사용 가능
            Cooldown,       // 쿨타임 중
            NotEnoughCost   // 코스트 부족
        }

        private ButtonState _currentState;

        // 색상 설정
        private static readonly Color COLOR_AVAILABLE = new Color(0.2f, 0.6f, 1f, 1f);       // 밝은 파란색
        private static readonly Color COLOR_COOLDOWN = new Color(0.3f, 0.3f, 0.3f, 1f);      // 어두운 회색
        private static readonly Color COLOR_NOT_ENOUGH = new Color(0.5f, 0.3f, 0.3f, 1f);    // 어두운 빨간색
        private static readonly Color COLOR_COOLDOWN_FILL = new Color(0.15f, 0.15f, 0.15f, 0.8f); // 쿨타임 오버레이

        /// <summary>
        /// 초기화
        /// </summary>
        public void Initialize(Student student, System.Action<Student> onSkillButtonClicked)
        {
            _student = student;
            _onSkillButtonClicked = onSkillButtonClicked;

            CreateUI();
            UpdateVisuals();
        }

        /// <summary>
        /// UI 생성
        /// </summary>
        private void CreateUI()
        {
            // 버튼 추가
            _button = gameObject.AddComponent<Button>();
            _button.onClick.AddListener(OnButtonClick);

            // 배경 이미지
            _background = gameObject.AddComponent<Image>();
            _background.sprite = CreateWhiteSprite();
            _background.color = COLOR_AVAILABLE;

            // 학생 이름 텍스트
            var nameObj = new GameObject("Name");
            nameObj.transform.SetParent(transform, false);
            var nameRect = nameObj.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0, 0.6f);
            nameRect.anchorMax = new Vector2(1, 1);
            nameRect.offsetMin = Vector2.zero;
            nameRect.offsetMax = Vector2.zero;

            _nameText = nameObj.AddComponent<Text>();
            _nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _nameText.fontSize = 14;
            _nameText.alignment = TextAnchor.MiddleCenter;
            _nameText.color = Color.white;
            _nameText.fontStyle = FontStyle.Bold;
            _nameText.text = _student.Data.studentName;
            _nameText.raycastTarget = false;

            // 코스트 텍스트
            var costObj = new GameObject("Cost");
            costObj.transform.SetParent(transform, false);
            var costRect = costObj.AddComponent<RectTransform>();
            costRect.anchorMin = new Vector2(0, 0);
            costRect.anchorMax = new Vector2(1, 0.6f);
            costRect.offsetMin = Vector2.zero;
            costRect.offsetMax = Vector2.zero;

            _costText = costObj.AddComponent<Text>();
            _costText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _costText.fontSize = 20;
            _costText.alignment = TextAnchor.MiddleCenter;
            _costText.color = new Color(1f, 0.9f, 0.3f);
            _costText.fontStyle = FontStyle.Bold;
            _costText.text = _student.GetSkillCost().ToString();
            _costText.raycastTarget = false;

            // 쿨타임 오버레이 (Fill)
            var cooldownFillObj = new GameObject("CooldownFill");
            cooldownFillObj.transform.SetParent(transform, false);
            var cooldownFillRect = cooldownFillObj.AddComponent<RectTransform>();
            cooldownFillRect.anchorMin = Vector2.zero;
            cooldownFillRect.anchorMax = Vector2.one;
            cooldownFillRect.offsetMin = Vector2.zero;
            cooldownFillRect.offsetMax = Vector2.zero;

            _cooldownFill = cooldownFillObj.AddComponent<Image>();
            _cooldownFill.sprite = CreateWhiteSprite();
            _cooldownFill.color = COLOR_COOLDOWN_FILL;
            _cooldownFill.type = Image.Type.Filled;
            _cooldownFill.fillMethod = Image.FillMethod.Vertical;
            _cooldownFill.fillOrigin = (int)Image.OriginVertical.Bottom;
            _cooldownFill.fillAmount = 0f;
            _cooldownFill.raycastTarget = false;

            // 쿨타임 텍스트
            var cooldownTextObj = new GameObject("CooldownText");
            cooldownTextObj.transform.SetParent(transform, false);
            var cooldownTextRect = cooldownTextObj.AddComponent<RectTransform>();
            cooldownTextRect.anchorMin = Vector2.zero;
            cooldownTextRect.anchorMax = Vector2.one;
            cooldownTextRect.offsetMin = Vector2.zero;
            cooldownTextRect.offsetMax = Vector2.zero;

            _cooldownText = cooldownTextObj.AddComponent<Text>();
            _cooldownText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _cooldownText.fontSize = 16;
            _cooldownText.alignment = TextAnchor.MiddleCenter;
            _cooldownText.color = Color.white;
            _cooldownText.fontStyle = FontStyle.Bold;
            _cooldownText.text = "";
            _cooldownText.raycastTarget = false;
        }

        /// <summary>
        /// White Sprite 생성 (UI용)
        /// </summary>
        private Sprite CreateWhiteSprite()
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        }

        /// <summary>
        /// 버튼 클릭 이벤트
        /// </summary>
        private void OnButtonClick()
        {
            if (_currentState == ButtonState.Available)
            {
                _onSkillButtonClicked?.Invoke(_student);
            }
        }

        /// <summary>
        /// 매 프레임 업데이트
        /// </summary>
        private void Update()
        {
            UpdateVisuals();
        }

        /// <summary>
        /// 시각적 업데이트
        /// </summary>
        public void UpdateVisuals()
        {
            DetermineState();
            ApplyVisuals();
        }

        /// <summary>
        /// 현재 상태 결정
        /// </summary>
        private void DetermineState()
        {
            if (!_student.CanUseSkill())
            {
                _currentState = ButtonState.Cooldown;
            }
            else
            {
                // 코스트는 CombatManager에서 체크하므로, 여기서는 Available로 설정
                // 실제 코스트 체크는 버튼 클릭 시 CombatManager에서 수행
                _currentState = ButtonState.Available;
            }
        }

        /// <summary>
        /// 상태에 따라 시각적 변경
        /// </summary>
        private void ApplyVisuals()
        {
            switch (_currentState)
            {
                case ButtonState.Available:
                    _background.color = COLOR_AVAILABLE;
                    _button.interactable = true;
                    _cooldownFill.fillAmount = 0f;
                    _cooldownText.text = "";
                    break;

                case ButtonState.Cooldown:
                    _background.color = COLOR_COOLDOWN;
                    _button.interactable = false;

                    // 쿨타임 진행도 표시 (아래에서 위로 채워짐)
                    float cooldownRatio = 1f - (_student.SkillCooldownRemaining / _student.Data.exSkill.cooldownTime);
                    _cooldownFill.fillAmount = 1f - cooldownRatio; // 남은 쿨타임 비율

                    // 남은 시간 표시
                    _cooldownText.text = $"{_student.SkillCooldownRemaining:F1}s";
                    break;

                case ButtonState.NotEnoughCost:
                    _background.color = COLOR_NOT_ENOUGH;
                    _button.interactable = false;
                    _cooldownFill.fillAmount = 0f;
                    _cooldownText.text = "";
                    break;
            }
        }

        /// <summary>
        /// 코스트 부족 상태 설정 (외부에서 호출)
        /// </summary>
        public void SetNotEnoughCost(bool notEnough)
        {
            if (notEnough && _currentState != ButtonState.Cooldown)
            {
                _currentState = ButtonState.NotEnoughCost;
            }
        }
    }
}
