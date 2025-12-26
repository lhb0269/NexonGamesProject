# 빌드 문제 해결 가이드

블루 아카이브 테스트 자동화 프로젝트의 빌드 과정에서 발생한 문제와 해결 방법을 기록합니다.

---

## 목차
1. [빌드 방법](#빌드-방법)
2. [문제 1: UI가 표시되지 않음](#문제-1-ui가-표시되지-않음)
3. [문제 2: Input System 오류](#문제-2-input-system-오류)
4. [문제 3: 직렬화 레이아웃 불일치](#문제-3-직렬화-레이아웃-불일치)
5. [문제 4: NUnit 참조 오류](#문제-4-nunit-참조-오류)

---

## 빌드 방법

### Unity 에디터에서 빌드
1. **Build Settings 열기**: `File` → `Build Settings`
2. **씬 추가 확인**: `Scenes/SampleScene`이 리스트에 있고 체크되어 있는지 확인
3. **플랫폼 선택**: Windows, macOS, Linux 중 선택
4. **Development Build 활성화** (권장):
   - `Development Build` 체크
   - `Script Debugging` 체크
   - 로그 출력 및 디버깅 가능
5. **Build 클릭**: 실행 파일 저장 위치 선택

### 빌드 결과물
```
빌드폴더/
├── NexonGamesProject.exe        # 실행 파일
├── NexonGamesProject_Data/      # 게임 데이터
├── MonoBleedingEdge/            # .NET 런타임
└── UnityPlayer.dll              # Unity 엔진
```

### 실행 및 로그 확인
- **실행**: `NexonGamesProject.exe` 더블 클릭
- **로그 위치**: `%USERPROFILE%\AppData\LocalLow\DefaultCompany\NexonGamesProject\Player.log`

---

## 문제 1: UI가 표시되지 않음

### 증상
- 빌드 실행 시 회색 화면만 보임
- TestProgressPanel이 나타나지 않음
- Unity Editor에서는 정상 작동

### 원인
**어셈블리 제약으로 인한 빌드 제외**
- `NexonGame.Tests.PlayMode` 어셈블리가 `UNITY_INCLUDE_TESTS` 제약 조건을 가짐
- 이 제약은 Unity Test Framework에서만 어셈블리를 포함하도록 지시
- 빌드 시에는 테스트 프레임워크가 포함되지 않으므로 해당 어셈블리도 제외됨
- TestVisualizationRunner가 이 어셈블리에 위치하여 빌드에서 누락됨

**어셈블리 정의 예시**:
```json
// NexonGame.Tests.PlayMode.asmdef
{
  "name": "NexonGame.Tests.PlayMode",
  "defineConstraints": [
    "UNITY_INCLUDE_TESTS"  // 이 제약으로 인해 빌드 제외
  ]
}
```

### 해결 방법

**별도 어셈블리 분리**

1. **새 어셈블리 생성**: `Assets/_Project/Scripts/Tests/Automation/`

2. **어셈블리 정의 파일 작성**:
```json
// NexonGame.Tests.Automation.asmdef
{
    "name": "NexonGame.Tests.Automation",
    "rootNamespace": "NexonGame.Tests",
    "references": [
        "NexonGame.Runtime",
        "Unity.InputSystem"
    ],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": [],  // 제약 없음 - 빌드 포함
    "versionDefines": [],
    "noEngineReferences": false
}
```

3. **파일 이동**:
   - `TestVisualizationRunner.cs` → `Tests/Automation/`
   - `TestBootstrap.cs` → `Tests/Automation/`

4. **네임스페이스 변경**:
```csharp
// 수정 전
namespace NexonGame.Tests.PlayMode

// 수정 후
namespace NexonGame.Tests.Automation
```

5. **씬 업데이트**:
   - Hierarchy에서 TestRunner GameObject 선택
   - Inspector에서 기존 TestBootstrap 제거
   - Automation 폴더의 TestBootstrap 추가

### 최종 어셈블리 구조
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

---

## 문제 2: Input System 오류

### 증상
```
InvalidOperationException: You are trying to read Input using the UnityEngine.Input class,
but you have switched active Input handling to Input System package in Player Settings.
```

### 원인
**Legacy Input System 사용**
- EventSystem 생성 시 `StandaloneInputModule` 추가
- `StandaloneInputModule`은 구형 Input System을 사용
- 프로젝트 설정은 New Input System으로 설정됨
- 두 시스템 간 충돌 발생

**문제 코드**:
```csharp
var eventSystemObj = new GameObject("EventSystem");
eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
eventSystemObj.AddComponent<StandaloneInputModule>();  // Legacy 방식
```

### 해결 방법

**InputSystemUIInputModule 사용**

1. **using 문 추가**:
```csharp
using UnityEngine.InputSystem.UI;
```

2. **코드 수정**:
```csharp
// 수정 전
eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

// 수정 후
eventSystemObj.AddComponent<InputSystemUIInputModule>();
```

3. **어셈블리 참조 추가**:
```json
// NexonGame.Tests.Automation.asmdef
{
  "references": [
    "NexonGame.Runtime",
    "Unity.InputSystem"  // 추가 필요
  ]
}
```

### 전체 코드 예시
```csharp
private void EnsureEventSystemExists()
{
    var eventSystem = FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
    if (eventSystem == null)
    {
        var eventSystemObj = new GameObject("EventSystem");
        eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystemObj.AddComponent<InputSystemUIInputModule>();  // New Input System
        Debug.Log("[TestVisualizationRunner] EventSystem 생성 완료");
    }
}
```

---

## 문제 3: 직렬화 레이아웃 불일치

### 증상
```
A scripted object (probably NexonGame.Tests.PlayMode.TestVisualizationRunner?)
has a different serialization layout when loading.
(Read 32 bytes but expected 64 bytes)
Did you #if UNITY_EDITOR a section of your serialized properties in any of your scripts?
```

### 원인
**씬 파일의 직렬화 데이터 불일치**

Unity는 MonoBehaviour를 씬에 저장할 때 해당 클래스의 필드 정보를 직렬화합니다:
- 씬 파일에 필드 이름, 타입, 크기 정보 저장
- 코드 수정 시 필드 구조가 변경될 수 있음
- 씬 파일의 직렬화 데이터가 현재 코드와 불일치
- Unity가 역직렬화 시도 시 오류 발생

**실패한 해결 시도**:
1. ❌ 컴포넌트 제거 후 재추가 - 씬 파일에 이미 저장된 데이터 때문에 실패
2. ❌ GameObject 삭제 후 재생성 - 동일한 이유로 실패
3. ❌ 빌드 폴더 삭제 - 빌드 캐시와 무관한 문제
4. ❌ Git restore 후 재설정 - 씬 파일이 다시 오염됨

### 해결 방법

**TestBootstrap 패턴 사용**

핵심 아이디어: 씬에는 **직렬화 필드가 없는 단순한 부트스트랩**만 저장하고, 복잡한 컴포넌트는 **런타임에 동적 생성**

**TestBootstrap.cs 작성**:
```csharp
using UnityEngine;
using System.Reflection;

namespace NexonGame.Tests.Automation
{
    /// <summary>
    /// 테스트 부트스트랩 - TestVisualizationRunner를 런타임에 생성
    /// 직렬화 문제를 피하기 위해 씬에는 빈 스크립트만 배치
    /// </summary>
    public class TestBootstrap : MonoBehaviour
    {
        private void Awake()
        {
            Debug.Log("[TestBootstrap] 시작");

            // 1. TestVisualizationRunner를 동적으로 생성
            var testRunnerObj = new GameObject("TestVisualizationRunner");
            var testRunner = testRunnerObj.AddComponent<TestVisualizationRunner>();

            // 2. Reflection을 사용해서 _autoStart를 true로 설정
            var autoStartField = typeof(TestVisualizationRunner).GetField("_autoStart",
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (autoStartField != null)
            {
                autoStartField.SetValue(testRunner, true);
                Debug.Log("[TestBootstrap] _autoStart를 true로 설정");
            }

            // 3. 부트스트랩은 더 이상 필요 없으므로 제거
            Destroy(gameObject);

            Debug.Log("[TestBootstrap] TestVisualizationRunner 생성 완료");
        }
    }
}
```

### 장점
1. **직렬화 문제 회피**: TestBootstrap은 필드가 없어 직렬화 불일치 없음
2. **환경 일관성**: 에디터와 빌드 모두 동일하게 작동
3. **유연성**: 런타임 생성이므로 필드 수정에 자유로움
4. **깔끔함**: 부트스트랩 후 자동 제거

### 적용 방법
1. TestBootstrap.cs 작성
2. 씬에서 TestVisualizationRunner 제거
3. TestBootstrap 컴포넌트를 가진 GameObject 추가
4. 씬 저장 및 빌드

---

## 문제 4: NUnit 참조 오류

### 증상
```
error CS0246: The type or namespace name 'UnitySetUpAttribute' could not be found
error CS0246: The type or namespace name 'UnityTest' could not be found
```

### 원인
**빌드에 NUnit 프레임워크 미포함**

초기 시도: `defineConstraints`를 제거하여 TestVisualizationRunner를 빌드에 포함
```json
{
  "defineConstraints": []  // 제약 제거
}
```

**발생한 문제**:
- 제약 제거로 **모든 PlayMode 어셈블리 파일**이 빌드에 포함됨
- `StagePlayModeTests.cs`, `CombatPlayModeTests.cs` 등도 빌드 대상이 됨
- 이 파일들은 NUnit 속성(`[UnityTest]`, `[UnitySetUp]` 등) 사용
- 빌드에는 NUnit 프레임워크(`nunit.framework.dll`)가 포함되지 않음
- 컴파일 오류 발생

### 해결 방법

**어셈블리 책임 분리**

**원칙**:
- **PlayMode**: NUnit 테스트 전용 (에디터 전용, 빌드 제외)
- **Automation**: 빌드 가능한 자동화 코드 (빌드 포함)

**PlayMode 어셈블리 (빌드 제외)**:
```json
// NexonGame.Tests.PlayMode.asmdef
{
    "name": "NexonGame.Tests.PlayMode",
    "references": [
        "NexonGame.Runtime",
        "UnityEngine.TestRunner",
        "UnityEditor.TestRunner",
        "Unity.InputSystem"
    ],
    "overrideReferences": true,
    "precompiledReferences": [
        "nunit.framework.dll"  // NUnit 참조
    ],
    "defineConstraints": [
        "UNITY_INCLUDE_TESTS"  // 테스트 환경에서만 포함
    ]
}
```

**Automation 어셈블리 (빌드 포함)**:
```json
// NexonGame.Tests.Automation.asmdef
{
    "name": "NexonGame.Tests.Automation",
    "references": [
        "NexonGame.Runtime",
        "Unity.InputSystem"
    ],
    "overrideReferences": false,
    "precompiledReferences": [],  // NUnit 참조 없음
    "defineConstraints": []  // 제약 없음 - 항상 포함
}
```

### 폴더 구조
```
Assets/_Project/Scripts/Tests/
│
├── PlayMode/                          # 에디터 전용
│   ├── NexonGame.Tests.PlayMode.asmdef
│   ├── StagePlayModeTests.cs          # [UnityTest] 사용
│   ├── CombatPlayModeTests.cs         # [UnitySetUp] 사용
│   └── BlueArchiveIntegrationTests.cs # [UnityTearDown] 사용
│
└── Automation/                        # 빌드 포함
    ├── NexonGame.Tests.Automation.asmdef
    ├── TestVisualizationRunner.cs     # NUnit 미사용
    └── TestBootstrap.cs               # NUnit 미사용
```

### 검증 방법
```bash
# PlayMode 어셈블리 확인 (UNITY_INCLUDE_TESTS 있어야 함)
cat Assets/_Project/Scripts/Tests/PlayMode/NexonGame.Tests.PlayMode.asmdef

# Automation 어셈블리 확인 (defineConstraints 비어있어야 함)
cat Assets/_Project/Scripts/Tests/Automation/NexonGame.Tests.Automation.asmdef
```

---

## 요약

### 해결된 4가지 문제

| 문제 | 원인 | 해결 방법 |
|------|------|----------|
| UI 미표시 | `UNITY_INCLUDE_TESTS` 제약으로 빌드 제외 | Tests.Automation 어셈블리 분리 |
| Input System 오류 | Legacy InputModule 사용 | InputSystemUIInputModule 사용 |
| 직렬화 불일치 | 씬 파일의 직렬화 데이터 충돌 | TestBootstrap 패턴 (런타임 생성) |
| NUnit 참조 오류 | 빌드에 NUnit 미포함 | PlayMode/Automation 역할 분리 |

### 핵심 교훈

1. **어셈블리 제약 이해**: `defineConstraints`가 빌드 포함 여부를 결정
2. **직렬화 주의**: 복잡한 컴포넌트는 런타임 생성 고려
3. **Input System 일관성**: 프로젝트 설정과 코드 일치 필요
4. **책임 분리**: 테스트 코드와 프로덕션 코드 명확히 구분

---

## 참고 자료

- [Unity Assembly Definitions](https://docs.unity3d.com/Manual/ScriptCompilationAssemblyDefinitionFiles.html)
- [Unity New Input System](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/manual/index.html)
- [Unity Test Framework](https://docs.unity3d.com/Packages/com.unity.test-framework@1.1/manual/index.html)
