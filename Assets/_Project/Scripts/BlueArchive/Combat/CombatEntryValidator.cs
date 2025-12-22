using UnityEngine;
using NexonGame.BlueArchive.Stage;

namespace NexonGame.BlueArchive.Combat
{
    /// <summary>
    /// 전투 진입 검증 결과
    /// </summary>
    public class CombatEntryResult
    {
        public bool CanEnterCombat { get; set; }
        public string FailureReason { get; set; }
        public StageState RequiredState { get; set; }
        public StageState CurrentState { get; set; }

        public CombatEntryResult(bool canEnter, string reason = "")
        {
            CanEnterCombat = canEnter;
            FailureReason = reason;
        }
    }

    /// <summary>
    /// 전투 진입 검증 시스템
    /// - 전투 진입 조건 검증
    /// - 스테이지 상태 확인
    /// - 전투 준비 상태 관리
    /// </summary>
    public class CombatEntryValidator
    {
        private StageController _stageController;

        // 통계
        public int TotalEntryAttempts { get; private set; }
        public int SuccessfulEntries { get; private set; }
        public int FailedEntries { get; private set; }

        public CombatEntryValidator(StageController stageController)
        {
            _stageController = stageController;
            TotalEntryAttempts = 0;
            SuccessfulEntries = 0;
            FailedEntries = 0;
        }

        /// <summary>
        /// 전투 진입 가능 여부 검증
        /// </summary>
        public CombatEntryResult ValidateEntry()
        {
            TotalEntryAttempts++;

            // 스테이지가 초기화되었는지 확인
            if (_stageController.GetStageData() == null)
            {
                FailedEntries++;
                return new CombatEntryResult(false, "스테이지가 초기화되지 않음")
                {
                    CurrentState = _stageController.CurrentState
                };
            }

            // 현재 상태가 ReadyForBattle인지 확인
            if (_stageController.CurrentState != StageState.ReadyForBattle)
            {
                FailedEntries++;
                return new CombatEntryResult(false, $"전투 준비 상태가 아님 (현재: {_stageController.CurrentState})")
                {
                    RequiredState = StageState.ReadyForBattle,
                    CurrentState = _stageController.CurrentState
                };
            }

            // 플레이어가 전투 위치에 있는지 확인
            if (_stageController.PlayerPosition != _stageController.BattlePosition)
            {
                FailedEntries++;
                return new CombatEntryResult(false, "플레이어가 전투 위치에 없음")
                {
                    CurrentState = _stageController.CurrentState
                };
            }

            // 모든 조건 통과
            SuccessfulEntries++;
            Debug.Log("[CombatEntryValidator] 전투 진입 조건 통과!");
            return new CombatEntryResult(true);
        }

        /// <summary>
        /// 전투 진입 시도
        /// </summary>
        public bool TryEnterCombat()
        {
            CombatEntryResult result = ValidateEntry();

            if (result.CanEnterCombat)
            {
                _stageController.StartBattle();
                Debug.Log("[CombatEntryValidator] 전투 진입 성공!");
                return true;
            }
            else
            {
                Debug.LogWarning($"[CombatEntryValidator] 전투 진입 실패: {result.FailureReason}");
                return false;
            }
        }

        /// <summary>
        /// 전투 진입 전 요구사항 체크리스트
        /// </summary>
        public string GetEntryRequirementsChecklist()
        {
            bool stageInitialized = _stageController.GetStageData() != null;
            bool correctState = _stageController.CurrentState == StageState.ReadyForBattle;
            bool atBattlePosition = _stageController.PlayerPosition == _stageController.BattlePosition;

            return $"=== 전투 진입 요구사항 ===\n" +
                   $"[{(stageInitialized ? "✓" : "✗")}] 스테이지 초기화됨\n" +
                   $"[{(correctState ? "✓" : "✗")}] 전투 준비 상태 (현재: {_stageController.CurrentState})\n" +
                   $"[{(atBattlePosition ? "✓" : "✗")}] 전투 위치 도달 (현재: {_stageController.PlayerPosition}, 목표: {_stageController.BattlePosition})";
        }

        /// <summary>
        /// 통계 정보 반환
        /// </summary>
        public string GetStatistics()
        {
            float successRate = TotalEntryAttempts > 0
                ? (float)SuccessfulEntries / TotalEntryAttempts * 100f
                : 0f;

            return $"=== 전투 진입 통계 ===\n" +
                   $"총 시도 횟수: {TotalEntryAttempts}\n" +
                   $"성공: {SuccessfulEntries}\n" +
                   $"실패: {FailedEntries}\n" +
                   $"성공률: {successRate:F1}%";
        }

        /// <summary>
        /// 통계 초기화
        /// </summary>
        public void ResetStatistics()
        {
            TotalEntryAttempts = 0;
            SuccessfulEntries = 0;
            FailedEntries = 0;
            Debug.Log("[CombatEntryValidator] 통계 초기화");
        }
    }
}
