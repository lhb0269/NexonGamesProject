using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace NexonGame.BlueArchive.Combat
{
    /// <summary>
    /// 전투 로그 엔트리 타입
    /// </summary>
    public enum CombatLogType
    {
        CombatStart,        // 전투 시작
        CombatEnd,          // 전투 종료
        SkillUsed,          // 스킬 사용
        DamageDealt,        // 데미지 가함
        DamageTaken,        // 데미지 받음
        UnitDefeated,       // 유닛 격파
        CostSpent,          // 코스트 소모
        Healing,            // 힐링
        StateChange         // 상태 변화
    }

    /// <summary>
    /// 전투 로그 엔트리
    /// </summary>
    [Serializable]
    public class CombatLogEntry
    {
        public float Timestamp;          // 로그 발생 시간
        public CombatLogType LogType;    // 로그 타입
        public string ActorName;         // 행위자 이름
        public string TargetName;        // 대상 이름 (선택적)
        public int Value;                // 수치 값 (데미지, 힐량 등)
        public string Message;           // 로그 메시지

        public CombatLogEntry(CombatLogType logType, string actorName, string message, string targetName = "", int value = 0)
        {
            Timestamp = Time.time;
            LogType = logType;
            ActorName = actorName;
            TargetName = targetName;
            Value = value;
            Message = message;
        }

        public override string ToString()
        {
            return $"[{Timestamp:F2}s] [{LogType}] {Message}";
        }
    }

    /// <summary>
    /// 전투 로그 시스템
    /// - 전투 중 발생하는 모든 이벤트 기록
    /// - 데미지 추적 및 통계
    /// - 테스트 검증용 데이터 제공
    /// </summary>
    public class CombatLogSystem
    {
        private List<CombatLogEntry> _logs = new List<CombatLogEntry>();
        private float _combatStartTime;
        private bool _isCombatActive;

        // 통계
        public int TotalDamageDealt { get; private set; }
        public int TotalDamageTaken { get; private set; }
        public int TotalSkillsUsed { get; private set; }
        public int TotalEnemiesDefeated { get; private set; }
        public int TotalCostSpent { get; private set; }

        // 학생별 통계 (학생 이름 → 데미지)
        private Dictionary<string, int> _studentDamageStats = new Dictionary<string, int>();
        public IReadOnlyDictionary<string, int> StudentDamageStats => _studentDamageStats;

        // 로그 접근
        public IReadOnlyList<CombatLogEntry> Logs => _logs;
        public int LogCount => _logs.Count;

        // 이벤트
        public event Action<CombatLogEntry> OnLogAdded;

        /// <summary>
        /// 전투 시작 로그
        /// </summary>
        public void LogCombatStart(string stageName)
        {
            _combatStartTime = Time.time;
            _isCombatActive = true;
            AddLog(CombatLogType.CombatStart, "System", $"전투 시작: {stageName}");
        }

        /// <summary>
        /// 전투 종료 로그
        /// </summary>
        public void LogCombatEnd(bool victory)
        {
            float duration = Time.time - _combatStartTime;
            string result = victory ? "승리" : "패배";
            AddLog(CombatLogType.CombatEnd, "System", $"전투 종료: {result} (소요 시간: {duration:F2}초)");
            _isCombatActive = false;
        }

        /// <summary>
        /// 스킬 사용 로그
        /// </summary>
        public void LogSkillUsed(string actorName, string skillName, int costSpent)
        {
            TotalSkillsUsed++;
            AddLog(CombatLogType.SkillUsed, actorName, $"{actorName}이(가) [{skillName}] 스킬 사용 (코스트: {costSpent})", "", costSpent);
        }

        /// <summary>
        /// 데미지 가한 로그
        /// </summary>
        public void LogDamageDealt(string actorName, string targetName, int damage)
        {
            TotalDamageDealt += damage;

            // 학생별 데미지 통계 업데이트
            if (!_studentDamageStats.ContainsKey(actorName))
            {
                _studentDamageStats[actorName] = 0;
            }
            _studentDamageStats[actorName] += damage;

            Debug.Log($"[CombatLogSystem] 데미지 기록: {actorName} → {targetName}: {damage} (누적: {_studentDamageStats[actorName]}, 총: {TotalDamageDealt})");

            AddLog(CombatLogType.DamageDealt, actorName, $"{actorName} → {targetName}: {damage} 데미지", targetName, damage);
        }

        /// <summary>
        /// 데미지 받은 로그
        /// </summary>
        public void LogDamageTaken(string actorName, int damage, int remainingHP)
        {
            TotalDamageTaken += damage;
            AddLog(CombatLogType.DamageTaken, actorName, $"{actorName}이(가) {damage} 데미지 받음 (남은 HP: {remainingHP})", "", damage);
        }

        /// <summary>
        /// 유닛 격파 로그
        /// </summary>
        public void LogUnitDefeated(string defeatedUnit, string killerName)
        {
            TotalEnemiesDefeated++;
            AddLog(CombatLogType.UnitDefeated, killerName, $"{killerName}이(가) {defeatedUnit}을(를) 격파!", defeatedUnit);
        }

        /// <summary>
        /// 코스트 소모 로그
        /// </summary>
        public void LogCostSpent(string actorName, int amount, int remainingCost)
        {
            TotalCostSpent += amount;
            AddLog(CombatLogType.CostSpent, actorName, $"코스트 소모: -{amount} (남은 코스트: {remainingCost})", "", amount);
        }

        /// <summary>
        /// 힐링 로그
        /// </summary>
        public void LogHealing(string actorName, string targetName, int amount, int currentHP)
        {
            AddLog(CombatLogType.Healing, actorName, $"{actorName} → {targetName}: {amount} 회복 (현재 HP: {currentHP})", targetName, amount);
        }

        /// <summary>
        /// 상태 변화 로그
        /// </summary>
        public void LogStateChange(string actorName, string stateDescription)
        {
            AddLog(CombatLogType.StateChange, actorName, $"{actorName}: {stateDescription}");
        }

        /// <summary>
        /// 로그 추가 (내부 메서드)
        /// </summary>
        private void AddLog(CombatLogType logType, string actorName, string message, string targetName = "", int value = 0)
        {
            var entry = new CombatLogEntry(logType, actorName, message, targetName, value);
            _logs.Add(entry);
            OnLogAdded?.Invoke(entry);
            Debug.Log($"[CombatLog] {entry}");
        }

        /// <summary>
        /// 특정 타입의 로그만 필터링
        /// </summary>
        public List<CombatLogEntry> GetLogsByType(CombatLogType logType)
        {
            return _logs.FindAll(log => log.LogType == logType);
        }

        /// <summary>
        /// 특정 액터의 로그만 필터링
        /// </summary>
        public List<CombatLogEntry> GetLogsByActor(string actorName)
        {
            return _logs.FindAll(log => log.ActorName == actorName);
        }

        /// <summary>
        /// 전투 통계 요약
        /// </summary>
        public string GetCombatSummary()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=== 전투 통계 요약 ===");
            sb.AppendLine($"총 로그 수: {_logs.Count}");
            sb.AppendLine($"총 데미지: {TotalDamageDealt}");
            sb.AppendLine($"받은 데미지: {TotalDamageTaken}");
            sb.AppendLine($"스킬 사용: {TotalSkillsUsed}회");
            sb.AppendLine($"격파한 적: {TotalEnemiesDefeated}");
            sb.AppendLine($"소모한 코스트: {TotalCostSpent}");
            return sb.ToString();
        }

        /// <summary>
        /// 전체 로그를 문자열로 출력
        /// </summary>
        public string GetFullLog()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=== 전투 로그 ===");
            foreach (var log in _logs)
            {
                sb.AppendLine(log.ToString());
            }
            return sb.ToString();
        }

        /// <summary>
        /// 로그 초기화
        /// </summary>
        public void Clear()
        {
            _logs.Clear();
            TotalDamageDealt = 0;
            TotalDamageTaken = 0;
            TotalSkillsUsed = 0;
            TotalEnemiesDefeated = 0;
            TotalCostSpent = 0;
            _studentDamageStats.Clear();
            _isCombatActive = false;
            Debug.Log("[CombatLogSystem] 로그 초기화");
        }

        /// <summary>
        /// 현재 전투가 진행 중인지 확인
        /// </summary>
        public bool IsCombatActive()
        {
            return _isCombatActive;
        }
    }
}
