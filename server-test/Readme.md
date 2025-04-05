# RSP Backend Server Tests

### Commands

Restore and run test
```bash
./scripts/runtest.sh
```

Restore all packages when the main source files are updated.

```bash
dotnet restore --force --no-cache && dotnet build
```
