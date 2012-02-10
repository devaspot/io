
using System;
using System.Runtime.InteropServices;
using System.Collections;

namespace io
{
    public delegate IEnumerator FiberProc(IoCoroutine o);

    public class Fiber : AjaiFiber
    {
        public IoCoroutine currentCoro = null;
        public FiberProc currentRoutine = null;
        public long uniqueId = 0;
        public Fiber() { }
        public Fiber(int i) { uniqueId = i; }
        public override void Run()
        {
            if (currentRoutine != null)
                currentRoutine(currentCoro);
        }
    }

}
