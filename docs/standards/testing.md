# Testing Standards: Widget Depot

This document describes the testing conventions and NuGet packages to use in the Widget Depot solution.

---

## Overall Approach

- Unit tests should focus on observable behavior.
- Keep the number of assertions per test to a minimum.

---

## NuGet Packages

| Package | Purpose |
|---|---|
| `xunit` | Test framework |
| `xunit.runner.visualstudio` | Test runner integration |
| `Moq` | Mocking dependencies |
| `Shouldy` | Readable assertion syntax |

---

## Test Project

All tests live in `WidgetDepot.Tests` under the `tests/` folder. The folder structure mirrors the `Features/` folder structure of the main application:

```
tests/
└── WidgetDepot.Tests/
    └── Features/
        ├── Catalog/
        ├── Accounts/
        └── ...
```

---

## Conventions

- Use **Moq** to mock dependencies (services, repositories, etc.) passed into the class under test.
- Use **Shouldly** for all assertions — prefer `result.ShouldBe(expected)` over `Assert.Equal`.
- Name test methods using the pattern: `MethodName_Scenario_ExpectedBehavior`.
- Each test class should test one unit (one class or handler).
- Arrange/Act/Assert structure — separate each section with a blank line.
