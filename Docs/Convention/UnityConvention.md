# Unity 프로젝트 규칙

## 1. 기본 폴더 구조

현재 프로젝트의 Assets 폴더 구조는 다음과 같다.

Assets
├─ Animations
├─ Fonts
├─ Materials
├─ Prefabs
├─ Scenes
├─ Scripts
└─ TextMesh Pro
---
┌─ Documentation
├─ Fonts
├─ Resources
├─ Shaders
└─ Sprites  

===

각 작업자는 업무일지에 지정된 본인 담당 업무를 기준으로
해당하는 폴더에서 작업을 진행한다.

본인 업무와 직접 관련되지 않은 폴더 또는 파일은
사전 협의 없이 임의로 수정하지 않는다.

---

## 2. 폴더별 사용

Animations
└─ Animation Clip, Animator 관련 파일

Fonts
└─ 프로젝트에서 사용하는 Font

Materials
└─ Material 관련 파일

Prefabs
└─ 프로젝트에서 사용하는 Prefab

Scenes
└─ Unity Scene

Scripts
└─ C# Script

TextMesh Pro
└─ TextMesh Pro 관련 기본 리소스

필요한 하위 폴더가 새로 필요한 경우
팀원과 공유 후 생성한다.

---

## 3. Scene 규칙

- Scene은 담당자를 기준으로 작업한다.
- 다른 작업자가 사용 중인 Scene은 임의로 수정하지 않는다.
- 다른 작업자의 Scene 수정이 필요한 경우 먼저 해당 작업자와 협의한다.
- 공용 Scene 변경 시 변경 내용을 PR에 반드시 작성한다.

---

## 4. Prefab 규칙

- 본인 업무와 관련된 Prefab을 기준으로 작업한다.
- 다른 작업자의 Prefab을 사전 협의 없이 수정하지 않는다.
- 여러 기능에서 함께 사용하는 Prefab은 공용 파일로 취급한다.
- 공용 Prefab 수정이 필요한 경우 관련 작업자와 먼저 상의한다.
- Prefab 변경 사항은 PR에 명시한다.

---

## 5. Unity 파일 관리

- `.meta` 파일은 임의로 삭제하거나 수정하지 않는다.
- 본인 작업과 관계없는 파일을 Commit하지 않는다.
- `ProjectSettings` 변경이 필요한 경우 팀원에게 공유한다.
- `Packages` 변경이 필요한 경우 팀원에게 공유한다.
- 새로운 최상위 폴더를 임의로 생성하지 않는다.

폴더 구조 변경이 필요한 경우
팀원과 상의 후 적용한다.