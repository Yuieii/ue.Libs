// Copyright (c) 2025 Yuieii.

namespace ue.Core.Tests
{
    public class Tests
    {
        private Mutex<List<int>, IReadOnlyList<int>, List<int>> _mutex;
    
        [SetUp]
        public void Setup()
        {
            _mutex = new Mutex<List<int>, IReadOnlyList<int>, List<int>>([1, 2, 3]);
        }

        [Test]
        public async Task Test1()
        {
            using var guard = await _mutex.AcquireExclusiveAsync();
            Console.WriteLine($"Value = {string.Join(", ", guard.Value.Select(v => v.ToString()))}");
        }

        [TearDown]
        public void Teardown()
        {
            _mutex.Dispose();
        }
    }
}