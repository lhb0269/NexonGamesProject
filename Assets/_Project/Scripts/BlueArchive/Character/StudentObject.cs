using UnityEngine;

namespace NexonGame.BlueArchive.Character
{
    /// <summary>
    /// 학생 오브젝트 (GameObject)
    /// - Student 로직 클래스 래핑
    /// - 비주얼 표현 (HP 바, 이름, 스프라이트)
    /// - 스킬 이펙트 재생
    /// </summary>
    public class StudentObject : MonoBehaviour
    {
        [Header("Visual Components")]
        [SerializeField] private Renderer _renderer;
        [SerializeField] private Color _studentColor = Color.blue;

        // 로직 클래스
        public Student Student { get; private set; }

        // 비주얼 상태
        private Color _originalColor;
        private MaterialPropertyBlock _propertyBlock;

        private void Awake()
        {
            // Renderer 자동 찾기
            if (_renderer == null)
            {
                _renderer = GetComponent<Renderer>();
            }

            _propertyBlock = new MaterialPropertyBlock();
            _originalColor = _studentColor;
        }

        /// <summary>
        /// 초기화
        /// </summary>
        public void Initialize(Student student)
        {
            Student = student;
            gameObject.name = $"Student_{student.Data.studentName}";

            UpdateVisual();
        }

        /// <summary>
        /// 비주얼 업데이트
        /// </summary>
        public void UpdateVisual()
        {
            if (Student == null) return;

            // HP 비율에 따라 색상 변경
            float hpRatio = (float)Student.CurrentHP / Student.Data.maxHP;
            Color currentColor = Color.Lerp(Color.red, _studentColor, hpRatio);

            SetColor(currentColor);

            // 사망 처리
            if (!Student.IsAlive)
            {
                SetColor(Color.gray);
            }
        }

        /// <summary>
        /// 색상 설정
        /// </summary>
        private void SetColor(Color color)
        {
            if (_renderer != null)
            {
                _renderer.GetPropertyBlock(_propertyBlock);
                _propertyBlock.SetColor("_Color", color);
                _renderer.SetPropertyBlock(_propertyBlock);
            }
        }

        /// <summary>
        /// 스킬 애니메이션 재생
        /// </summary>
        public void PlaySkillAnimation()
        {
            // 간단한 플래시 효과
            StartCoroutine(FlashEffect());
        }

        /// <summary>
        /// 플래시 효과
        /// </summary>
        private System.Collections.IEnumerator FlashEffect()
        {
            // 흰색으로 플래시
            SetColor(Color.white);
            yield return new WaitForSeconds(0.1f);

            // 원래 색으로 복귀
            UpdateVisual();
        }

        /// <summary>
        /// 데미지 받기 (비주얼 표현)
        /// </summary>
        public void TakeDamageEffect(int damage)
        {
            // 빨간색 플래시
            StartCoroutine(DamageFlashEffect());
        }

        /// <summary>
        /// 데미지 플래시 효과
        /// </summary>
        private System.Collections.IEnumerator DamageFlashEffect()
        {
            SetColor(Color.red);
            yield return new WaitForSeconds(0.15f);
            UpdateVisual();
        }

        /// <summary>
        /// 디버그 정보
        /// </summary>
        public string GetDebugInfo()
        {
            if (Student == null) return "Not initialized";

            return $"{Student.Data.studentName}\n" +
                   $"HP: {Student.CurrentHP}/{Student.Data.maxHP}\n" +
                   $"Skill CD: {Student.CurrentSkillCooldown:F1}s";
        }

        /// <summary>
        /// Gizmo 표시
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (Student != null)
            {
                Gizmos.color = Student.IsAlive ? Color.cyan : Color.gray;
                Gizmos.DrawWireSphere(transform.position, 0.6f);

                #if UNITY_EDITOR
                UnityEditor.Handles.Label(
                    transform.position + Vector3.up * 1.5f,
                    GetDebugInfo()
                );
                #endif
            }
        }
    }
}
