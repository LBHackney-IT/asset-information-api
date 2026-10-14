# PR Summary — .NET 10 Upgrade

Upgrades the solution from `net8.0` to `net10.0`. Scope is strictly limited to changes required to restore, build, run, and deploy on .NET 10. No general modernisation, refactoring, or dependency updates were performed.

---

## Changed

### Project files

| File | Change |
|------|--------|
| `AssetInformationApi/AssetInformationApi.csproj` | `TargetFramework`: `net8.0` → `net10.0` |
| `AssetInformationApi.Tests/AssetInformationApi.Tests.csproj` | `TargetFramework`: `net8.0` → `net10.0` |

### NuGet packages

| Package | Previous version | New version | Why required |
|---------|-----------------|-------------|--------------|
| `Microsoft.AspNetCore.Mvc.Versioning` | 5.1.0 | **removed** | Package was archived; ships no net9.0/net10.0-compatible assets — restore fails on net10.0 |
| `Microsoft.AspNetCore.Mvc.Versioning.ApiExplorer` | 5.1.0 | **removed** | Same as above |
| `Asp.Versioning.Mvc` | *(not present)* | 10.2.1 | Replacement for the removed package; the official successor maintained by the ASP.NET team |
| `Asp.Versioning.Mvc.ApiExplorer` | *(not present)* | 10.2.1 | Replacement for the removed ApiExplorer package |
| `Microsoft.AspNetCore.Mvc.Testing` | 8.0.11 | 10.0.0 | Framework-versioned package with a hard dependency on ASP.NET Core 8 runtime components; must match target framework |

All other packages are unchanged.

### Source code

| File | Change |
|------|--------|
| `AssetInformationApi/Startup.cs` | Replaced `using Microsoft.AspNetCore.Mvc.Versioning` and `using Microsoft.AspNetCore.Mvc.ApiExplorer` with `using Asp.Versioning` and `using Asp.Versioning.ApiExplorer`. Updated `AddApiVersioning()` registration to use the new fluent builder (`.AddMvc().AddApiExplorer()`); removed the now-unnecessary manual `services.AddSingleton<IApiVersionDescriptionProvider, DefaultApiVersionDescriptionProvider>()` which the new package registers automatically. |
| `AssetInformationApi/Versioning/ApiVersionExtensions.cs` | `using Microsoft.AspNetCore.Mvc` → `using Asp.Versioning` (type `ApiVersion` moved namespace) |
| `AssetInformationApi/Versioning/ApiVersionDescriptionExtensions.cs` | `using Microsoft.AspNetCore.Mvc.ApiExplorer` → `using Asp.Versioning.ApiExplorer` |
| `AssetInformationApi.Tests/Versioning/ApiVersionExtensionsTests.cs` | Same using swap as above |
| `AssetInformationApi.Tests/Versioning/ApiVersionDescriptionExtensionsTests.cs` | Same using swap; updated `ApiVersionDescription` constructor call — in `Asp.Versioning` the third parameter changed from `bool isDeprecated` to `SunsetPolicy?`; `false` (not deprecated) maps to omitting the parameter (`null`) |
| `AssetInformationApi/V1/Controllers/AssetInformationApiController.cs` | Added `using Asp.Versioning;` — `[ApiVersion("1.0")]` attribute uses `ApiVersion`/`ApiVersionAttribute` which moved from `Microsoft.AspNetCore.Mvc` to the `Asp.Versioning` namespace when the package was replaced |
| `AssetInformationApi/Program.cs` | Replaced `WebHost.CreateDefaultBuilder` / `IWebHostBuilder` pattern with `Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>())` — in .NET 10 `WebHost` and `IWebHostBuilder.Build()` are promoted from obsolete warnings to hard `ASPDEPR008` errors; `Startup.cs` is unchanged |

### Docker

| File | Change |
|------|--------|
| `AssetInformationApi/Dockerfile` | Base image `mcr.microsoft.com/dotnet/sdk:8.0` → `mcr.microsoft.com/dotnet/sdk:10.0` |
| `AssetInformationApi.Tests/Dockerfile` | Base image `mcr.microsoft.com/dotnet/sdk:8.0` → `mcr.microsoft.com/dotnet/sdk:10.0` |

### Deployment

| File | Change |
|------|--------|
| `AssetInformationApi/serverless.yml` | `runtime: dotnet8` → `runtime: dotnet10`; artifact path `net8.0/` → `net10.0/` |
| `AssetInformationApi/build.sh` | `--framework net8.0` → `--framework net10.0`; output path `net8.0/` → `net10.0/`; removed `--version 4.0.0` pin from `dotnet tool install Amazon.Lambda.Tools` (v4 does not recognise `--framework net10.0`) |
| `AssetInformationApi/build.cmd` | `--framework net8.0` → `--framework net10.0`; output path `net8.0/` → `net10.0/` |

### CI

| File | Change |
|------|--------|
| `.circleci/config.yml` | `docker-dotnet` executor image `mcr.microsoft.com/dotnet/sdk:8.0` → `mcr.microsoft.com/dotnet/sdk:10.0` |

---

## Validation

> Build and test results to be filled in after running `dotnet restore`, `dotnet build`, and `dotnet test` with the .NET 10 SDK installed.

**Pre-conditions:**
- .NET 10 SDK must be installed locally (this machine currently has 8.0.426 only)
- CI executor image `mcr.microsoft.com/dotnet/sdk:10.0` must be available in the CircleCI environment

**Steps to validate:**
1. `dotnet restore AssetInformationApi.sln`
2. `dotnet build AssetInformationApi.sln -c Release`
3. `dotnet test`
4. Docker builds: `docker build -f AssetInformationApi/Dockerfile .` and test Dockerfile
5. Lambda packaging: `cd AssetInformationApi && ./build.sh`

---

## Explicitly out of scope

The following were discovered during the migration but deliberately left unchanged as they are unrelated to the .NET 10 upgrade:

- **Swashbuckle version mismatch** — `Swashbuckle.AspNetCore` 5.4.1 vs `SwaggerUI` 6.9.0 is a pre-existing inconsistency; left unchanged as the application was building on net8.0 with this state
- **FluentValidation.AspNetCore 10.3.3** — deprecated in favour of v11+, but the existing version ships netstandard2.0/net7.0 assets that are compatible under net10.0; no upgrade required
- **General NuGet package updates** — xunit 2.4.1, FluentAssertions 5.10.3, AutoFixture 4.17.0, Moq 4.16.1, coverlet.msbuild 3.0.3, AWSXRayRecorder 2.x, Newtonsoft.Json 13.0.3, and others were not updated; newer versions exist but there is no compatibility requirement for the framework upgrade
- **`Microsoft.CodeAnalysis.NetAnalyzers` 9.0.0** — left at current version; upgrade to 10.0.0 only if build fails with `TreatWarningsAsErrors=true`
- **`Amazon.Lambda.AspNetCoreServer` 5.1.1** — left at current version; upgrade only if restore fails
- **`check-code-formatting` CircleCI job** — uses `dotnet tool install dotnet-format` which is redundant in .NET 10 (built-in); left unchanged as it is a pre-existing issue unrelated to the framework version
- **`global.json`** — not added; no SDK pinning file was present before and none is required by the upgrade itself
