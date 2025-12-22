// Copyright (c) 2025 Yuieii.

namespace ue.Core.Tests
{
    file static class TestExtensions
    {
        public static Option<uint> CheckedSub(this uint val, uint sub)
        {
            try
            {
                return Option.Some(checked(val - sub));
            }
            catch (OverflowException)
            {
                return Option.None;
            }
        }
    }

    public class ResultTests
    {
        [Test]
        public void TestProcess()
        {
            var list = new List<uint> { 3, 2, 1, 10 };
            var shared = uint.MinValue;
            var res = list
                .Select(x =>
                {
                    shared += x;
                    return x.CheckedSub(2).ToSuccessOr("Underflow!");
                })
                .Process();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(res, Is.EqualTo(Result<IEnumerable<uint>, string>.Error("Underflow!")));
                Assert.That(shared, Is.EqualTo(6));
            }
        }
    }
}