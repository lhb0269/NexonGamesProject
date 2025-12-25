# 블루 아카이브 테스트 자동화 프로젝트 코드 가이드

## 목차
1. [프로젝트 개요](#프로젝트-개요)
2. [아키텍처 구조](#아키텍처-구조)
3. [핵심 시스템](#핵심-시스템)
4. [코드 작성 규칙](#코드-작성-규칙)
5. [테스트 작성 가이드](#테스트-작성-가이드)
6. [자주 사용하는 패턴](#자주-사용하는-패턴)

---

## 프로젝트 개요

### 기술 스택
- **Unity 버전**: 6000.2.9f1
- **렌더 파이프라인**: Universal Render Pipeline (URP) 17.2.0
- **입력 시스템**: New Input System 1.14.2
- **테스트 프레임워크**: Unity Test Framework 1.6.0
- **아키텍처 패턴**: 의존성 주입 (Dependency Injection)

### 프로젝트 목표
블루 아카이브 Normal 1-4 스테이지의 전투 시스템을 재현하고, 다음 항목을 자동으로 검증하는 테스트 시스템을 구축한다:
1. 플랫폼 이동 검증 (8방향 인접 이동)
2. 전투 진입 검증
3. EX 스킬 사용 및 코스트 소모 검증
4. 전투별 데미지 추적
5. 보상 획득 검증

---

## 아키텍처 구조

### 의존성 주입 패턴

프로젝트는 **싱글톤을 사용하지 않으며**, 모든 매니저와 시스템이 **의존성 주입**을 통해 관리된다.

#### Service Locator 패턴
```csharp
// 서비스 등록 (GameBootstrapper.cs)
ServiceLocator.Instance.Register<IAudioManager>(audioManager);
ServiceLocator.Instance.Register<ISceneLoader>(sceneLoader);

// 서비스 사용
private IAudioManager _audioManager;

private void Start()
{
    _audioManager = ServiceLocator.Instance.Get<IAudioManager>();
}
```

#### 핵심 원칙
1. **모든 매니저는 인터페이스를 구현한다**
2. **싱글톤 패턴을 사용하지 않는다**
3. **의존성은 생성자 또는 Initialize 메서드로 주입한다**
4. **ServiceLocator를 통해서만 서비스에 접근한다**

### 디렉토리 구조
```
Assets/_Project/
├── Scripts/
│   ├── Core/                    # ServiceLocator, GameBootstrapper
│   ├── Managers/                # 서비스 구현체 (Audio, Input, UI, Scene)
│   ├── BlueArchive/             # 게임 로직
│   │   ├── Stage/               # 스테이지 시스템
│   │   ├── Character/           # 학생 및 적 캐릭터
│   │   ├── Combat/              # 전투 시스템
│   │   ├── Skill/               # 스킬 및 코스트
│   │   ├── Reward/              # 보상 시스템
│   │   ├── Data/                # 스크립터블 오브젝트
│   │   └── UI/                  # 게임 UI
│   └── Tests/
│       ├── EditMode/            # 단위 테스트
│       └── PlayMode/            # 통합 테스트
├── Prefabs/
├── Scenes/
└── Settings/
```

---

## 핵심 시스템

### 1. 스테이지 시스템 (Stage System)

#### 구조
- **StageManager** (MonoBehaviour): GameObject 생성 및 비주얼 관리
- **StageController** (Pure C#): 순수 로직 처리
- **PlatformObject**: 클릭 가능한 플랫폼 (8방향 인접 이동)

#### 주요 기능
```csharp
// 스테이지 초기화
_stageManager.InitializeStage(stageData);

// 플랫폼 클릭을 통한 이동 (8방향 인접 검증)
_stageManager.SimulatePlatformClick(new Vector2Int(1, 1));

// 인접 여부 확인
bool isAdjacent = IsAdjacent(from, to); // 상하좌우 + 대각선
```

#### 상태 관리
```csharp
public enum StageState
{
    NotStarted,      // 시작 전
    InProgress,      // 진행 중
    ReadyForBattle,  // 전투 준비
    InBattle,        // 전투 중
    BattleComplete,  // 전투 완료
    StageCleared     // 스테이지 클리어
}
```

### 2. 전투 시스템 (Combat System)

#### 구조
- **CombatManager** (MonoBehaviour): 전투 오브젝트 및 UI 관리
- **CombatSystem** (Pure C#): 전투 로직 처리
- **CostSystem**: 코스트 회복 및 소모
- **CombatLogSystem**: 전투 이벤트 기록

#### 전투 흐름
```csharp
// 1. 전투 초기화
_combatManager.InitializeCombat(students, enemies, stageName);

// 2. 스킬 사용 (코스트 자동 검증)
SkillExecutionResult result = _combatManager.UseStudentSkill(studentIndex);

// 3. 코스트 회복 (시간 기반 자동)
_combatSystem.Update(deltaTime); // CostSystem이 자동으로 코스트 회복

// 4. 전투 결과 확인
if (_combatManager.CurrentState == CombatState.Victory)
{
    // 승리 처리
}
```

#### 코스트 시스템
```csharp
// 코스트 시스템 초기화
var costSystem = new CostSystem(
    maxCost: 10,        // 최대 코스트
    regenRate: 1f,      // 초당 회복량
    startingCost: 5     // 시작 코스트
);

// 코스트 소모 (자동으로 CombatLog에 기록됨)
bool success = costSystem.TrySpendCost(requiredCost);

// 코스트 통계
int totalSpent = costSystem.TotalCostSpent;
```

### 3. 스킬 시스템 (Skill System)

#### 스킬 실행 흐름
```csharp
// SkillExecutor가 다음을 순서대로 처리:
// 1. 스킬 데이터 확인
// 2. 쿨다운 확인
// 3. 코스트 확인 및 소모
// 4. 스킬 사용 (쿨다운 시작)
// 5. 로그 기록 (스킬 사용 + 코스트 소모)
// 6. 대상 선택 및 데미지 계산
// 7. 격파 확인 및 로그 기록

SkillExecutionResult result = _skillExecutor.ExecuteSkill(student, enemies);
```

#### 스킬 데이터 (ScriptableObject)
```csharp
[CreateAssetMenu(fileName = "NewSkill", menuName = "BlueArchive/Skill")]
public class SkillData : ScriptableObject
{
    public string skillName;
    public SkillTargetType targetType;  // Single, Multiple, Area
    public int baseDamage;
    public float damageMultiplier;
    public float cooldownTime;
    public int costRequired;
}
```

### 4. 캐릭터 시스템 (Character System)

#### Student (학생)
```csharp
public class Student
{
    public StudentData Data { get; private set; }
    public int CurrentHP { get; private set; }
    public bool IsAlive => CurrentHP > 0;

    // 스킬 쿨다운 관리
    public float SkillCooldownRemaining { get; private set; }
    public bool CanUseSkill() => SkillCooldownRemaining <= 0f;

    // 스킬 사용
    public void UseSkill()
    {
        SkillCooldownRemaining = Data.exSkill.cooldownTime;
    }

    // 쿨다운 업데이트
    public void UpdateCooldown(float deltaTime)
    {
        if (SkillCooldownRemaining > 0)
            SkillCooldownRemaining -= deltaTime;
    }
}
```

#### Enemy (적)
```csharp
public class Enemy
{
    public EnemyData Data { get; private set; }
    public int CurrentHP { get; private set; }
    public bool IsAlive => CurrentHP > 0;

    public int Attack() => Data.attackPower;
    public int TakeDamage(int damage)
    {
        int actualDamage = Mathf.Min(damage, CurrentHP);
        CurrentHP -= actualDamage;
        return actualDamage;
    }
}
```

### 5. UI 시스템 (UI System)

#### 주요 UI 컴포넌트
1. **TestProgressPanel**: 5개 체크포인트 진행 상태 표시
2. **CostDisplay**: 코스트 바 표시
3. **SkillButtonPanel**: 학생별 스킬 버튼 (코스트바 위)
4. **CombatLogPanel**: 전투 로그 표시
5. **CombatStatusPanel**: 학생별 데미지 통계 표시
6. **RewardResultPanel**: 보상 결과 표시

#### UI 생성 패턴
```csharp
// 프로그래매틱 UI 생성
private void CreateUI()
{
    var panelObj = new GameObject("Panel");
    var rectTransform = panelObj.AddComponent<RectTransform>();
    var image = panelObj.AddComponent<Image>();

    // URP에서는 _BaseColor 사용
    image.color = new Color(0.2f, 0.2f, 0.3f);
}
```

---

## 코드 작성 규칙

### 네이밍 규칙

#### 클래스 및 메서드
```csharp
// 클래스명: PascalCase
public class StageManager { }

// 메서드명: PascalCase
public void InitializeStage() { }

// 프로퍼티: PascalCase
public int CurrentHP { get; private set; }
```

#### 필드 및 변수
```csharp
// Private 필드: _camelCase (언더스코어 접두사)
private int _currentCost;
private StageManager _stageManager;

// 상수: UPPER_SNAKE_CASE
private const float BUTTON_WIDTH = 80f;

// 지역 변수: camelCase
int totalDamage = 0;
```

#### 인터페이스
```csharp
// 인터페이스: I + PascalCase
public interface IAudioManager { }
public interface ISceneLoader { }
```

### 주석 규칙

#### XML 문서 주석
```csharp
/// <summary>
/// 플레이어를 지정된 위치로 이동시킨다.
/// 인접하지 않은 위치로는 이동할 수 없다.
/// </summary>
/// <param name="targetPosition">목표 그리드 위치</param>
/// <returns>이동 성공 여부</returns>
public bool MovePlayer(Vector2Int targetPosition)
{
    // 구현...
}
```

#### 코드 내 주석
```csharp
// 인접 여부 확인 (8방향: 상하좌우 + 대각선)
bool isAdjacent = (dx <= 1 && dy <= 1) && !(dx == 0 && dy == 0);

// TODO: 추가 검증 로직 필요
// FIXME: 경계 처리 버그 수정 필요
```

### 네임스페이스 구조
```csharp
namespace NexonGame.Core { }           // 핵심 시스템
namespace NexonGame.Managers { }       // 매니저
namespace NexonGame.BlueArchive.Stage { }
namespace NexonGame.BlueArchive.Combat { }
namespace NexonGame.BlueArchive.Character { }
namespace NexonGame.Tests.PlayMode { }
```

### 파일 구조 규칙
```csharp
using UnityEngine;
using System.Collections.Generic;
using NexonGame.BlueArchive.Data;

namespace NexonGame.BlueArchive.Combat
{
    /// <summary>
    /// 클래스 설명
    /// </summary>
    public class CombatManager : MonoBehaviour
    {
        // 1. 상수
        private const float MAX_DISTANCE = 10f;

        // 2. SerializeField
        [Header("References")]
        [SerializeField] private Transform _container;

        // 3. Private 필드
        private CombatSystem _combatSystem;
        private List<Student> _students;

        // 4. 프로퍼티
        public CombatState CurrentState => _combatSystem?.CurrentState ?? CombatState.NotStarted;

        // 5. Unity 라이프사이클
        private void Awake() { }
        private void Start() { }
        private void Update() { }

        // 6. Public 메서드
        public void InitializeCombat() { }

        // 7. Private 메서드
        private void CreateUI() { }
    }
}
```

---

## 테스트 작성 가이드

### AAA 패턴 (Arrange-Act-Assert)

모든 테스트는 AAA 패턴을 엄격하게 따른다.

```csharp
[UnityTest]
public IEnumerator TestName_Condition_ExpectedResult()
{
    Debug.Log("\n[테스트] 테스트 설명");

    // ========================================
    // Arrange: 테스트 환경 준비
    // ========================================
    Debug.Log("  [Arrange] 준비 단계");

    _stageManager.InitializeStage(_testStageData);
    Vector2Int initialPosition = _stageManager.PlayerPosition;
    yield return null;

    // ========================================
    // Act: 테스트 대상 실행
    // ========================================
    Debug.Log("  [Act] 실행 단계");

    _stageManager.SimulatePlatformClick(targetPosition);
    yield return null;

    // ========================================
    // Assert: 결과 검증
    // ========================================
    Debug.Log("  [Assert] 검증 단계");

    Assert.AreEqual(targetPosition, _stageManager.PlayerPosition,
        "이동이 성공해야 함");
    Assert.AreEqual(1, _stageManager.TotalMovesInStage,
        "이동 횟수가 1이어야 함");

    Debug.Log("  ✓ 테스트 통과");
    yield return null;
}
```

### 테스트 네이밍 규칙
```csharp
// 패턴: MethodName_Condition_ExpectedResult

// 좋은 예
[UnityTest]
public IEnumerator MovePlayer_AdjacentPlatform_ShouldSucceed() { }

[UnityTest]
public IEnumerator MovePlayer_NonAdjacentPlatform_ShouldFail() { }

[UnityTest]
public IEnumerator UseSkill_NotEnoughCost_ShouldFail() { }
```

### 통합 테스트 구조
```csharp
public class BlueArchiveIntegrationTests
{
    // Setup
    [UnitySetUp]
    public IEnumerator SetUp()
    {
        // 테스트 환경 초기화
        yield return null;
    }

    // TearDown
    [UnityTearDown]
    public IEnumerator TearDown()
    {
        // 정리 작업
        yield return null;
    }

    // 메인 통합 테스트
    [UnityTest]
    public IEnumerator FullIntegration_AllFiveCheckpoints_ShouldPass()
    {
        yield return CheckpointOne_PlatformMovement();
        yield return CheckpointTwo_BattleEntry();
        yield return CheckpointThree_SkillUsage();
        yield return CheckpointFour_DamageTracking();
        yield return CheckpointFive_RewardVerification();
    }

    // 개별 체크포인트 (Private)
    private IEnumerator CheckpointOne_PlatformMovement() { }
}
```

### 단위 테스트 예제
```csharp
[Test]
public void IsAdjacent_DiagonalPosition_ReturnsTrue()
{
    // Arrange
    var from = new Vector2Int(0, 0);
    var to = new Vector2Int(1, 1);

    // Act
    bool result = IsAdjacent(from, to);

    // Assert
    Assert.IsTrue(result, "대각선 위치는 인접한 것으로 판정되어야 함");
}
```

### Assert 사용 가이드
```csharp
// 기본 Assert
Assert.AreEqual(expected, actual, "실패 메시지");
Assert.AreNotEqual(unexpected, actual);
Assert.IsTrue(condition, "조건이 참이어야 함");
Assert.IsFalse(condition);

// Null 검사
Assert.IsNull(obj);
Assert.IsNotNull(obj, "객체가 null이 아니어야 함");

// 컬렉션 검사
Assert.IsEmpty(collection);
Assert.IsNotEmpty(collection, "컬렉션이 비어있지 않아야 함");

// 숫자 비교
Assert.Greater(actual, expected, "더 커야 함");
Assert.GreaterOrEqual(actual, expected);
Assert.Less(actual, expected);

// 예외 검사
Assert.Throws<ArgumentException>(() =>
{
    // 예외를 발생시키는 코드
});
```

---

## 자주 사용하는 패턴

### 1. MonoBehaviour + Pure C# 분리 패턴

Unity 오브젝트 관리와 순수 로직을 분리한다.

```csharp
// StageManager.cs (MonoBehaviour - GameObject 관리)
public class StageManager : MonoBehaviour
{
    private StageController _stageController; // Pure C# 로직
    private List<PlatformObject> _platforms;  // Unity 오브젝트

    public void InitializeStage(StageData stageData)
    {
        // 로직 초기화
        _stageController = new StageController();
        _stageController.InitializeStage(stageData);

        // 비주얼 생성
        CreatePlatforms();
        CreatePlayerMarker();
    }

    public bool MovePlayer(Vector2Int targetPosition)
    {
        // 로직 처리는 Controller에 위임
        bool success = _stageController.MovePlayer(targetPosition);

        // 비주얼 업데이트
        if (success)
        {
            UpdatePlayerMarkerPosition();
        }

        return success;
    }
}

// StageController.cs (Pure C# - 로직만)
public class StageController
{
    private StageData _stageData;
    private Vector2Int _playerPosition;
    private StageState _currentState;

    public bool MovePlayer(Vector2Int targetPosition)
    {
        // 순수 로직만 처리
        if (!IsValidMove(targetPosition))
            return false;

        _playerPosition = targetPosition;
        UpdateState();
        return true;
    }
}
```

### 2. 이벤트 기반 통신 패턴

```csharp
// 이벤트 정의
public class CostSystem
{
    public event Action<int> OnCostChanged;
    public event Action<int, int> OnCostSpent; // 소모량, 남은 코스트

    public void AddCost(int amount)
    {
        CurrentCost += amount;
        OnCostChanged?.Invoke(CurrentCost);
    }
}

// 이벤트 구독
public class CostDisplay : MonoBehaviour
{
    private CostSystem _costSystem;

    private void Start()
    {
        _costSystem.OnCostChanged += UpdateDisplay;
        _costSystem.OnCostSpent += ShowCostSpentEffect;
    }

    private void OnDestroy()
    {
        _costSystem.OnCostChanged -= UpdateDisplay;
        _costSystem.OnCostSpent -= ShowCostSpentEffect;
    }

    private void UpdateDisplay(int currentCost)
    {
        _costText.text = $"{currentCost}/{_costSystem.MaxCost}";
    }
}
```

### 3. Result 객체 패턴

작업 결과를 명확하게 전달한다.

```csharp
public class SkillExecutionResult
{
    public bool Success { get; set; }
    public string FailureReason { get; set; }
    public int TotalDamage { get; set; }
    public List<string> TargetsHit { get; set; }
    public int CostSpent { get; set; }
}

// 사용
public SkillExecutionResult UseSkill(Student student)
{
    var result = new SkillExecutionResult();

    if (!CanUseSkill(student))
    {
        result.Success = false;
        result.FailureReason = "스킬 쿨다운 중";
        return result;
    }

    // 스킬 실행...
    result.Success = true;
    result.TotalDamage = totalDamage;
    return result;
}

// 호출부
var result = UseSkill(student);
if (!result.Success)
{
    Debug.LogWarning($"스킬 사용 실패: {result.FailureReason}");
    return;
}
```

### 4. 통계 수집 패턴

```csharp
public class CombatLogSystem
{
    // 통계 프로퍼티
    public int TotalDamageDealt { get; private set; }
    public int TotalSkillsUsed { get; private set; }
    public int TotalCostSpent { get; private set; }

    // 학생별 통계
    private Dictionary<string, int> _studentDamageStats = new Dictionary<string, int>();
    public IReadOnlyDictionary<string, int> StudentDamageStats => _studentDamageStats;

    // 로그 기록 + 통계 업데이트
    public void LogDamageDealt(string actorName, string targetName, int damage)
    {
        TotalDamageDealt += damage;

        if (!_studentDamageStats.ContainsKey(actorName))
            _studentDamageStats[actorName] = 0;

        _studentDamageStats[actorName] += damage;

        AddLog(CombatLogType.DamageDealt, actorName,
            $"{actorName} → {targetName}: {damage} 데미지");
    }
}
```

### 5. 시뮬레이션 메서드 패턴 (테스트용)

테스트를 위한 프로그래매틱 인터페이스를 제공한다.

```csharp
// 프로덕션 코드
public class PlatformObject : MonoBehaviour
{
    // 실제 마우스 클릭
    private void OnMouseDown()
    {
        _onPlatformClicked?.Invoke(_gridPosition);
    }

    // 테스트용 시뮬레이션
    public void SimulateClick()
    {
        OnMouseDown();
    }
}

// 테스트 코드
[UnityTest]
public IEnumerator Test_PlatformClick()
{
    // Arrange
    _stageManager.InitializeStage(_testStageData);
    yield return null;

    // Act: 시뮬레이션 메서드 사용
    _stageManager.SimulatePlatformClick(new Vector2Int(1, 1));
    yield return null;

    // Assert
    Assert.AreEqual(new Vector2Int(1, 1), _stageManager.PlayerPosition);
}
```

### 6. ScriptableObject 데이터 패턴

```csharp
// 데이터 정의
[CreateAssetMenu(fileName = "NewStudent", menuName = "BlueArchive/Student")]
public class StudentData : ScriptableObject
{
    [Header("기본 정보")]
    public string studentName;
    public int maxHP;
    public int attackPower;

    [Header("스킬")]
    public SkillData exSkill;
    public int skillCost;

    [Header("비주얼")]
    public Sprite characterSprite;
}

// 사용
public class Student
{
    public StudentData Data { get; private set; }

    public void Initialize(StudentData data)
    {
        Data = data;
        CurrentHP = data.maxHP;
    }

    public int GetSkillCost() => Data.skillCost;
}
```

---

## 체크리스트

### 새로운 기능 추가 시
- [ ] 인터페이스 정의 (필요시)
- [ ] Pure C# 로직 클래스 작성
- [ ] MonoBehaviour 래퍼 작성
- [ ] XML 문서 주석 작성
- [ ] 단위 테스트 작성 (EditMode)
- [ ] 통합 테스트 작성 (PlayMode)
- [ ] AAA 패턴 준수 확인
- [ ] 네이밍 규칙 준수 확인
- [ ] 의존성 주입 확인 (싱글톤 사용 금지)

### 코드 리뷰 시
- [ ] 싱글톤 패턴 사용 여부
- [ ] 인터페이스를 통한 의존성 주입 확인
- [ ] XML 문서 주석 존재 여부
- [ ] 테스트 코드 AAA 패턴 준수
- [ ] 네이밍 규칙 준수
- [ ] MonoBehaviour와 Pure C# 분리 확인
- [ ] 매직 넘버 없이 상수 사용 확인
- [ ] 이벤트 구독 해제 확인 (OnDestroy)

### 테스트 작성 시
- [ ] AAA 패턴 (Arrange-Act-Assert) 준수
- [ ] 명확한 섹션 구분 주석
- [ ] 테스트 이름이 명확함 (MethodName_Condition_ExpectedResult)
- [ ] Assert 실패 메시지 포함
- [ ] Setup/TearDown에서 정리 작업 수행
- [ ] 테스트 독립성 보장 (다른 테스트에 영향 없음)

---

## 참고 자료

### 프로젝트 문서
- `CLAUDE.md`: 프로젝트 개요 및 아키텍처 설명
- `README.md`: 프로젝트 소개 및 실행 방법
- `CHANGELOG.md`: 버전별 변경 이력
- `CONTRIBUTING.md`: 기여 가이드라인

### Unity 공식 문서
- [URP 문서](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@latest)
- [New Input System](https://docs.unity3d.com/Packages/com.unity.inputsystem@latest)
- [Unity Test Framework](https://docs.unity3d.com/Packages/com.unity.test-framework@latest)

### 테스트 패턴
- AAA Pattern (Arrange-Act-Assert)
- Given-When-Then Pattern (BDD)
- Test Fixture Pattern

---

## 변경 이력

### 2025-12-25
- 초기 코드 가이드 작성
- AAA 패턴 가이드 추가
- 플랫폼 클릭 시스템 패턴 추가
