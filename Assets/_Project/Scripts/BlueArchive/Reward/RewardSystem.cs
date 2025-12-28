using System;
using System.Collections.Generic;
using UnityEngine;
using NexonGame.BlueArchive.Data;
using NexonGame.BlueArchive.Combat;

namespace NexonGame.BlueArchive.Reward
{
    /// <summary>
    /// 보상 지급 결과
    /// </summary>
    public class RewardGrantResult
    {
        public bool Success { get; set; }
        public string FailureReason { get; set; }
        public List<RewardItemData> GrantedRewards { get; set; }
        public int TotalRewardCount { get; set; }

        public RewardGrantResult()
        {
            GrantedRewards = new List<RewardItemData>();
        }
    }

    /// <summary>
    /// 보상 시스템
    /// - 스테이지 클리어 보상 지급
    /// - 보상 검증 및 기록
    /// - 보상 통계 추적
    /// </summary>
    public class RewardSystem
    {
        // 보상 인벤토리 (간단한 구현)
        private Dictionary<RewardItemType, int> _inventory;

        // 통계
        public int TotalRewardsGranted { get; private set; }
        public int TotalCurrencyGained { get; private set; }
        public int TotalMaterialsGained { get; private set; }
        public int TotalEquipmentsGained { get; private set; }
        public int TotalExpGained { get; private set; }

        // 이벤트
        public event Action<RewardItemData> OnRewardGranted;
        public event Action<List<RewardItemData>> OnAllRewardsGranted;

        public RewardSystem()
        {
            _inventory = new Dictionary<RewardItemType, int>();
            foreach (RewardItemType type in Enum.GetValues(typeof(RewardItemType)))
            {
                _inventory[type] = 0;
            }

            TotalRewardsGranted = 0;
            TotalCurrencyGained = 0;
            TotalMaterialsGained = 0;
            TotalEquipmentsGained = 0;
            TotalExpGained = 0;
        }

        /// <summary>
        /// 보상 계산 (테스트용)
        /// </summary>
        public RewardGrantResult CalculateRewards(string stageName, int totalMoves, CombatLogSystem combatLog)
        {
            RewardGrantResult result = new RewardGrantResult();

            // 기본 보상 생성
            result.GrantedRewards.Add(new RewardItemData
            {
                itemName = "크레딧",
                itemType = RewardItemType.Currency,
                quantity = 1000
            });

            result.GrantedRewards.Add(new RewardItemData
            {
                itemName = "노트",
                itemType = RewardItemType.Material,
                quantity = 5
            });

            result.GrantedRewards.Add(new RewardItemData
            {
                itemName = "T1 가방",
                itemType = RewardItemType.Equipment,
                quantity = 1
            });

            result.GrantedRewards.Add(new RewardItemData
            {
                itemName = "전술 EXP",
                itemType = RewardItemType.Exp,
                quantity = 150
            });

            result.Success = true;
            result.TotalRewardCount = result.GrantedRewards.Count;

            return result;
        }

        /// <summary>
        /// 스테이지 클리어 보상 지급
        /// </summary>
        public RewardGrantResult GrantStageRewards(StageData stageData, CombatResult combatResult)
        {
            RewardGrantResult result = new RewardGrantResult();

            // 전투 승리 확인
            if (combatResult.State != CombatState.Victory)
            {
                result.Success = false;
                result.FailureReason = $"전투 승리가 아님 (현재 상태: {combatResult.State})";
                return result;
            }

            // 스테이지 데이터 확인
            if (stageData == null || stageData.rewards == null || stageData.rewards.Count == 0)
            {
                result.Success = false;
                result.FailureReason = "보상 데이터 없음";
                return result;
            }

            // 보상 지급
            foreach (var reward in stageData.rewards)
            {
                GrantReward(reward);
                result.GrantedRewards.Add(reward);
                Debug.Log($"[RewardSystem] 보상 지급: {reward.itemName} x{reward.quantity} ({reward.itemType})");
            }

            result.Success = true;
            result.TotalRewardCount = stageData.rewards.Count;
            TotalRewardsGranted += result.TotalRewardCount;

            OnAllRewardsGranted?.Invoke(result.GrantedRewards);

            Debug.Log($"[RewardSystem] 스테이지 '{stageData.stageName}' 보상 지급 완료! 총 {result.TotalRewardCount}개");
            return result;
        }

        /// <summary>
        /// 개별 보상 지급
        /// </summary>
        /// <summary>
        /// 보상 지급 (public - TestVisualizationRunner에서 사용)
        /// </summary>
        public void GrantReward(RewardItemData reward)
        {
            // 인벤토리에 추가
            _inventory[reward.itemType] += reward.quantity;

            // 타입별 통계 업데이트
            switch (reward.itemType)
            {
                case RewardItemType.Currency:
                    TotalCurrencyGained += reward.quantity;
                    break;
                case RewardItemType.Material:
                    TotalMaterialsGained += reward.quantity;
                    break;
                case RewardItemType.Equipment:
                    TotalEquipmentsGained += reward.quantity;
                    break;
                case RewardItemType.Exp:
                    TotalExpGained += reward.quantity;
                    break;
            }

            OnRewardGranted?.Invoke(reward);
        }

        /// <summary>
        /// 인벤토리에서 특정 타입의 아이템 개수 조회
        /// </summary>
        public int GetInventoryCount(RewardItemType itemType)
        {
            return _inventory[itemType];
        }

        /// <summary>
        /// 전체 인벤토리 조회
        /// </summary>
        public Dictionary<RewardItemType, int> GetInventory()
        {
            return new Dictionary<RewardItemType, int>(_inventory);
        }

        /// <summary>
        /// 보상 시스템 초기화
        /// </summary>
        public void Reset()
        {
            _inventory.Clear();
            foreach (RewardItemType type in Enum.GetValues(typeof(RewardItemType)))
            {
                _inventory[type] = 0;
            }

            TotalRewardsGranted = 0;
            TotalCurrencyGained = 0;
            TotalMaterialsGained = 0;
            TotalEquipmentsGained = 0;
            TotalExpGained = 0;

            Debug.Log("[RewardSystem] 보상 시스템 초기화");
        }

        /// <summary>
        /// 보상 통계 정보
        /// </summary>
        public string GetStatistics()
        {
            return $"=== 보상 통계 ===\n" +
                   $"총 보상 지급: {TotalRewardsGranted}개\n" +
                   $"화폐: {TotalCurrencyGained}\n" +
                   $"재료: {TotalMaterialsGained}\n" +
                   $"장비: {TotalEquipmentsGained}\n" +
                   $"경험치: {TotalExpGained}";
        }

        /// <summary>
        /// 인벤토리 상태 문자열
        /// </summary>
        public string GetInventoryStatus()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("=== 인벤토리 ===");

            foreach (var kvp in _inventory)
            {
                if (kvp.Value > 0)
                {
                    sb.AppendLine($"{kvp.Key}: {kvp.Value}");
                }
            }

            return sb.ToString();
        }
    }
}
