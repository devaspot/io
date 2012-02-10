using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

// Superlightweight C# threads

namespace io
{
    public class CSharpFiber
    {
        public Stack<IEnumerator> stackFrame = new Stack<IEnumerator>(); // Stack of calling Threads
        public Stack<IoCoroutine> stackCoroFrame = new Stack<IoCoroutine>(); // Stack of calling Coroutines
        public IEnumerator currentRoutine; // holds current Thread IP
        public IoCoroutine currentCoro; // holds current Coroutine 
        public int uniqueId = 0;
        public CSharpFiber() { }
        public CSharpFiber(IEnumerator entryPoint) { this.currentRoutine = entryPoint; }
        public CSharpFiber(IoCoroutine coro) { this.currentCoro = coro; }
        public bool Step()
        {
            //Console.Write("stack-" + currentCoro.uniqueId + "(");
            //foreach (IoCoroutine cor in stackCoroFrame)
            //    Console.Write(cor.GetHashCode() + ",");
            //Console.WriteLine(")");

            if (currentRoutine.MoveNext())
            {
                IoCoroutine subRoutine = currentRoutine.Current as IoCoroutine;
                if (subRoutine != null)
                {
                    stackFrame.Push(this.currentRoutine);
                    stackCoroFrame.Push(this.currentCoro);
                    //currentRoutine = subRoutine.fiber.currentRoutine;
                    currentCoro = subRoutine;
                }
            }
            else if (stackFrame.Count > 0)
            {
                currentRoutine = stackFrame.Pop();
                currentCoro = stackCoroFrame.Pop();
            }
            else
            {
                OnFiberTerminated(new FiberTerminatedEventArgs(currentRoutine.Current));
                return false;
            }

            return true;
        }

        public event EventHandler<FiberTerminatedEventArgs> FiberTerminated;

        public virtual void OnFiberTerminated(FiberTerminatedEventArgs e)
        {
            if (FiberTerminated != null)
                FiberTerminated(this, e);
        }
    }

    public class FiberTerminatedEventArgs : EventArgs
    {
        private readonly object result;
        public FiberTerminatedEventArgs(object result) { this.result = result; }
        public object Result { get { return this.result; } }
    }

}
