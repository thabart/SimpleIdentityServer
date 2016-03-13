#region copyright
// Copyright 2015 Habart Thierry
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;

namespace System.Security.Cryptography
{
    public abstract class SafeHandleZeroOrMinusOneIsInvalid : SafeHandle
    {
        protected SafeHandleZeroOrMinusOneIsInvalid(bool ownsHandle) : base(IntPtr.Zero, ownsHandle)
        {
        }

        public override bool IsInvalid
        {
            [SecurityCritical]
            get
            {
                return handle == null || handle == new IntPtr(-1);
            }
        }
    }

    internal sealed class SafeBCryptAlgorithmHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeBCryptAlgorithmHandle() : base(true)
        {
        }

        [DllImport("bcrypt")]
        private static extern BCryptNative.ErrorCode BCryptCloseAlgorithmProvider(IntPtr hAlgorithm, int flags);

        protected override bool ReleaseHandle()
        {
            return BCryptCloseAlgorithmProvider(handle, 0) == BCryptNative.ErrorCode.Success;
        }
    }

    internal sealed class SafeBCryptHashHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private IntPtr m_hashObject;

        private SafeBCryptHashHandle() : base(true)
        {
        }

        /// <summary>
        ///     Buffer holding the hash object. This buffer should be allocated with Marshal.AllocCoTaskMem.
        /// </summary>
        internal IntPtr HashObject
        {
            get { return m_hashObject; }

            set
            {
                Contract.Requires(value != IntPtr.Zero);
                m_hashObject = value;
            }
        }


        [DllImport("bcrypt")]
        private static extern BCryptNative.ErrorCode BCryptDestroyHash(IntPtr hHash);

        protected override bool ReleaseHandle()
        {
            bool success = BCryptDestroyHash(handle) == BCryptNative.ErrorCode.Success;

            // The hash object buffer must be released only after destroying the hash handle
            if (m_hashObject != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(m_hashObject);
            }

            return success;
        }
    }

    internal sealed class SafeBCryptKeyHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        internal SafeBCryptKeyHandle() : base(true) { }

        [DllImport("bcrypt.dll")]
        internal static extern BCryptNative.ErrorCode BCryptDestroyKey(IntPtr hKey);
        
        protected override bool ReleaseHandle()
        {
            return BCryptDestroyKey(handle) == BCryptNative.ErrorCode.Success;
        }
    }
}
