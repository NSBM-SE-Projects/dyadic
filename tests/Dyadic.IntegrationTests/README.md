# Dyadic.IntegrationTests

Integration tests split into two categories:

| Category | Provider | Speed | Purpose |
|---|---|---|---|
| `InMemory` | EF Core InMemory | Fast | Relationship mapping, round-trips |
| `SqlServer` | Real SQL Server | Slower | Schema enforcement, cascades, migrations |

---

## Running InMemory tests (no setup needed)

```bash
dotnet test tests/Dyadic.IntegrationTests --filter "Category=InMemory"
```

---

## Running SqlServer tests locally

**Prerequisites:**
1. Docker running with the SQL Server container:
   ```bash
   make up
   ```
2. Set the connection string environment variable:
   ```bash
   # macOS / Linux
   export ConnectionStrings__DefaultConnection="Server=localhost,1433;Database=Dyadic_CI;User Id=sa;Password=StrongPassword123@;TrustServerCertificate=True"

   # Windows PowerShell
   $env:ConnectionStrings__DefaultConnection="Server=localhost,1433;Database=Dyadic_CI;User Id=sa;Password=StrongPassword123@;TrustServerCertificate=True"
   ```
   Replace the password with the value from your `.env` file (`MSSQL_SA_PASSWORD`).

3. Run:
   ```bash
   dotnet test tests/Dyadic.IntegrationTests --filter "Category=SqlServer"
   ```

Each SQL Server test creates its own isolated database (`Dyadic_Test_<guid>`) and drops it after the test completes. Tests are independent and safe to run in parallel.

---

## Running all integration tests

```bash
dotnet test tests/Dyadic.IntegrationTests
```

---

## CI

The CI pipeline runs both categories sequentially:
1. InMemory tests first (fast signal, no external deps)
2. SqlServer tests second (schema fidelity, requires the mssql service container)

See `.github/workflows/ci.yml` for details.
