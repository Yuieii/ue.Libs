// Copyright (c) 2025 Yuieii.

namespace ue.Core.Tests
{
    [TestFixture]
    public class MutexTests
    {
        [Test]
        public void TestAcquireShared()
        {
            var mutex = new Mutex<int>(1);
            using var l = mutex.AcquireShared();
            Assert.That(l.Value, Is.EqualTo(1));

            var thread = new Thread(() =>
            {
                var r = mutex.AcquireShared();
                Assert.That(r.Value, Is.EqualTo(1));
            });
            thread.Start();
            thread.Join();
        }

        [Test]
        public void TestAcquireContainer()
        {
            var mutex = new Mutex<int>(1);

            using (var l = mutex.AcquireContainer())
            {
                l.Value = 2;
                Assert.That(l.Value, Is.EqualTo(2));
            }

            using (var l = mutex.AcquireShared())
            {
                Assert.That(l.Value, Is.EqualTo(2));
            }
        }

        [Test]
        public void TestAcquireExclusive()
        {
            var mutex = new Mutex<int>(1);
            using var l = mutex.AcquireContainer();
            l.Value = 2;

            using (Assert.EnterMultipleScope())
            {
                Assert.That(l.Value, Is.EqualTo(2));
                Assert.That(mutex.TryAcquireShared().IsError, Is.True);
            }
        }

        [Test]
        public void TestTryAcquireExclusive()
        {
            var mutex = new Mutex<int>(1);
            using var l = mutex.AcquireShared();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(l.Value, Is.EqualTo(1));
                Assert.That(mutex.TryAcquireExclusive().IsError, Is.True);
            }
        }

        [Test]
        public void TestSelect()
        {
            var mutex = new Mutex<int>(1);
            using var l = mutex.AcquireShared()
                .Select(x => x * 2);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(l.Value, Is.EqualTo(2));
                Assert.That(mutex.TryAcquireExclusive().IsError, Is.True);
            }
        }
    }
}