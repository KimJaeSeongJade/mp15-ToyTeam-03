# 1. 코드 스타일

수업 교본에서 사용한 표기법을 기본으로 따른다.

| 대상 | 표기법 | 예시 |
|---|---|---|
| 지역 변수 | camelCase | `currentHealth` |
| 클래스 | PascalCase | `PlayerController` |
| 메서드 | PascalCase | `TakeDamage()` |
| 프로퍼티 | PascalCase | `MaxHealth` |
| private 필드 | _ + camelCase | `_currentHealth` |
| 상수 | UPPER_SNAKE_CASE | `MAX_LEVEL` |

## 접근 제한자

접근 제한자는 `private`을 기본으로 한다.

외부에서 값 확인이 필요한 경우 가능한 한 프로퍼티를 사용한다.

상속 관계에서 자식 클래스가 접근해야 하는 경우 `protected` 사용을 고려한다.

불필요한 `public` 사용을 최소화한다.

외부에서 값을 확인해야 하는 경우에는 필드를 직접 `public`으로 공개하기보다
프로퍼티를 통해 접근하도록 한다.

기본 형태는 다음과 같다.

```csharp
private int _currentHealth;

public int CurrentHealth
{
    get { return _currentHealth; }
}

외부에서 값을 읽기만 하면 되는 경우에는 get만 제공한다.

외부에서 값을 직접 변경해야 하는 경우에만 set을 추가한다.
private int _currentHealth;

public int CurrentHealth
{
    get { return _currentHealth; }
    set { _currentHealth = value; }
}

set 안에서 외부에서 전달된 값은 value라는 이름으로 사용할 수 있다.
public int CurrentHealth
{
    get { return _currentHealth; }
    set
    {
        _currentHealth = value;
    }
}

프로퍼티 사용 시 기본 원칙은 다음과 같다.
- 단순히 외부에서 값을 확인할 목적이라면 get만 사용한다.
- 외부에서 값을 변경해야 할 이유가 명확한 경우에만 set을 사용한다.
- 불필요하게 public 필드를 사용하지 않는다.
- 값 변경 시 추가 검사가 필요한 경우 set 내부에서 처리할 수 있다.
예:
private int _currentHealth;

public int CurrentHealth
{
    get { return _currentHealth; }
    set
    {
        if (value < 0)
        {
            _currentHealth = 0;
            return;
        }

        _currentHealth = value;
    }
}

외부에서 직접 수정할 필요가 없는 값은 읽기 전용 프로퍼티로 유지한다.