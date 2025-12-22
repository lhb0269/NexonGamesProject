using System;
using UnityEngine;

namespace NexonGame.BlueArchive.Combat
{
    /// <summary>
    /// 블루 아카이브 코스트 시스템
    /// - 시간에 따라 자동으로 코스트 증가
    /// - 스킬 사용 시 코스트 소모
    /// - 최대 코스트 제한
    /// </summary>
    public class CostSystem
    {
        // 코스트 설정
        public int MaxCost { get; private set; } = 10;
        public int CurrentCost { get; private set; }

        // 코스트 회복 설정
        public float CostRegenRate { get; private set; } = 1f; // 초당 회복량
        private float _costAccumulator = 0f;

        // 통계
        public int TotalCostGained { get; private set; }
        public int TotalCostSpent { get; private set; }
        public int SkillUsageCount { get; private set; }

        // 이벤트
        public event Action<int> OnCostChanged;
        public event Action<int, int> OnCostSpent; // 소모량, 남은 코스트

        public CostSystem(int maxCost = 10, float regenRate = 1f, int startingCost = 0)
        {
            MaxCost = maxCost;
            CostRegenRate = regenRate;
            CurrentCost = startingCost;
            _costAccumulator = 0f;
            TotalCostGained = 0;
            TotalCostSpent = 0;
            SkillUsageCount = 0;
        }

        /// <summary>
        /// 코스트를 업데이트합니다 (시간에 따른 자동 회복)
        /// </summary>
        public void Update(float deltaTime)
        {
            if (CurrentCost >= MaxCost)
            {
                _costAccumulator = 0f;
                return;
            }

            _costAccumulator += CostRegenRate * deltaTime;

            while (_costAccumulator >= 1f && CurrentCost < MaxCost)
            {
                _costAccumulator -= 1f;
                AddCost(1);
            }
        }

        /// <summary>
        /// 코스트를 추가합니다
        /// </summary>
        public void AddCost(int amount)
        {
            if (amount <= 0) return;

            int previousCost = CurrentCost;
            CurrentCost = Mathf.Min(MaxCost, CurrentCost + amount);
            int actualGained = CurrentCost - previousCost;

            if (actualGained > 0)
            {
                TotalCostGained += actualGained;
                OnCostChanged?.Invoke(CurrentCost);
                Debug.Log($"[CostSystem] 코스트 증가: +{actualGained} (현재: {CurrentCost}/{MaxCost})");
            }
        }

        /// <summary>
        /// 코스트를 소모합니다
        /// </summary>
        /// <returns>코스트 소모 성공 여부</returns>
        public bool TrySpendCost(int amount)
        {
            if (amount <= 0)
            {
                Debug.LogWarning("[CostSystem] 코스트 소모량이 0 이하입니다.");
                return false;
            }

            if (CurrentCost < amount)
            {
                Debug.LogWarning($"[CostSystem] 코스트 부족: 필요 {amount}, 현재 {CurrentCost}");
                return false;
            }

            CurrentCost -= amount;
            TotalCostSpent += amount;
            SkillUsageCount++;

            OnCostChanged?.Invoke(CurrentCost);
            OnCostSpent?.Invoke(amount, CurrentCost);

            Debug.Log($"[CostSystem] 코스트 소모: -{amount} (남은 코스트: {CurrentCost}/{MaxCost}, 스킬 사용 횟수: {SkillUsageCount})");
            return true;
        }

        /// <summary>
        /// 코스트가 충분한지 확인합니다
        /// </summary>
        public bool HasEnoughCost(int amount)
        {
            return CurrentCost >= amount;
        }

        /// <summary>
        /// 코스트를 특정 값으로 설정합니다 (테스트용)
        /// </summary>
        public void SetCost(int cost)
        {
            CurrentCost = Mathf.Clamp(cost, 0, MaxCost);
            OnCostChanged?.Invoke(CurrentCost);
        }

        /// <summary>
        /// 코스트를 최대치로 채웁니다
        /// </summary>
        public void FillCost()
        {
            int gained = MaxCost - CurrentCost;
            if (gained > 0)
            {
                CurrentCost = MaxCost;
                TotalCostGained += gained;
                OnCostChanged?.Invoke(CurrentCost);
                Debug.Log($"[CostSystem] 코스트 최대 충전: {CurrentCost}/{MaxCost}");
            }
        }

        /// <summary>
        /// 시스템을 리셋합니다
        /// </summary>
        public void Reset()
        {
            CurrentCost = 0;
            _costAccumulator = 0f;
            TotalCostGained = 0;
            TotalCostSpent = 0;
            SkillUsageCount = 0;
            OnCostChanged?.Invoke(CurrentCost);
            Debug.Log("[CostSystem] 시스템 리셋");
        }

        /// <summary>
        /// 현재 상태 정보를 문자열로 반환합니다
        /// </summary>
        public string GetStatusString()
        {
            return $"코스트: {CurrentCost}/{MaxCost} | 획득: {TotalCostGained} | 소모: {TotalCostSpent} | 스킬 사용: {SkillUsageCount}회";
        }
    }
}
