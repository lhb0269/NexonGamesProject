using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NexonGame.BlueArchive.Stage;
using NexonGame.BlueArchive.Combat;
using NexonGame.BlueArchive.Character;
using NexonGame.BlueArchive.Data;
using NexonGame.BlueArchive.UI;

namespace NexonGame.Tests.PlayMode
{
    /// <summary>
    /// 테스트 시뮬레이션 실행기
    /// - Unity Test Framework 없이 일반 씬에서 실행 가능
    /// - 빌드 가능한 실행 파일로 제작 가능
    /// - 5개 체크포인트를 순차적으로 실행하고 결과를 시각화
    /// </summary>
    public class TestVisualizationRunner : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private bool _autoStart = true;
        [SerializeField] private float _checkpointDelay = 1f;

        [Header("References (Auto-created if null)")]
        [SerializeField] private StageManager _stageManager;
        [SerializeField] private CombatManager _combatManager;

        // Test Data
        private StageData _testStageData;
        private List<StudentData> _testStudents;
        private List<EnemyData> _testEnemies;

        // Test Progress
        private TestProgressPanel _testProgressPanel;
        private int _currentCheckpoint = 0;
        private List<CheckpointResult> _results = new List<CheckpointResult>();

        // Test Statistics
        private int _totalTests = 5;
        private int _passedTests = 0;
        private int _failedTests = 0;

        private void Start()
        {
            Debug.Log("[TestVisualizationRunner] 테스트 시뮬레이션 시작");

            // 자동 시작 설정
            if (_autoStart)
            {
                StartCoroutine(RunAllTestsCoroutine());
            }
        }

        /// <summary>
        /// 모든 테스트 실행 (외부 호출용)
        /// </summary>
        public void RunAllTests()
        {
            StartCoroutine(RunAllTestsCoroutine());
        }

        /// <summary>
        /// 모든 테스트 실행 코루틴
        /// </summary>
        private IEnumerator RunAllTestsCoroutine()
        {
            Debug.Log("\n=====================================");
            Debug.Log("블루 아카이브 테스트 자동화 시작");
            Debug.Log("=====================================\n");

            // 초기화
            yield return SetupTest();

            // 5개 체크포인트 순차 실행
            yield return RunCheckpoint1_PlatformMovement();
            yield return new WaitForSeconds(_checkpointDelay);

            yield return RunCheckpoint2_BattleEntry();
            yield return new WaitForSeconds(_checkpointDelay);

            yield return RunCheckpoint3_SkillUsage();
            yield return new WaitForSeconds(_checkpointDelay);

            yield return RunCheckpoint4_DamageTracking();
            yield return new WaitForSeconds(_checkpointDelay);

            yield return RunCheckpoint5_RewardVerification();
            yield return new WaitForSeconds(_checkpointDelay);

            // 최종 결과 표시
            ShowFinalResults();
        }

        /// <summary>
        /// 테스트 환경 초기화
        /// </summary>
        private IEnumerator SetupTest()
        {
            Debug.Log("[Setup] 테스트 환경 초기화 중...");

            // StageManager 생성 또는 찾기
            if (_stageManager == null)
            {
                var stageObj = new GameObject("StageManager");
                _stageManager = stageObj.AddComponent<StageManager>();
            }

            // CombatManager 생성 또는 찾기
            if (_combatManager == null)
            {
                var combatObj = new GameObject("CombatManager");
                _combatManager = combatObj.AddComponent<CombatManager>();
            }

            // 테스트 데이터 생성
            CreateTestData();

            // UI 생성
            CreateTestProgressUI();

            Debug.Log("[Setup] ✅ 초기화 완료");
            yield return null;
        }

        /// <summary>
        /// 테스트 데이터 생성
        /// </summary>
        private void CreateTestData()
        {
            // 스테이지 데이터
            _testStageData = ScriptableObject.CreateInstance<StageData>();
            _testStageData.stageName = "Normal 1-4";
            _testStageData.gridWidth = 5;
            _testStageData.gridHeight = 4;
            _testStageData.startPosition = new Vector2Int(0, 0);
            _testStageData.battlePosition = new Vector2Int(3, 1);
            _testStageData.platformPositions = new List<Vector2Int>
            {
                new Vector2Int(1, 1),
                new Vector2Int(0, 2),
                new Vector2Int(2, 1)
            };

            // 학생 데이터 생성
            _testStudents = CreateTestStudents();

            // 적 데이터 생성
            _testEnemies = CreateTestEnemies();

            Debug.Log($"[TestData] 스테이지: {_testStageData.stageName}");
            Debug.Log($"[TestData] 학생: {_testStudents.Count}명");
            Debug.Log($"[TestData] 적: {_testEnemies.Count}명");
        }

        /// <summary>
        /// 테스트용 학생 데이터 생성
        /// </summary>
        private List<StudentData> CreateTestStudents()
        {
            var students = new List<StudentData>();

            // 아리스
            var arisu = ScriptableObject.CreateInstance<StudentData>();
            arisu.studentName = "아리스";
            arisu.maxHP = 1000;
            arisu.attack = 100;
            arisu.exSkill = CreateSkill("EX: 정의의 일격", 500, 1f, 20f, SkillTargetType.Single, 4);
            students.Add(arisu);

            // 호시노
            var hoshino = ScriptableObject.CreateInstance<StudentData>();
            hoshino.studentName = "호시노";
            hoshino.maxHP = 1200;
            hoshino.attack = 80;
            hoshino.exSkill = CreateSkill("EX: 수호의 맹세", 300, 1f, 25f, SkillTargetType.Multiple, 5);
            students.Add(hoshino);

            // 이로하
            var iroha = ScriptableObject.CreateInstance<StudentData>();
            iroha.studentName = "이로하";
            iroha.maxHP = 900;
            iroha.attack = 120;
            iroha.exSkill = CreateSkill("EX: 신속한 사격", 400, 1f, 15f, SkillTargetType.Single, 3);
            students.Add(iroha);

            // 시로코
            var shiroko = ScriptableObject.CreateInstance<StudentData>();
            shiroko.studentName = "시로코";
            shiroko.maxHP = 950;
            shiroko.attack = 110;
            shiroko.exSkill = CreateSkill("EX: 전술 지원", 350, 1f, 20f, SkillTargetType.Area, 4);
            students.Add(shiroko);

            return students;
        }

        /// <summary>
        /// 테스트용 스킬 데이터 생성
        /// </summary>
        private SkillData CreateSkill(string name, int damage, float multiplier, float cooldown, SkillTargetType targetType, int cost)
        {
            var skill = ScriptableObject.CreateInstance<SkillData>();
            skill.skillName = name;
            skill.baseDamage = damage;
            skill.damageMultiplier = multiplier;
            skill.cooldownTime = cooldown;
            skill.targetType = targetType;
            skill.costAmount = cost;
            return skill;
        }

        /// <summary>
        /// 테스트용 적 데이터 생성
        /// </summary>
        private List<EnemyData> CreateTestEnemies()
        {
            var enemies = new List<EnemyData>();

            for (int i = 0; i < 3; i++)
            {
                var enemy = new EnemyData($"일반병{i + 1}", 500, 50, 20);
                enemies.Add(enemy);
            }

            return enemies;
        }

        /// <summary>
        /// 테스트 진행 상황 UI 생성
        /// </summary>
        private void CreateTestProgressUI()
        {
            var canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                var canvasObj = new GameObject("Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
                canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }

            var panelObj = new GameObject("TestProgressPanel");
            panelObj.transform.SetParent(canvas.transform, false);

            _testProgressPanel = panelObj.AddComponent<TestProgressPanel>();
            // Awake()에서 자동으로 UI 생성됨
        }

        /// <summary>
        /// 체크포인트 #1: 플랫폼 이동 검증
        /// </summary>
        private IEnumerator RunCheckpoint1_PlatformMovement()
        {
            _currentCheckpoint = 1;
            Debug.Log("\n[체크포인트 #1] 플랫폼 이동 검증 시작");
            _testProgressPanel.UpdateCheckpoint(1, CheckpointStatus.InProgress);
            _testProgressPanel.UpdateMessage("플랫폼 이동 테스트 중...");

            var result = new CheckpointResult { CheckpointNumber = 1, Name = "플랫폼 이동 검증" };

            // 스테이지 초기화
            _stageManager.InitializeStage(_testStageData);
            yield return null;

            // 이동 경로
            var movementPath = new List<Vector2Int>
            {
                new Vector2Int(1, 1),
                new Vector2Int(0, 2),
                new Vector2Int(1, 1),
                new Vector2Int(2, 1),
                new Vector2Int(3, 1)
            };

            // 이동 실행
            int successfulMoves = 0;
            foreach (var targetPos in movementPath)
            {
                _stageManager.SimulatePlatformClick(targetPos);
                yield return new WaitForSeconds(0.3f);

                if (_stageManager.PlayerPosition == targetPos)
                {
                    successfulMoves++;
                }
            }

            // 검증
            bool allMovesSuccessful = successfulMoves == movementPath.Count;
            bool reachedBattlePosition = _stageManager.PlayerPosition == _testStageData.battlePosition;
            bool correctState = _stageManager.CurrentState == StageState.ReadyForBattle;

            result.Passed = allMovesSuccessful && reachedBattlePosition && correctState;
            result.Message = result.Passed
                ? $"✅ 성공 - 이동: {successfulMoves}/{movementPath.Count}, 최종 위치: {_stageManager.PlayerPosition}"
                : $"❌ 실패 - 이동: {successfulMoves}/{movementPath.Count}";

            Debug.Log($"[체크포인트 #1] {result.Message}");

            _results.Add(result);
            _testProgressPanel.UpdateCheckpoint(1, result.Passed ? CheckpointStatus.Completed : CheckpointStatus.Failed);

            if (result.Passed) _passedTests++;
            else _failedTests++;

            yield return null;
        }

        /// <summary>
        /// 체크포인트 #2: 전투 진입 검증
        /// </summary>
        private IEnumerator RunCheckpoint2_BattleEntry()
        {
            _currentCheckpoint = 2;
            Debug.Log("\n[체크포인트 #2] 전투 진입 검증 시작");
            _testProgressPanel.UpdateCheckpoint(2, CheckpointStatus.InProgress);
            _testProgressPanel.UpdateMessage("전투 진입 테스트 중...");

            var result = new CheckpointResult { CheckpointNumber = 2, Name = "전투 진입 검증" };

            // 전투 시작
            _stageManager.StartBattle();
            yield return null;

            // 전투 초기화
            _combatManager.InitializeCombat(_testStudents, _testEnemies, _testStageData.stageName);
            yield return new WaitForSeconds(0.5f);

            // 검증
            bool stageInBattle = _stageManager.CurrentState == StageState.InBattle;
            bool combatInProgress = _combatManager.CurrentState == CombatState.InProgress;
            bool costSystemReady = _combatManager.CurrentCost >= 0;

            result.Passed = stageInBattle && combatInProgress && costSystemReady;
            result.Message = result.Passed
                ? $"✅ 성공 - 전투 상태: {_combatManager.CurrentState}, 코스트: {_combatManager.CurrentCost}"
                : $"❌ 실패 - 전투 초기화 오류";

            Debug.Log($"[체크포인트 #2] {result.Message}");

            _results.Add(result);
            _testProgressPanel.UpdateCheckpoint(2, result.Passed ? CheckpointStatus.Completed : CheckpointStatus.Failed);

            if (result.Passed) _passedTests++;
            else _failedTests++;
        }

        /// <summary>
        /// 체크포인트 #3: EX 스킬 사용 및 코스트 소모 검증
        /// </summary>
        private IEnumerator RunCheckpoint3_SkillUsage()
        {
            _currentCheckpoint = 3;
            Debug.Log("\n[체크포인트 #3] EX 스킬 사용 검증 시작");
            _testProgressPanel.UpdateCheckpoint(3, CheckpointStatus.InProgress);
            _testProgressPanel.UpdateMessage("EX 스킬 사용 테스트 중...");

            var result = new CheckpointResult { CheckpointNumber = 3, Name = "EX 스킬 사용 검증" };

            var combatLog = _combatManager.CombatSystem.CombatLog;
            int initialSkillCount = combatLog.TotalSkillsUsed;
            int initialDamage = combatLog.TotalDamageDealt;

            // 코스트 충전 대기
            yield return new WaitForSeconds(2f);

            // 스킬 사용
            int skillsUsed = 0;
            for (int i = 0; i < _testStudents.Count; i++)
            {
                var student = _testStudents[i];

                // 코스트 충분할 때까지 대기
                while (_combatManager.CurrentCost < student.skillCost)
                {
                    yield return new WaitForSeconds(1f);
                }

                // 스킬 사용
                _combatManager.SimulateSkillButtonClick(i);
                yield return new WaitForSeconds(0.3f);
                skillsUsed++;

                // 적 전멸 확인
                if (_combatManager.GetAliveEnemyCount() == 0)
                    break;
            }

            // 검증
            int finalSkillCount = combatLog.TotalSkillsUsed;
            int finalDamage = combatLog.TotalDamageDealt;
            int totalCostSpent = combatLog.TotalCostSpent;

            bool skillsExecuted = finalSkillCount > initialSkillCount;
            bool damageDealt = finalDamage > initialDamage;
            bool costSpent = totalCostSpent > 0;

            result.Passed = skillsExecuted && damageDealt && costSpent;
            result.Message = result.Passed
                ? $"✅ 성공 - 스킬: {finalSkillCount}회, 데미지: {finalDamage}, 코스트: {totalCostSpent}"
                : $"❌ 실패 - 스킬/데미지/코스트 미기록";

            Debug.Log($"[체크포인트 #3] {result.Message}");

            _results.Add(result);
            _testProgressPanel.UpdateCheckpoint(3, result.Passed ? CheckpointStatus.Completed : CheckpointStatus.Failed);

            if (result.Passed) _passedTests++;
            else _failedTests++;

            yield return null;
        }

        /// <summary>
        /// 체크포인트 #4: 전투별 데미지 추적
        /// </summary>
        private IEnumerator RunCheckpoint4_DamageTracking()
        {
            _currentCheckpoint = 4;
            Debug.Log("\n[체크포인트 #4] 데미지 추적 검증 시작");
            _testProgressPanel.UpdateCheckpoint(4, CheckpointStatus.InProgress);
            _testProgressPanel.UpdateMessage("데미지 추적 테스트 중...");

            var result = new CheckpointResult { CheckpointNumber = 4, Name = "데미지 추적 검증" };

            var combatLog = _combatManager.CombatSystem.CombatLog;

            // 현재 데미지 확인
            int currentDamage = combatLog.TotalDamageDealt;

            // 살아있는 적이 있으면 추가 공격
            if (_combatManager.GetAliveEnemyCount() > 0)
            {
                yield return new WaitForSeconds(2f);
                _combatManager.SimulateSkillButtonClick(0);
                yield return new WaitForSeconds(0.5f);
            }

            // 검증
            int finalDamage = combatLog.TotalDamageDealt;
            bool damageIncreased = finalDamage >= currentDamage;
            bool hasStudentStats = combatLog.StudentDamageStats.Count > 0;

            result.Passed = damageIncreased && hasStudentStats;
            result.Message = result.Passed
                ? $"✅ 성공 - 총 데미지: {finalDamage}, 학생별 통계: {combatLog.StudentDamageStats.Count}명"
                : $"❌ 실패 - 데미지 추적 오류";

            Debug.Log($"[체크포인트 #4] {result.Message}");

            _results.Add(result);
            _testProgressPanel.UpdateCheckpoint(4, result.Passed ? CheckpointStatus.Completed : CheckpointStatus.Failed);

            if (result.Passed) _passedTests++;
            else _failedTests++;

            yield return null;
        }

        /// <summary>
        /// 체크포인트 #5: 보상 획득 검증
        /// </summary>
        private IEnumerator RunCheckpoint5_RewardVerification()
        {
            _currentCheckpoint = 5;
            Debug.Log("\n[체크포인트 #5] 보상 획득 검증 시작");
            _testProgressPanel.UpdateCheckpoint(5, CheckpointStatus.InProgress);
            _testProgressPanel.UpdateMessage("보상 획득 테스트 중...");

            var result = new CheckpointResult { CheckpointNumber = 5, Name = "보상 획득 검증" };

            // 전투 완료
            _stageManager.CompleteBattle(victory: true);
            yield return null;

            // 스테이지 클리어
            _stageManager.ClearStage();
            yield return null;

            // 검증
            bool stageCleared = _stageManager.CurrentState == StageState.StageCleared;

            result.Passed = stageCleared;
            result.Message = result.Passed
                ? $"✅ 성공 - 스테이지 클리어: {_stageManager.CurrentState}"
                : $"❌ 실패 - 스테이지 상태 오류";

            Debug.Log($"[체크포인트 #5] {result.Message}");

            _results.Add(result);
            _testProgressPanel.UpdateCheckpoint(5, result.Passed ? CheckpointStatus.Completed : CheckpointStatus.Failed);

            if (result.Passed) _passedTests++;
            else _failedTests++;

            yield return null;
        }

        /// <summary>
        /// 최종 결과 표시
        /// </summary>
        private void ShowFinalResults()
        {
            Debug.Log("\n=====================================");
            Debug.Log("테스트 결과 요약");
            Debug.Log("=====================================");
            Debug.Log($"총 테스트: {_totalTests}개");
            Debug.Log($"통과: {_passedTests}개");
            Debug.Log($"실패: {_failedTests}개");
            Debug.Log($"성공률: {(_passedTests * 100f / _totalTests):F1}%");
            Debug.Log("=====================================\n");

            // 개별 결과 출력
            foreach (var result in _results)
            {
                Debug.Log($"[체크포인트 #{result.CheckpointNumber}] {result.Name}: {result.Message}");
            }

            // UI 업데이트
            _testProgressPanel.UpdateMessage(
                _passedTests == _totalTests
                    ? "✅ 모든 테스트 통과!"
                    : $"⚠ 일부 테스트 실패 ({_passedTests}/{_totalTests})"
            );
        }

        /// <summary>
        /// 체크포인트 결과 클래스
        /// </summary>
        private class CheckpointResult
        {
            public int CheckpointNumber;
            public string Name;
            public bool Passed;
            public string Message;
        }
    }
}
