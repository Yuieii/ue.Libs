// Copyright (c) 2025 Yuieii.

namespace ue.Core.Tests
{
    [TestFixture]
    public class EitherTests
    {
        [Test]
        public void TestSwap()
        {
            var x = Either.Left(12).FulfillRightType<string>();
            var y = x.Swap();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(x.UnwrapLeft(), Is.EqualTo(12));
                Assert.That(y.UnwrapRight(), Is.EqualTo(12));
                Assert.That(x.Left.IsSome, Is.True);
                Assert.That(y.Left.IsNone, Is.True);   
            }
        }
    }
}