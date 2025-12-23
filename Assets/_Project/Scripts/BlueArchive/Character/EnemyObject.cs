using UnityEngine;

namespace NexonGame.BlueArchive.Character
{
    /// <summary>
    /// 적 오브젝트 (GameObject)
    /// - Enemy 로직 클래스 래핑
    /// - 비주얼 표현 (HP 바, 이름)
    /// - 격파 효과
    /// </summary>
    public class EnemyObject : MonoBehaviour
    {
        [Header("Visual Components")]
        [SerializeField] private Renderer _renderer;
        [SerializeField] private Color _enemyColor = Color.red;

        // 로직 클래스
        public Enemy Enemy { get; private set; }

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
            _originalColor = _enemyColor;
        }

        /// <summary>
        /// 초기화
        /// </summary>
        public void Initialize(Enemy enemy)
        {
            Enemy = enemy;
            gameObject.name = $"Enemy_{enemy.Data.enemyName}";

            UpdateVisual();
        }

        /// <summary>
        /// 비주얼 업데이트
        /// </summary>
        public void UpdateVisual()
        {
            if (Enemy == null) return;

            // 이미 비활성화된 경우 무시
            if (!gameObject.activeSelf) return;

            // HP 비율에 따라 색상 변경
            float hpRatio = (float)Enemy.CurrentHP / Enemy.Data.maxHP;
            Color currentColor = Color.Lerp(Color.black, _enemyColor, hpRatio);

            SetColor(currentColor);

            // 격파 처리
            if (!Enemy.IsAlive)
            {
                PlayDefeatedEffect();
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
        /// 데미지 받기 효과
        /// </summary>
        public void TakeDamageEffect(int damage)
        {
            StartCoroutine(DamageFlashEffect());
        }

        /// <summary>
        /// 데미지 플래시 효과
        /// </summary>
        private System.Collections.IEnumerator DamageFlashEffect()
        {
            SetColor(Color.white);
            yield return new WaitForSeconds(0.1f);
            UpdateVisual();
        }

        /// <summary>
        /// 격파 효과
        /// </summary>
        private void PlayDefeatedEffect()
        {
            SetColor(Color.gray);

            // 서서히 축소
            StartCoroutine(ScaleDownEffect());
        }

        /// <summary>
        /// 축소 효과
        /// </summary>
        private System.Collections.IEnumerator ScaleDownEffect()
        {
            Vector3 originalScale = transform.localScale;
            float duration = 0.5f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, t);
                yield return null;
            }

            // 비활성화
            gameObject.SetActive(false);
        }

        /// <summary>
        /// 디버그 정보
        /// </summary>
        public string GetDebugInfo()
        {
            if (Enemy == null) return "Not initialized";

            return $"{Enemy.Data.enemyName}\n" +
                   $"HP: {Enemy.CurrentHP}/{Enemy.Data.maxHP}\n" +
                   $"Alive: {Enemy.IsAlive}";
        }

        /// <summary>
        /// Gizmo 표시
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (Enemy != null)
            {
                Gizmos.color = Enemy.IsAlive ? Color.red : Color.gray;
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
