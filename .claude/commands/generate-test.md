---
description: Unity 클래스에 대한 단위 테스트 자동 생성
tags: [testing, unity, automation]
---

# 테스트 코드 자동 생성

당신은 Unity 프로젝트의 **테스트 코드 작성 전문가**입니다.

## 작업 개요

사용자가 지정한 Unity 클래스에 대해 **DI 기반 단위 테스트 코드**를 자동으로 생성합니다.

## 입력 형식

사용자는 다음 중 하나의 방식으로 명령을 실행합니다:

1. `/generate-test HamburgerMenuPopup` - 클래스명만 제공
2. `/generate-test Assets/_Project/Scripts/UI/Popups/HamburgerMenuPopup.cs` - 파일 경로 제공
3. `/generate-test popup` - 타입 키워드 제공 (모든 팝업 테스트)

## 에이전트 가이드라인

`.claude/agents/unity-test-writer.md` 파일의 규칙을 **엄격히 준수**하여 테스트를 작성하세요.

### 필수 준수 사항

1. **파일 구조**
   - namespace: `MobileGame.Tests.PlayMode.{카테고리}`
   - 클래스명: `{대상클래스명}Tests`
   - #region으로 섹션 구분

2. **Setup/Teardown 패턴**
   - `[UnitySetUp]`에서 Mock 생성, 컨테이너 빌드, DI 주입
   - `[UnityTearDown]`에서 Mock 초기화, GameObject 파괴, 컨테이너 정리

3. **테스트 메서드 명명**
   - Given-When-Then 패턴
   - `When{동작}_Then{결과}` 형식
   - 한국어 주석으로 Given-When-Then 명시

4. **Assert 메시지**
   - 모든 Assert에 실패 이유를 명확히 설명하는 메시지 포함
   - 예: `Assert.IsNotNull(obj, "UIManager가 주입되어야 합니다.")`

5. **Mock 객체 사용**
   - `MockUIManager`, `MockGameManager` 등 기존 Mock만 사용
   - 각 테스트 전 `Reset()` 호출

6. **테스트 카테고리**
   - Basic Lifecycle (Show, Close 등)
   - DI Injection (의존성 주입 검증)
   - Button Interactions (버튼 클릭 이벤트)
   - Data Display (데이터 표시)
   - Edge Cases (경계 조건)
   - Integration (통합 테스트)

## 작업 단계

### 1단계: 분석 (Analysis)

```
TODO 작성:
- [x] 대상 클래스 파일 읽기
- [x] 의존성 파악
- [x] Public 메서드 식별
- [x] 버튼 이벤트 확인
```

**실행 작업**:
- 대상 클래스 파일을 읽어 구조 파악
- `[Inject]` 어트리뷰트로 의존성 확인
- `[SerializeField]` 버튼 필드 확인
- Public 메서드 목록 작성

**출력**:
```markdown
## 분석 결과: HamburgerMenuPopup

### 의존성
- IUIManager (BasePopup에서 상속)

### Public 메서드
- Show()
- ClosePopup()

### 버튼
- closeBtn (닫기 버튼)
- settingBtn (설정 버튼)

### 이벤트 핸들러
- OnCloseBtnClicked()
- OnSettingBtnClicked()
```

### 2단계: 계획 (Planning)

```
TODO 업데이트:
- [x] 분석 완료
- [in_progress] 테스트 케이스 목록 작성
- [ ] 테스트 파일 작성
```

**테스트 케이스 작성**:
- 각 Public 메서드마다 최소 1개 테스트
- 각 버튼 클릭마다 1개 테스트
- DI 주입 검증 1개
- 경계 조건 테스트 (옵션)

**출력**:
```markdown
## 테스트 계획

### Basic Lifecycle (3개)
1. WhenShow_ThenGameObjectActivated
2. WhenClosePopup_ThenUIManagerClosePopupCalled
3. WhenHide_ThenGameObjectDeactivated

### DI Injection (1개)
4. WhenInjected_ThenUIManagerNotNull

### Button Interactions (2개)
5. WhenCloseButtonClicked_ThenPopupClosed
6. WhenSettingButtonClicked_ThenSettingPopupOpened

총 6개 테스트
```

### 3단계: 작성 (Implementation)

```
TODO 업데이트:
- [x] 분석 완료
- [x] 테스트 케이스 목록 작성
- [in_progress] 테스트 파일 작성
```

**파일 생성**:
- 경로: `Assets/Tests/PlayMode/UI/{클래스명}Tests.cs`
- 템플릿: `.claude/agents/unity-test-writer.md`의 예제 참고

**코드 작성 순서**:
1. using 문
2. namespace 및 클래스 주석
3. Fields 섹션
4. Setup & Teardown
5. 각 카테고리별 테스트 메서드
6. Helper Methods

### 4단계: 검증 (Validation)

```
TODO 업데이트:
- [x] 분석 완료
- [x] 테스트 케이스 목록 작성
- [x] 테스트 파일 작성
- [in_progress] 코드 검증
```

**체크리스트**:
- [ ] 모든 using 문 포함 (NUnit, UnityEngine.TestTools, VContainer 등)
- [ ] namespace 올바른지 확인
- [ ] #region 구조 올바른지 확인
- [ ] 모든 Assert에 메시지 포함
- [ ] Given-When-Then 주석 포함
- [ ] Setup에서 yield return null 포함
- [ ] Teardown에서 Dispose() 호출
- [ ] Mock Reset() 호출

### 5단계: 보고 (Reporting)

```
TODO 완료:
- [x] 모든 작업 완료
```

**출력 형식**:
```markdown
## 테스트 생성 완료

### 생성된 파일
- `Assets/Tests/PlayMode/UI/HamburgerMenuPopupTests.cs`

### 테스트 커버리지
- ✅ Show() 호출 시 활성화 (WhenShow_ThenGameObjectActivated)
- ✅ ClosePopup() 호출 시 UIManager.ClosePopup 호출 (WhenClosePopup_ThenUIManagerClosePopupCalled)
- ✅ DI 주입 검증 (WhenInjected_ThenUIManagerNotNull)
- ✅ 닫기 버튼 클릭 시 팝업 닫기 (WhenCloseButtonClicked_ThenPopupClosed)
- ✅ 설정 버튼 클릭 시 Setting 팝업 열기 (WhenSettingButtonClicked_ThenSettingPopupOpened)

### 통계
- 총 테스트: 5개
- 카테고리: 3개 (Lifecycle, DI, Buttons)
- 줄 수: 245줄

### 다음 단계
Unity 에디터에서 Test Runner를 열고 테스트를 실행하여 검증하세요:
1. Window > General > Test Runner
2. PlayMode 탭 선택
3. Run All 클릭
```

## 특수 케이스 처리

### 팝업 테스트
- BasePopup 상속 확인
- ClosePopup() 오버라이드 테스트
- onPopupClosed 콜백 테스트

### 매니저 테스트
- Initialize() 메서드 테스트
- Singleton 패턴 사용하지 않는지 확인
- 인터페이스 구현 검증

### 컨트롤러 테스트
- ButtonBinder 사용 패턴 (MainMenuControllerTests.cs 참고)
- 여러 버튼 클릭 통합 테스트
- 팝업 중복 열기 방지 테스트

## 에러 처리

### 대상 클래스를 찾을 수 없는 경우
```
❌ 오류: 'HamburgerMenuPopup' 클래스를 찾을 수 없습니다.

다음을 확인하세요:
1. 클래스명 철자 확인
2. 파일이 Assets/ 폴더에 있는지 확인
3. 전체 경로로 재시도: /generate-test Assets/_Project/Scripts/UI/Popups/HamburgerMenuPopup.cs
```

### 이미 테스트가 존재하는 경우
```
⚠️ 경고: HamburgerMenuPopupTests.cs 파일이 이미 존재합니다.

옵션:
1. 기존 파일 덮어쓰기 (Y)
2. 새 파일명으로 저장 (N - HamburgerMenuPopupTests2.cs 생성)
3. 취소 (C)

선택: [Y/N/C]
```

### Mock 객체가 없는 경우
```
⚠️ 경고: ICustomManager에 대한 Mock 객체가 없습니다.

해결 방법:
1. Assets/Tests/Mocks/MockCustomManager.cs를 먼저 생성하거나
2. 해당 의존성 없이 테스트 작성 (수동으로 나중에 추가)

계속하시겠습니까? [Y/N]
```

## 예제 실행

### 예제 1: 단일 클래스 테스트
```
User: /generate-test HamburgerMenuPopup
Agent: [분석 → 계획 → 작성 → 검증 → 보고]
```

### 예제 2: 경로로 지정
```
User: /generate-test Assets/_Project/Scripts/Managers/GameManager.cs
Agent: [GameManagerTests.cs 생성]
```

### 예제 3: 타입별 일괄 생성
```
User: /generate-test popup
Agent: [모든 팝업 클래스 분석 → 각각 테스트 파일 생성]
```

## 제약사항

- ❌ 프로덕션 코드 수정 금지 (테스트 코드만 생성)
- ❌ Unity 에디터 실행 불가 (테스트 실행은 사용자가)
- ❌ 새 Mock 객체 생성 금지 (기존 Mock만 사용)
- ✅ 기존 테스트 패턴 엄격히 준수

## 시작

이제 사용자가 제공한 대상 클래스에 대한 테스트 코드를 생성하세요.

**사용자 입력 대기 중...**
