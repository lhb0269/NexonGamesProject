using UnityEngine;
using System.Collections.Generic;
using NexonGame.BlueArchive.Data;

namespace NexonGame.BlueArchive.Stage
{
    /// <summary>
    /// 스테이지 매니저 (MonoBehaviour)
    /// - StageController 로직 클래스 래핑
    /// - GameObject 생성 및 비주얼 관리
    /// - PlayMode 테스트에서 사용
    /// </summary>
    public class StageManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GridVisualizer _gridVisualizer;
        [SerializeField] private Transform _platformContainer;
        [SerializeField] private GameObject _platformPrefab;
        [SerializeField] private GameObject _playerMarkerPrefab;

        // 로직 클래스 (Pure C#)
        private StageController _stageController;
        private GridManager _gridManager;

        // 생성된 오브젝트들
        private List<PlatformObject> _platforms;
        private GameObject _playerMarker;
        private StageData _currentStageData;

        // 프로퍼티
        public StageController StageController => _stageController;
        public StageState CurrentState => _stageController?.CurrentState ?? StageState.NotStarted;
        public Vector2Int PlayerPosition => _stageController?.PlayerPosition ?? Vector2Int.zero;
        public int TotalMovesInStage => _stageController?.TotalMovesInStage ?? 0;

        private void Awake()
        {
            _platforms = new List<PlatformObject>();

            // GridVisualizer 자동 찾기
            if (_gridVisualizer == null)
            {
                _gridVisualizer = GetComponentInChildren<GridVisualizer>();
            }

            // PlatformContainer 자동 생성
            if (_platformContainer == null)
            {
                var containerObj = new GameObject("PlatformContainer");
                containerObj.transform.SetParent(transform);
                _platformContainer = containerObj.transform;
            }
        }

        /// <summary>
        /// 스테이지 초기화
        /// </summary>
        public void InitializeStage(StageData stageData)
        {
            if (stageData == null)
            {
                Debug.LogError("[StageManager] StageData is null!");
                return;
            }

            _currentStageData = stageData;

            // 로직 클래스 생성
            _gridManager = new GridManager(stageData.gridWidth, stageData.gridHeight);
            _stageController = new StageController();
            _stageController.InitializeStage(stageData);

            // 비주얼 생성
            CreateGridVisual();
            CreatePlatforms();
            CreatePlayerMarker();

            Debug.Log($"[StageManager] Stage '{stageData.stageName}' initialized!");
        }

        /// <summary>
        /// 그리드 비주얼 생성
        /// </summary>
        private void CreateGridVisual()
        {
            if (_gridVisualizer != null)
            {
                _gridVisualizer.CreateGrid(_currentStageData.gridWidth, _currentStageData.gridHeight);
            }
        }

        /// <summary>
        /// 플랫폼 오브젝트 생성
        /// </summary>
        private void CreatePlatforms()
        {
            // 기존 플랫폼 제거
            ClearPlatforms();

            if (_platformPrefab == null)
            {
                Debug.LogWarning("[StageManager] Platform prefab is not assigned. Creating placeholder platforms.");
                CreatePlaceholderPlatforms();
                return;
            }

            // 시작 위치 플랫폼
            CreatePlatformAt(_currentStageData.startPosition, PlatformType.Start);

            // 일반 플랫폼들
            foreach (var pos in _currentStageData.platformPositions)
            {
                CreatePlatformAt(pos, PlatformType.Normal);
            }

            // 전투 위치 플랫폼
            CreatePlatformAt(_currentStageData.battlePosition, PlatformType.Battle);

            Debug.Log($"[StageManager] Created {_platforms.Count} platforms");
        }

        /// <summary>
        /// 특정 위치에 플랫폼 생성
        /// </summary>
        private void CreatePlatformAt(Vector2Int gridPos, PlatformType type)
        {
            GameObject platformObj = Instantiate(_platformPrefab, _platformContainer);
            platformObj.name = $"Platform_{gridPos.x}_{gridPos.y}";

            Vector3 worldPos = GridToWorldPosition(gridPos);
            platformObj.transform.position = worldPos;

            PlatformObject platform = platformObj.GetComponent<PlatformObject>();
            if (platform == null)
            {
                platform = platformObj.AddComponent<PlatformObject>();
            }

            platform.Initialize(gridPos, type, OnPlatformClicked);
            _platforms.Add(platform);
        }

        /// <summary>
        /// Placeholder 플랫폼 생성 (프리팹 없을 때)
        /// </summary>
        private void CreatePlaceholderPlatforms()
        {
            // 시작 위치
            CreatePlaceholderPlatform(_currentStageData.startPosition, PlatformType.Start);

            // 일반 플랫폼
            foreach (var pos in _currentStageData.platformPositions)
            {
                CreatePlaceholderPlatform(pos, PlatformType.Normal);
            }

            // 전투 위치
            CreatePlaceholderPlatform(_currentStageData.battlePosition, PlatformType.Battle);
        }

        /// <summary>
        /// Placeholder 플랫폼 생성
        /// </summary>
        private void CreatePlaceholderPlatform(Vector2Int gridPos, PlatformType type)
        {
            GameObject platformObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            platformObj.name = $"Platform_{gridPos.x}_{gridPos.y}";
            platformObj.transform.SetParent(_platformContainer);
            platformObj.transform.position = GridToWorldPosition(gridPos);
            platformObj.transform.localScale = new Vector3(0.8f, 0.1f, 0.8f);

            PlatformObject platform = platformObj.AddComponent<PlatformObject>();
            platform.Initialize(gridPos, type, OnPlatformClicked);
            _platforms.Add(platform);
        }

        /// <summary>
        /// 플레이어 마커 생성
        /// </summary>
        private void CreatePlayerMarker()
        {
            if (_playerMarker != null)
            {
                Destroy(_playerMarker);
            }

            if (_playerMarkerPrefab != null)
            {
                _playerMarker = Instantiate(_playerMarkerPrefab, transform);
            }
            else
            {
                // Placeholder 마커
                _playerMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                _playerMarker.name = "PlayerMarker";
                _playerMarker.transform.SetParent(transform);
                _playerMarker.transform.localScale = Vector3.one * 0.5f;

                // 색상 변경
                var renderer = _playerMarker.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = Color.cyan;
                }
            }

            UpdatePlayerMarkerPosition();
        }

        /// <summary>
        /// 플레이어 마커 위치 업데이트
        /// </summary>
        private void UpdatePlayerMarkerPosition()
        {
            if (_playerMarker != null && _stageController != null)
            {
                Vector3 worldPos = GridToWorldPosition(_stageController.PlayerPosition);
                worldPos.y += 0.5f; // 약간 위로
                _playerMarker.transform.position = worldPos;
            }
        }

        /// <summary>
        /// 플랫폼 클릭 핸들러
        /// </summary>
        private void OnPlatformClicked(Vector2Int targetPosition)
        {
            Debug.Log($"[StageManager] 플랫폼 클릭됨: {targetPosition}");

            // 인접 여부 확인
            if (!IsAdjacent(_stageController.PlayerPosition, targetPosition))
            {
                Debug.LogWarning($"[StageManager] 이동 실패: {targetPosition}는 인접하지 않은 플랫폼입니다.");
                return;
            }

            // 이동 시도
            bool success = MovePlayer(targetPosition);

            if (!success)
            {
                Debug.LogWarning($"[StageManager] 이동 실패: {targetPosition}로 이동할 수 없습니다.");
            }
        }

        /// <summary>
        /// 두 위치가 인접한지 확인 (8방향: 상하좌우 + 대각선)
        /// </summary>
        private bool IsAdjacent(Vector2Int from, Vector2Int to)
        {
            int dx = Mathf.Abs(to.x - from.x);
            int dy = Mathf.Abs(to.y - from.y);

            // 8방향 인접: dx와 dy가 모두 0 또는 1이어야 하며, 동일 위치는 제외
            bool isAdjacent = (dx <= 1 && dy <= 1) && !(dx == 0 && dy == 0);

            Debug.Log($"[StageManager] 인접 체크: {from} → {to}, dx={dx}, dy={dy}, 결과={isAdjacent}");

            return isAdjacent;
        }

        /// <summary>
        /// 플레이어 이동
        /// </summary>
        public bool MovePlayer(Vector2Int targetPosition)
        {
            bool success = _stageController.MovePlayer(targetPosition);

            if (success)
            {
                UpdatePlayerMarkerPosition();
                Debug.Log($"[StageManager] Player moved to {targetPosition}");
            }

            return success;
        }

        /// <summary>
        /// 전투 위치까지의 경로 계산
        /// </summary>
        public List<Vector2Int> GetPathToBattle()
        {
            return _stageController.GetPathToBattle();
        }

        /// <summary>
        /// 전투 진입 가능 여부
        /// </summary>
        public bool CanEnterBattle()
        {
            return _stageController != null && _stageController.CurrentState == StageState.ReadyForBattle;
        }

        /// <summary>
        /// 전투 시작
        /// </summary>
        public void StartBattle()
        {
            _stageController.StartBattle();
            Debug.Log("[StageManager] Battle started!");
        }

        /// <summary>
        /// 전투 완료
        /// </summary>
        public void CompleteBattle(bool victory)
        {
            _stageController.CompleteBattle(victory);
            Debug.Log($"[StageManager] Battle completed! Victory: {victory}");
        }

        /// <summary>
        /// 스테이지 클리어
        /// </summary>
        public void ClearStage()
        {
            _stageController.ClearStage();
            Debug.Log("[StageManager] Stage cleared!");
        }

        /// <summary>
        /// 그리드 좌표를 월드 좌표로 변환
        /// </summary>
        private Vector3 GridToWorldPosition(Vector2Int gridPos)
        {
            // 그리드 중심을 원점으로
            float offsetX = _currentStageData.gridWidth / 2f;
            float offsetZ = _currentStageData.gridHeight / 2f;

            return new Vector3(
                gridPos.x - offsetX + 0.5f,
                0f,
                gridPos.y - offsetZ + 0.5f
            );
        }

        /// <summary>
        /// 모든 플랫폼 제거
        /// </summary>
        private void ClearPlatforms()
        {
            foreach (var platform in _platforms)
            {
                if (platform != null)
                {
                    Destroy(platform.gameObject);
                }
            }
            _platforms.Clear();
        }

        /// <summary>
        /// 스테이지 리셋
        /// </summary>
        public void ResetStage()
        {
            ClearPlatforms();

            if (_playerMarker != null)
            {
                Destroy(_playerMarker);
            }

            if (_gridVisualizer != null)
            {
                _gridVisualizer.ClearGrid();
            }

            _stageController = null;
            _gridManager = null;
            _currentStageData = null;

            Debug.Log("[StageManager] Stage reset");
        }

        private void OnDestroy()
        {
            ResetStage();
        }

        /// <summary>
        /// 특정 위치의 플랫폼 찾기 (테스트용)
        /// </summary>
        public PlatformObject FindPlatformAt(Vector2Int gridPosition)
        {
            foreach (var platform in _platforms)
            {
                if (platform != null && platform.GridPosition == gridPosition)
                {
                    return platform;
                }
            }
            return null;
        }

        /// <summary>
        /// 플랫폼 클릭 시뮬레이션 (테스트용)
        /// </summary>
        public void SimulatePlatformClick(Vector2Int gridPosition)
        {
            var platform = FindPlatformAt(gridPosition);
            if (platform != null)
            {
                platform.SimulateClick();
            }
            else
            {
                Debug.LogWarning($"[StageManager] 플랫폼을 찾을 수 없음: {gridPosition}");
            }
        }

        /// <summary>
        /// 디버그 정보 표시
        /// </summary>
        public string GetDebugInfo()
        {
            if (_stageController == null) return "Stage not initialized";

            return $"Stage: {_currentStageData.stageName}\n" +
                   $"State: {CurrentState}\n" +
                   $"Player Position: {PlayerPosition}\n" +
                   $"Total Moves: {TotalMovesInStage}\n" +
                   $"Platforms: {_platforms.Count}";
        }
    }

    /// <summary>
    /// 플랫폼 타입
    /// </summary>
    public enum PlatformType
    {
        Start,    // 시작 위치
        Normal,   // 일반 플랫폼
        Battle    // 전투 위치
    }
}
