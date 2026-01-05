// Copyright (c) 2025 Yuieii.

namespace ue.Core.Tests
{
    public class DependencyContainerTests
    {
        [Test]
        public void TestGetService()
        {
            var container = Singleton<DependencyContainer>.Instance;
            container.Register<IReadOnlyList<int>>([1, 2, 3]);

            using (Assert.EnterMultipleScope())
            {
                var service = container.TryGet<IReadOnlyList<int>>();
                Assert.That(service.IsSome, Is.True);

                var unwrapped = service.Unwrap();
                Assert.That(unwrapped, Is.EquivalentTo([1, 2, 3]));

                var notExist = container.TryGet<List<int>>();
                Assert.That(notExist.IsSome, Is.False);
            }
        }
    }
}