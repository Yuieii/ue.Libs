// Copyright (c) 2026 Yuieii.

namespace ue.Core.Tests
{
    [TestFixture]
    public class SingletonTests
    {
        private class InferredSingleton : ISingleton<InferredSingleton>
        {
            public int Value { get; private init; } = 24;

            public static InferredSingleton Instance { get; } = new()
            {
                Value = 57
            };
        }
        
        private class ExplicitSingleton : ISingleton<ExplicitSingleton>
        {
            public int Value { get; private init; } = 249;

            static ExplicitSingleton ISingleton<ExplicitSingleton>.Instance { get; } = new()
            {
                Value = 573
            };
        }
        
        [Test]
        public void TestInferFromInterface()
        {
            var inferred = Singleton<InferredSingleton>.Instance;
            Assert.That(inferred.Value, Is.EqualTo(57));
            
            var exp = Singleton<ExplicitSingleton>.Instance;
            Assert.That(exp.Value, Is.EqualTo(573));
        }
    }
}