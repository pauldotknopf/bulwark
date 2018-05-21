using Xunit;

namespace Bulwark.Tests
{
    public static class AssertExtensions
    {
        public static void ShouldBeEqualTo(this object obj, object value)
        {
            Assert.Equal(value, obj);
        }

        public static void ShouldBeTrue(this bool value)
        {
            Assert.True(value);
        }

        public static void ShouldBeFalse(this bool value)
        {
            Assert.False(value);
        }
    }
}