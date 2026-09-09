# Contributing to UnifiedPos

Contributions are welcome. You can help by reporting printer compatibility, reproducing protocol issues, improving documentation, adding tests or submitting code.

## Before opening an issue

- Do not include printer passwords, API keys, private certificates or customer data.
- Include the printer manufacturer and model, connection type, target framework and operating system.
- Include a minimal receipt example and the exact exception or printer response.

## Pull requests

1. Create a focused branch from `main`.
2. Add or update tests for behavior changes.
3. Run `dotnet build UnifiedPos.slnx --configuration Release`.
4. Run `dotnet run --project tests/UnifiedPos.Tests --configuration Release`.
5. Explain the affected transport and printer models in the pull request.

By contributing, you agree that your contribution is licensed under the repository's MIT License.
