using System.Collections.Generic;
using UnityEngine;
using NexonGame.BlueArchive.Character;
using NexonGame.BlueArchive.Skill;

namespace NexonGame.BlueArchive.Combat
{
    /// <summary>
    /// 전투 상태
    /// </summary>
    public enum CombatState
    {
        NotStarted,
        InProgress,
        Victory,
        Defeat
    }

    /// <summary>
    /// 전투 결과
    /// </summary>
    public class CombatResult
    {
        public CombatState State { get; set; }
        public int TotalDamageDealt { get; set; }
        public int TotalDamageTaken { get; set; }
        public int EnemiesDefeated { get; set; }
        public int SkillsUsed { get; set; }
        public float CombatDuration { get; set; }
        public bool AllStudentsAlive { get; set; }

        public CombatResult()
        {
            State = CombatState.NotStarted;
        }
    }

    /// <summary>
    /// 전투 시스템
    /// - 학생과 적의 전투 관리
    /// - 스킬 실행 및 데미지 계산
    /// - 승패 판정
    /// </summary>
    public class CombatSystem
    {
        private List<Student> _students;
        private List<Enemy> _enemies;
        private CostSystem _costSystem;
        private CombatLogSystem _combatLog;
        private SkillExecutor _skillExecutor;

        private CombatState _currentState;
        private float _combatStartTime;

        public CombatState CurrentState => _currentState;
        public List<Student> Students => _students;
        public List<Enemy> Enemies => _enemies;
        public CostSystem CostSystem => _costSystem;
        public CombatLogSystem CombatLog => _combatLog;
        public SkillExecutor SkillExecutor => _skillExecutor;

        public CombatSystem()
        {
            _students = new List<Student>();
            _enemies = new List<Enemy>();
            _costSystem = new CostSystem(maxCost: 10, regenRate: 1f, startingCost: 5);
            _combatLog = new CombatLogSystem();
            _skillExecutor = new SkillExecutor(_costSystem, _combatLog);
            _currentState = CombatState.NotStarted;
        }

        /// <summary>
        /// 전투 초기화
        /// </summary>
        public void InitializeCombat(List<Student> students, List<Enemy> enemies, string stageName = "Unknown Stage")
        {
            _students = new List<Student>(students);
            _enemies = new List<Enemy>(enemies);

            _currentState = CombatState.InProgress;
            _combatStartTime = Time.time;

            _combatLog.LogCombatStart(stageName);
            Debug.Log($"[CombatSystem] 전투 시작: {stageName}");
            Debug.Log($"[CombatSystem] 학생 {_students.Count}명 vs 적 {_enemies.Count}명");
        }

        /// <summary>
        /// 학생의 스킬을 사용합니다
        /// </summary>
        public SkillExecutionResult UseStudentSkill(Student student)
        {
            if (_currentState != CombatState.InProgress)
            {
                return new SkillExecutionResult
                {
                    Success = false,
                    FailureReason = $"전투 중이 아님 (상태: {_currentState})"
                };
            }

            if (!_students.Contains(student))
            {
                return new SkillExecutionResult
                {
                    Success = false,
                    FailureReason = "전투에 참가하지 않은 학생"
                };
            }

            if (!student.IsAlive)
            {
                return new SkillExecutionResult
                {
                    Success = false,
                    FailureReason = "학생이 전투 불능 상태"
                };
            }

            // 스킬 실행
            SkillExecutionResult result = _skillExecutor.ExecuteSkill(student, _enemies);

            // 승패 확인
            CheckCombatEnd();

            return result;
        }

        /// <summary>
        /// 적의 공격을 처리합니다
        /// </summary>
        public void ProcessEnemyAttack(Enemy enemy)
        {
            if (_currentState != CombatState.InProgress)
                return;

            if (!enemy.IsAlive)
                return;

            // 살아있는 학생 목록
            List<Student> aliveStudents = _students.FindAll(s => s.IsAlive);
            if (aliveStudents.Count == 0)
                return;

            // 랜덤한 학생 공격
            Student target = aliveStudents[Random.Range(0, aliveStudents.Count)];
            int damage = enemy.Attack();
            int actualDamage = target.TakeDamage(damage);

            _combatLog.LogDamageTaken(target.Data.studentName, actualDamage, target.CurrentHP);

            // 학생 전멸 확인
            CheckCombatEnd();
        }

        /// <summary>
        /// 시간 업데이트 (코스트 회복, 쿨다운)
        /// </summary>
        public void Update(float deltaTime)
        {
            if (_currentState != CombatState.InProgress)
                return;

            // 코스트 회복
            _costSystem.Update(deltaTime);

            // 학생 쿨다운 업데이트
            foreach (var student in _students)
            {
                if (student.IsAlive)
                {
                    student.UpdateCooldown(deltaTime);
                }
            }
        }

        /// <summary>
        /// 전투 종료 조건 확인
        /// </summary>
        private void CheckCombatEnd()
        {
            // 모든 적 격파 → 승리
            bool allEnemiesDefeated = _enemies.TrueForAll(e => !e.IsAlive);
            if (allEnemiesDefeated)
            {
                _currentState = CombatState.Victory;
                _combatLog.LogCombatEnd(victory: true);
                Debug.Log("[CombatSystem] 전투 승리!");
                return;
            }

            // 모든 학생 전멸 → 패배
            bool allStudentsDefeated = _students.TrueForAll(s => !s.IsAlive);
            if (allStudentsDefeated)
            {
                _currentState = CombatState.Defeat;
                _combatLog.LogCombatEnd(victory: false);
                Debug.Log("[CombatSystem] 전투 패배...");
                return;
            }
        }

        /// <summary>
        /// 전투 결과 생성
        /// </summary>
        public CombatResult GetCombatResult()
        {
            CombatResult result = new CombatResult();
            result.State = _currentState;
            result.TotalDamageDealt = _combatLog.TotalDamageDealt;
            result.TotalDamageTaken = _combatLog.TotalDamageTaken;
            result.EnemiesDefeated = _combatLog.TotalEnemiesDefeated;
            result.SkillsUsed = _combatLog.TotalSkillsUsed;
            result.CombatDuration = Time.time - _combatStartTime;
            result.AllStudentsAlive = _students.TrueForAll(s => s.IsAlive);

            return result;
        }

        /// <summary>
        /// 살아있는 학생 수
        /// </summary>
        public int GetAliveStudentCount()
        {
            return _students.FindAll(s => s.IsAlive).Count;
        }

        /// <summary>
        /// 살아있는 적 수
        /// </summary>
        public int GetAliveEnemyCount()
        {
            return _enemies.FindAll(e => e.IsAlive).Count;
        }

        /// <summary>
        /// 스킬 사용 가능한 학생 목록
        /// </summary>
        public List<Student> GetStudentsWithAvailableSkills()
        {
            List<Student> available = new List<Student>();

            foreach (var student in _students)
            {
                if (student.IsAlive && _skillExecutor.CanExecuteSkill(student))
                {
                    available.Add(student);
                }
            }

            return available;
        }

        /// <summary>
        /// 전투 상태 정보
        /// </summary>
        public string GetCombatStatus()
        {
            return $"=== 전투 상태 ===\n" +
                   $"상태: {_currentState}\n" +
                   $"학생: {GetAliveStudentCount()}/{_students.Count}\n" +
                   $"적: {GetAliveEnemyCount()}/{_enemies.Count}\n" +
                   $"코스트: {_costSystem.CurrentCost}/{_costSystem.MaxCost}\n" +
                   $"스킬 사용: {_combatLog.TotalSkillsUsed}회\n" +
                   $"총 데미지: {_combatLog.TotalDamageDealt}";
        }

        /// <summary>
        /// 전투 초기화 (재사용)
        /// </summary>
        public void Reset()
        {
            _students.Clear();
            _enemies.Clear();
            _costSystem.Reset();
            _combatLog.Clear();
            _skillExecutor.ResetStatistics();
            _currentState = CombatState.NotStarted;
            Debug.Log("[CombatSystem] 전투 시스템 초기화");
        }
    }
}
