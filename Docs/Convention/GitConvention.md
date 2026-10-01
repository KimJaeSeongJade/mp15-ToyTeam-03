# Git 협업 규칙


## 1. 기본 원칙

모든 개발 작업은 아래 흐름을 기준으로 진행한다.

1. Issue 생성 → Todo → 작업 시작 →  
2. In Progress → Branch 생성 및 작업 → 
3. Pull Request → Review → Merge → 
4. Done + Issue Close

GitHub Projects와 Issue, Pull Request는 서로 연결하여 관리되며, 
1번 과정에서 작업할 내용을 정리하여 선언
2번 과정에서 선언한 내용을 바탕으로 브랜치를 생성하여 작업 진행
3번 과정에서 작업이 마무리 되면 PR 요청을 진행 후 Review와 Merge 진행
4번 과정에서 문제가 없는 경우 병합을 진행하여 Done + Issue Close 처리로 작업 완료

---

## 2. 기본 Branch

프로젝트의 기본 개발 흐름은 다음과 같다.

main\
└─ main-develop\
    └─ 작업 Branch

- `main`은 최종 결과물을 관리한다.
- 실제 개발 작업은 `main-develop`을 기준으로 진행한다.
- 개인 작업은 `main-develop`에서 새로운 작업 Branch를 생성하여 진행한다.
- `main`, `main-develop`에서 직접 기능 개발을 진행하지 않는다.

---

## 3. Issue

모든 작업은 Issue 생성부터 시작한다.

Issue 작성 시 아래 내용을 확인한다.

- 하나의 작업 단위로 작성한다.
- 제목만 보고 작업 내용을 알 수 있도록 작성한다.
- 담당자(Assignee)를 반드시 지정한다.

Issue가 생성되면 Projects Board의 `Todo` 상태로 관리한다.

작업을 시작하면 `In Progress`로 변경한다.

### Sub Issue



---

## 4. Branch 생성

작업 Branch는 `main-develop`을 기준으로 생성한다.

가능하면 Issue의 `Development → Create a branch` 기능을 사용하여
Issue와 Branch를 연결한다.

Branch 이름은 다음 형식을 따른다.

`type/작업자이니셜/작업내용`

예시:

`feat/yr1103/monster-movement`

`fix/yr1103/tower-targeting`

`docs/yr1103/unity-convention`

### Branch type

| Type | 의미 |
|---|---|
| feat | 기능 구현 |
| fix | 버그 수정 |
| refactor | 리팩토링 |
| docs | 문서 작업 |
| test | 테스트 |

>### 작업 내용은 가능한 한 짧고 명확하게 작성하며, 여러 단어는 `-`로 구분한다.

---

## 5. Commit 규칙

Commit은 의미 있는 작업 단위로 나누어 작성한다.

형식:

`[Tag] 작업 내용`

예시:

`[Feat] 몬스터 이동 구현`

`[Fix] 타워 타겟팅 오류 수정`

`[Docs] Unity 컨벤션 추가`

### Commit Tag

| Tag | 사용 |
|---|---|
| Feat | 기능 추가 |
| Fix | 버그 수정 |
| Refactor | 코드 리팩토링 |
| Docs | 문서 수정 |
| Rename | 파일 또는 폴더 이름 변경 |
| Remove | 파일 삭제 |
| Comment | 주석 추가 및 수정 |
| Test | 테스트 |

필요한 경우 Commit 본문에 세부 내용을 추가한다.

---

## 6. Pull Request

작업 단위가 완료되면 Pull Request를 생성한다. (이하 `PR`)

`PR`의 `Base Branch`는 `main-develop`으로 설정한다.

`PR` 본문에는 반드시 연결할 Issue 번호를 작성한다.

예:`Closes #12`

여러 Issue를 연결하는 경우:

`Closes #12, Closes #15`

`PR`과 Issue가 연결되면 Projects에서 해당 작업의 상태가
`TODO` → `In Progress`로 관리된다.

---

## 7. PR 작성 내용

PR에는 아래 내용을 작성한다.

### 무엇을 했나요?

구현하거나 수정한 기능을 작성한다.

관련 `Script, Prefab, Scene`이 있다면 함께 작성한다.

### 왜 했나요?

해당 작업이 필요한 이유를 작성한다.

### 확인한 내용

- 실행 여부
- Console Error 여부
- 기능 정상 동작 여부
- 관계없는 파일 포함 여부
- Scene / Prefab 변경 여부




### 작성 방법 및 유의사항

위 양식에 따라 PR을 작성한다.
다른 팀원이 사용해야 하는 코드 또는 기능이라면
사용 방법을 스크린샷 등을 활용하여 작성한다.

확인이 필요한 사항 또는 주의 사항을 작성한다.

필요한 팀원은 `@멘션`으로 호출한다.

---

## 8. Review

PR이 올라오면 아래 인원이 우선적으로 확인한다.

- 해당 기능과 직접 관련된 작업자
- 팀장
- 팀장이 확인하기 어려운 경우 PM

Review 시 다음 사항을 확인한다.

- 다른 기능에 영향을 주는 변경이 있는지
- Scene / Prefab 변경이 있는지
- 관계없는 파일이 포함되어 있지 않은지
- 기능이 정상적으로 동작하는지
- 이해하기 어려운 코드가 있는지

일반적인 의견은 `Comment`로 작성한다.

반드시 수정해야 할 내용은 `Request changes`로 작성한다.

---

## 9. Merge

Review가 완료된 뒤 Merge 여부를 확정한다.

Merge는 관련 작업자와 팀장 확인 후 진행한다.

팀장 확인이 어려운 경우 PM과 상의하여 결정한다.

PR이 기본 Branch에 Merge되면 GitHub 자동화에 의해:

- 연결된 Issue가 Close된다.
- Projects 상태가 `Done`으로 변경된다.

따라서 Merge 이후 Issue Close와 Done 변경은 수동으로 하지 않는다.


### Merge 주의사항

Merge 진행 시에 `main-develop` 브랜치로 병합을 진행하려는 것인지 확인한다.


### Merge 양식

Merge하는 경우 제목은 다음과 같이 설정한다.

> `Merge branch 'main-develop' ← '본인이 작업한 브랜치명'`
> 
> 세부내용 : 작업 중에 Issue 작업 당시와 변경점이 있다면 작성해주세요.

---

## 10. 다시 작업해야 하는 경우

완료된 작업에 다시 수정이 필요한 경우나 잘못 완료한 작업의 경우
닫힌 Issue에서 `Reopen issue`를 사용한다.

Issue를 Reopen하면 Projects 상태가 다시 `In Progress`로 변경된다.

Projects Board에서 Done 카드를 직접 다른 상태로 옮기기만 하면
Issue는 닫힌 상태로 남을 수 있으므로,
재작업 시에는 반드시 Issue에서 Reopen한다.