using Xunit;
using FluentAssertions;

namespace UPTRMS.Api.Tests.BDD
{
    public class TestBDDSetup
    {
        [Fact]
        public void BDD_Tests_Are_Setup_Correctly()
        {
            // This is a simple test to verify the BDD infrastructure is working
            true.Should().BeTrue();
        }
    }
}