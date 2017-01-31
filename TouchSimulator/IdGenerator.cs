using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WpfApplication1
{
    static class IdGenerator
    {
        static private int _int;
        private static uint _uint;
        private static readonly object _mutex = new object();

        public static int GetUniqueInt()
        {
            Interlocked.Increment(ref _int);
            return _int;
        }

        public static uint GetUinqueUInt()
        {
            lock (_mutex)
            {
                if (_uint > 256)
                {
                    ResetUint();
                }
                if (_uint == uint.MaxValue)
                    throw new IndexOutOfRangeException();
                else
                {
                    _uint++;
                    return _uint;
                }
            }
        }

        public static void ResetUint()
        {
            lock (_mutex)
            {
                _uint = uint.MinValue;
            }
        }
    }
}
