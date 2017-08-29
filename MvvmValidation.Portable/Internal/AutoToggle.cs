using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvvmValidation.Internal
{
    public class AutoToggle
    {
        private readonly bool defaultValue;
        private int refCount = 0;

        public AutoToggle(bool defaultValue)
        {
            this.defaultValue = defaultValue;
        }

        public bool Value => refCount > 0 ? !defaultValue : defaultValue;

        public IDisposable Toggle()
        {
            return new Lock(this);
        }

        private class Lock : IDisposable
        {
            private readonly AutoToggle toggle;

            public Lock(AutoToggle toggle)
            {
                this.toggle = toggle;
                System.Threading.Interlocked.Increment(ref toggle.refCount);
            }

            public void Dispose()
            {
                System.Threading.Interlocked.Decrement(ref toggle.refCount);
            }
        }
    }
}
