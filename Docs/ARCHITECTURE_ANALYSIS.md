# 블루 아카이브 테스트 자동화 시스템 아키텍처 분석

## 📊 현재 상태 분석

### 구현 완료 (EditMode)

#### 데이터 레이어
```
ScriptableObject 기반
├── StudentData - 학생 데이터
├── EnemyData - 적 데이터
├── SkillData - 스킬 데이터
├── StageData - 스테이지 데이터
└── RewardItemData - 보상 아이템 데이터

Preset Classes
├── StudentPresets - 4명의 학생 (Shiroko, Hoshino, Aru, Haruna)
└── StagePresets - Normal 1-4 스테이지
```

#### 로직 레이어 (Pure C#)
```
Stage System
├── GridManager - 그리드 관리 (10x5)
├── StageController - 스테이지 상태 관리
└── CombatEntryValidator - 전투 진입 검증

Character System
├── Student - 학생 런타임 인스턴스
└── Enemy - 적 런타임 인스턴스

Combat System
├── CombatSystem - 전투 흐름 관리
├── CostSystem - 코스트 시스템 (0-10, 자동 회복)
├── CombatLogSystem - 전투 로그 기록
└── SkillExecutor - 스킬 실행 엔진

Reward System
├── RewardSystem - 보상 지급 및 인벤토리
└── RewardValidator - 보상 검증
```

#### 테스트 레이어 (EditMode)
```
Unit Tests (173개)
├── StudentDataTests (14)
├── CostSystemTests (17)
├── CombatLogTests (16)
├── GridManagerTests (20)
├── StageControllerTests (18)
├── CombatEntryTests (14)
├── CombatSystemTests (21)
├── SkillExecutorTests (20)
├── StudentPresetTests (13)
└── RewardSystemTests (20)

Integration Tests (5개)
└── BlueArchiveIntegrationTests
    ├── Integration_FullStageFlow_AllCheckpoints
    ├── Integration_AllCheckpoints_Summary
    ├── Integration_Normal1_4_StageData_IsValid
    └── Integration_StudentPresets_AllValid
```

**총 테스트 수**: 178개

---

## 🎯 PlayMode 전환 분석

### 필요한 변경 사항

#### 1. 아키텍처 레이어 추가
```
현재 (EditMode Only):
[Tests] → [Logic] → [Data]

목표 (EditMode + PlayMode):
[PlayMode Tests] → [Presentation Layer] → [Logic] → [Data]
                         ↑
                    [EditMode Tests]
```

#### 2. Presentation Layer (새로 추가)
```
MonoBehaviour Managers
├── StageManager - StageController 래핑
├── CombatManager - CombatSystem 래핑
└── TestUIManager - UI 제어

GameObject Wrappers
├── StudentObject - Student 래핑 + 비주얼
├── EnemyObject - Enemy 래핑 + 비주얼
└── PlatformObject - 플랫폼 비주얼

Visualizers
├── GridVisualizer - 그리드 렌더링
├── HPBarVisualizer - HP 바 표시
└── SkillEffectVisualizer - 스킬 이펙트

UI Controllers
├── TestProgressPanel - 체크포인트 진행 상황
├── CombatLogPanel - 전투 로그 표시
├── CombatStatusPanel - 코스트 & 학생 상태
└── RewardResultPanel - 보상 결과
```

---

## 🏗️ 설계 패턴 분석

### 1. Model-View-Presenter (MVP) 패턴 적용

#### Model (기존 유지)
- Pure C# 로직 클래스
- 비즈니스 로직만 포함
- Unity 의존성 없음
- EditMode 테스트 가능

```csharp
// Model 예시 (기존)
public class CombatSystem
{
    private CostSystem _costSystem;
    private List<Student> _students;
    private List<Enemy> _enemies;

    public SkillExecutionResult UseStudentSkill(Student student)
    {
        // 순수 로직만
    }
}
```

#### View (새로 추가)
- MonoBehaviour 컴포넌트
- 비주얼 표현만 담당
- Presenter로부터 데이터 받음

```csharp
// View 예시 (새로 추가)
public class StudentObject : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _sprite;
    [SerializeField] private Slider _hpBar;
    [SerializeField] private TextMeshProUGUI _nameText;

    public void UpdateDisplay(string name, int currentHP, int maxHP)
    {
        _nameText.text = name;
        _hpBar.value = (float)currentHP / maxHP;
    }

    public void PlaySkillAnimation()
    {
        // 애니메이션만
    }
}
```

#### Presenter (새로 추가)
- Model과 View 연결
- 게임 이벤트 처리
- UI 업데이트 로직

```csharp
// Presenter 예시 (새로 추가)
public class CombatManager : MonoBehaviour
{
    private CombatSystem _combatSystem; // Model
    private List<StudentObject> _studentObjects; // View
    private TestUIManager _uiManager; // View

    public void UseStudentSkill(Student student)
    {
        // Model 호출
        var result = _combatSystem.UseStudentSkill(student);

        // View 업데이트
        var studentObj = FindStudentObject(student);
        studentObj.PlaySkillAnimation();

        _uiManager.AddCombatLog($"{student.Data.studentName} 스킬 사용!");
    }
}
```

### 2. Observer 패턴 (이벤트 시스템)

#### 기존 이벤트 (Model 레이어)
```csharp
// 이미 구현됨
public class CostSystem
{
    public event Action<int, int> OnCostChanged; // (old, new)
}

public class RewardSystem
{
    public event Action<RewardItemData> OnRewardGranted;
    public event Action<List<RewardItemData>> OnAllRewardsGranted;
}
```

#### 추가 필요 이벤트 (Presenter 레이어)
```csharp
public class CombatManager : MonoBehaviour
{
    // Model 이벤트 구독
    private void SubscribeToEvents()
    {
        _combatSystem.CostSystem.OnCostChanged += HandleCostChanged;
        _combatSystem.OnSkillUsed += HandleSkillUsed;
        _combatSystem.OnEnemyDefeated += HandleEnemyDefeated;
    }

    // View 업데이트
    private void HandleCostChanged(int oldCost, int newCost)
    {
        _uiManager.UpdateCostDisplay(newCost);
    }

    private void HandleSkillUsed(Student student, SkillExecutionResult result)
    {
        var studentObj = FindStudentObject(student);
        studentObj.PlaySkillAnimation();

        _uiManager.AddCombatLog($"{student.Data.studentName}의 스킬!");
    }
}
```

### 3. Factory 패턴 (오브젝트 생성)

```csharp
public class CharacterFactory : MonoBehaviour
{
    [SerializeField] private StudentObject _studentPrefab;
    [SerializeField] private EnemyObject _enemyPrefab;

    public StudentObject CreateStudent(Student student, Vector3 position)
    {
        var obj = Instantiate(_studentPrefab, position, Quaternion.identity);
        obj.Initialize(student);
        return obj;
    }

    public EnemyObject CreateEnemy(Enemy enemy, Vector3 position)
    {
        var obj = Instantiate(_enemyPrefab, position, Quaternion.identity);
        obj.Initialize(enemy);
        return obj;
    }
}
```

### 4. Object Pool 패턴 (최적화)

```csharp
public class CombatLogPool : MonoBehaviour
{
    [SerializeField] private CombatLogEntry _logEntryPrefab;
    private Queue<CombatLogEntry> _pool = new Queue<CombatLogEntry>();

    public CombatLogEntry GetLogEntry()
    {
        if (_pool.Count > 0)
        {
            var entry = _pool.Dequeue();
            entry.gameObject.SetActive(true);
            return entry;
        }

        return Instantiate(_logEntryPrefab);
    }

    public void ReturnLogEntry(CombatLogEntry entry)
    {
        entry.gameObject.SetActive(false);
        _pool.Enqueue(entry);
    }
}
```

---

## 🔄 데이터 흐름 분석

### EditMode (현재)
```
[Test]
   ↓ 직접 호출
[Logic Class] (Pure C#)
   ↓ 데이터 조회
[ScriptableObject Data]
```

**예시**:
```csharp
[Test]
public void CombatSystem_UseSkill_ShouldConsumeCorrectCost()
{
    // 직접 생성
    var combatSystem = new CombatSystem();
    var student = new Student(StudentPresets.CreateShiroko());

    // 직접 호출
    var result = combatSystem.UseStudentSkill(student);

    // 직접 검증
    Assert.AreEqual(3, result.CostSpent);
}
```

### PlayMode (목표)
```
[PlayMode Test]
   ↓ GameObject 조작
[MonoBehaviour Manager]
   ↓ 로직 호출
[Logic Class] (Pure C#)
   ↓ 데이터 조회
[ScriptableObject Data]
   ↓ 이벤트 발생
[MonoBehaviour Manager]
   ↓ UI 업데이트
[UI GameObject]
```

**예시**:
```csharp
[UnityTest]
public IEnumerator PlayMode_UseSkill_ShouldUpdateUI()
{
    // GameObject 찾기
    var combatManager = GameObject.Find("CombatManager").GetComponent<CombatManager>();
    var uiManager = GameObject.Find("UIManager").GetComponent<TestUIManager>();

    // Manager 통해 호출
    var student = combatManager.GetStudent(0);
    combatManager.UseStudentSkill(student);

    // UI 업데이트 대기
    yield return new WaitForSeconds(0.1f);

    // UI 검증
    var costText = GameObject.Find("CostText").GetComponent<TextMeshProUGUI>();
    Assert.IsTrue(costText.text.Contains("7/10")); // 코스트 소모 확인
}
```

---

## 📐 클래스 다이어그램

### StageManager 계층 구조
```
MonoBehaviour: StageManager
    ├── StageController _stageController (로직)
    ├── GridVisualizer _gridVisualizer (비주얼)
    ├── CharacterFactory _characterFactory
    ├── List<PlatformObject> _platforms
    └── TestUIManager _uiManager

    메서드:
    + InitializeStage(StageData)
    + MovePlayer(Vector2Int)
    + GetPathToBattle() : List<Vector2Int>
    - CreatePlatforms(List<Vector2Int>)
    - UpdatePlayerVisual()
```

### CombatManager 계층 구조
```
MonoBehaviour: CombatManager
    ├── CombatSystem _combatSystem (로직)
    ├── List<StudentObject> _studentObjects (비주얼)
    ├── List<EnemyObject> _enemyObjects (비주얼)
    ├── CharacterFactory _characterFactory
    └── TestUIManager _uiManager

    메서드:
    + InitializeCombat(List<Student>, List<Enemy>)
    + UseStudentSkill(Student) : SkillExecutionResult
    + Update() - 코스트 자동 회복
    - CreateCharacterObjects()
    - UpdateCharacterVisuals()
    - HandleSkillUsed(Student, SkillExecutionResult)
    - HandleEnemyDefeated(Enemy)
```

### TestUIManager 계층 구조
```
MonoBehaviour: TestUIManager
    ├── TestProgressPanel _progressPanel
    ├── CombatLogPanel _logPanel
    ├── CombatStatusPanel _statusPanel
    └── RewardResultPanel _rewardPanel

    메서드:
    + UpdateCheckpoint(int, CheckpointStatus)
    + AddCombatLog(string)
    + UpdateCostDisplay(int)
    + UpdateStudentHP(Student)
    + ShowRewardResult(RewardGrantResult)
```

---

## 🧩 컴포넌트 의존성 분석

### 의존성 그래프
```
TestUIManager
    ↑
    │ (UI 업데이트)
    │
StageManager ←→ CombatManager
    ↓              ↓
GridVisualizer   CharacterFactory
    ↓              ↓
PlatformObject   StudentObject/EnemyObject
                   ↓
              Student/Enemy (로직)
                   ↓
              StudentData/EnemyData
```

### ServiceLocator와의 통합
```
기존 DI 구조 유지:
ServiceLocator
    ├── IAudioManager
    ├── ISceneLoader
    ├── IInputManager
    └── IUIManager (기존)

새로 추가할 서비스:
ServiceLocator
    ├── IStageManager (새)
    ├── ICombatManager (새)
    └── ITestUIManager (새)
```

**등록 방법** (GameBootstrapper.cs):
```csharp
private void InitializeServices()
{
    // 기존 서비스들...

    // PlayMode 서비스 추가
    var stageManager = FindObjectOfType<StageManager>();
    if (stageManager != null)
    {
        ServiceLocator.Instance.Register<IStageManager>(stageManager);
    }

    var combatManager = FindObjectOfType<CombatManager>();
    if (combatManager != null)
    {
        ServiceLocator.Instance.Register<ICombatManager>(combatManager);
    }
}
```

---

## 🎮 PlayMode 테스트 시나리오

### 시나리오 1: 플랫폼 이동 (체크포인트 #1)
```
1. 씬 로드
2. StageManager 초기화
3. 플랫폼 오브젝트 생성 확인 (6개)
4. 시작 위치 확인 (0, 2)
5. 경로 계산
6. 각 플랫폼으로 이동
   - 이동 애니메이션 대기
   - 현재 위치 UI 업데이트 확인
7. 전투 위치 도착 확인 (7, 2)
8. 상태 변경 확인 (ReadyForBattle)
9. UI에 체크포인트 #1 완료 표시
```

### 시나리오 2: 전투 진입 (체크포인트 #2)
```
1. 전투 진입 조건 검증
2. CombatManager 초기화
3. 학생 오브젝트 생성 (4개)
4. 적 오브젝트 생성 (3개)
5. 캐릭터 위치 배치 확인
6. 코스트 게이지 UI 표시
7. 학생 상태 UI 표시
8. 전투 로그 패널 활성화
9. UI에 체크포인트 #2 완료 표시
```

### 시나리오 3-5: 전투 진행 (체크포인트 #3-5)
```
1. 코스트 충전 대기
2. Shiroko 스킬 사용
   - 스킬 애니메이션 재생 확인
   - 코스트 소모 UI 업데이트 확인 (10→7)
   - 전투 로그 추가 확인
   - 데미지 표시 확인
3. 적 HP 바 업데이트 확인
4. 다른 학생들 스킬 사용
5. 전투 로그에 모든 스킬 기록 확인
6. 총 데미지 추적 확인
7. 적 격파 시 사라지는 효과 확인
8. UI에 체크포인트 #3-5 완료 표시
```

### 시나리오 6: 보상 획득 (체크포인트 #6)
```
1. 모든 적 격파
2. 승리 상태 확인
3. 보상 결과 패널 표시
4. 보상 목록 UI 표시 (4개)
   - 크레딧 1000
   - 노트 5
   - T1 가방 1
   - 전술 EXP 150
5. 전투 통계 표시
   - 이동 횟수
   - 스킬 사용 횟수
   - 총 데미지
6. 인벤토리 업데이트 확인
7. UI에 체크포인트 #6 완료 표시
8. 테스트 완료 메시지 표시
```

---

## 🔍 기술적 과제 및 해결 방안

### 과제 1: EditMode와 PlayMode 테스트 동기화
**문제**: 같은 로직을 두 번 테스트해야 함

**해결 방안**:
- EditMode: 로직 검증, 빠른 실행 (178개 유지)
- PlayMode: 통합 검증, 시나리오 테스트 (10개 추가)
- 중복 최소화, 역할 명확히 구분

### 과제 2: PlayMode 테스트 실행 시간
**문제**: PlayMode는 느림 (씬 로드, 대기 시간)

**해결 방안**:
- 필수 시나리오만 PlayMode로 테스트
- 대기 시간 최소화 (`yield return null` 활용)
- CI/CD에서는 EditMode 우선 실행

### 과제 3: UI 업데이트 타이밍
**문제**: 비동기 UI 업데이트 확인 어려움

**해결 방안**:
```csharp
// 이벤트 기반 동기화
private bool _uiUpdateComplete = false;

private void OnUIUpdated()
{
    _uiUpdateComplete = true;
}

// 테스트에서 대기
yield return new WaitUntil(() => _uiUpdateComplete);
```

### 과제 4: 프리팹 참조 관리
**문제**: 테스트 씬에서 프리팹 찾기 어려움

**해결 방안**:
```csharp
public class TestResourceLoader : MonoBehaviour
{
    [SerializeField] private StudentObject _studentPrefab;
    [SerializeField] private EnemyObject _enemyPrefab;
    [SerializeField] private PlatformObject _platformPrefab;

    private static TestResourceLoader _instance;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public static StudentObject StudentPrefab => _instance._studentPrefab;
    public static EnemyObject EnemyPrefab => _instance._enemyPrefab;
    public static PlatformObject PlatformPrefab => _instance._platformPrefab;
}
```

### 과제 5: 메모리 관리
**문제**: 테스트 반복 시 메모리 누수

**해결 방안**:
```csharp
[TearDown]
public void TearDown()
{
    // 생성된 모든 오브젝트 제거
    var allObjects = FindObjectsOfType<GameObject>();
    foreach (var obj in allObjects)
    {
        if (obj.CompareTag("TestObject"))
        {
            Destroy(obj);
        }
    }

    // 씬 언로드
    SceneManager.UnloadSceneAsync("BlueArchiveTestScene");

    // 가비지 컬렉션 강제 실행 (테스트 환경에서만)
    System.GC.Collect();
}
```

---

## 📊 성능 분석

### EditMode 테스트 성능
```
총 테스트 수: 178개
예상 실행 시간: 3-5초
메모리 사용량: ~50MB

장점:
- 빠른 실행
- 로직 검증에 집중
- CI/CD 적합

단점:
- 비주얼 검증 불가
- 통합 검증 제한적
```

### PlayMode 테스트 성능 (예상)
```
총 테스트 수: 10개
예상 실행 시간: 30-60초
메모리 사용량: ~200MB

장점:
- 실제 게임 환경 검증
- UI/비주얼 확인 가능
- 사용자 관점 테스트

단점:
- 느린 실행 속도
- 씬 로드 오버헤드
- 디버깅 어려움
```

### 하이브리드 전략
```
로컬 개발:
1. EditMode 테스트 (빠른 검증)
2. 변경 사항 확인
3. PlayMode 테스트 (통합 검증)
4. 최종 확인

CI/CD:
1. EditMode 테스트 (178개) - 빌드마다
2. PlayMode 테스트 (10개) - PR 머지 전만
3. 실행 파일 빌드 - 릴리스 시만
```

---

## 🎯 구현 우선순위 재확인

### Phase 1: 핵심 구조 (Day 9)
1. StageManager + GridVisualizer
2. PlatformObject
3. 간단한 PlayMode 테스트

**검증 기준**: 플랫폼이 씬에 표시되고 이동 가능

### Phase 2: 캐릭터 시스템 (Day 10)
1. CombatManager
2. StudentObject + EnemyObject
3. HP 바 표시

**검증 기준**: 학생/적이 씬에 표시되고 HP 바 업데이트

### Phase 3: UI 시스템 (Day 11)
1. TestProgressPanel (필수)
2. CombatLogPanel (권장)
3. CombatStatusPanel (권장)

**검증 기준**: 체크포인트 진행 상황이 UI에 표시됨

### Phase 4: 통합 (Day 12)
1. 전체 PlayMode 통합 테스트
2. 6개 체크포인트 자동 실행
3. 버그 수정

**검증 기준**: PlayMode 테스트 모두 통과

---

## 📝 결론

### 현재 시스템 강점
- ✅ 로직과 데이터 완전 분리
- ✅ 의존성 주입 패턴 적용
- ✅ 포괄적인 EditMode 테스트
- ✅ 확장 가능한 구조

### PlayMode 전환의 이점
- 🎯 실제 게임 환경 검증
- 🎯 시각적 테스트 확인
- 🎯 자동화 실행 파일 가능
- 🎯 사용자 관점 테스트

### 구현 전략
- 기존 로직 클래스 유지 (Pure C#)
- MonoBehaviour 래퍼 추가 (Presentation)
- 이벤트 기반 UI 업데이트
- 최소 비주얼로 빠른 구현

### 예상 결과
- EditMode 테스트: 178개 유지 (로직 검증)
- PlayMode 테스트: 10개 추가 (통합 검증)
- 실행 파일: PlayMode 테스트 자동 실행
- 총 개발 기간: 5일 (Day 9-13)

**다음 단계: Day 9 작업 시작 (GameObject 기반 구조 구현)** 🚀
