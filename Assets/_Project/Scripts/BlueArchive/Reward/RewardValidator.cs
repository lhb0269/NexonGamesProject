using System.Collections.Generic;
using UnityEngine;
using NexonGame.BlueArchive.Data;
using NexonGame.BlueArchive.Combat;

namespace NexonGame.BlueArchive.Reward
{
    /// <summary>
    /// 보상 검증 결과
    /// </summary>
    public class RewardValidationResult
    {
        public bool IsValid { get; set; }
        public string FailureReason { get; set; }
        public List<string> ValidationErrors { get; set; }
        public int ExpectedRewardCount { get; set; }
        public int ActualRewardCount { get; set; }

        public RewardValidationResult()
        {
            ValidationErrors = new List<string>();
        }
    }

    /// <summary>
    /// 보상 검증 시스템
    /// - 스테이지 클리어 보상 검증
    /// - 보상 지급 조건 확인
    /// - 보상 무결성 검증 (테스트 체크포인트 #6)
    /// </summary>
    public class RewardValidator
    {
        private RewardSystem _rewardSystem;

        // 통계
        public int TotalValidations { get; private set; }
        public int SuccessfulValidations { get; private set; }
        public int FailedValidations { get; private set; }

        public RewardValidator(RewardSystem rewardSystem)
        {
            _rewardSystem = rewardSystem;
            TotalValidations = 0;
            SuccessfulValidations = 0;
            FailedValidations = 0;
        }

        /// <summary>
        /// 보상 지급 조건 검증
        /// </summary>
        public RewardValidationResult ValidateRewardConditions(StageData stageData, CombatResult combatResult)
        {
            TotalValidations++;

            RewardValidationResult result = new RewardValidationResult();
            result.ExpectedRewardCount = stageData.rewards != null ? stageData.rewards.Count : 0;

            // 1. 전투 승리 확인
            if (combatResult.State != CombatState.Victory)
            {
                result.IsValid = false;
                result.FailureReason = $"전투 승리 조건 미충족 (현재: {combatResult.State})";
                result.ValidationErrors.Add("전투에서 승리하지 못함");
                FailedValidations++;
                return result;
            }

            // 2. 스테이지 데이터 확인
            if (stageData == null)
            {
                result.IsValid = false;
                result.FailureReason = "스테이지 데이터 없음";
                result.ValidationErrors.Add("스테이지 데이터가 null");
                FailedValidations++;
                return result;
            }

            // 3. 보상 데이터 확인
            if (stageData.rewards == null || stageData.rewards.Count == 0)
            {
                result.IsValid = false;
                result.FailureReason = "보상 데이터 없음";
                result.ValidationErrors.Add("보상 목록이 비어있음");
                FailedValidations++;
                return result;
            }

            // 4. 각 보상 아이템 유효성 검증
            foreach (var reward in stageData.rewards)
            {
                if (reward == null)
                {
                    result.ValidationErrors.Add("null 보상 아이템 발견");
                    continue;
                }

                if (string.IsNullOrEmpty(reward.itemName))
                {
                    result.ValidationErrors.Add($"아이템 이름 없음 (타입: {reward.itemType})");
                }

                if (reward.quantity <= 0)
                {
                    result.ValidationErrors.Add($"{reward.itemName}: 수량이 0 이하 ({reward.quantity})");
                }
            }

            // 검증 결과 판정
            if (result.ValidationErrors.Count > 0)
            {
                result.IsValid = false;
                result.FailureReason = $"보상 데이터 검증 실패 ({result.ValidationErrors.Count}개 오류)";
                FailedValidations++;
            }
            else
            {
                result.IsValid = true;
                SuccessfulValidations++;
                Debug.Log("[RewardValidator] 보상 조건 검증 성공");
            }

            return result;
        }

        /// <summary>
        /// 보상 지급 후 검증 (실제로 지급되었는지 확인)
        /// </summary>
        public RewardValidationResult ValidateRewardGrant(StageData stageData, RewardGrantResult grantResult)
        {
            TotalValidations++;

            RewardValidationResult result = new RewardValidationResult();
            result.ExpectedRewardCount = stageData.rewards != null ? stageData.rewards.Count : 0;
            result.ActualRewardCount = grantResult.TotalRewardCount;

            // 1. 보상 지급 성공 여부 확인
            if (!grantResult.Success)
            {
                result.IsValid = false;
                result.FailureReason = $"보상 지급 실패: {grantResult.FailureReason}";
                result.ValidationErrors.Add(grantResult.FailureReason);
                FailedValidations++;
                return result;
            }

            // 2. 보상 개수 일치 확인
            if (result.ExpectedRewardCount != result.ActualRewardCount)
            {
                result.IsValid = false;
                result.FailureReason = $"보상 개수 불일치 (예상: {result.ExpectedRewardCount}, 실제: {result.ActualRewardCount})";
                result.ValidationErrors.Add(result.FailureReason);
                FailedValidations++;
                return result;
            }

            // 3. 각 보상 아이템이 지급되었는지 확인
            foreach (var expectedReward in stageData.rewards)
            {
                bool found = grantResult.GrantedRewards.Exists(r =>
                    r.itemName == expectedReward.itemName &&
                    r.itemType == expectedReward.itemType &&
                    r.quantity == expectedReward.quantity);

                if (!found)
                {
                    result.ValidationErrors.Add($"보상 미지급: {expectedReward.itemName} x{expectedReward.quantity}");
                }
            }

            // 4. 인벤토리 확인 (실제로 추가되었는지)
            foreach (var reward in stageData.rewards)
            {
                int inventoryCount = _rewardSystem.GetInventoryCount(reward.itemType);
                if (inventoryCount < reward.quantity)
                {
                    result.ValidationErrors.Add($"인벤토리 수량 부족: {reward.itemType} (필요: {reward.quantity}, 현재: {inventoryCount})");
                }
            }

            // 검증 결과 판정
            if (result.ValidationErrors.Count > 0)
            {
                result.IsValid = false;
                result.FailureReason = $"보상 지급 검증 실패 ({result.ValidationErrors.Count}개 오류)";
                FailedValidations++;
            }
            else
            {
                result.IsValid = true;
                SuccessfulValidations++;
                Debug.Log("[RewardValidator] 보상 지급 검증 성공");
            }

            return result;
        }

        /// <summary>
        /// 전체 프로세스 검증 (조건 → 지급 → 확인)
        /// </summary>
        public RewardValidationResult ValidateFullRewardProcess(StageData stageData, CombatResult combatResult, RewardGrantResult grantResult)
        {
            RewardValidationResult conditionResult = ValidateRewardConditions(stageData, combatResult);
            if (!conditionResult.IsValid)
            {
                return conditionResult;
            }

            RewardValidationResult grantValidation = ValidateRewardGrant(stageData, grantResult);
            return grantValidation;
        }

        /// <summary>
        /// 검증 통계 정보
        /// </summary>
        public string GetStatistics()
        {
            float successRate = TotalValidations > 0
                ? (float)SuccessfulValidations / TotalValidations * 100f
                : 0f;

            return $"=== 보상 검증 통계 ===\n" +
                   $"총 검증: {TotalValidations}회\n" +
                   $"성공: {SuccessfulValidations}회\n" +
                   $"실패: {FailedValidations}회\n" +
                   $"성공률: {successRate:F1}%";
        }

        /// <summary>
        /// 통계 초기화
        /// </summary>
        public void ResetStatistics()
        {
            TotalValidations = 0;
            SuccessfulValidations = 0;
            FailedValidations = 0;
            Debug.Log("[RewardValidator] 검증 통계 초기화");
        }
    }
}
