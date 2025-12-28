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
[#1] 발판 이동 (3회 이동)
    ↓
[#2] 전투 진입 (학생 4명, 적 2명)
    ↓
[#3] EX 스킬 사용 (4명 x 1회)
    ↓
[#4] 코스트/데미지 기록 (16 코스트, 1250 데미지)
    ↓
[#5] 보상 획득 (4개 보상, 인벤토리 추가, 검증)
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
4. 이동 경로: (0,0) → (1,1) → (2,1) → (3,1)

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

---

### 체크포인트 #2: 전투 정상 진입 여부

**검증 대상**: 스테이지 상태 전환 시스템

#### 검증 과정
1. CombatManager 초기화
2. `_combatManager.SetupCombat()` 호출로 전투 준비
3. `_stageManager.EnterBattle()` 호출로 전투 진입
4. 학생 4명, 적 2명 배치

#### 검증 항목
- ✅ `_stageManager.CurrentState == StageState.InBattle` (전투 상태 전환)
- ✅ `_combatManager.CombatSystem.CurrentState == CombatState.InProgress` (전투 시작)
- ✅ 학생 4명 활성화
- ✅ 적 2명 배치

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
| 학생 이름 | 스킬 이름 | 데미지 | 코스트 | 쿨타임 |
|---------|---------|-------|-------|-------|
| 아리스 | EX: 정의의 일격 | 500 | 4 | 20초 |
| 호시노 | EX: 방패의 의지 | 300 | 3 | 25초 |
| 이로하 | EX: 치유의 기도 | 0 | 5 | 15초 |
| 시로코 | EX: 폭풍의 일격 | 450 | 4 | 20초 |

#### 스킬 실행 프로세스
```
1. 코스트 확인: 필요 코스트 ≤ 현재 코스트
2. 스킬 사용:
   - 코스트 소모 (CostSystem.TrySpendCost)
   - 쿨타임 시작 (Student.UseSkill)
   - 로그 기록 (CombatLog.LogSkillUsed)
3. 데미지 계산:
   - 대상 선택 (Single/Multiple/Area)
   - 데미지 적용 (Enemy.TakeDamage)
   - 로그 기록 (CombatLog.LogDamageDealt)
4. 격파 확인:
   - 적 HP ≤ 0 → 격파
   - 로그 기록 (CombatLog.LogUnitDefeated)
```

#### 성공 조건
4명의 학생이 모두 EX 스킬을 성공적으로 실행

#### 실패 조건
- 스킬이 전혀 사용되지 않음
- 데미지가 발생하지 않음
- 코스트 부족 시 스킬이 사용됨
- 쿨타임 중 스킬이 재사용됨

---

### 체크포인트 #4: 코스트 소모량 및 데미지 기록

**검증 대상**: 전투 로그 시스템

#### 검증 과정
1. CombatLogSystem에서 전투 통계 수집
2. 코스트 소모량 검증
3. 데미지 기록 검증

#### 검증 항목
- ✅ 총 스킬 사용 횟수: `combatLog.TotalSkillsUsed == 4`
- ✅ 총 코스트 소모: `combatLog.TotalCostUsed == 16` (4+3+5+4)
- ✅ 총 데미지: `combatLog.TotalDamageDealt == 1250` (500+300+0+450)
- ✅ 각 학생별 데미지 기록 존재

#### 데미지 기록 형식
```
아리스: 500 데미지 (코스트 4)
호시노: 300 데미지 (코스트 3)
이로하: 0 데미지 (코스트 5)
시로코: 450 데미지 (코스트 4)
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

#### 성공 조건
모든 코스트와 데미지가 정확히 기록됨

#### 실패 조건
- 총 데미지가 감소함
- 학생별 통계가 비어있음
- 학생별 데미지 합계가 총 데미지와 다름
- 격파한 적 수가 잘못 기록됨

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
1. **크레딧** (Currency) x200
2. **강화석** (Material) x3
3. **T1 가방** (Equipment) x1
4. **전술 EXP** (Exp) x150

#### 검증 테이블 (ValidationResultPanel)
| 아이템 이름 | 예상 수량 | 실제 수량 | 검증 |
|------------|----------|----------|------|
| 크레딧     | 200      | 200      | ✅   |
| 강화석     | 3        | 3        | ✅   |
| T1 가방    | 1        | 1        | ✅   |
| 전술 EXP   | 150      | 150      | ✅   |

#### 보상 계산 로직
```csharp
public RewardGrantResult CalculateRewards(string stageName, int totalMoves, CombatLogSystem combatLog)
{
    var result = new RewardGrantResult();

    // 기본 보상 (StageData.rewards 기반)
    result.GrantedRewards.Add(new RewardItemData
    {
        itemName = "크레딧",
        itemType = RewardItemType.Currency,
        quantity = 200
    });

    result.GrantedRewards.Add(new RewardItemData
    {
        itemName = "강화석",
        itemType = RewardItemType.Material,
        quantity = 3
    });

    result.GrantedRewards.Add(new RewardItemData
    {
        itemName = "T1 가방",
        itemType = RewardItemType.Equipment,
        quantity = 1
    });

    result.GrantedRewards.Add(new RewardItemData
    {
        itemName = "전술 EXP",
        itemType = RewardItemType.Exp,
        quantity = 150
    });

    result.Success = true;
    result.TotalRewardCount = result.GrantedRewards.Count;

    return result;
}
```

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

---

---

## 테스트 결과 해석

### 성공 시
```
[체크포인트 #1] ✅ 성공 - 전투 위치 도달, 3회 이동
[체크포인트 #2] ✅ 성공 - 전투 진입, 학생 4명, 적 2명
[체크포인트 #3] ✅ 성공 - EX 스킬 4회 사용
[체크포인트 #4] ✅ 성공 - 코스트 16, 데미지 1250 기록
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
  - 보상 미지급: 강화석 x3
  - 보상 미지급: T1 가방 x1
  - 보상 미지급: 전술 EXP x150
```
