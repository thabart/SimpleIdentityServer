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
using System.Threading;
using StringMaker = System.Security.Cryptography.Tokenizer.StringMaker;

namespace System.Security.Cryptography
{
    internal sealed class SharedStatics
    {
        internal static SharedStatics _sharedStatics = new SharedStatics();
        
        SharedStatics()
        {
            _Remoting_Identity_IDGuid = null;
            _Remoting_Identity_IDSeqNum = 0x40; // Reserve initial numbers for well known objects.
            _maker = null; 
        }

        private string _Remoting_Identity_IDGuid;
        
        private StringMaker _maker; 
        [System.Security.SecuritySafeCritical]  // auto-generated 
        static public StringMaker GetSharedStringMaker()
        { 
            StringMaker maker = null;

            bool tookLock = false;
            try
            {
                Monitor.Enter(_sharedStatics, ref tookLock); 
 
                if (_sharedStatics._maker != null)
                { 
                    maker = _sharedStatics._maker;
                    _sharedStatics._maker = null;
                }
            } 
            finally {
                if (tookLock) 
                    Monitor.Exit(_sharedStatics); 
            }
 
            if (maker == null)
            {
                maker = new StringMaker();
            } 

            return maker; 
        } 

        [System.Security.SecuritySafeCritical]  // auto-generated 
        static public void ReleaseSharedStringMaker(ref StringMaker maker)
        {
            // save this stringmaker so someone else can use it
            bool tookLock = false; 
            try 
            { 
                Monitor.Enter(_sharedStatics, ref tookLock);
 
                _sharedStatics._maker = maker;
                maker = null;
            }
            finally { 
                if (tookLock)
                    Monitor.Exit(_sharedStatics); 
            } 
        }

        // Note this may not need to be process-wide.
        private int _Remoting_Identity_IDSeqNum;
        internal static int Remoting_Identity_GetNextSeqNum()
        {
            return Interlocked.Increment(ref _sharedStatics._Remoting_Identity_IDSeqNum);
        }


        // This is the total amount of memory currently "reserved" via
        // all MemoryFailPoints allocated within the process.
        // Stored as a long because we need to use Interlocked.Add.
        private long _memFailPointReservedMemory;
        
        internal static long AddMemoryFailPointReservation(long size)
        {
            // Size can legitimately be negative - see Dispose. 
            return Interlocked.Add(ref _sharedStatics._memFailPointReservedMemory, (long)size);
        }

        internal static ulong MemoryFailPointReservedMemory
        {
            get
            {
                Contract.Assert(_sharedStatics._memFailPointReservedMemory >= 0, "Process-wide MemoryFailPoint reserved memory was negative!");
                return (ulong)_sharedStatics._memFailPointReservedMemory;
            }
        }
    }
}
