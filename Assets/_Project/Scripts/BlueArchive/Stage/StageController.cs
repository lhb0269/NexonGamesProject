using System;
using System.Collections.Generic;
using UnityEngine;
using NexonGame.BlueArchive.Data;
using NexonGame.BlueArchive.Combat;

namespace NexonGame.BlueArchive.Stage
{
    /// <summary>
    /// 스테이지 상태
    /// </summary>
    public enum StageState
    {
        NotStarted,     // 시작 전
        MovingToBattle, // 전투 위치로 이동 중
        ReadyForBattle, // 전투 준비 완료
        InBattle,       // 전투 중
        BattleComplete, // 전투 완료
        StageCleared    // 스테이지 클리어
    }

    /// <summary>
    /// 스테이지 컨트롤러
    /// - 스테이지 진행 관리
    /// - 플랫폼 이동 및 전투 진입 처리
    /// - 스테이지 상태 관리
    /// </summary>
    public class StageController
    {
        private StageData _stageData;
        private GridManager _gridManager;
        private StageState _currentState;

        // 플레이어 위치
        private Vector2Int _playerPosition;

        // 이벤트
        public event Action<StageState, StageState> OnStateChanged; // 이전 상태, 새 상태
        public event Action<Vector2Int> OnPlayerMoved;
        public event Action OnBattleReached;
        public event Action OnStageCleared;

        // 통계
        public int TotalMovesInStage { get; private set; }
        public float StageStartTime { get; private set; }
        public float StageDuration => Time.time - StageStartTime;

        public StageState CurrentState => _currentState;
        public Vector2Int PlayerPosition => _playerPosition;
        public Vector2Int BattlePosition => _stageData != null ? _stageData.battlePosition : Vector2Int.zero;
        public GridManager Grid => _gridManager;

        public StageController()
        {
            _currentState = StageState.NotStarted;
            TotalMovesInStage = 0;
        }

        /// <summary>
        /// 스테이지 초기화
        /// </summary>
        public void InitializeStage(StageData stageData)
        {
            _stageData = stageData;
            _currentState = StageState.NotStarted;
            TotalMovesInStage = 0;
            StageStartTime = Time.time;

            // 그리드 생성
            _gridManager = new GridManager(stageData.gridWidth, stageData.gridHeight);

            // 플랫폼 설정
            _gridManager.SetPlatforms(stageData.platformPositions, GridCellType.Platform);

            // 시작 위치 설정
            _gridManager.SetPlatform(stageData.startPosition, GridCellType.Start);

            // 전투 위치 설정
            _gridManager.SetPlatform(stageData.battlePosition, GridCellType.Battle);

            // 플레이어를 시작 위치에 배치
            _playerPosition = stageData.startPosition;
            _gridManager.SetOccupied(_playerPosition, true);

            Debug.Log($"[StageController] 스테이지 초기화: {stageData.stageName}");
            Debug.Log($"[StageController] 그리드: {stageData.gridWidth}x{stageData.gridHeight}");
            Debug.Log($"[StageController] 시작 위치: {stageData.startPosition}, 전투 위치: {stageData.battlePosition}");
            Debug.Log(_gridManager.VisualizeGrid());

            ChangeState(StageState.MovingToBattle);
        }

        /// <summary>
        /// 플레이어를 특정 위치로 이동
        /// </summary>
        public bool MovePlayer(Vector2Int targetPosition)
        {
            // 상태 체크
            if (_currentState != StageState.MovingToBattle)
            {
                Debug.LogWarning($"[StageController] 이동 불가 상태: {_currentState}");
                return false;
            }

            // 이동 가능 여부 체크
            if (!_gridManager.IsWalkable(targetPosition))
            {
                Debug.LogWarning($"[StageController] 이동 불가능한 위치: {targetPosition}");
                return false;
            }

            // 인접 여부 체크 (한 번에 한 칸만 이동 가능)
            if (!_gridManager.IsAdjacent(_playerPosition, targetPosition))
            {
                Debug.LogWarning($"[StageController] 인접하지 않은 위치: {_playerPosition} → {targetPosition}");
                return false;
            }

            // 이전 위치 점유 해제
            _gridManager.SetOccupied(_playerPosition, false);

            // 플레이어 이동
            _playerPosition = targetPosition;
            _gridManager.SetOccupied(_playerPosition, true);
            _gridManager.RecordMove(_playerPosition);
            TotalMovesInStage++;

            Debug.Log($"[StageController] 플레이어 이동: {_playerPosition} (총 {TotalMovesInStage}회)");

            OnPlayerMoved?.Invoke(_playerPosition);

            // 전투 위치 도달 체크
            if (_playerPosition == _stageData.battlePosition)
            {
                ChangeState(StageState.ReadyForBattle);
                OnBattleReached?.Invoke();
                Debug.Log("[StageController] 전투 위치 도달!");
            }

            return true;
        }

        /// <summary>
        /// 자동으로 전투 위치까지 이동 (테스트용)
        /// </summary>
        public List<Vector2Int> GetPathToBattle()
        {
            // 간단한 경로 찾기 (맨하탄 거리 기반)
            List<Vector2Int> path = new List<Vector2Int>();
            Vector2Int current = _playerPosition;

            // 최대 반복 횟수 (무한 루프 방지)
            int maxIterations = 100;
            int iterations = 0;

            while (current != _stageData.battlePosition && iterations < maxIterations)
            {
                iterations++;

                List<Vector2Int> adjacent = _gridManager.GetAdjacentWalkableCells(current);
                if (adjacent.Count == 0)
                    break;

                // 전투 위치에 가장 가까운 셀 선택
                Vector2Int next = adjacent[0];
                int minDistance = _gridManager.GetManhattanDistance(next, _stageData.battlePosition);

                foreach (var cell in adjacent)
                {
                    int distance = _gridManager.GetManhattanDistance(cell, _stageData.battlePosition);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        next = cell;
                    }
                }

                path.Add(next);
                current = next;
            }

            return path;
        }

        /// <summary>
        /// 전투 시작
        /// </summary>
        public void StartBattle()
        {
            if (_currentState != StageState.ReadyForBattle)
            {
                Debug.LogWarning($"[StageController] 전투 시작 불가 상태: {_currentState}");
                return;
            }

            ChangeState(StageState.InBattle);
            Debug.Log("[StageController] 전투 시작!");
        }

        /// <summary>
        /// 전투 완료
        /// </summary>
        public void CompleteBattle(bool victory)
        {
            if (_currentState != StageState.InBattle)
            {
                Debug.LogWarning($"[StageController] 전투 완료 호출 불가 상태: {_currentState}");
                return;
            }

            if (victory)
            {
                ChangeState(StageState.BattleComplete);
                Debug.Log("[StageController] 전투 승리!");
            }
            else
            {
                Debug.Log("[StageController] 전투 패배!");
                // 패배 처리는 추후 구현
            }
        }

        /// <summary>
        /// 스테이지 클리어
        /// </summary>
        public void ClearStage()
        {
            if (_currentState != StageState.BattleComplete)
            {
                Debug.LogWarning($"[StageController] 스테이지 클리어 불가 상태: {_currentState}");
                return;
            }

            ChangeState(StageState.StageCleared);
            OnStageCleared?.Invoke();

            float duration = StageDuration;
            Debug.Log($"[StageController] 스테이지 클리어! (소요 시간: {duration:F2}초, 이동 횟수: {TotalMovesInStage})");
        }

        /// <summary>
        /// 상태 변경
        /// </summary>
        private void ChangeState(StageState newState)
        {
            StageState oldState = _currentState;
            _currentState = newState;

            Debug.Log($"[StageController] 상태 변경: {oldState} → {newState}");
            OnStateChanged?.Invoke(oldState, newState);
        }

        /// <summary>
        /// 현재 스테이지 정보 문자열 반환
        /// </summary>
        public string GetStageInfo()
        {
            if (_stageData == null)
                return "스테이지 미초기화";

            return $"스테이지: {_stageData.stageName}\n" +
                   $"상태: {_currentState}\n" +
                   $"플레이어 위치: {_playerPosition}\n" +
                   $"전투 위치: {_stageData.battlePosition}\n" +
                   $"이동 횟수: {TotalMovesInStage}\n" +
                   $"소요 시간: {StageDuration:F2}초";
        }

        /// <summary>
        /// 스테이지 데이터 가져오기
        /// </summary>
        public StageData GetStageData()
        {
            return _stageData;
        }
    }
}
