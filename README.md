# 블루 아카이브 테스트 자동화 검증 과정

## Unity Test Framework의 제약과 해결 방법

### Unity Test Framework의 제약
- **Unity Test Runner는 에디터 환경에서만 실행됨**
  - 빌드된 실행 파일(.exe)에는 테스트 코드가 포함되지 않음
  - `[UnityTest]` 어트리뷰트가 있는 코드는 빌드에서 제외됨

### 과제 요구사항 충족
- **"자동화 실행 파일"** = 빌드된 .exe 파일
- 테스트 검증 과정을 **"시각적으로 보여주는"** 실행 파일 필요

### 해결 방법
1. **TestVisualizationRunner.cs 생성** (빌드에 포함됨)
   - 테스트 로직을 일반 MonoBehaviour로 재구현
   - `[UnityTest]` 어트리뷰트 없이 작성하여 빌드에 포함
2. **체크포인트 진행 상황을 UI로 시각화**
   - TestProgressPanel로 실시간 진행 상황 표시
   - 각 체크포인트별 통과/실패 상태 시각화
3. **두 가지 테스트 스크립트 병행 사용**
   - `BlueArchiveIntegrationTests.cs`: 개발 중 NUnit Assert 검증용 (에디터 전용)
   - `TestVisualizationRunner.cs`: 실행 파일 데모용 (빌드 포함)

---

## 목차
1. [전체 테스트 흐름](#전체-테스트-흐름)
2. [체크포인트별 검증 과정](#체크포인트별-검증-과정)
   - [체크포인트 #1: 발판 이동 정상 여부](#체크포인트-1-발판-이동-정상-여부)
   - [체크포인트 #2: 전투 정상 진입 여부](#체크포인트-2-전투-정상-진입-여부)
   - [체크포인트 #3: 각 학생별 EX 스킬 사용 여부](#체크포인트-3-각-학생별-ex-스킬-사용-여부)
   - [체크포인트 #4: 코스트 소모량 및 데미지 기록](#체크포인트-4-코스트-소모량-및-데미지-기록)
   - [체크포인트 #5: 보상 획득 검증](#체크포인트-5-보상-획득-검증)

---

## 전체 테스트 흐름

```
[Setup] 초기화
    ↓
[#1] 발판 이동
    ↓
[#2] 전투 진입
    ↓
[#3] EX 스킬 사용
    ↓
[#4] 코스트/데미지 기록
    ↓
[#5] 보상 획득 (인벤토리 추가, 검증)
    ↓
[Result] 최종 결과 표시
```

---

## 체크포인트별 검증 과정

### 체크포인트 #1: 발판 이동 정상 여부

**검증 대상**: 그리드 기반 이동 시스템

#### 검증 과정
1. StageManager 초기화 및 스테이지 로드
2. 시작 위치(0, 0)에서 전투 위치(3, 1)까지 이동 시뮬레이션
3. 각 이동마다 `StageManager.MoveToPosition()` 호출
4. 이동 경로: (0,0) → (1,1) → (0,2) → (1,1) → (2,1) → (3,1)

#### 검증 항목
- ✅ `_stageManager.CurrentPosition == battlePosition` (최종 위치 도달)
- ✅ `_stageManager.TotalMovesInStage > 0` (이동 횟수 기록)
- ✅ 이동 횟수 로그 출력

#### 성공 조건
전투 위치에 정확히 도달하고 이동 횟수가 기록됨

#### 실패 조건
- 인접하지 않은 플랫폼으로 이동 시도
- 존재하지 않는 플랫폼으로 이동 시도
- 이동 횟수가 예상과 다름
- 최종 위치가 전투 위치가 아님
```csharp
// ========================================
// Assert: 이동 결과 검증
// ========================================

// 성공한 이동 횟수 확인
Assert.AreEqual(movementPath.Count, successfulMoves,
    $"모든 이동이 성공해야 함 ({movementPath.Count}회)");

// 최종 위치가 전투 위치인지 확인
Assert.AreEqual(_testStageData.battlePosition, _stageManager.PlayerPosition,
    "전투 위치에 도착해야 함");

// 스테이지 상태가 전투 준비 상태인지 확인
Assert.AreEqual(StageState.ReadyForBattle, _stageManager.CurrentState,
    "전투 준비 상태여야 함");

// 총 이동 횟수 확인
Assert.AreEqual(movementPath.Count, _stageManager.TotalMovesInStage,
    "총 이동 횟수가 일치해야 함");
```
---

### 체크포인트 #2: 전투 정상 진입 여부

**검증 대상**: 스테이지 상태 전환 시스템

#### 검증 과정
1. CombatManager 초기화
2. `_combatManager.SetupCombat()` 호출로 전투 준비
3. `_stageManager.EnterBattle()` 호출로 전투 진입
4. 학생 4명, 적 3명 배치

#### 검증 항목
- ✅ `_stageManager.CurrentState == StageState.InBattle` (전투 상태 전환)
- ✅ `_combatManager.CombatSystem.CurrentState == CombatState.InProgress` (전투 시작)
- ✅ 학생 4명 활성화
- ✅ 적 3명 배치

#### 전투 초기화 과정
```
1. StageController: InProgress → InBattle 상태 전환
2. CombatSystem: 학생 및 적 데이터 등록
3. CombatManager: 학생/적 오브젝트 생성 (3D 비주얼)
4. UI 패널 생성 (CostDisplay, SkillButtonPanel, CombatLogPanel, CombatStatusPanel)
5. CostSystem: 자동 코스트 회복 시작 (초당 1 코스트)
```

#### 성공 조건
스테이지와 전투 시스템이 모두 전투 상태로 전환됨

#### 실패 조건
- 스테이지 상태가 InBattle로 변경되지 않음
- 전투 매니저가 초기화되지 않음
- 캐릭터 오브젝트 생성 개수가 다름
- UI 패널이 생성되지 않음

### 전투 진입 및 오브젝트 생성 검증
```csharp
// ========================================
// Assert: 전투 진입 및 오브젝트 생성 검증
// ========================================

// 스테이지 상태가 전투 중으로 변경되었는지 확인
Assert.AreEqual(StageState.InBattle, _stageManager.CurrentState,
    "스테이지 상태가 InBattle이어야 함");

// 전투 매니저 상태가 진행 중인지 확인
Assert.AreEqual(CombatState.InProgress, _combatManager.CurrentState,
    "전투 매니저 상태가 InProgress여야 함");

// 학생 오브젝트 생성 검증
var studentObjects = Object.FindObjectsByType<StudentObject>(FindObjectsSortMode.None);
Assert.AreEqual(_testStudents.Count, studentObjects.Length,
    $"학생 오브젝트 {_testStudents.Count}명 생성되어야 함");

// 적 오브젝트 생성 검증
var enemyObjects = Object.FindObjectsByType<EnemyObject>(FindObjectsSortMode.None);
Assert.AreEqual(_testEnemies.Count, enemyObjects.Length,
    $"적 오브젝트 {_testEnemies.Count}명 생성되어야 함");

// UI 패널 생성 검증
var costDisplay = Object.FindFirstObjectByType<CostDisplay>();
var combatLogPanel = Object.FindFirstObjectByType<CombatLogPanel>();
var combatStatusPanel = Object.FindFirstObjectByType<CombatStatusPanel>();
var skillButtonPanel = Object.FindFirstObjectByType<SkillButtonPanel>();

Assert.IsNotNull(costDisplay, "CostDisplay가 생성되어야 함");
Assert.IsNotNull(combatLogPanel, "CombatLogPanel이 생성되어야 함");
Assert.IsNotNull(combatStatusPanel, "CombatStatusPanel이 생성되어야 함");
Assert.IsNotNull(skillButtonPanel, "SkillButtonPanel이 생성되어야 함");

// 코스트 시스템 초기화 검증
Assert.Greater(_combatManager.MaxCost, 0, "최대 코스트가 설정되어야 함");
Assert.GreaterOrEqual(_combatManager.CurrentCost, 0, "현재 코스트가 0 이상이어야 함");
```
---

### 체크포인트 #3: 각 학생별 EX 스킬 사용 여부

**검증 대상**: 스킬 실행 및 코스트 시스템

#### 검증 과정
1. 각 학생의 EX 스킬을 순차적으로 실행
2. 스킬별 0.5초 간격으로 실행
3. `_combatManager.CombatSystem.UseSkill(student, student.exSkill)` 호출

#### 검증 항목
- ✅ 모든 학생의 스킬 실행 완료 (`allSkillsUsed`)
- ✅ 총 스킬 사용 횟수 == 학생 수
- ✅ 스킬별 실행 로그 출력 (이름, 데미지, 코스트)

#### 학생별 스킬 데이터
| 학생 이름 | HP | ATK | DEF | 스킬 이름 | 스킬 데미지 | 타겟 | 코스트 | 쿨타임 |
|---------|-----|-----|-----|---------|------------|------|-------|-------|
| Shiroko | 2492 | 340 | 19 | 드론 소환: 화력 지원 | 720 (400 × 1.8) | Single | 2 | 20초 |
| Hoshino | 3275 | 213 | 175 | 전술 진압 | 653 (435 × 1.5) | Single | 4 | 25초 |
| Aru | 2505 | 451 | 19 | 하드보일드 샷 | 548 (274 × 2.0) | Multiple | 4 | 22초 |
| Haruna | 2451 | 457 | 19 | 꿰뚫는 엘레강스 | 1265 (506 × 2.5) | Single | 4 | 30초 |

**적 데이터** (Normal 1-4):
- 일반병 A: HP 800, ATK 45, **DEF 15**
- 일반병 B: HP 800, ATK 45, **DEF 15**
- 정예병: HP 1200, ATK 60, **DEF 20**

#### 스킬 실행 프로세스
```
1. 코스트 확인: 필요 코스트 ≤ 현재 코스트
2. 스킬 사용:
   - 코스트 소모 (CostSystem.TrySpendCost)
   - 쿨타임 시작 (Student.UseSkill)
   - 로그 기록 (CombatLog.LogSkillUsed)
3. 데미지 계산:
   - 기본 데미지 계산: baseDamage × damageMultiplier
   - 대상 선택 (Single/Multiple/Area)
   - 방어력 적용: actualDamage = Max(1, skillDamage - enemyDef)
   - 데미지 적용 (Enemy.TakeDamage)
   - 로그 기록 (CombatLog.LogDamageDealt)
4. 격파 확인:
   - 적 HP ≤ 0 → 격파
   - 로그 기록 (CombatLog.LogUnitDefeated)
```

**예시**: Shiroko의 스킬(720 데미지)을 일반병 A(DEF 15)에게 사용
- 실제 데미지 = 720 - 15 = **705**

#### 성공 조건
4명의 학생이 모두 EX 스킬을 성공적으로 실행

#### 실패 조건
- 스킬이 전혀 사용되지 않음
- 데미지가 발생하지 않음
- 코스트 부족 시 스킬이 사용됨
- 쿨타임 중 스킬이 재사용됨

### 결과 검증
```csharp
// ========================================
// Assert: 결과 검증
// ========================================
int finalSkillCount = combatLog.TotalSkillsUsed;
int finalDamage = combatLog.TotalDamageDealt;
int finalCost = _combatManager.CurrentCost;

// 스킬이 최소 1회 이상 사용되었는지 검증
Assert.Greater(finalSkillCount, initialSkillCount,
    "스킬이 최소 1회 이상 사용되어야 함");

// 데미지가 발생했는지 검증
Assert.Greater(finalDamage, initialDamage,
    "데미지가 발생했어야 함");

// 코스트가 소모되었는지 검증 (코스트 회복 고려)
int totalCostSpent = combatLog.TotalCostSpent;
Assert.Greater(totalCostSpent, 0,
    "코스트가 소모되었어야 함");
```
---

### 체크포인트 #4: 코스트 소모량 및 데미지 기록

**검증 대상**: 전투 로그 시스템

#### 검증 과정
1. CombatLogSystem에서 전투 통계 수집
2. 코스트 소모량 검증
3. 데미지 기록 검증

#### 검증 항목
- ✅ 총 스킬 사용 횟수: `combatLog.TotalSkillsUsed == 4`
- ✅ 총 코스트 소모: `combatLog.TotalCostUsed == 14` (2+4+4+4)
- ✅ 총 데미지: 방어력 적용 후 실제 데미지 (동적 계산)
- ✅ 각 학생별 데미지 기록 존재

#### 데미지 기록 형식
```
스킬 데미지 계산 공식:
- 기본 데미지: baseDamage × damageMultiplier
- 실제 데미지: Max(1, 기본 데미지 - 적 방어력)

학생별 스킬 데미지 (방어력 적용 전):
- Shiroko: 720 (400 × 1.8)
- Hoshino: 652 (435 × 1.5)
- Aru: 548 (274 × 2.0) × 타겟 수 (Multiple)
- Haruna: 1265 (506 × 2.5)

적 방어력:
- 일반병 A/B: DEF 15
- 정예병: DEF 20

실제 전투 시나리오 (테스트 결과):
1. Shiroko → 일반병 타겟: 720 - 15 = 705 데미지
2. Hoshino → 일반병 타겟: 652 - 15 = 637 데미지
3. Aru → Multiple 타겟 (살아있는 적 대상):
   - 일반병: 548 - 15 = 533 데미지
   - 정예병: 548 - 20 = 528 데미지
   - 총 데미지: 1061 (타겟 수에 따라 변동)
4. Haruna → 타겟: 1250 데미지 (1265 - 15 or 1265 - 20)

총 데미지: 약 3653 (전투 상황에 따라 변동)

※ 실제 데미지는 스킬 사용 순서, 적 격파 타이밍에 따라 달라질 수 있습니다.
```

#### 성공 조건
모든 코스트와 데미지가 정확히 기록됨

#### 실패 조건
- 총 데미지가 감소함
- 학생별 통계가 비어있음
- 학생별 데미지 합계가 총 데미지와 다름
- 격파한 적 수가 잘못 기록됨

#### 데미지 기록 검증
```csharp
// ========================================
// Assert: 데미지 기록 검증
// ========================================
int finalDamage = combatLog.TotalDamageDealt;
int finalEnemiesDefeated = combatLog.TotalEnemiesDefeated;
int finalSkillsUsed = combatLog.TotalSkillsUsed;

// 총 데미지가 증가했거나 유지되어야 함
Assert.GreaterOrEqual(finalDamage, initialDamage,
    "총 데미지는 감소하지 않아야 함");

// 학생별 데미지 통계가 존재해야 함
var studentDamageStats = combatLog.StudentDamageStats;
Assert.Greater(studentDamageStats.Count, 0,
    "학생별 데미지 통계가 기록되어야 함");
```
---

### 체크포인트 #5: 보상 획득 검증

**검증 대상**: 보상 시스템 및 인벤토리 통합

#### 검증 과정

##### 1단계: RewardResultPanel 표시 (1.5초)
- 전투 통계 수집 (이동, 스킬, 데미지, 격파)
- RewardSystem으로 보상 계산
- RewardResultPanel 생성 및 표시

##### 2단계: InventoryPanel 초기화 (0.5초)
- InventoryPanel 생성
- RewardSystem.OnRewardGranted 이벤트 구독
- 빈 인벤토리 UI 표시

##### 3단계: 보상을 인벤토리에 추가 (각 0.4초 간격)
- 각 보상을 `rewardSystem.GrantReward()` 호출
- InventoryPanel이 이벤트를 받아 애니메이션과 함께 표시
- 보상 4개 순차 추가

##### 4단계: 검증 수행 및 결과 표시 (3초)
- RewardValidator 생성
- `ValidateRewardGrant()` 실행
- ValidationResultPanel로 결과 시각화

#### 검증 항목
- ✅ 스테이지 클리어: `_stageManager.CurrentState == StageState.StageCleared`
- ✅ 보상 유효성: `rewardResult.GrantedRewards.Count > 0`
- ✅ 인벤토리 존재: `inventoryPanel.GetInventoryData() != null`
- ✅ **예상 vs 실제 일치**: `validationResult.IsValid`

#### 예상 보상 (StageData.rewards)
1. **크레딧** (Currency) x1000
2. **노트** (Note) x5
3. **T1 가방** (Equipment) x1
4. **전술 EXP** (Exp) x150

#### 검증 테이블 (ValidationResultPanel)
| 아이템 이름 | 예상 수량 | 실제 수량 | 검증 |
|------------|----------|----------|------|
| 크레딧     | 1000      | 1000      | ✅   |
| 노트     | 5        | 5        | ✅   |
| T1 가방    | 1        | 1        | ✅   |
| 전술 EXP   | 150      | 150      | ✅   |



#### UI 표시 흐름
```
RewardResultPanel (중앙) - 보상 목록 + 전투 통계
    ↓ (1.5초 대기)
InventoryPanel (오른쪽) - 빈 인벤토리 생성
    ↓ (0.5초 대기)
보상 하나씩 추가 (0.4초 간격) - 애니메이션 효과
    ↓ (1초 대기)
ValidationResultPanel (중앙 하단) - 예상 vs 실제 비교 테이블
    ↓ (3초 대기)
최종 검증 완료
```

#### 성공 조건
모든 보상이 인벤토리에 정확히 추가되고 검증 통과

#### 실패 조건
- 스테이지 상태가 StageCleared가 아님
- 보상 결과가 null임
- 보상 항목이 비어있음
- 인벤토리 데이터가 없음
- 예상 보상과 실제 인벤토리 수량이 불일치

### 검증 패널 생성 확인
```csharp
// Assert: 검증 패널 생성 확인
Assert.IsNotNull(validationPanel, "ValidationResultPanel이 생성되어야 함");
Debug.Log("  ✅ ValidationResultPanel 표시 완료");

yield return new WaitForSeconds(3f);

// ========================================
// Assert: 최종 검증 (2단계)
// ========================================

// 인벤토리 데이터 존재 확인
Assert.Greater(inventoryData.Count, 0, "인벤토리에 아이템이 있어야 함");

// 모든 보상이 인벤토리에 정확히 추가되었는지 확인
foreach (var reward in rewardResult.GrantedRewards)
{
Assert.IsTrue(inventoryData.ContainsKey(reward.itemType),
    $"인벤토리에 {reward.itemType} 타입이 있어야 함");

int inventoryQuantity = inventoryData[reward.itemType];
Assert.AreEqual(reward.quantity, inventoryQuantity,
    $"{reward.itemName}: 예상 {reward.quantity}, 실제 {inventoryQuantity}");
}


_testProgressPanel.UpdateCheckpoint(5, CheckpointStatus.Completed);
_testProgressPanel.UpdateMessage("보상 획득 및 검증 완료!");
```
---

## 테스트 결과 해석

### 성공 시
```
[체크포인트 #1] ✅ 성공 - 전투 위치 도달, 5회 이동
[체크포인트 #2] ✅ 성공 - 전투 진입, 학생 4명, 적 3명
[체크포인트 #3] ✅ 성공 - EX 스킬 4회 사용
[체크포인트 #4] ✅ 성공 - 코스트 14, 데미지 3653 기록
[체크포인트 #5] ✅ 성공 - 스테이지 클리어, 보상 4개 획득, 인벤토리 추가 및 검증 완료

=====================================
✅ 모든 체크포인트 통과! (5/5)
=====================================
```

### 실패 시
각 체크포인트는 실패 시 상세한 실패 이유를 로그에 출력합니다:

```
[체크포인트 #5] ❌ 실패 - 스테이지: True, 보상: True, 인벤토리: True, 검증: False

검증 실패 이유: 보상 개수 불일치 (예상: 4, 실제: 0)
  - 보상 미지급: 크레딧 x200
  - 보상 미지급: 노트 x3
  - 보상 미지급: T1 가방 x1
  - 보상 미지급: 전술 EXP x150
```
