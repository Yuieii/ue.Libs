// Copyright (c) 2025 Yuieii.

namespace ue.Core.Tests
{
    [TestFixture]
    public class OptionTests
    {
        [Test]
        public void TestIsSome()
        {
            var x = Option.Some(2);
            Assert.That(x.IsSome, Is.True);

            var y = Option<int>.None;
            Assert.That(y.IsSome, Is.False);
            
            var z = 3.ToOption();
            Assert.That(z.IsSome, Is.True);
        }

        [Test]
        public void TestIsNone()
        {
            var x = Option.Some(2);
            Assert.That(x.IsNone, Is.False);

            var y = Option<int>.None;
            Assert.That(y.IsNone, Is.True);
        }

        [Test]
        public void TestUnwrap()
        {
            var x = Option.Some("air");
            Assert.That(x.Unwrap(), Is.EqualTo("air"));
        }

        [Test]
        public void TestUnwrapThrows()
        {
            var x = Option<string>.None;
            Assert.Throws<InvalidOperationException>(() => x.Unwrap());
        }

        [Test]
        public void TestOr()
        {
            {
                var x = Option.Some(2);
                var y = Option<int>.None;
                Assert.That(x.Or(y), Is.EqualTo(Option.Some(2)));
            }

            {
                var x = Option<int>.None;
                var y = Option.Some(100);
                Assert.That(x.Or(y), Is.EqualTo(Option.Some(100)));
            }

            {
                var x = Option.Some(2);
                var y = Option.Some(100);
                Assert.That(x.Or(y), Is.EqualTo(Option.Some(2)));
            }

            {
                var x = Option<int>.None;
                var y = Option<int>.None;
                Assert.That(x.Or(y), Is.EqualTo(Option<int>.None));
            }
        }

        [Test]
        public void TestOrGet()
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(Option.Some("barbarians").OrGet(Vikings), Is.EqualTo(Option.Some("barbarians")));
                Assert.That(Option<string>.None.OrGet(Vikings), Is.EqualTo(Option.Some("vikings")));
                Assert.That(Option<string>.None.OrGet(Nobody), Is.EqualTo(Option<string>.None));
            }

            return;

            Option<string> Nobody() => Option.None;
            Option<string> Vikings() => Option.Some("vikings");
        }

        [Test]
        public void TestOrElse()
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(Option.Some("car").OrElse("bike"), Is.EqualTo("car"));
                Assert.That(Option<string>.None.OrElse("bike"), Is.EqualTo("bike"));
            }
        }

        [Test]
        public void TestOrElseGet()
        {
            var k = 10;

            using (Assert.EnterMultipleScope())
            {
                Assert.That(Option.Some(4).OrElseGet(() => 2 * k), Is.EqualTo(4));
                Assert.That(Option<int>.None.OrElseGet(() => 2 * k), Is.EqualTo(20));
            }
        }

        [Test]
        public void TestCast()
        {
            var list = Option.Some(new List<int> { 1, 2, 3 });
            var boxed = list.Cast<IReadOnlyList<int>>();
            Assert.That(boxed.IsSome, Is.True);

            var upcast = boxed.Cast<List<int>>();
            Assert.That(upcast.IsSome, Is.True);

            var invalid = boxed.Cast<int[]>();
            Assert.That(invalid.IsSome, Is.False);
        }

        [Test]
        public void TestSelect()
        {
            var maybeSomeString = Option.Some("Hello, World!");
            var maybeSomeLength = maybeSomeString.Select(s => s.Length);
            Assert.That(maybeSomeLength, Is.EqualTo(Option.Some(13)));

            var x = Option<string>.None;
            Assert.That(x.Select(s => s.Length), Is.EqualTo(Option<int>.None));
        }

        [Test]
        public void TestInspect()
        {
            var list = new List<int> { 1, 2, 3 };

            var x = list.GetOptional(1)
                .IfSome(i => Console.WriteLine($"got: {i}"))
                .Expect("list should be long enough");

            Assert.That(x, Is.EqualTo(2));

            // Prints nothing
            Assert.DoesNotThrow(() =>
            {
                list.GetOptional(5)
                    .IfSome(i => throw new InvalidOperationException());
            });
        }

        [Test]
        public void TestXor()
        {
            {
                var x = Option.Some(2);
                var y = Option<int>.None;
                Assert.That(x.Xor(y), Is.EqualTo(Option.Some(2)));
            }

            {
                var x = Option<int>.None;
                var y = Option.Some(100);
                Assert.That(x.Xor(y), Is.EqualTo(Option.Some(100)));
            }

            {
                var x = Option.Some(2);
                var y = Option.Some(100);
                Assert.That(x.Xor(y), Is.EqualTo(Option<int>.None));
            }

            {
                var x = Option<int>.None;
                var y = Option<int>.None;
                Assert.That(x.Xor(y), Is.EqualTo(Option<int>.None));
            }
        }

        [Test]
        public void TestZip()
        {
            var x = Option.Some(1);
            var y = Option.Some("hi");
            var z = Option<byte>.None;

            using (Assert.EnterMultipleScope())
            {
                Assert.That(x.Zip(y), Is.EqualTo(Option.Some((1, "hi"))));
                Assert.That(x.Zip(z), Is.EqualTo(Option<(int, byte)>.None));
            }
        }

        [Test]
        public void TestZipWithFunction()
        {
            var x = Option.Some(17.5f);
            var y = Option.Some(42.7f);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(x.Zip(y, New), Is.EqualTo(Option.Some(new Point(17.5f, 42.7f))));
                Assert.That(x.Zip(Option<float>.None, New), Is.EqualTo(Option<Point>.None));
            }

            return;

            Point New(float a, float b) => new(a, b);
        }

        [Test]
        public void TestUnzip()
        {
            var x = Option.Some(((byte) 1, "hi"));
            var y = Option<(byte, string)>.None;

            using (Assert.EnterMultipleScope())
            {
                Assert.That(x.Unzip(), Is.EqualTo((Option.Some((byte) 1), Option.Some("hi"))));
                Assert.That(y.Unzip(), Is.EqualTo((Option<byte>.None, Option<string>.None)));
            }
        }

        [Test]
        public void TestLinq()
        {
            var func = Option.Some((int a) => a * 3);
            var x = Option.Some(2);
            
            var result = 
                from f in func
                from n in x
                select f(n);
            
            Assert.That(result, Is.EqualTo(Option.Some(6)));
        }

        private struct Point(float x, float y)
        {
            public float X { get; set; } = x;
            public float Y { get; set; } = y;
        }
    }
}