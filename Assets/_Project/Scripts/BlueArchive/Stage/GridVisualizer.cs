using UnityEngine;
using System.Collections.Generic;

namespace NexonGame.BlueArchive.Stage
{
    /// <summary>
    /// 그리드 시각화 컴포넌트
    /// - 그리드 라인 렌더링
    /// - 좌표 표시 (옵션)
    /// </summary>
    public class GridVisualizer : MonoBehaviour
    {
        [Header("Grid Settings")]
        [SerializeField] private Color _gridLineColor = new Color(1f, 1f, 1f, 0.3f);
        [SerializeField] private float _lineWidth = 0.02f;
        [SerializeField] private bool _showCoordinates = true;

        [Header("Coordinate Text")]
        [SerializeField] private GameObject _coordinateTextPrefab;
        [SerializeField] private float _coordinateTextHeight = 0.1f;

        private int _gridWidth;
        private int _gridHeight;
        private List<LineRenderer> _gridLines;
        private List<GameObject> _coordinateTexts;

        private void Awake()
        {
            _gridLines = new List<LineRenderer>();
            _coordinateTexts = new List<GameObject>();
        }

        /// <summary>
        /// 그리드 생성
        /// </summary>
        public void CreateGrid(int width, int height)
        {
            _gridWidth = width;
            _gridHeight = height;

            ClearGrid();
            CreateGridLines();

            if (_showCoordinates)
            {
                CreateCoordinateLabels();
            }

            Debug.Log($"[GridVisualizer] Grid created: {width}x{height}");
        }

        /// <summary>
        /// 그리드 라인 생성
        /// </summary>
        private void CreateGridLines()
        {
            // 수평선 (Z축 방향)
            for (int x = 0; x <= _gridWidth; x++)
            {
                CreateLine(
                    GetWorldPosition(x, 0),
                    GetWorldPosition(x, _gridHeight)
                );
            }

            // 수직선 (X축 방향)
            for (int z = 0; z <= _gridHeight; z++)
            {
                CreateLine(
                    GetWorldPosition(0, z),
                    GetWorldPosition(_gridWidth, z)
                );
            }
        }

        /// <summary>
        /// 단일 라인 생성
        /// </summary>
        private void CreateLine(Vector3 start, Vector3 end)
        {
            GameObject lineObj = new GameObject("GridLine");
            lineObj.transform.SetParent(transform);

            LineRenderer lineRenderer = lineObj.AddComponent<LineRenderer>();
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, end);

            lineRenderer.startWidth = _lineWidth;
            lineRenderer.endWidth = _lineWidth;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = _gridLineColor;
            lineRenderer.endColor = _gridLineColor;

            // 렌더링 설정
            lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lineRenderer.receiveShadows = false;

            _gridLines.Add(lineRenderer);
        }

        /// <summary>
        /// 좌표 레이블 생성
        /// </summary>
        private void CreateCoordinateLabels()
        {
            if (_coordinateTextPrefab == null)
            {
                // Prefab이 없으면 간단한 placeholder
                CreatePlaceholderLabels();
                return;
            }

            for (int x = 0; x < _gridWidth; x++)
            {
                for (int z = 0; z < _gridHeight; z++)
                {
                    Vector3 worldPos = GetWorldPosition(x, z);
                    worldPos += new Vector3(0.5f, _coordinateTextHeight, 0.5f); // 셀 중앙

                    GameObject textObj = Instantiate(_coordinateTextPrefab, worldPos, Quaternion.identity, transform);
                    textObj.name = $"Coord_{x}_{z}";

                    var textMesh = textObj.GetComponent<TextMesh>();
                    if (textMesh != null)
                    {
                        textMesh.text = $"({x},{z})";
                        textMesh.fontSize = 10;
                        textMesh.color = Color.white;
                        textMesh.anchor = TextAnchor.MiddleCenter;
                    }

                    _coordinateTexts.Add(textObj);
                }
            }
        }

        /// <summary>
        /// Placeholder 레이블 생성 (TextMesh 사용)
        /// </summary>
        private void CreatePlaceholderLabels()
        {
            for (int x = 0; x < _gridWidth; x++)
            {
                for (int z = 0; z < _gridHeight; z++)
                {
                    Vector3 worldPos = GetWorldPosition(x, z);
                    worldPos += new Vector3(0.5f, _coordinateTextHeight, 0.5f);

                    GameObject textObj = new GameObject($"Coord_{x}_{z}");
                    textObj.transform.SetParent(transform);
                    textObj.transform.position = worldPos;
                    textObj.transform.rotation = Quaternion.Euler(90f, 0f, 0f); // 아래를 보도록

                    TextMesh textMesh = textObj.AddComponent<TextMesh>();
                    textMesh.text = $"({x},{z})";
                    textMesh.fontSize = 10;
                    textMesh.characterSize = 0.05f;
                    textMesh.color = new Color(1f, 1f, 1f, 0.7f);
                    textMesh.anchor = TextAnchor.MiddleCenter;

                    _coordinateTexts.Add(textObj);
                }
            }
        }

        /// <summary>
        /// 그리드 좌표를 월드 좌표로 변환
        /// </summary>
        private Vector3 GetWorldPosition(int gridX, int gridZ)
        {
            float offsetX = _gridWidth / 2f;
            float offsetZ = _gridHeight / 2f;

            return new Vector3(
                gridX - offsetX,
                0f,
                gridZ - offsetZ
            );
        }

        /// <summary>
        /// 그리드 클리어
        /// </summary>
        public void ClearGrid()
        {
            // 라인 제거
            foreach (var line in _gridLines)
            {
                if (line != null)
                {
                    Destroy(line.gameObject);
                }
            }
            _gridLines.Clear();

            // 좌표 텍스트 제거
            foreach (var text in _coordinateTexts)
            {
                if (text != null)
                {
                    Destroy(text);
                }
            }
            _coordinateTexts.Clear();
        }

        /// <summary>
        /// 그리드 라인 색상 변경
        /// </summary>
        public void SetGridLineColor(Color color)
        {
            _gridLineColor = color;

            foreach (var line in _gridLines)
            {
                if (line != null)
                {
                    line.startColor = color;
                    line.endColor = color;
                }
            }
        }

        /// <summary>
        /// 좌표 표시 토글
        /// </summary>
        public void SetShowCoordinates(bool show)
        {
            _showCoordinates = show;

            foreach (var text in _coordinateTexts)
            {
                if (text != null)
                {
                    text.SetActive(show);
                }
            }
        }

        private void OnDestroy()
        {
            ClearGrid();
        }

        /// <summary>
        /// 에디터에서 그리드 미리보기 (Gizmos)
        /// </summary>
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying && _gridWidth > 0 && _gridHeight > 0)
            {
                Gizmos.color = _gridLineColor;

                // 수평선
                for (int x = 0; x <= _gridWidth; x++)
                {
                    Gizmos.DrawLine(
                        GetWorldPosition(x, 0),
                        GetWorldPosition(x, _gridHeight)
                    );
                }

                // 수직선
                for (int z = 0; z <= _gridHeight; z++)
                {
                    Gizmos.DrawLine(
                        GetWorldPosition(0, z),
                        GetWorldPosition(_gridWidth, z)
                    );
                }
            }
        }
    }
}
