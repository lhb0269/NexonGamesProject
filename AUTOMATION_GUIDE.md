# 블루 아카이브 테스트 자동화 사용 가이드

## 목차
1. [개요](#개요)
2. [실행 환경](#실행-환경)
3. [테스트 실행 방법](#테스트-실행-방법)
4. [체크포인트 상세 설명](#체크포인트-상세-설명)
5. [테스트 결과 해석](#테스트-결과-해석)
6. [문제 해결](#문제-해결)

---

## 개요

### 테스트 목적
이 자동화 테스트는 블루 아카이브 Normal 1-4 스테이지의 핵심 게임 메커니즘이 정상적으로 작동하는지 검증합니다. 5개의 체크포인트를 통해 플랫폼 이동부터 보상 획득까지 전체 게임 플로우를 자동으로 테스트합니다.

### 테스트 범위
- **스테이지**: Normal 1-4 (블루 아카이브 초반 스테이지)
- **학생**: 4명의 캐릭터 (각각 고유한 스킬과 능력치)
- **적**: 3명의 일반 몬스터
- **검증 항목**: 5개 체크포인트 (이동, 전투, 스킬, 데미지, 보상)

---

## 테스트 실행 방법

### 1. Unity Editor에서 실행

#### 방법: Test Runner 창 사용
1. Unity Editor 상단 메뉴: `Window` → `General` → `Test Runner`
2. `PlayMode` 탭 선택
3. `BlueArchiveIntegrationTests` 확장
4. `FullIntegration_AllFiveCheckpoints_ShouldPass` 우클릭
5. `Run Selected` 클릭

### . 테스트 실행 시간
- **전체 통합 테스트**: 약 30-40초
- **개별 체크포인트**: 각 5-10초
- **단위 테스트**: 각 1초 미만

---

## 체크포인트 상세 설명

### 체크포인트 #1: 플랫폼 이동 검증

#### 테스트 의도
플레이어가 그리드 기반 맵에서 인접한 플랫폼으로만 이동할 수 있는지 검증합니다. 블루 아카이브의 전략적 이동 시스템을 재현하며, 8방향(상하좌우 + 대각선) 인접 검증이 핵심입니다.

#### 테스트 시나리오
1. **시작 위치**: (0, 0) - 녹색 플랫폼
2. **이동 경로**:
   - (0,0) → (1,1) - 대각선 이동 (NE)
   - (1,1) → (0,2) - 대각선 이동 (NW)
   - (0,2) → (1,1) - 대각선 이동 (SE) - 되돌아오기
   - (1,1) → (2,1) - 우측 이동 (E)
   - (2,1) → (3,1) - 우측 이동 (E) - 전투 위치 도착
3. **총 이동 횟수**: 5회

#### 검증 항목
```
✓ 플랫폼 생성 개수 확인 (일반 4개 + 전투 1개 = 5개)
✓ 모든 이동이 성공적으로 완료됨
✓ 최종 위치가 전투 위치 (3, 1)임
✓ 스테이지 상태가 ReadyForBattle로 변경됨
✓ 총 이동 횟수가 5회로 기록됨
```

#### 인접 검증 로직
```csharp
bool IsAdjacent(Vector2Int from, Vector2Int to)
{
    int dx = Mathf.Abs(to.x - from.x);
    int dy = Mathf.Abs(to.y - from.y);

    // 8방향 인접: dx와 dy가 모두 0 또는 1이어야 하며, 동일 위치는 제외
    return (dx <= 1 && dy <= 1) && !(dx == 0 && dy == 0);
}
```

#### 실패 조건
- 인접하지 않은 플랫폼으로 이동 시도
- 존재하지 않는 플랫폼으로 이동 시도
- 이동 횟수가 예상과 다름
- 최종 위치가 전투 위치가 아님

---

### 체크포인트 #2: 전투 진입 검증

#### 테스트 의도
스테이지에서 전투로 전환되는 과정이 정상적으로 작동하는지 검증합니다. 전투 매니저 초기화, 캐릭터 오브젝트 생성, UI 패널 생성, 코스트 시스템 초기화 등 전투 준비 상태를 종합적으로 검증합니다.

#### 테스트 시나리오
1. **전제 조건**: 체크포인트 #1 완료 (전투 위치 도착)
2. **전투 시작**: `StageManager.StartBattle()` 호출
3. **전투 초기화**: `CombatManager.InitializeCombat()` 호출
4. **오브젝트 생성**: 학생 4명, 적 3명, UI 패널 4개

#### 검증 항목
```
✓ 스테이지 상태가 InBattle로 변경됨
✓ 전투 매니저 상태가 InProgress로 변경됨
✓ 학생 오브젝트 4개 생성됨
✓ 적 오브젝트 3개 생성됨
✓ CostDisplay 패널 생성됨
✓ CombatLogPanel 패널 생성됨
✓ CombatStatusPanel 패널 생성됨
✓ SkillButtonPanel 패널 생성됨
✓ 코스트 시스템 초기화됨 (최대 10, 시작 5)
```

#### 전투 초기화 과정
```
1. StageController: InProgress → InBattle 상태 전환
2. CombatSystem: 학생 및 적 데이터 등록
3. CombatManager:
   - StudentObject 생성 (3D 비주얼)
   - EnemyObject 생성 (3D 비주얼)
   - CostDisplay 생성 (코스트 바)
   - SkillButtonPanel 생성 (학생별 스킬 버튼)
   - CombatLogPanel 생성 (전투 로그)
   - CombatStatusPanel 생성 (학생별 데미지 통계)
4. CostSystem: 자동 코스트 회복 시작 (초당 1 코스트)
```

#### 실패 조건
- 스테이지 상태가 InBattle로 변경되지 않음
- 전투 매니저가 초기화되지 않음
- 캐릭터 오브젝트 생성 개수가 다름
- UI 패널이 생성되지 않음
- 코스트 시스템이 초기화되지 않음

---

### 체크포인트 #3: EX 스킬 사용 및 코스트 소모 검증

#### 테스트 의도
블루 아카이브의 핵심 전투 메커니즘인 EX 스킬 시스템이 정상적으로 작동하는지 검증합니다. 스킬 버튼 클릭, 코스트 소모, 쿨타임 적용, 데미지 발생, 로그 기록 등 스킬 사용의 전체 프로세스를 검증합니다.

#### 테스트 시나리오
1. **코스트 충전 대기**: 2초 대기 (코스트 자동 회복)
2. **스킬 버튼 클릭**: 학생 4명의 스킬 순차 사용
   - 학생마다 필요 코스트 확인
   - 코스트 충분할 때까지 대기
   - 스킬 버튼 클릭 시뮬레이션
   - 적 전멸 시 조기 종료
3. **결과 기록**: 스킬 사용 횟수, 데미지, 코스트 소모량

#### 검증 항목
```
✓ 스킬이 최소 1회 이상 사용됨
✓ 데미지가 발생함 (TotalDamageDealt > 0)
✓ 코스트가 소모됨 (TotalCostSpent > 0)
✓ 스킬 사용 로그가 기록됨
✓ 코스트 소모 로그가 기록됨
```

#### 스킬 실행 프로세스
```
1. 코스트 확인:
   - 필요 코스트 < 현재 코스트 → 대기
   - 필요 코스트 ≤ 현재 코스트 → 진행

2. 스킬 사용:
   - 코스트 소모 (CostSystem.TrySpendCost)
   - 쿨타임 시작 (Student.UseSkill)
   - 로그 기록 (CombatLog.LogSkillUsed)
   - 로그 기록 (CombatLog.LogCostSpent) ← 중요!

3. 데미지 계산:
   - 대상 선택 (Single/Multiple/Area)
   - 데미지 계산 (baseDamage * damageMultiplier)
   - 데미지 적용 (Enemy.TakeDamage)
   - 로그 기록 (CombatLog.LogDamageDealt)

4. 격파 확인:
   - 적 HP ≤ 0 → 격파
   - 로그 기록 (CombatLog.LogUnitDefeated)
```

#### 학생별 스킬 데이터
| 학생 이름 | 스킬 이름 | 필요 코스트 | 데미지 | 쿨타임 | 대상 |
|---------|---------|-----------|-------|-------|------|
| 아리스 | EX: 정의의 일격 | 4 | 500 | 20초 | Single |
| 호시노 | EX: 수호의 맹세 | 5 | 300 | 25초 | Multiple |
| 이로하 | EX: 신속한 사격 | 3 | 400 | 15초 | Single |
| 시로코 | EX: 전술 지원 | 4 | 350 | 20초 | Area |

#### 실패 조건
- 스킬이 전혀 사용되지 않음
- 데미지가 발생하지 않음
- 코스트가 소모되지 않음 (TotalCostSpent = 0)
- 코스트 부족 시 스킬이 사용됨
- 쿨타임 중 스킬이 재사용됨

---

### 체크포인트 #4: 전투별 데미지 추적

#### 테스트 의도
전투 로그 시스템이 모든 데미지를 정확하게 기록하고, 학생별 데미지 통계를 올바르게 집계하는지 검증합니다. 이는 전투 결과 분석과 보상 계산의 기반이 됩니다.

#### 테스트 시나리오
1. **현재 상태 확인**: 체크포인트 #3의 누적 데미지 확인
2. **추가 공격**: 살아있는 적이 있으면 추가 스킬 사용
3. **통계 검증**: 총 데미지 및 학생별 데미지 통계 확인

#### 검증 항목
```
✓ 총 데미지가 증가 또는 유지됨 (감소하지 않음)
✓ 학생별 데미지 통계가 존재함 (StudentDamageStats.Count > 0)
✓ 각 학생의 데미지가 올바르게 집계됨
✓ 격파한 적 수가 기록됨
✓ 스킬 사용 횟수가 기록됨
```

#### 데미지 추적 시스템
```csharp
// CombatLogSystem의 학생별 데미지 통계
private Dictionary<string, int> _studentDamageStats = new Dictionary<string, int>();

// 데미지 발생 시 자동으로 집계
public void LogDamageDealt(string actorName, string targetName, int damage)
{
    TotalDamageDealt += damage;

    if (!_studentDamageStats.ContainsKey(actorName))
        _studentDamageStats[actorName] = 0;

    _studentDamageStats[actorName] += damage;
}
```

#### 통계 집계 방식
1. **실시간 집계**: 데미지 발생 시마다 즉시 기록
2. **학생별 분류**: Dictionary<학생이름, 총데미지>
3. **읽기 전용 접근**: IReadOnlyDictionary로 외부 제공
4. **전투 종료 시 보존**: 전투 완료 후에도 통계 유지

#### 실패 조건
- 총 데미지가 감소함
- 학생별 통계가 비어있음
- 학생별 데미지 합계가 총 데미지와 다름
- 격파한 적 수가 잘못 기록됨

---

### 체크포인트 #5: 보상 획득 검증

#### 테스트 의도
스테이지 클리어 후 보상 시스템이 정상적으로 작동하는지 검증합니다. 전투 통계를 기반으로 보상을 계산하고, 보상 결과 UI를 표시하는 전체 프로세스를 검증합니다.

#### 테스트 시나리오
1. **전투 통계 수집**: 총 이동 횟수, 스킬 사용, 총 데미지, 격파한 적
2. **전투 완료**: `StageManager.CompleteBattle(victory: true)` 호출
3. **보상 계산**: `RewardSystem.CalculateRewards()` 호출
4. **스테이지 클리어**: `StageManager.ClearStage()` 호출
5. **보상 UI 표시**: `RewardResultPanel.ShowRewards()` 호출

#### 검증 항목
```
✓ 스테이지 상태가 StageCleared로 변경됨
✓ 보상 결과 객체가 null이 아님
✓ 보상 항목이 최소 1개 이상 존재함
✓ 각 보상의 수량이 0보다 큼
✓ RewardResultPanel이 생성됨
```

#### 보상 계산 로직
```csharp
public class RewardSystem
{
    public RewardResult CalculateRewards(string stageName, int totalMoves, CombatLogSystem combatLog)
    {
        var result = new RewardResult();

        // 기본 보상
        result.AddReward("크레딧", 1000);
        result.AddReward("경험치", 500);

        // 스킬 사용 보너스
        if (combatLog.TotalSkillsUsed >= 3)
            result.AddReward("스킬 북", 1);

        // 완벽한 클리어 보너스 (이동 최소화)
        if (totalMoves <= 5)
            result.AddReward("완벽 클리어 보너스", 200);

        // 격파 보상
        result.AddReward("전리품", combatLog.TotalEnemiesDefeated * 50);

        return result;
    }
}
```

#### 보상 종류
| 보상 이름 | 획득 조건 | 수량 |
|---------|---------|------|
| 크레딧 | 기본 보상 | 1000 |
| 경험치 | 기본 보상 | 500 |
| 스킬 북 | 스킬 3회 이상 사용 | 1 |
| 완벽 클리어 보너스 | 이동 5회 이하 | 200 |
| 전리품 | 적 격파당 | 격파 수 × 50 |

#### 보상 UI 구성
```
┌─────────────────────────────────────┐
│  스테이지 클리어!                      │
│  Normal 1-4                          │
├─────────────────────────────────────┤
│  획득한 보상:                          │
│  • 크레딧 x1000                       │
│  • 경험치 x500                        │
│  • 스킬 북 x1                         │
│  • 완벽 클리어 보너스 x200              │
│  • 전리품 x150                        │
├─────────────────────────────────────┤
│  전투 통계:                            │
│  • 총 이동 횟수: 5회                   │
│  • 스킬 사용: 4회                      │
│  • 총 데미지: 1750                     │
│  • 격파한 적: 3명                      │
└─────────────────────────────────────┘
```

#### 실패 조건
- 스테이지 상태가 StageCleared가 아님
- 보상 결과가 null임
- 보상 항목이 비어있음
- 보상 수량이 0 이하임
- RewardResultPanel이 생성되지 않음

---

## 테스트 결과 해석

### 성공 조건
모든 체크포인트가 통과하면 다음 메시지가 표시됩니다:
```
=====================================
✅ 모든 체크포인트 통과!
=====================================
```

### 실패 시 로그 분석

#### Assert 실패
```
AssertionException: 코스트가 소모되었어야 함
  Expected: greater than 0
  But was:  0

at CheckpointThree_SkillUsage() line 552
```
**해석**: 코스트 소모가 기록되지 않음 → `CombatLog.LogCostSpent()` 호출 확인 필요

#### NullReferenceException
```
NullReferenceException: Object reference not set to an instance of an object
at CombatManager.UseStudentSkill() line 296
```
**해석**: 전투 초기화가 제대로 되지 않음 → `InitializeCombat()` 호출 확인 필요

#### 타임아웃
```
UnityTest execution timed out after 300 seconds
```
**해석**: 무한 대기 상태 → 코스트 회복이 작동하지 않거나 적이 격파되지 않음

### 테스트 진행 상황 UI

테스트 실행 중 Unity Game View에 다음과 같은 진행 상황 패널이 표시됩니다:

```
┌─────────────────────────────────────┐
│  블루 아카이브 자동화 테스트            │
├─────────────────────────────────────┤
│  ⏳ 체크포인트 #1: 플랫폼 이동 검증     │
│  ⏳ 체크포인트 #2: 전투 진입 검증       │
│  ⏳ 체크포인트 #3: EX 스킬 사용        │
│  ⏳ 체크포인트 #4: 전투별 데미지 추적   │
│  ⏳ 체크포인트 #5: 보상 획득 검증       │
├─────────────────────────────────────┤
│  [████░░░░░░] 2 / 5                 │
├─────────────────────────────────────┤
│  현재: 전투 진입 테스트 중...           │
└─────────────────────────────────────┘
```

---

## 빌드 및 배포

### 빌드 실행 파일 생성

#### Unity 에디터에서 빌드
1. **Build Settings 열기**: `File` → `Build Settings`
2. **씬 추가 확인**: `Scenes/SampleScene`이 리스트에 있고 체크되어 있는지 확인
3. **플랫폼 선택**: Windows, macOS, Linux 중 선택
4. **Development Build 활성화** (권장):
   - `Development Build` 체크
   - `Script Debugging` 체크
   - 로그 출력 및 디버깅 가능
5. **Build 클릭**: 실행 파일 저장 위치 선택

#### 빌드 결과물
```
빌드폴더/
├── NexonGamesProject.exe        # 실행 파일
├── NexonGamesProject_Data/      # 게임 데이터
├── MonoBleedingEdge/            # .NET 런타임
└── UnityPlayer.dll              # Unity 엔진
```

### 실행 파일 사용법

#### 자동 실행 모드
1. `NexonGamesProject.exe` 더블 클릭
2. 자동으로 테스트 시작
3. TestProgressPanel UI로 진행 상황 확인
4. 약 30-40초 후 자동 완료

#### 로그 확인
- **위치**: `%USERPROFILE%\AppData\LocalLow\DefaultCompany\NexonGamesProject\Player.log`
- **내용**: 모든 체크포인트 결과 및 디버그 로그

---

## 문제 해결

### 빌드 관련 문제

#### 문제 1: 빌드 실행 시 UI가 표시되지 않음

**증상**:
- 빌드 실행 시 회색 화면만 보임
- TestProgressPanel이 나타나지 않음
- Unity Editor에서는 정상 작동

**원인**:
- `NexonGame.Tests.PlayMode` 어셈블리가 `UNITY_INCLUDE_TESTS` 제약으로 빌드에서 제외됨
- TestVisualizationRunner가 테스트 전용 어셈블리에 위치

**해결 방법**:
1. **별도 어셈블리 생성**: `NexonGame.Tests.Automation` 어셈블리 신규 생성
2. **파일 이동**:
   - `TestVisualizationRunner.cs` → `Assets/_Project/Scripts/Tests/Automation/`
   - `TestBootstrap.cs` → `Assets/_Project/Scripts/Tests/Automation/`
3. **네임스페이스 변경**: `NexonGame.Tests.Automation` 사용
4. **씬 업데이트**: TestRunner GameObject의 TestBootstrap 컴포넌트를 Automation 폴더 것으로 교체

**확인 방법**:
```bash
# 어셈블리 정의 확인
cat Assets/_Project/Scripts/Tests/Automation/NexonGame.Tests.Automation.asmdef

# defineConstraints가 비어있어야 함
"defineConstraints": []
```

#### 문제 2: Input System 관련 오류

**증상**:
```
InvalidOperationException: You are trying to read Input using the UnityEngine.Input class,
but you have switched active Input handling to Input System package in Player Settings.
```

**원인**:
- EventSystem에 `StandaloneInputModule` 사용 (Legacy Input System)
- 프로젝트는 New Input System 사용

**해결 방법**:
```csharp
// 수정 전
eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

// 수정 후
using UnityEngine.InputSystem.UI;
eventSystemObj.AddComponent<InputSystemUIInputModule>();
```

**어셈블리 참조 추가**:
```json
// NexonGame.Tests.Automation.asmdef
{
  "references": [
    "NexonGame.Runtime",
    "Unity.InputSystem"  // 추가
  ]
}
```

#### 문제 3: 직렬화 레이아웃 불일치 오류

**증상**:
```
A scripted object has a different serialization layout when loading.
(Read 32 bytes but expected 64 bytes)
```

**원인**:
- 씬 파일에 저장된 TestVisualizationRunner의 직렬화 데이터가 코드와 불일치
- Unity가 씬 파일의 직렬화 데이터를 캐싱

**해결 방법 - TestBootstrap 패턴 사용**:
```csharp
// TestBootstrap.cs - 직렬화 필드가 없는 부트스트랩
public class TestBootstrap : MonoBehaviour
{
    private void Awake()
    {
        // 런타임에 동적으로 TestVisualizationRunner 생성
        var testRunnerObj = new GameObject("TestVisualizationRunner");
        var testRunner = testRunnerObj.AddComponent<TestVisualizationRunner>();

        // Reflection으로 _autoStart 설정
        var autoStartField = typeof(TestVisualizationRunner).GetField("_autoStart",
            BindingFlags.NonPublic | BindingFlags.Instance);
        autoStartField?.SetValue(testRunner, true);

        // 부트스트랩 제거
        Destroy(gameObject);
    }
}
```

**장점**:
- 씬에는 TestBootstrap만 저장 (직렬화 필드 없음)
- TestVisualizationRunner는 런타임에 생성 (직렬화 불일치 회피)
- 빌드 환경과 에디터 환경 모두 동일하게 작동

#### 문제 4: NUnit 참조 오류 (빌드 시)

**증상**:
```
error CS0246: The type or namespace name 'UnitySetUpAttribute' could not be found
```

**원인**:
- `defineConstraints`를 제거하면서 NUnit 테스트 파일들도 빌드에 포함됨
- 빌드에는 NUnit 프레임워크가 포함되지 않음

**해결 방법**:
1. **어셈블리 분리**:
   - PlayMode: NUnit 테스트 코드 (`UNITY_INCLUDE_TESTS` 제약 유지)
   - Automation: 빌드 가능한 자동화 코드 (제약 없음)

2. **어셈블리 구조**:
```
Assets/_Project/Scripts/Tests/
├── PlayMode/                          # NUnit 테스트 전용
│   ├── NexonGame.Tests.PlayMode.asmdef
│   │   └── defineConstraints: ["UNITY_INCLUDE_TESTS"]
│   ├── StagePlayModeTests.cs
│   ├── CombatPlayModeTests.cs
│   └── BlueArchiveIntegrationTests.cs
│
└── Automation/                        # 빌드 포함 자동화
    ├── NexonGame.Tests.Automation.asmdef
    │   └── defineConstraints: []
    ├── TestVisualizationRunner.cs
    └── TestBootstrap.cs
```

### 실행 관련 문제

#### 문제: Development Build 콘솔이 표시되지 않음

**해결 방법**:
- Development Build 옵션은 콘솔 창을 자동으로 표시하지 않음
- 로그 파일 확인: `Player.log` 참조

#### 문제: 테스트가 멈춤 (무한 대기)

**원인**:
- 코스트 회복 시스템이 작동하지 않음
- 적이 격파되지 않음

**해결 방법**:
1. Player.log 확인
2. CombatManager의 Update() 메서드 확인
3. CostSystem.Update() 호출 확인

---
