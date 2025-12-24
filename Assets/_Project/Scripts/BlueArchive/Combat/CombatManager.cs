using UnityEngine;
using System.Collections.Generic;
using NexonGame.BlueArchive.Character;
using NexonGame.BlueArchive.Data;
using NexonGame.BlueArchive.Skill;
using NexonGame.BlueArchive.UI;

namespace NexonGame.BlueArchive.Combat
{
    /// <summary>
    /// 전투 매니저 (MonoBehaviour)
    /// - CombatSystem 로직 클래스 래핑
    /// - 학생/적 GameObject 생성 및 관리
    /// - PlayMode 테스트에서 사용
    /// </summary>
    public class CombatManager : MonoBehaviour
    {
        [Header("Character Prefabs")]
        [SerializeField] private GameObject _studentPrefab;
        [SerializeField] private GameObject _enemyPrefab;

        [Header("Character Containers")]
        [SerializeField] private Transform _studentContainer;
        [SerializeField] private Transform _enemyContainer;

        [Header("Spawn Positions")]
        [SerializeField] private Vector3 _studentSpawnOffset = new Vector3(-3, 0, 0);
        [SerializeField] private float _studentSpacing = 1.5f;
        [SerializeField] private Vector3 _enemySpawnOffset = new Vector3(3, 0, 0);
        [SerializeField] private float _enemySpacing = 1.5f;

        // 로직 클래스 (Pure C#)
        private CombatSystem _combatSystem;
        private CostSystem _costSystem;
        private CombatLogSystem _combatLog;
        private SkillExecutor _skillExecutor;

        // 생성된 오브젝트들
        private List<StudentObject> _studentObjects;
        private List<EnemyObject> _enemyObjects;
        private Dictionary<Student, StudentObject> _studentObjectMap;
        private Dictionary<Enemy, EnemyObject> _enemyObjectMap;

        // UI 컴포넌트
        private CostDisplay _costDisplay;
        private CombatLogPanel _combatLogPanel;
        private CombatStatusPanel _combatStatusPanel;

        // 프로퍼티
        public CombatSystem CombatSystem => _combatSystem;
        public CombatState CurrentState => _combatSystem?.CurrentState ?? CombatState.NotStarted;
        public int CurrentCost => _costSystem?.CurrentCost ?? 0;
        public int MaxCost => _costSystem?.MaxCost ?? 10;

        private void Awake()
        {
            _studentObjects = new List<StudentObject>();
            _enemyObjects = new List<EnemyObject>();
            _studentObjectMap = new Dictionary<Student, StudentObject>();
            _enemyObjectMap = new Dictionary<Enemy, EnemyObject>();

            // 컨테이너 자동 생성
            if (_studentContainer == null)
            {
                var studentContainerObj = new GameObject("StudentContainer");
                studentContainerObj.transform.SetParent(transform);
                _studentContainer = studentContainerObj.transform;
            }

            if (_enemyContainer == null)
            {
                var enemyContainerObj = new GameObject("EnemyContainer");
                enemyContainerObj.transform.SetParent(transform);
                _enemyContainer = enemyContainerObj.transform;
            }
        }

        /// <summary>
        /// 전투 초기화
        /// </summary>
        public void InitializeCombat(List<StudentData> studentDataList, List<EnemyData> enemyDataList, string stageName = "TestStage")
        {
            // 로직 클래스 생성
            _combatSystem = new CombatSystem();
            _costSystem = _combatSystem.CostSystem;
            _combatLog = _combatSystem.CombatLog;
            _skillExecutor = _combatSystem.SkillExecutor;

            // Student 인스턴스 생성
            List<Student> students = new List<Student>();
            foreach (var data in studentDataList)
            {
                students.Add(new Student(data));
            }

            // Enemy 인스턴스 생성
            List<Enemy> enemies = new List<Enemy>();
            foreach (var data in enemyDataList)
            {
                enemies.Add(new Enemy(data));
            }

            // 전투 시스템 초기화
            _combatSystem.InitializeCombat(students, enemies, stageName);

            // GameObject 생성
            CreateStudentObjects(students);
            CreateEnemyObjects(enemies);

            // UI 생성
            CreateCostDisplay();
            CreateCombatLogPanel();
            CreateCombatStatusPanel(students);

            // 초기 로그
            _combatLogPanel?.AddLog($"전투 시작: {stageName}", CombatLogPanel.LogType.System);
            _combatLogPanel?.AddLog($"{students.Count}명 학생 vs {enemies.Count}명 적", CombatLogPanel.LogType.System);

            Debug.Log($"[CombatManager] 전투 초기화 완료: {students.Count}명 학생 vs {enemies.Count}명 적");
        }

        /// <summary>
        /// 학생 오브젝트 생성
        /// </summary>
        private void CreateStudentObjects(List<Student> students)
        {
            ClearStudentObjects();

            for (int i = 0; i < students.Count; i++)
            {
                Vector3 position = CalculateStudentPosition(i, students.Count);
                StudentObject studentObj = CreateStudentObject(students[i], position);
                _studentObjects.Add(studentObj);
                _studentObjectMap[students[i]] = studentObj;
            }
        }

        /// <summary>
        /// 적 오브젝트 생성
        /// </summary>
        private void CreateEnemyObjects(List<Enemy> enemies)
        {
            ClearEnemyObjects();

            for (int i = 0; i < enemies.Count; i++)
            {
                Vector3 position = CalculateEnemyPosition(i, enemies.Count);
                EnemyObject enemyObj = CreateEnemyObject(enemies[i], position);
                _enemyObjects.Add(enemyObj);
                _enemyObjectMap[enemies[i]] = enemyObj;
            }
        }

        /// <summary>
        /// 학생 오브젝트 생성
        /// </summary>
        private StudentObject CreateStudentObject(Student student, Vector3 position)
        {
            GameObject obj;

            if (_studentPrefab != null)
            {
                obj = Instantiate(_studentPrefab, position, Quaternion.identity, _studentContainer);
            }
            else
            {
                // Placeholder
                obj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                obj.transform.SetParent(_studentContainer);
                obj.transform.position = position;
                obj.transform.localScale = new Vector3(0.5f, 1f, 0.5f);
            }

            obj.name = $"Student_{student.Data.studentName}";

            StudentObject studentObject = obj.GetComponent<StudentObject>();
            if (studentObject == null)
            {
                studentObject = obj.AddComponent<StudentObject>();
            }

            studentObject.Initialize(student);
            return studentObject;
        }

        /// <summary>
        /// 적 오브젝트 생성
        /// </summary>
        private EnemyObject CreateEnemyObject(Enemy enemy, Vector3 position)
        {
            GameObject obj;

            if (_enemyPrefab != null)
            {
                obj = Instantiate(_enemyPrefab, position, Quaternion.identity, _enemyContainer);
            }
            else
            {
                // Placeholder
                obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                obj.transform.SetParent(_enemyContainer);
                obj.transform.position = position;
                obj.transform.localScale = new Vector3(0.8f, 1.2f, 0.8f);

                // 빨간색으로 표시
                var renderer = obj.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = Color.red;
                }
            }

            obj.name = $"Enemy_{enemy.Data.enemyName}";

            EnemyObject enemyObject = obj.GetComponent<EnemyObject>();
            if (enemyObject == null)
            {
                enemyObject = obj.AddComponent<EnemyObject>();
            }

            enemyObject.Initialize(enemy);
            return enemyObject;
        }

        /// <summary>
        /// 학생 위치 계산
        /// </summary>
        private Vector3 CalculateStudentPosition(int index, int totalCount)
        {
            float startY = -(totalCount - 1) * _studentSpacing / 2f;
            return _studentSpawnOffset + new Vector3(0, startY + index * _studentSpacing, 0);
        }

        /// <summary>
        /// 적 위치 계산
        /// </summary>
        private Vector3 CalculateEnemyPosition(int index, int totalCount)
        {
            float startY = -(totalCount - 1) * _enemySpacing / 2f;
            return _enemySpawnOffset + new Vector3(0, startY + index * _enemySpacing, 0);
        }

        /// <summary>
        /// 코스트 UI 생성
        /// </summary>
        private void CreateCostDisplay()
        {
            if (_costDisplay != null)
            {
                Destroy(_costDisplay.gameObject);
            }

            var costDisplayObj = new GameObject("CostDisplay");
            costDisplayObj.transform.SetParent(transform);
            _costDisplay = costDisplayObj.AddComponent<CostDisplay>();

            // 초기 코스트 표시
            UpdateCostDisplay();
        }

        /// <summary>
        /// 전투 로그 패널 생성
        /// </summary>
        private void CreateCombatLogPanel()
        {
            if (_combatLogPanel != null)
            {
                Destroy(_combatLogPanel.gameObject);
            }

            var logPanelObj = new GameObject("CombatLogPanel");
            logPanelObj.transform.SetParent(transform);
            _combatLogPanel = logPanelObj.AddComponent<CombatLogPanel>();
        }

        /// <summary>
        /// 전투 상태 패널 생성
        /// </summary>
        private void CreateCombatStatusPanel(List<Student> students)
        {
            if (_combatStatusPanel != null)
            {
                Destroy(_combatStatusPanel.gameObject);
            }

            var statusPanelObj = new GameObject("CombatStatusPanel");
            statusPanelObj.transform.SetParent(transform);
            _combatStatusPanel = statusPanelObj.AddComponent<CombatStatusPanel>();
            _combatStatusPanel.InitializeStudents(students);
            _combatStatusPanel.SetCombatLog(_combatSystem.CombatLog); // 데미지 통계용
        }

        /// <summary>
        /// 학생 스킬 사용
        /// </summary>
        public SkillExecutionResult UseStudentSkill(int studentIndex)
        {
            if (studentIndex < 0 || studentIndex >= _studentObjects.Count)
            {
                Debug.LogError($"[CombatManager] 잘못된 학생 인덱스: {studentIndex}");
                return null;
            }

            StudentObject studentObj = _studentObjects[studentIndex];
            Student student = studentObj.Student;

            // 로직 실행
            SkillExecutionResult result = _combatSystem.UseStudentSkill(student);

            if (result != null && result.Success)
            {
                // 스킬 사용 로그
                _combatLogPanel?.AddLog($"{student.Data.studentName} EX 스킬 사용!", CombatLogPanel.LogType.Skill);

                // 데미지 로그
                if (result.TotalDamage > 0)
                {
                    _combatLogPanel?.AddLog($"{result.TotalDamage} 데미지 ({result.TargetsHit}명 타격)", CombatLogPanel.LogType.Damage);
                }

                // 비주얼 업데이트
                studentObj.PlaySkillAnimation();
                UpdateAllVisuals();
            }

            return result;
        }

        /// <summary>
        /// 모든 비주얼 업데이트
        /// </summary>
        public void UpdateAllVisuals()
        {
            // 학생 HP 업데이트
            foreach (var studentObj in _studentObjects)
            {
                studentObj.UpdateVisual();
            }

            // 적 HP 업데이트 및 격파 처리
            foreach (var enemyObj in _enemyObjects)
            {
                enemyObj.UpdateVisual();

                if (!enemyObj.Enemy.IsAlive && enemyObj.gameObject.activeSelf)
                {
                    enemyObj.gameObject.SetActive(false);
                    Debug.Log($"[CombatManager] {enemyObj.Enemy.Data.enemyName} 격파!");

                    // 격파 로그
                    _combatLogPanel?.AddLog($"{enemyObj.Enemy.Data.enemyName} 격파!", CombatLogPanel.LogType.Defeat);
                }
            }

            // 코스트 UI 업데이트
            UpdateCostDisplay();

            // 학생 상태 패널 업데이트
            _combatStatusPanel?.UpdateAllStudents();
        }

        /// <summary>
        /// 코스트 UI 업데이트
        /// </summary>
        private void UpdateCostDisplay()
        {
            if (_costDisplay == null || _costSystem == null) return;

            // 회복 중인지 확인 (최대 코스트가 아닌 경우)
            bool isRegenerating = CurrentCost < MaxCost && CurrentState == CombatState.InProgress;

            _costDisplay.UpdateCost(CurrentCost, MaxCost, isRegenerating);
        }

        /// <summary>
        /// 코스트 자동 회복 (매 프레임)
        /// </summary>
        private void Update()
        {
            if (_combatSystem != null && CurrentState == CombatState.InProgress)
            {
                _combatSystem.Update(Time.deltaTime);

                // 코스트 UI 업데이트 (매 프레임)
                UpdateCostDisplay();
            }
        }

        /// <summary>
        /// 학생 가져오기
        /// </summary>
        public Student GetStudent(int index)
        {
            if (index >= 0 && index < _studentObjects.Count)
            {
                return _studentObjects[index].Student;
            }
            return null;
        }

        /// <summary>
        /// 적 가져오기
        /// </summary>
        public Enemy GetEnemy(int index)
        {
            if (index >= 0 && index < _enemyObjects.Count)
            {
                return _enemyObjects[index].Enemy;
            }
            return null;
        }

        /// <summary>
        /// 학생 개수
        /// </summary>
        public int StudentCount => _studentObjects.Count;

        /// <summary>
        /// 적 개수
        /// </summary>
        public int EnemyCount => _enemyObjects.Count;

        /// <summary>
        /// 살아있는 적 개수 (프로퍼티)
        /// </summary>
        public int AliveEnemyCount
        {
            get
            {
                int count = 0;
                foreach (var enemyObj in _enemyObjects)
                {
                    if (enemyObj.Enemy.IsAlive) count++;
                }
                return count;
            }
        }

        /// <summary>
        /// 살아있는 적 개수 (메서드)
        /// </summary>
        public int GetAliveEnemyCount()
        {
            return AliveEnemyCount;
        }

        /// <summary>
        /// 학생 오브젝트 제거
        /// </summary>
        private void ClearStudentObjects()
        {
            foreach (var obj in _studentObjects)
            {
                if (obj != null)
                {
                    Destroy(obj.gameObject);
                }
            }
            _studentObjects.Clear();
            _studentObjectMap.Clear();
        }

        /// <summary>
        /// 적 오브젝트 제거
        /// </summary>
        private void ClearEnemyObjects()
        {
            foreach (var obj in _enemyObjects)
            {
                if (obj != null)
                {
                    Destroy(obj.gameObject);
                }
            }
            _enemyObjects.Clear();
            _enemyObjectMap.Clear();
        }

        /// <summary>
        /// 전투 리셋
        /// </summary>
        public void ResetCombat()
        {
            ClearStudentObjects();
            ClearEnemyObjects();

            // UI 제거
            if (_costDisplay != null)
            {
                Destroy(_costDisplay.gameObject);
                _costDisplay = null;
            }

            if (_combatLogPanel != null)
            {
                Destroy(_combatLogPanel.gameObject);
                _combatLogPanel = null;
            }

            if (_combatStatusPanel != null)
            {
                Destroy(_combatStatusPanel.gameObject);
                _combatStatusPanel = null;
            }

            _combatSystem = null;
            _costSystem = null;
            _combatLog = null;
            _skillExecutor = null;

            Debug.Log("[CombatManager] 전투 리셋");
        }

        private void OnDestroy()
        {
            ResetCombat();
        }

        /// <summary>
        /// 디버그 정보
        /// </summary>
        public string GetDebugInfo()
        {
            if (_combatSystem == null) return "Combat not initialized";

            return $"Combat State: {CurrentState}\n" +
                   $"Cost: {CurrentCost}/{MaxCost}\n" +
                   $"Students: {StudentCount}\n" +
                   $"Enemies: {AliveEnemyCount}/{EnemyCount}";
        }
    }
}
