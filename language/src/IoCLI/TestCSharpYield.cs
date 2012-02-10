using System;
using System.Collections.Generic;
using System.Threading;
using System.Reflection;
using System.Windows.Forms;
using System.Collections;

namespace io
{
	public class IoCLI {

        public static IoState state = new IoState();

        private static IEnumerator Recurse2(IoCoroutine f)
        {
            yield return null; // just create and quit

            IoCoroutine ccc = IoCoroutine.createObject(IoCLI.state);
            ccc.fiber = new Fiber();
            ccc.fiber.currentCoro = ccc;
            ccc.fiber.currentRoutine = IoCLI.Recurse3(ccc);
            ccc.rawSetResult(IoNumber.newWithDouble(IoCLI.state, 42));
            ccc.rawSetRunLocals(IoCLI.state.core);
            ccc.rawSetRunMessage(IoCLI.state.nilMessage);
            ccc.rawSetRunTarget(ccc);
            IoCLI.yieldingCoros.Add(ccc);

            yield return ccc;
        }

        private static IEnumerator Recurse3(IoCoroutine f)
        {
            IoCoroutine ccc = IoCoroutine.createObject(IoCLI.state);
            ccc.fiber = new Fiber();
            ccc.fiber.currentCoro = ccc;
            ccc.fiber.currentRoutine = IoCLI.Recurse4(ccc);
            ccc.rawSetResult(IoNumber.newWithDouble(IoCLI.state, 42));
            ccc.rawSetRunLocals(IoCLI.state.core);
            ccc.rawSetRunMessage(IoCLI.state.nilMessage);
            ccc.rawSetRunTarget(ccc);
            IoCLI.yieldingCoros.Add(ccc);

            Console.WriteLine("Recurse3+" + i++);

            yield return ccc;
        }

        private static IEnumerator Recurse4(IoCoroutine f)
        {
            int i = 0;
            while (i < 10)
            {
                Console.WriteLine("Recurse4+" + i++);
                yield return null;
            }
        }

        public static ArrayList yieldingCoros = new ArrayList();

        [STAThread]
        static void Main()
        {
            // Test Fibers

            IoCoroutine[] coros = new IoCoroutine[1];
            
            for (int i = 0; i < coros.Length; i++)
            {
                coros[i] = IoCoroutine.createObject(IoCLI.state);
                coros[i].fiber = new Fiber();
                coros[i].fiber.currentCoro = coros[i];
                coros[i].fiber.uniqueId = i;
                coros[i].fiber.currentRoutine = IoCLI.Recurse2(coros[i]);
                IoCLI.yieldingCoros.Add(coros[i]);
            }

            for (int j = 0; j < 5; j++)
            foreach (IoCoroutine f in coros)
            {
                
                IoCoroutine.slotResume(f, null, null);
            }

            IoCLI.state.prompt(IoCLI.state);

        }
    }
}
