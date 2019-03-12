﻿using Pipelines.Sockets.Unofficial.Internal;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Pipelines.Sockets.Unofficial.Threading
{
    public partial class MutexSlim
    {
        /// <summary>
        /// The result of a Wait/WaitAsync operation on MutexSlim; the caller *must* check Success to see whether the mutex was obtained
        /// </summary>
        public readonly struct LockToken : IDisposable, IEquatable<LockToken>
        {
            /// <summary>
            /// Compare two LockToken instances for equality
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(in LockToken x, in LockToken y) => x.Equals(y);

            /// <summary>
            /// Compare two LockToken instances for equality
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(in LockToken x, in LockToken y) => !x.Equals(y);

            /// <summary>
            /// Compare two LockToken instances for equality
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override bool Equals(object obj) => obj is LockToken other && Equals(other);

            /// <summary>
            /// See Object.GetHashCode
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override int GetHashCode() => (_parent?.GetHashCode() ?? 0) ^ _token;

            /// <summary>
            /// See Object.ToString()
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override string ToString() => LockState.ToString(_token);

            bool IEquatable<LockToken>.Equals(LockToken other) => Equals(in other);

            /// <summary>
            /// Compare two LockToken instances for equality
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            [CLSCompliant(false)]
            public bool Equals(in LockToken other)
            {
                if (_parent != null) return ReferenceEquals(_parent, other._parent);
                if (other._parent != null) return false;
                return _token == other._token;
            }

            private readonly MutexSlim _parent;
            private readonly int _token;

            /// <summary>
            /// Indicates whether the mutex was successfully taken
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator true(in LockToken token) => LockState.GetState(token._token) == LockState.Success;

            /// <summary>
            /// Indicates whether the mutex was successfully taken
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator false(in LockToken token) => LockState.GetState(token._token) != LockState.Success;

            /// <summary>
            /// Indicates whether the mutex was successfully taken
            /// </summary>
            public bool Success
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => LockState.GetState(_token) == LockState.Success;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal LockToken(MutexSlim parent, int token)
            {
                _parent = parent;
                _token = token;
            }

            /// <summary>
            /// Release the mutex, if obtained
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose()
            {
                if (LockState.GetState(_token) == LockState.Success) _parent.Release(_token, demandMatch: true);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal LockToken AssertNotCanceled()
            {
                if (LockState.IsCanceled(_token)) Throw.TaskCanceled();
                return this;
            }
        }
    }
}
