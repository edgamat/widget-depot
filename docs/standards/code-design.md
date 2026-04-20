# Coding Design Standards: Widget Depot

This document describes the approach and conventions for code design in the Widget Depot Solution

---

## Approach

- Attempt to adhere to the SOLID principles

| Principle | Summary |
| --------- | ------- |
| S - Single Responsibility | One class, one reason to change |
| O - Open/Closed | Open for extension, closed for modification |
| L - Liskov Substitution | Subtypes must be substitutable for base types |
| I - Interface Segregation | Many specific interfaces > one general interface |
| D - Dependency Inversion | Depend on abstractions, not concretions |

- Keep the number of dependencies of a class to a minimum. Write unit tests to guide your work. If a class is hard to test, it probably needs to be changed. 

- Prefer pure functions/methods where possible. Try to keep the side-affects out of the core business logic. This typically leads to better design and makes things easier to test.

## C# Specifics

- Use file-scoped namespaces
- Don't use primary constructors
- Don't use the null-forgiving operator (!) where possible (obey the compiler).
- All methods that return an awaitable task should include an `Async` suffix in their name. Note, this does not apply to unit test methods
- Remove unused using directives
