using Xunit;

namespace Store.Application.Tests.Common;

public class MapperProfilesTests
{
    [Fact]
    public void Configuration_ShouldBeValid()
    {
        // MapperFactory.Create() calls AssertConfigurationIsValid() internally —
        // this test just makes sure that assertion runs in CI and fails loudly if a mapping is broken.
        MapperFactory.Create();
    }
}
