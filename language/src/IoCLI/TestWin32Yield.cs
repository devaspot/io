using System;
using System.Collections.Generic;
using System.Threading;
using System.Reflection;
using System.Windows.Forms;
using System.Collections;
using System.Reflection.Emit;

namespace io
{
	public class IoCLI : IDisposable {

        public static IoState state = new IoState();

        private static IEnumerator Recurse2(IoCoroutine f)
        {
            //yield return null; // just create and quit // 1

            //f.fiber.Yield(f);

            IoCoroutine ccc = IoCoroutine.createObject(IoCLI.state);
            ccc.fiber = new Fiber();
            ccc.fiber.currentCoro = ccc;
            //yield return 
            ccc.fiber.currentRoutine = new FiberProc(IoCLI.Recurse3/*(ccc)*/);
            if (coro.fiber.State == 4)
            {
                Console.WriteLine("Creation Error. Fiber Exceeds on Recurse 2 " + f.uniqueId);
                IoCLI.yieldingCoros.Remove(f);
                return null;
            }
            ccc.rawSetResult(IoNumber.newWithDouble(IoCLI.state, 42));
            ccc.rawSetRunLocals(IoCLI.state.core);
            ccc.rawSetRunMessage(IoCLI.state.nilMessage);
            ccc.rawSetRunTarget(ccc);
            IoCLI.yieldingCoros.Add(ccc);

            //Console.WriteLine("Coro2 " + f.uniqueId + " creates Coro3 + " + ccc.uniqueId);

            IoCLI.yieldingCoros.Remove(f);
            //yield return null; // 2

            return null;
        }

        delegate void SomeDelegate();

        private static IEnumerator Recurse3(IoCoroutine f)
        {
            int i = 0;
            
            IoCoroutine ccc = IoCoroutine.createObject(IoCLI.state);
            ccc.fiber = new Fiber();
            ccc.fiber.currentCoro = ccc;
            //yield return 
            ccc.fiber.currentRoutine = new FiberProc(IoCLI.Recurse4/*(ccc)*/);
            if (coro.fiber.State == 4)
            {
                Console.WriteLine("Creation Error. Fiber Exceeds on Recurse 3 " + f.uniqueId);
                IoCLI.yieldingCoros.Remove(f);
                return null;
            }
            ccc.rawSetResult(IoNumber.newWithDouble(IoCLI.state, 42));
            ccc.rawSetRunLocals(IoCLI.state.core);
            ccc.rawSetRunMessage(IoCLI.state.nilMessage);
            ccc.rawSetRunTarget(ccc);
            IoCLI.yieldingCoros.Add(ccc);
            
            IoCLI.yieldingCorosCount++;

            //Console.WriteLine("Coro3 " + f.uniqueId + " creates Coro4 + " + ccc.uniqueId);

            while (i < 2)
            {
               // f.fiber.Yield(f);
                //Console.WriteLine("Recurse3 Fiber: " + f.uniqueId + " Iteration: " + i++);
                //yield return null;
                i++;
            }

            IoCLI.yieldingCoros.Remove(f);

            return null;
        }

        private static IEnumerator Recurse4(IoCoroutine f)
        {
            int i = 0;
            while (i < 4)
            {
                //Console.WriteLine("Recurse4 Fiber: " + f.uniqueId + " Iteration: " + i++);
                Console.WriteLine("[" + f.uniqueId + ":" + Convert.ToString(i++) + "]");
                f.fiber.Yield(f);
                //yield return null; 
            }

            IoCLI.yieldingCoros.Remove(f);

            IoCLI.coro = null;

            return null;
            //yield return null;
        }

        public static ArrayList yieldingCoros = new ArrayList();
        public static int yieldingCorosCount = 0;

        public static IoCoroutine coro = null;

        [MTAThread]
        static void Main()
        {
            // Test Fibers

            for (int i = 0; i <400; i++)
            {
                IoCoroutine coro = IoCoroutine.createObject(IoCLI.state);
                coro.fiber = new Fiber();
                coro.fiber.currentCoro = coro;
                coro.fiber.uniqueId = coro.uniqueId;
                coro.fiber.currentRoutine = new FiberProc(IoCLI.Recurse2/*(coro)*/);
                if (coro.fiber.State == 4)
                {
                    Console.WriteLine("Creation Error. Fiber Exceeds on " + i);
                    break;
                }
                IoCLI.yieldingCoros.Add(coro);
                if (IoCLI.coro == null)
                    IoCLI.coro = coro;
            }

            while (true)
            {
                ArrayList al = new ArrayList();
                foreach (IoCoroutine coro in IoCLI.yieldingCoros)
                {
                    if (coro != null)
                    al.Add(coro);
                }
                if (al.Count == 0) break;
                foreach (IoCoroutine coro in al)
                    {
                        if (coro != null)
                        IoCoroutine.slotResume(coro, null, null);
                    }
            }

            IoCLI.state.prompt(IoCLI.state);

        }

        #region IDisposable Members

        public void Dispose()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }
}
