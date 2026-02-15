# Changelog

## [Unreleased]

Target: NServiceBus 10.x | .NET 10.0

- Support NServiceBus 10.x
- Target net10.0
- Replace custom `RateGate` with .NET built-in `TokenBucketRateLimiter` (`System.Threading.RateLimiting`), fixing thread safety issues and improving performance
- Propagate pipeline `CancellationToken` to the rate limiter for clean endpoint shutdown
- Use `Stopwatch.GetElapsedTime()` instead of allocating `Stopwatch` instances
- Code cleanup: simplify Dispose, modern argument validation, extract `ApplyConfiguration` helper
- Remove redundant `Microsoft.SourceLink.GitHub` (built into .NET 8+ SDK)
- Modernize GitHub Actions CI workflow (actions v4, .NET 10.x, pack on tag, publish via dispatch)
- Add justfile for build, pack, and publish workflows

### Dependencies

- NServiceBus [10.0.0, 11.0.0)
- System.Threading.RateLimiting 8.0.0
- MinVer 7.0.0

## [4.0.0] - 2024-06-14

Target: NServiceBus 9.x | .NET 8.0

- Support NServiceBus 9.x

### Dependencies

- MinVer 5.0.0
- Microsoft.SourceLink.GitHub 8.0.0

## [3.1.0] - 2023-07-16

- Add `StartDurationThreshold` configuration to control throttle delay warnings
- Improved messages with guidance
- Add demo project
- Add CI workflow

### Dependencies

- Microsoft.SourceLink.GitHub 1.1.1

## [3.0.0] - 2022-12-09

Target: NServiceBus 8.x

- Support NServiceBus 8.x

## [2.1.0] - 2020-05-23

- Add `ApplyRateLimiting(this EndpointConfiguration, int)` overload for simple rate per second

## [2.0.0] - 2019-09-24

Target: NServiceBus 7.x

- Support NServiceBus 7.x

## [1.0.4] - 2017-11-25

- Fix feature not being enabled

## [1.0.3] - 2017-11-24

- Fix version restriction to NServiceBus 6.x only
- Add logging of configuration values

## [1.0.2] - 2017-11-23

- Multi-target net452 and .NET Standard 2.0

## [1.0.1] - 2017-11-19

- Fix automatic interval calculation

## [1.0.0] - 2017-11-19

Target: NServiceBus 6.x

- Initial release

[Unreleased]: https://github.com/ramonsmits/NServiceBus.RateLimiter/compare/4.0.0...HEAD
[4.0.0]: https://github.com/ramonsmits/NServiceBus.RateLimiter/compare/3.1.0...4.0.0
[3.1.0]: https://github.com/ramonsmits/NServiceBus.RateLimiter/compare/3.0.0...3.1.0
[3.0.0]: https://github.com/ramonsmits/NServiceBus.RateLimiter/compare/2.1.0...3.0.0
[2.1.0]: https://github.com/ramonsmits/NServiceBus.RateLimiter/compare/2.0.0...2.1.0
[2.0.0]: https://github.com/ramonsmits/NServiceBus.RateLimiter/compare/1.0.4...2.0.0
[1.0.4]: https://github.com/ramonsmits/NServiceBus.RateLimiter/compare/1.0.3...1.0.4
[1.0.3]: https://github.com/ramonsmits/NServiceBus.RateLimiter/compare/1.0.2...1.0.3
[1.0.2]: https://github.com/ramonsmits/NServiceBus.RateLimiter/compare/1.0.1...1.0.2
[1.0.1]: https://github.com/ramonsmits/NServiceBus.RateLimiter/compare/1.0.0...1.0.1
[1.0.0]: https://github.com/ramonsmits/NServiceBus.RateLimiter/releases/tag/1.0.0
