using UnityEngine;

namespace NexonGame.BlueArchive.Stage
{
    /// <summary>
    /// 플랫폼 오브젝트
    /// - 발판 비주얼 표현
    /// - 플랫폼 타입별 색상 구분
    /// - 하이라이트 효과
    /// </summary>
    public class PlatformObject : MonoBehaviour
    {
        [Header("Visual Settings")]
        [SerializeField] private Renderer _renderer;
        [SerializeField] private Color _startColor = Color.green;
        [SerializeField] private Color _normalColor = Color.gray;
        [SerializeField] private Color _battleColor = Color.red;
        [SerializeField] private Color _highlightColor = Color.yellow;

        private Vector2Int _gridPosition;
        private PlatformType _platformType;
        private Color _originalColor;
        private bool _isHighlighted;
        private MaterialPropertyBlock _propertyBlock;

        /// <summary>
        /// 프로퍼티
        /// </summary>
        public Vector2Int GridPosition => _gridPosition;
        public PlatformType Type => _platformType;

        private void Awake()
        {
            // Renderer 자동 찾기
            if (_renderer == null)
            {
                _renderer = GetComponent<Renderer>();
            }

            _propertyBlock = new MaterialPropertyBlock();
        }

        /// <summary>
        /// 초기화
        /// </summary>
        public void Initialize(Vector2Int gridPosition, PlatformType type)
        {
            _gridPosition = gridPosition;
            _platformType = type;

            // 태그 설정 (태그가 없으면 무시)
            try
            {
                gameObject.tag = "Platform";
            }
            catch (UnityException)
            {
                // Platform 태그가 없으면 무시
                Debug.LogWarning("[PlatformObject] 'Platform' 태그가 정의되지 않았습니다. Edit → Project Settings → Tags에서 추가하세요.");
            }

            gameObject.name = $"Platform_{type}_{gridPosition.x}_{gridPosition.y}";

            UpdateVisual();
        }

        /// <summary>
        /// 비주얼 업데이트
        /// </summary>
        private void UpdateVisual()
        {
            // 타입별 색상 설정
            _originalColor = _platformType switch
            {
                PlatformType.Start => _startColor,
                PlatformType.Battle => _battleColor,
                _ => _normalColor
            };

            SetColor(_originalColor);
        }

        /// <summary>
        /// 색상 설정
        /// </summary>
        private void SetColor(Color color)
        {
            if (_renderer != null)
            {
                // MaterialPropertyBlock 사용으로 Material 인스턴스 생성 방지
                _renderer.GetPropertyBlock(_propertyBlock);
                _propertyBlock.SetColor("_Color", color);
                _renderer.SetPropertyBlock(_propertyBlock);
            }
        }

        /// <summary>
        /// 하이라이트 표시
        /// </summary>
        public void SetHighlight(bool highlight)
        {
            _isHighlighted = highlight;
            SetColor(highlight ? _highlightColor : _originalColor);
        }

        /// <summary>
        /// 활성화/비활성화
        /// </summary>
        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }

        /// <summary>
        /// 마우스 오버 시 (선택 사항)
        /// </summary>
        private void OnMouseEnter()
        {
            if (!_isHighlighted)
            {
                SetColor(Color.Lerp(_originalColor, Color.white, 0.3f));
            }
        }

        /// <summary>
        /// 마우스 나갈 때 (선택 사항)
        /// </summary>
        private void OnMouseExit()
        {
            if (!_isHighlighted)
            {
                SetColor(_originalColor);
            }
        }

        /// <summary>
        /// 디버그 정보
        /// </summary>
        public string GetDebugInfo()
        {
            return $"Platform at {_gridPosition}, Type: {_platformType}";
        }

        /// <summary>
        /// Gizmo 표시 (에디터용)
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 0.9f);

            // 좌표 표시
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(
                transform.position + Vector3.up * 0.5f,
                $"({_gridPosition.x}, {_gridPosition.y})"
            );
            #endif
        }
    }
}
