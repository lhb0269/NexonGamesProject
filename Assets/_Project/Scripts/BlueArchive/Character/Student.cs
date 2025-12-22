using UnityEngine;
using NexonGame.BlueArchive.Data;

namespace NexonGame.BlueArchive.Character
{
    /// <summary>
    /// 학생 런타임 인스턴스
    /// </summary>
    public class Student
    {
        public StudentData Data { get; private set; }

        // 현재 상태
        public int CurrentHP { get; private set; }
        public bool IsAlive => CurrentHP > 0;

        // 스킬 상태
        public float SkillCooldownRemaining { get; private set; }
        public bool IsSkillReady => SkillCooldownRemaining <= 0f;

        // 통계
        public int TotalDamageDealt { get; private set; }
        public int TotalDamageTaken { get; private set; }
        public int SkillUsedCount { get; private set; }

        public Student(StudentData data)
        {
            Data = data;
            CurrentHP = data.maxHP;
            SkillCooldownRemaining = 0f;
            TotalDamageDealt = 0;
            TotalDamageTaken = 0;
            SkillUsedCount = 0;
        }

        /// <summary>
        /// 데미지를 받습니다
        /// </summary>
        public int TakeDamage(int damage)
        {
            int actualDamage = Mathf.Max(1, damage - Data.defense);
            CurrentHP = Mathf.Max(0, CurrentHP - actualDamage);
            TotalDamageTaken += actualDamage;

            Debug.Log($"[{Data.studentName}] 데미지 받음: {actualDamage} (남은 HP: {CurrentHP})");
            return actualDamage;
        }

        /// <summary>
        /// 스킬을 사용합니다
        /// </summary>
        public bool UseSkill()
        {
            if (!IsSkillReady)
            {
                Debug.LogWarning($"[{Data.studentName}] 스킬이 쿨다운 중입니다. 남은 시간: {SkillCooldownRemaining}초");
                return false;
            }

            SkillCooldownRemaining = Data.exSkill.cooldownTime;
            SkillUsedCount++;

            Debug.Log($"[{Data.studentName}] EX 스킬 사용! ({Data.exSkill.skillName}) - 사용 횟수: {SkillUsedCount}");
            return true;
        }

        /// <summary>
        /// 스킬 사용에 필요한 코스트를 반환합니다
        /// </summary>
        public int GetSkillCost()
        {
            return Data.exSkill != null ? Data.exSkill.costAmount : 0;
        }

        /// <summary>
        /// 스킬 사용이 가능한지 확인합니다 (쿨다운 체크)
        /// </summary>
        public bool CanUseSkill()
        {
            return IsSkillReady && Data.exSkill != null;
        }

        /// <summary>
        /// 스킬 쿨다운을 업데이트합니다
        /// </summary>
        public void UpdateCooldown(float deltaTime)
        {
            if (SkillCooldownRemaining > 0f)
            {
                SkillCooldownRemaining = Mathf.Max(0f, SkillCooldownRemaining - deltaTime);
            }
        }

        /// <summary>
        /// 데미지를 가합니다
        /// </summary>
        public void RecordDamage(int damage)
        {
            TotalDamageDealt += damage;
        }

        /// <summary>
        /// HP를 회복합니다
        /// </summary>
        public void Heal(int amount)
        {
            CurrentHP = Mathf.Min(Data.maxHP, CurrentHP + amount);
            Debug.Log($"[{Data.studentName}] HP 회복: +{amount} (현재 HP: {CurrentHP})");
        }
    }
}
