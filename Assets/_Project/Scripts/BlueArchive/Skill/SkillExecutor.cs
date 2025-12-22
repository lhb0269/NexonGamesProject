using System.Collections.Generic;
using UnityEngine;
using NexonGame.BlueArchive.Data;
using NexonGame.BlueArchive.Character;
using NexonGame.BlueArchive.Combat;

namespace NexonGame.BlueArchive.Skill
{
    /// <summary>
    /// 스킬 실행 결과
    /// </summary>
    public class SkillExecutionResult
    {
        public bool Success { get; set; }
        public string FailureReason { get; set; }
        public int TotalDamage { get; set; }
        public List<string> TargetsHit { get; set; }
        public int CostSpent { get; set; }

        public SkillExecutionResult()
        {
            TargetsHit = new List<string>();
        }
    }

    /// <summary>
    /// 스킬 실행 시스템
    /// - EX 스킬 실행 및 데미지 계산
    /// - 코스트 소모 처리
    /// - 전투 로그 기록
    /// </summary>
    public class SkillExecutor
    {
        private CostSystem _costSystem;
        private CombatLogSystem _combatLog;

        // 통계
        public int TotalSkillsExecuted { get; private set; }
        public int TotalDamageDealt { get; private set; }

        public SkillExecutor(CostSystem costSystem, CombatLogSystem combatLog)
        {
            _costSystem = costSystem;
            _combatLog = combatLog;
            TotalSkillsExecuted = 0;
            TotalDamageDealt = 0;
        }

        /// <summary>
        /// 학생의 EX 스킬을 실행합니다
        /// </summary>
        public SkillExecutionResult ExecuteSkill(Student student, List<Enemy> targets)
        {
            SkillExecutionResult result = new SkillExecutionResult();

            // 스킬 데이터 확인
            if (student.Data.exSkill == null)
            {
                result.Success = false;
                result.FailureReason = "스킬 데이터 없음";
                return result;
            }

            SkillData skill = student.Data.exSkill;

            // 쿨다운 확인
            if (!student.CanUseSkill())
            {
                result.Success = false;
                result.FailureReason = $"스킬 쿨다운 중 (남은 시간: {student.SkillCooldownRemaining:F1}초)";
                return result;
            }

            // 코스트 확인 및 소모
            int requiredCost = student.GetSkillCost();
            if (!_costSystem.TrySpendCost(requiredCost))
            {
                result.Success = false;
                result.FailureReason = $"코스트 부족 (필요: {requiredCost}, 현재: {_costSystem.CurrentCost})";
                return result;
            }

            result.CostSpent = requiredCost;

            // 스킬 사용 (쿨다운 시작)
            student.UseSkill();

            // 로그 기록
            _combatLog.LogSkillUsed(student.Data.studentName, skill.skillName, requiredCost);

            // 대상 선택 및 데미지 계산
            List<Enemy> selectedTargets = SelectTargets(targets, skill.targetType);
            int totalDamage = 0;

            foreach (var target in selectedTargets)
            {
                if (!target.IsAlive)
                    continue;

                // 데미지 계산
                int baseDamage = skill.baseDamage;
                int finalDamage = Mathf.RoundToInt(baseDamage * skill.damageMultiplier);

                // 데미지 적용
                int actualDamage = target.TakeDamage(finalDamage);
                totalDamage += actualDamage;

                // 학생 통계 업데이트
                student.RecordDamage(actualDamage);

                // 로그 기록
                _combatLog.LogDamageDealt(student.Data.studentName, target.Data.enemyName, actualDamage);

                result.TargetsHit.Add(target.Data.enemyName);

                // 적 격파 확인
                if (!target.IsAlive)
                {
                    _combatLog.LogUnitDefeated(target.Data.enemyName, student.Data.studentName);
                }
            }

            result.Success = true;
            result.TotalDamage = totalDamage;
            TotalSkillsExecuted++;
            TotalDamageDealt += totalDamage;

            Debug.Log($"[SkillExecutor] {student.Data.studentName}의 [{skill.skillName}] 실행 완료! " +
                      $"총 데미지: {totalDamage}, 타겟 수: {selectedTargets.Count}");

            return result;
        }

        /// <summary>
        /// 스킬 타겟 타입에 따라 대상 선택
        /// </summary>
        private List<Enemy> SelectTargets(List<Enemy> allTargets, SkillTargetType targetType)
        {
            List<Enemy> selected = new List<Enemy>();
            List<Enemy> aliveTargets = allTargets.FindAll(e => e.IsAlive);

            if (aliveTargets.Count == 0)
                return selected;

            switch (targetType)
            {
                case SkillTargetType.Single:
                    // 첫 번째 살아있는 적 선택
                    selected.Add(aliveTargets[0]);
                    break;

                case SkillTargetType.Multiple:
                    // 최대 3명 선택
                    int count = Mathf.Min(3, aliveTargets.Count);
                    for (int i = 0; i < count; i++)
                    {
                        selected.Add(aliveTargets[i]);
                    }
                    break;

                case SkillTargetType.Area:
                    // 모든 살아있는 적 선택
                    selected.AddRange(aliveTargets);
                    break;
            }

            return selected;
        }

        /// <summary>
        /// 스킬 실행 가능 여부 확인 (코스트 포함)
        /// </summary>
        public bool CanExecuteSkill(Student student)
        {
            if (student.Data.exSkill == null)
                return false;

            if (!student.CanUseSkill())
                return false;

            int requiredCost = student.GetSkillCost();
            if (!_costSystem.HasEnoughCost(requiredCost))
                return false;

            return true;
        }

        /// <summary>
        /// 통계 초기화
        /// </summary>
        public void ResetStatistics()
        {
            TotalSkillsExecuted = 0;
            TotalDamageDealt = 0;
            Debug.Log("[SkillExecutor] 통계 초기화");
        }

        /// <summary>
        /// 통계 정보 반환
        /// </summary>
        public string GetStatistics()
        {
            return $"=== 스킬 실행 통계 ===\n" +
                   $"총 스킬 실행: {TotalSkillsExecuted}회\n" +
                   $"총 데미지: {TotalDamageDealt}";
        }
    }
}
