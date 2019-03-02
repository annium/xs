using Annium.Testing;
using static Annium.Testing.Asserts;

namespace {{name}}
{
    public class SampleTest
    {
        [Fact]
        public void True_IsTrue()
        {
            // arrange
            var value = true;

            // assert
            Assert(value).IsTrue();
        }
    }
}