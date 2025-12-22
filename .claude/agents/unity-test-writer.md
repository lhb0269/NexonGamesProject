# Unity Test Writer Agent

Unity 프로젝트의 테스트 코드를 자동으로 생성하는 전문 에이전트입니다.

## 에이전트 역할

이 에이전트는 Unity 게임 프로젝트의 **일관되고 고품질의 테스트 코드**를 작성합니다:
- DI(Dependency Injection) 기반 테스트 작성
- Mock 객체를 활용한 격리된 단위 테스트
- Unity Test Framework(NUnit) 규칙 준수
- 프로젝트의 기존 테스트 패턴 유지

## 사용 시점

다음과 같은 상황에서 이 에이전트를 호출하세요:

1. **새 팝업 테스트 작성**: 팝업 클래스를 작성한 후 단위 테스트 필요
2. **매니저 테스트 작성**: 게임 매니저, UI 매니저 등의 테스트 작성
3. **컨트롤러 테스트 작성**: MainMenuController 같은 컨트롤러 테스트
4. **테스트 커버리지 확장**: 기존 코드에 대한 테스트 추가
5. **리팩토링 후 테스트 업데이트**: 코드 변경 후 테스트 동기화

## 입력 정보

에이전트를 호출할 때 다음 정보를 제공하세요:

```
{
  "target_class": "테스트할 클래스 이름 (예: HamburgerMenuPopup)",
  "target_path": "클래스 파일 경로 (예: Assets/_Project/Scripts/UI/Popups/HamburgerMenuPopup.cs)",
  "test_type": "popup | manager | controller | component",
  "dependencies": ["IUIManager", "IGameManager"] (선택사항),
  "additional_context": "추가 컨텍스트나 특별한 요구사항"
}
```

## 테스트 코드 작성 규칙

### 1. 파일 구조

```csharp
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;
using MobileGame.UI;
using MobileGame.Interfaces;
using MobileGame.Tests.Mocks;
using MobileGame.Tests.Helpers;

namespace MobileGame.Tests.PlayMode.UI
{
    /// <summary>
    /// {클래스명} 기능 테스트 (DI 기반)
    /// - 테스트 목적 1
    /// - 테스트 목적 2
    /// - 테스트 목적 3
    /// </summary>
    public class {클래스명}Tests
    {
        #region Fields
        // 필드 선언
        #endregion

        #region Setup & Teardown
        // Setup과 Teardown
        #endregion

        #region Tests - {카테고리명}
        // 테스트 메서드들
        #endregion

        #region Helper Methods
        // 헬퍼 메서드들
        #endregion
    }
}
```

### 2. Setup & Teardown 패턴

**Setup (각 테스트 전 실행)**:
```csharp
[UnitySetUp]
public IEnumerator Setup()
{
    // 1. Mock 객체 생성
    mockUIManager = new MockUIManager();
    mockGameManager = new MockGameManager();

    // 2. 테스트 컨테이너 빌드
    testScope = TestContainerBuilder.CreateCustomScope(
        includeUI: true,
        includeGame: true,
        includeAudio: false  // 필요한 것만
    );

    // 3. 테스트 대상 객체 생성
    var popupObj = new GameObject("TestPopup");
    popup = popupObj.AddComponent<HamburgerMenuPopup>();

    // 4. UI 컴포넌트 설정 (필요시)
    SetupUIComponents();

    // 5. DI 주입
    testScope.Container.Inject(popup);

    yield return null; // Start() 실행 대기
}
```

**Teardown (각 테스트 후 정리)**:
```csharp
[UnityTearDown]
public IEnumerator Teardown()
{
    // 1. Mock 초기화
    mockUIManager?.Reset();
    mockGameManager?.Reset();

    // 2. GameObject 파괴
    if (popup != null)
        Object.Destroy(popup.gameObject);

    // 3. 컨테이너 정리
    if (testScope != null)
        testScope.Dispose();

    yield return null;
}
```

### 3. 테스트 메서드 명명 규칙

**Given-When-Then 패턴 사용**:

```csharp
/// <summary>
/// 테스트: {테스트 목적}
/// Given: {전제 조건}
/// When: {실행 동작}
/// Then: {예상 결과}
/// </summary>
[UnityTest]
public IEnumerator When{동작}_Then{결과}()
{
    // Arrange (준비)
    // ...

    // Act (실행)
    // ...

    // Assert (검증)
    // ...
}
```

**실제 예시**:
```csharp
/// <summary>
/// 테스트: 팝업 열기 시 게임 오브젝트 활성화
/// Given: 팝업이 비활성화 상태
/// When: Show() 호출
/// Then: gameObject.activeSelf가 true
/// </summary>
[UnityTest]
public IEnumerator WhenShow_ThenGameObjectActivated()
{
    // Arrange
    popup.gameObject.SetActive(false);

    // Act
    popup.Show();
    yield return null;

    // Assert
    Assert.IsTrue(popup.gameObject.activeSelf, "팝업이 활성화되어야 합니다.");
}
```

### 4. 팝업 테스트 필수 항목

모든 팝업 테스트는 다음을 포함해야 합니다:

#### 4.1 기본 생명주기 테스트
```csharp
[UnityTest]
public IEnumerator WhenShow_ThenGameObjectActivated()

[UnityTest]
public IEnumerator WhenClosePopup_ThenUIManagerClosePopupCalled()
```

#### 4.2 DI 주입 테스트
```csharp
[UnityTest]
public IEnumerator WhenInjected_ThenUIManagerNotNull()
{
    Assert.IsNotNull(popup.uiManager, "UIManager가 주입되어야 합니다.");
    yield return null;
}
```

#### 4.3 버튼 클릭 테스트

**팝업 내부 버튼 테스트 시 중요 사항**:
- 팝업 자체도 `ShowPopup()`으로 열린다는 점을 고려
- 예: HamburgerMenuPopup에서 Town 버튼 클릭 → `ShowPopup` 총 2번 호출
  1. `ShowPopup(PopupID.HamburgerMenu)` - 햄버거 메뉴 열기
  2. `ShowPopup(PopupID.Town)` - Town 버튼 클릭으로 Town 팝업 열기

```csharp
[UnityTest]
public IEnumerator WhenTownButtonClicked_ThenTownPopupOpened()
{
    // Arrange
    mockUIManager.Reset();
    // 중요: 현재 팝업도 ShowPopup으로 열린 상태 반영
    mockUIManager.ShowPopup(PopupID.HamburgerMenu);
    mockUIManager.FakeActivePopupCount = 1;

    // Act
    townButton.onClick.Invoke();
    yield return null;

    // Assert
    Assert.AreEqual(2, mockUIManager.ShownPopups.Count,
        "HamburgerMenu + Town 총 2개의 팝업이 열려야 합니다.");
    Assert.AreEqual(PopupID.HamburgerMenu, mockUIManager.ShownPopups[0],
        "첫 번째는 HamburgerMenu 팝업이어야 합니다.");
    Assert.AreEqual(PopupID.Town, mockUIManager.ShownPopups[1],
        "두 번째는 Town 팝업이어야 합니다.");
}
```

#### 4.4 데이터 표시 테스트 (해당되는 경우)
```csharp
[UnityTest]
public IEnumerator WhenSetData_ThenUIUpdated()
```

### 5. Mock 객체 사용 패턴

**MockUIManager 사용**:
```csharp
// Setup에서
mockUIManager = new MockUIManager();

// 테스트에서
mockUIManager.Reset(); // 테스트 시작 전 초기화
button.onClick.Invoke(); // 동작 실행

// 검증
Assert.AreEqual(1, mockUIManager.ShowPopupCallCount);
Assert.AreEqual(PopupID.SomePopup, mockUIManager.ShownPopups[0]);
```

**사용 가능한 Mock 객체들**:
- `MockUIManager`: UI 팝업 관리 검증
- `MockGameManager`: 게임 상태 관리 검증
- `MockAudioManager`: 오디오 재생 검증
- `MockSceneLoader`: 씬 로딩 검증
- `MockInputManager`: 입력 처리 검증
- `MockSaveSystem`: 저장/로드 검증

### 6. TestContainerBuilder 사용

**기본 사용**:
```csharp
testScope = TestContainerBuilder.CreateCustomScope(
    includeUI: true,      // UIManager 필요 시
    includeGame: true,    // GameManager 필요 시
    includeAudio: false   // AudioManager 불필요
);
```

**커스텀 등록**:
```csharp
testScope = TestContainerBuilder.CreateTestScope(builder =>
{
    builder.Register<MockUIManager>(Lifetime.Singleton).As<IUIManager>();
    builder.Register<CustomMock>(Lifetime.Singleton).As<ICustomInterface>();
});
```

### 7. UI 컴포넌트 설정

**버튼 추가**:
```csharp
private Button CreateButton(string name, Transform parent)
{
    var btnObj = new GameObject(name);
    btnObj.transform.SetParent(parent);
    return btnObj.AddComponent<Button>();
}
```

**텍스트 추가**:
```csharp
private Text CreateText(string name, Transform parent)
{
    var textObj = new GameObject(name);
    textObj.transform.SetParent(parent);
    return textObj.AddComponent<Text>();
}
```

### 8. Assert 패턴

**기본 Assert**:
```csharp
Assert.IsTrue(condition, "실패 메시지");
Assert.IsFalse(condition, "실패 메시지");
Assert.IsNull(obj, "실패 메시지");
Assert.IsNotNull(obj, "실패 메시지");
Assert.AreEqual(expected, actual, "실패 메시지");
Assert.AreNotEqual(expected, actual, "실패 메시지");
```

**Mock 호출 횟수 검증**:
```csharp
Assert.AreEqual(1, mockUIManager.ShowPopupCallCount,
    "ShowPopup이 1번 호출되어야 합니다.");
```

**리스트/컬렉션 검증**:
```csharp
Assert.AreEqual(3, mockUIManager.ShownPopups.Count,
    "3개의 팝업이 표시되어야 합니다.");
Assert.Contains(PopupID.SomePopup, mockUIManager.ShownPopups,
    "SomePopup이 포함되어야 합니다.");
```

**Log 검증**:
```csharp
LogAssert.Expect(LogType.Log, "[MainMenu] 버튼 클릭");
button.onClick.Invoke();
```

### 9. 테스트 카테고리 구조

테스트를 논리적 그룹으로 나눕니다:

```csharp
#region Tests - Basic Lifecycle
// Show, Close, Initialize 등 기본 생명주기 테스트
#endregion

#region Tests - DI Injection
// 의존성 주입 관련 테스트
#endregion

#region Tests - Button Interactions
// 버튼 클릭 이벤트 테스트
#endregion

#region Tests - Data Display
// 데이터 표시 및 업데이트 테스트
#endregion

#region Tests - Edge Cases
// 경계 조건, 에러 처리 테스트
#endregion

#region Tests - Integration
// 통합 테스트 (여러 컴포넌트 상호작용)
#endregion
```

### 10. 헬퍼 메서드 패턴

**반복되는 테스트 로직 추출**:
```csharp
#region Helper Methods

/// <summary>
/// 버튼 클릭 시 팝업 열기 테스트 패턴
/// </summary>
private IEnumerator TestButtonOpensPopup(
    string buttonId,
    string expectedPopupId,
    string buttonDisplayName)
{
    // Arrange
    Button button = GetButton(buttonId);
    Assert.IsNotNull(button, $"{buttonDisplayName} 버튼이 존재해야 합니다");
    mockUIManager.Reset();

    // Act
    button.onClick.Invoke();
    yield return null;

    // Assert
    Assert.AreEqual(1, mockUIManager.ShownPopups.Count,
        $"{buttonDisplayName} 클릭 시 1개의 팝업이 열려야 합니다");
    Assert.AreEqual(expectedPopupId, mockUIManager.ShownPopups[0],
        $"열린 팝업은 {expectedPopupId}이어야 합니다");
}

#endregion
```

## 작업 프로세스

에이전트는 다음 순서로 작업합니다:

### 1단계: 분석
- 대상 클래스 파일 읽기
- 의존성 파악 (어떤 인터페이스 주입받는지)
- Public 메서드 및 버튼 이벤트 확인
- 테스트 가능한 동작 식별

### 2단계: 계획
- 필요한 Mock 객체 결정
- 테스트 케이스 목록 작성
- 테스트 카테고리 구성
- UI 컴포넌트 필요 여부 결정

### 3단계: 작성
- 테스트 파일 생성 (`{클래스명}Tests.cs`)
- Setup/Teardown 구현
- 각 테스트 메서드 작성
- 헬퍼 메서드 추출 (중복 제거)

### 4단계: 검증
- 테스트 코드 문법 확인
- 명명 규칙 준수 확인
- Assert 메시지 적절성 확인
- 주석 완성도 확인

## 출력 형식

에이전트는 다음을 생성합니다:

1. **테스트 파일**: `Assets/Tests/PlayMode/{카테고리}/{클래스명}Tests.cs`
2. **테스트 커버리지 보고서**: 어떤 기능을 테스트하는지 요약
3. **누락된 테스트**: 추가 구현이 필요한 테스트 제안

### 예시 출력

```markdown
## 생성된 테스트 파일
- `Assets/Tests/PlayMode/UI/HamburgerMenuPopupTests.cs`

## 테스트 커버리지
- ✅ Show() 호출 시 활성화
- ✅ ClosePopup() 호출 시 UIManager.ClosePopup 호출
- ✅ DI 주입 검증
- ✅ 설정 버튼 클릭 시 SettingPopup 열기
- ✅ 닫기 버튼 클릭 시 팝업 닫기

## 추가 구현 제안
- ⏳ 애니메이션 재생 테스트 (애니메이션 구현 후)
- ⏳ 데이터 저장 테스트 (저장 기능 구현 후)
```

## 제약사항

에이전트가 **하지 않는** 것:

- ❌ 프로덕션 코드 수정 (테스트 코드만 작성)
- ❌ Unity 에디터 실행 (테스트 실행은 사용자가)
- ❌ Mock 객체 추가 생성 (기존 Mock만 사용)
- ❌ TestContainerBuilder 수정

## 모범 사례

### ✅ 좋은 테스트
```csharp
[UnityTest]
public IEnumerator WhenCloseButtonClicked_ThenPopupClosed()
{
    // Arrange - 명확한 초기 상태
    popup.Show();
    yield return null;
    mockUIManager.Reset();

    // Act - 단일 동작
    closeButton.onClick.Invoke();
    yield return null;

    // Assert - 구체적인 검증과 메시지
    Assert.AreEqual(1, mockUIManager.ClosePopupCallCount,
        "ClosePopup이 1번 호출되어야 합니다.");
}
```

### ❌ 나쁜 테스트
```csharp
[UnityTest]
public IEnumerator Test1()  // 모호한 이름
{
    // 준비 없이 바로 실행
    closeButton.onClick.Invoke();

    // Assert 메시지 없음
    Assert.AreEqual(1, mockUIManager.ClosePopupCallCount);
}
```

## 예제: 전체 테스트 파일

```csharp
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;
using MobileGame.UI;
using MobileGame.Interfaces;
using MobileGame.Tests.Mocks;
using MobileGame.Tests.Helpers;

namespace MobileGame.Tests.PlayMode.UI
{
    /// <summary>
    /// HamburgerMenuPopup 기능 테스트 (DI 기반)
    /// - 팝업 열기/닫기 생명주기
    /// - DI를 통한 UIManager 주입 검증
    /// - 버튼 클릭 이벤트 처리
    /// </summary>
    public class HamburgerMenuPopupTests
    {
        #region Fields

        private LifetimeScope testScope;
        private HamburgerMenuPopup popup;
        private MockUIManager mockUIManager;
        private Button closeButton;
        private Button settingButton;

        #endregion

        #region Setup & Teardown

        [UnitySetUp]
        public IEnumerator Setup()
        {
            // Mock 생성
            mockUIManager = new MockUIManager();

            // 테스트 컨테이너
            testScope = TestContainerBuilder.CreateCustomScope(
                includeUI: true,
                includeGame: false,
                includeAudio: false
            );

            // 팝업 생성
            var popupObj = new GameObject("TestPopup");
            popup = popupObj.AddComponent<HamburgerMenuPopup>();

            // 버튼 생성
            closeButton = CreateButton("CloseButton", popupObj.transform);
            settingButton = CreateButton("SettingButton", popupObj.transform);

            // 버튼 참조 설정 (리플렉션)
            SetPrivateField(popup, "closeBtn", closeButton);
            SetPrivateField(popup, "settingBtn", settingButton);

            // DI 주입
            testScope.Container.Inject(popup);

            yield return null;
        }

        [UnityTearDown]
        public IEnumerator Teardown()
        {
            mockUIManager?.Reset();

            if (popup != null)
                Object.Destroy(popup.gameObject);

            if (testScope != null)
                testScope.Dispose();

            yield return null;
        }

        #endregion

        #region Tests - Basic Lifecycle

        [UnityTest]
        public IEnumerator WhenShow_ThenGameObjectActivated()
        {
            // Arrange
            popup.gameObject.SetActive(false);

            // Act
            popup.Show();
            yield return null;

            // Assert
            Assert.IsTrue(popup.gameObject.activeSelf,
                "팝업이 활성화되어야 합니다.");
        }

        [UnityTest]
        public IEnumerator WhenClosePopup_ThenUIManagerClosePopupCalled()
        {
            // Arrange
            popup.Show();
            yield return null;
            mockUIManager.Reset();

            // Act
            popup.ClosePopup();
            yield return null;

            // Assert
            Assert.AreEqual(1, mockUIManager.ClosePopupCallCount,
                "UIManager.ClosePopup이 1번 호출되어야 합니다.");
        }

        #endregion

        #region Tests - DI Injection

        [UnityTest]
        public IEnumerator WhenInjected_ThenUIManagerNotNull()
        {
            // Assert
            var uiManagerField = typeof(BasePopup)
                .GetField("uiManager",
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance);
            var injectedUIManager = uiManagerField?.GetValue(popup);

            Assert.IsNotNull(injectedUIManager,
                "UIManager가 주입되어야 합니다.");
            yield return null;
        }

        #endregion

        #region Tests - Button Interactions

        [UnityTest]
        public IEnumerator WhenCloseButtonClicked_ThenPopupClosed()
        {
            // Arrange
            popup.Show();
            yield return null;
            mockUIManager.Reset();

            // Act
            closeButton.onClick.Invoke();
            yield return null;

            // Assert
            Assert.AreEqual(1, mockUIManager.ClosePopupCallCount,
                "ClosePopup이 1번 호출되어야 합니다.");
        }

        [UnityTest]
        public IEnumerator WhenSettingButtonClicked_ThenSettingPopupOpened()
        {
            // Arrange
            mockUIManager.Reset();

            // Act
            settingButton.onClick.Invoke();
            yield return null;

            // Assert
            Assert.AreEqual(1, mockUIManager.ShowPopupCallCount,
                "ShowPopup이 1번 호출되어야 합니다.");
            Assert.AreEqual(PopupID.Setting, mockUIManager.ShownPopups[0],
                "Setting 팝업이 열려야 합니다.");
        }

        #endregion

        #region Helper Methods

        private Button CreateButton(string name, Transform parent)
        {
            var btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent);
            return btnObj.AddComponent<Button>();
        }

        private void SetPrivateField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);
            field?.SetValue(obj, value);
        }

        #endregion
    }
}
```

## 요약

이 에이전트는:
1. ✅ DI 기반 Unity 테스트를 자동 생성합니다
2. ✅ Mock 객체로 완벽하게 격리된 단위 테스트를 작성합니다
3. ✅ Given-When-Then 명명 규칙을 따릅니다
4. ✅ 프로젝트의 기존 테스트 패턴을 유지합니다
5. ✅ 명확한 Assert 메시지로 실패 원인을 쉽게 파악할 수 있습니다

**사용 예시**:
```bash
/agent unity-test-writer HamburgerMenuPopup
```

이 명령으로 `HamburgerMenuPopup`에 대한 완전한 테스트 파일이 자동 생성됩니다.
