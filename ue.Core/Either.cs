// Copyright (c) 2025 Yuieii.

using System;

namespace ue.Core
{
    public static class Either
    {
        public static PartialWithLeft<T> Left<T>(T value) => new(value);
        public static PartialWithRight<T> Right<T>(T value) => new(value);
        
        public class PartialWithLeft<T>(T value)
        {
            public Either<T, TRight> FulfillRightType<TRight>()
                => Either<T, TRight>.CreateLeft(value);
        }

        public class PartialWithRight<T>(T value)
        {
            public Either<TLeft, T> FulFillLeftType<TLeft>()
                => Either<TLeft, T>.CreateRight(value);
        }
    }
    
    public abstract class Either<TLeft, TRight>
    {
        public static Either<TLeft, TRight> CreateLeft(TLeft value)
        {
            Validation.EnsureNotNull(value, nameof(value));
            return new LeftBranch(value);
        }

        public static Either<TLeft, TRight> CreateRight(TRight value)
        {
            Validation.EnsureNotNull(value, nameof(value));
            return new RightBranch(value);
        }

        public static implicit operator Either<TLeft, TRight>(Either.PartialWithLeft<TLeft> part)
            => part.FulfillRightType<TRight>();

        public static implicit operator Either<TLeft, TRight>(Either.PartialWithRight<TRight> part)
            => part.FulFillLeftType<TLeft>();

        internal Either() {}

        public abstract bool IsLeft { get; }
        
        public virtual bool IsRight => !IsLeft;
        
        public abstract Option<TLeft> Left { get; }
        
        public abstract Option<TRight> Right { get; }

        public TLeft UnwrapLeft()
            => Left.Expect("Cannot unwrap Left value from Right");
        
        public TRight UnwrapRight()
            => Right.Expect("Cannot unwrap Right value from Left");
        
        public abstract Either<TOut, TRight> SelectLeft<TOut>(Func<TLeft, TOut> selector);
        
        public abstract Either<TLeft, TOut> SelectRight<TOut>(Func<TRight, TOut> selector);

        public abstract Either<TRight, TLeft> Swap();
        
        #region --- Left branch

        private class LeftBranch(TLeft value) : Either<TLeft, TRight>
        {
            private TLeft Value { get; } = value;
            
            public override bool IsLeft => true;

            public override Option<TLeft> Left 
                => Option.Some(Value);
            
            public override Option<TRight> Right 
                => Option.None;

            public override Either<TOut, TRight> SelectLeft<TOut>(Func<TLeft, TOut> selector) 
                => Either.Left(selector(Value));

            public override Either<TLeft, TOut> SelectRight<TOut>(Func<TRight, TOut> selector)
                => Either.Left(Value);

            public override Either<TRight, TLeft> Swap()
                => Either.Right(Value);
        }
        
        #endregion
        
        #region --- Right branch

        private class RightBranch(TRight value) : Either<TLeft, TRight>
        {
            private TRight Value { get; } = value;

            public override bool IsLeft => false;

            public override Option<TLeft> Left
                => Option.None;
            
            public override Option<TRight> Right
                => Option.Some(Value);

            public override Either<TOut, TRight> SelectLeft<TOut>(Func<TLeft, TOut> selector)
                => Either.Right(Value);

            public override Either<TLeft, TOut> SelectRight<TOut>(Func<TRight, TOut> selector)
                => Either.Right(selector(Value));
            
            public override Either<TRight, TLeft> Swap()
                => Either.Left(Value);
        }
        
        #endregion
    }
}