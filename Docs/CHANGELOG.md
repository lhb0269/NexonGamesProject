# 변경 이력

모든 주요 변경사항은 이 파일에 문서화됩니다.

형식은 [Keep a Changelog](https://keepachangelog.com/ko/1.0.0/)를 기반으로 하며,
이 프로젝트는 [Semantic Versioning](https://semver.org/lang/ko/)을 준수합니다.

## [Unreleased]

### 추가됨
- 의존성 주입(DI) 패턴 기반 프로젝트 구조 구축
- ServiceLocator를 통한 서비스 관리 시스템
- GameBootstrapper를 통한 게임 초기화 시스템
- 핵심 매니저 시스템:
  - AudioManager: 오디오 재생 및 관리
  - SceneLoader: 씬 로딩 및 전환
  - InputManager: Unity Input System 통합
  - UIManager: UI 패널 관리
- 에디터 도구:
  - ProjectSetupWindow: 프로젝트 설정 도구
  - SceneQuickStart: 씬 빠른 전환
  - AssetValidator: 에셋 검증 도구
- Assembly Definition 파일로 컴파일 최적화
- .editorconfig를 통한 코드 스타일 통일
- 프로젝트 문서 (README.md, CONTRIBUTING.md, CHANGELOG.md)
- 체계적인 폴더 구조 (_Project 기반)

### 변경됨
- 싱글톤 패턴 대신 DI 패턴 사용으로 설계 개선

## [0.1.0] - 2025-01-31

### 추가됨
- Unity 6000.2.9f1 기반 프로젝트 생성
- Universal Render Pipeline (URP) 설정
- Unity Input System 통합
- 기본 씬 구조 생성
