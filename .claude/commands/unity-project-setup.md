---
allowed-tools: Read, Write, Edit, Bash
argument-hint: [프로젝트명] | --2d | --3d | --mobile | --vr | --console
description: 산업 표준 구조, 필수 패키지 및 플랫폼 최적화 구성으로 전문적인 Unity 게임 개발 프로젝트를 설정하기 위해 적극적으로 사용하세요
---

# Unity 프로젝트 설정 및 개발 환경

전문 Unity 게임 개발 프로젝트 초기화: $ARGUMENTS

## 현재 Unity 환경

- Unity 버전: !`unity-editor --version 2>/dev/null || echo "Unity Editor not found"`
- 현재 디렉토리: !`pwd`
- 사용 가능한 템플릿: !`find . -name "*.unitypackage" 2>/dev/null | wc -l` Unity 패키지
- Git 상태: !`git status --porcelain 2>/dev/null | wc -l` 커밋되지 않은 변경사항
- 시스템 정보: !`system_profiler SPSoftwareDataType | grep "System Version" 2>/dev/null || uname -a`

## 작업

전문 개발 환경 및 플랫폼별 최적화를 갖춘 완전한 Unity 프로젝트를 설정합니다.

## 생성되는 것:

### 프로젝트 구조
```
Assets/
├── _Project/
│   ├── Scripts/
│   │   ├── Managers/
│   │   ├── Player/
│   │   ├── UI/
│   │   ├── Gameplay/
│   │   └── Utilities/
│   ├── Art/
│   │   ├── Textures/
│   │   ├── Materials/
│   │   ├── Models/
│   │   └── Animations/
│   ├── Audio/
│   │   ├── Music/
│   │   ├── SFX/
│   │   └── Voice/
│   ├── Prefabs/
│   │   ├── Characters/
│   │   ├── Environment/
│   │   ├── UI/
│   │   └── Effects/
│   ├── Scenes/
│   │   ├── Development/
│   │   ├── Production/
│   │   └── Testing/
│   ├── Settings/
│   │   ├── Input/
│   │   ├── Rendering/
│   │   └── Audio/
│   └── Resources/
├── Plugins/
├── StreamingAssets/
└── Editor/
    ├── Scripts/
    └── Resources/
```

### 필수 패키지
- Universal Render Pipeline (URP)
- Input System
- Cinemachine
- ProBuilder
- Timeline
- Addressables
- Unity Analytics
- Version Control (사용 가능한 경우)

### 프로젝트 설정
- 타겟 플랫폼에 최적화된 품질 설정
- 입력 시스템 구성
- 물리 설정
- 시간 및 렌더링 구성
- 여러 플랫폼을 위한 빌드 설정

### 개발 도구
- 코드 포맷팅 규칙 (.editorconfig)
- Unity 최적화된 .gitignore가 포함된 Git 구성
- 더 나은 컴파일을 위한 Assembly definition 파일
- 워크플로우 개선을 위한 커스텀 에디터 스크립트

### 버전 관리 설정
- Git 저장소 초기화
- Unity 전용 .gitignore
- 대용량 에셋을 위한 LFS 구성
- 브랜칭 전략 문서

## 사용법:

```bash
npx claude-code-templates@latest --command unity-project-setup
```

## 대화형 옵션:

1. **프로젝트 타입 선택**
   - 2D 게임
   - 3D 게임
   - 모바일 게임
   - VR/AR 게임
   - 하이브리드 (2D/3D)

2. **타겟 플랫폼**
   - PC (Windows/Mac/Linux)
   - 모바일 (iOS/Android)
   - 콘솔 (PlayStation/Xbox/Nintendo)
   - WebGL
   - VR (Oculus/SteamVR)

3. **버전 관리**
   - Git
   - Plastic SCM
   - Perforce
   - 없음

4. **추가 패키지**
   - TextMeshPro
   - Post Processing
   - Unity Ads
   - Unity Analytics
   - Unity Cloud Build
   - 커스텀 패키지 선택

## 생성되는 파일:

### 핵심 스크립트
- `GameManager.cs` - 메인 게임 컨트롤러
- `SceneLoader.cs` - 씬 관리 시스템
- `AudioManager.cs` - 오디오 시스템 컨트롤러
- `InputManager.cs` - 입력 처리 시스템
- `UIManager.cs` - UI 시스템 매니저
- `SaveSystem.cs` - 저장/로드 기능

### 에디터 도구
- `ProjectSetupWindow.cs` - 커스텀 에디터 윈도우
- `SceneQuickStart.cs` - 씬 설정 자동화
- `AssetValidator.cs` - 에셋 검증 도구
- `BuildAutomation.cs` - 빌드 파이프라인 헬퍼

### 구성 파일
- `ProjectSettings.asset` - 최적화된 프로젝트 설정
- `QualitySettings.asset` - 멀티 플랫폼 품질 계층
- `InputActions.inputactions` - 입력 시스템 구성
- `AssemblyDefinitions` - 모듈식 컴파일 설정

### 문서
- `README.md` - 프로젝트 개요 및 설정 지침
- `CONTRIBUTING.md` - 개발 가이드라인
- `CHANGELOG.md` - 버전 히스토리 템플릿
- `API_REFERENCE.md` - 코드 문서 템플릿

## 설정 후 체크리스트:

- [ ] 타겟 플랫폼에 맞게 품질 설정 검토 및 조정
- [ ] 게임 컨트롤을 위한 입력 액션 구성
- [ ] 모든 타겟 플랫폼에 대한 빌드 구성 설정
- [ ] 폴더 구조 검토 및 필요 시 이름 변경
- [ ] 버전 관리 구성 및 초기 커밋 수행
- [ ] 필요한 경우 지속적 통합 설정
- [ ] 분석 및 충돌 보고 구성
- [ ] 코딩 표준 검토 및 사용자 정의

## 플랫폼별 구성:

### 모바일
- 터치 입력 구성
- 성능 최적화 설정
- 배터리 사용 최적화
- 앱 스토어 제출 설정

### PC
- 다중 해상도 지원
- 키보드/마우스 입력 설정
- 그래픽 옵션 메뉴 템플릿
- Windows/Mac/Linux 빌드 구성

### 콘솔
- 플랫폼별 입력 매핑
- 업적/트로피 통합 설정
- 온라인 서비스 구성
- 인증 요구 사항 템플릿

이 명령어는 프로토타입부터 출시 게임까지 확장 가능한 프로덕션급 Unity 프로젝트 구조를 생성하며, 산업 모범 사례 및 Unity의 권장 패턴을 따릅니다.