using Xunit;

namespace UPTRMS.Api.Tests.BDD;

/// <summary>
/// Test collection for BDD tests that need to run sequentially.
/// This prevents parallel execution which can cause issues with shared state in MockAuthenticationService.
/// </summary>
[CollectionDefinition("BDD_Sequential_Tests", DisableParallelization = true)]
public class BDDSequentialTestCollection
{
    // This class has no code, and is never created.
    // Its purpose is to be the place to apply [CollectionDefinition]
}