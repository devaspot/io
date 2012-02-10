using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace io
{
    public class IoCoroutine : IoObject
    {
        public override string name { get { return "Coroutine"; } }
        public object fiber = null;

        public override string ToString()
        {
            return "Coroutine+"+uniqueId.ToString();
        }

        public new static IoCoroutine createProto(IoState state)
        {
            IoCoroutine s = new IoCoroutine();
            return s.proto(state) as IoCoroutine;
        }

        public new static IoCoroutine createObject(IoState state)
        {
            IoCoroutine s = new IoCoroutine();
            return s.clone(state) as IoCoroutine;
        }

        public override IoObject proto(IoState state)
        {
            IoCoroutine pro = new IoCoroutine();
            pro.tag.state = state;
          //  pro.tag.cloneFunc = new IoTagCloneFunc(this.clone);
            pro.createSlots();
            pro.createProtos();
            state.registerProtoWithFunc(name, new IoStateProto(name, pro, new IoStateProtoFunc(this.proto)));
            pro.protos.Add(state.protoWithInitFunc("Object"));

            IoCFunction[] methodTable = new IoCFunction[] {
                new IoCFunction("run", new IoMethodFunc(IoCoroutine.slotRun)),
                new IoCFunction("main", new IoMethodFunc(IoCoroutine.slotMain)),
                new IoCFunction("resume", new IoMethodFunc(IoCoroutine.slotResume)),
                new IoCFunction("isCurrent", new IoMethodFunc(IoCoroutine.slotIsCurrent)),                  
                new IoCFunction("currentCoroutine", new IoMethodFunc(IoCoroutine.slotCurrentCoroutine)),
                new IoCFunction("implementation", new IoMethodFunc(IoCoroutine.slotImplementation)),
                new IoCFunction("setMessageDebugging", new IoMethodFunc(IoCoroutine.slotSetMessageDebugging)),
			};

            pro.addTaglessMethodTable(state, methodTable);
            return pro;
        }

        public void rawReturnToParent()
        {
            IoCoroutine parentCoro = rawParentCoroutine() as IoCoroutine;
            if (parentCoro != null && parentCoro.name.Equals("Coroutine"))
            {
                IoCoroutine.slotResume(parentCoro, null, null);
            }
            else
            {
                if (this == this.tag.state.mainCoroutine)
                {
                    Console.WriteLine("IoCoroutine error: attempt to return from main coro");
                }
            }
        }

        public IEnumerator coroStart(IoCoroutine coro)
        {
            //IoCoroutine self = coro;
            //IoObject result = null;
            //self.tag.state.currentCoroutine = self;
            //if (self.tag.state.mainMessage != null)
            //    result = self.tag.state.mainMessage.localsPerformOn(self, self);
            //self.rawSetResult(result);
            //self.rawReturnToParent();
            yield return null;
        }

        // Coroutine clone run

        public static IoObject slotRun(IoObject target, IoObject locals, IoObject message)
        {
            IoCoroutine self = target as IoCoroutine;
            //Fiber coro = self.fiber;
            //if (coro == null)
            //    self.fiber = new Fiber();

            //{
            //    // actually we don't need get current Corotine cos we hasn't do SwithFromTo
            //    // just switch
            //    // IoCoroutine current = target.tag.state.currentCoroutine;
            //    self.fiber.currentRoutine = new FiberProc(self.coroStart/*(self)*/);
            //    self.fiber.Run();
            //}
            return self.rawResult();
        }

        public static IoObject slotMain(IoObject target, IoObject locals, IoObject message)
        {
            IoCoroutine self = target as IoCoroutine;
            //IoObject runTarget = self.rawRunTarget();
            //IoObject runLocals = self.rawRunLocals();
            //IoMessage runMessage = self.rawRunMessage() as IoMessage;
            //if (runLocals != null && runMessage != null && runTarget != null)
            //    runMessage.localsPerformOn(runTarget, runLocals);
            //else
            //    Console.WriteLine("Coroutine 'main' missed needed parameters");
            return self.tag.state.ioNil;
        }

        //public object Yield()
        //{
        //    yield return null;
        //}

        public static IoObject slotResume(IoObject target, IoObject locals, IoObject message)
        {
            IoCoroutine self = target as IoCoroutine;
            //object ret = null;

            //if (self.fiber != null)
            //{
            //    IoCoroutine current = target.tag.state.currentCoroutine;
            //    target.tag.state.currentCoroutine = self;
            //    ret = self.fiber.Resume();
            //    //if (ret == null)
            //    //{
            //    //    Console.WriteLine("Fiber Exceeds on " + self.fiber.uniqueId);
            //    //    throw new Exception("Can't resume Fiber");
            //    //}
            //}
            //else
            //{
            //    IoCoroutine.slotRun(self, null, null);
            //}

            return self;
        }

        public static IoObject slotIsCurrent(IoObject target, IoObject locals, IoObject message)
        {
            IoObject b =  target.tag.state.currentCoroutine.compare(target) == 0 ? target.tag.state.ioTrue : target.tag.state.ioFalse;
            return b;
        }

        public static IoObject slotCurrentCoroutine(IoObject target, IoObject locals, IoObject message)
        {
            return target.tag.state.currentCoroutine;
        }


        public static IoObject slotImplementation(IoObject target, IoObject locals, IoObject message)
        {
            return target;
        }

        public static IoObject slotSetMessageDebugging(IoObject target, IoObject locals, IoObject message)
        {
            return target;
        }

        public override IoObject clone(IoState state)
        {
            IoCoroutine proto = state.protoWithInitFunc(name) as IoCoroutine;
            IoCoroutine result = new IoCoroutine();
            uniqueIdCounter++;
            result.uniqueId = uniqueIdCounter;
            result.tag = proto.tag; 
            result.createProtos();
            result.createSlots();
            result.protos.Add(proto);
            return result;
        }

        // runTarget
        public void rawSetRunTarget(IoObject v) { slots["runTarget"] = v; }
        public IoObject rawRunTarget() { return slots["runTarget"] as IoObject; }

        // runMessage
        public void rawSetRunMessage(IoObject v) { slots["runMessage"] = v; }
        public IoObject rawRunMessage() { return slots["runMessage"] as IoObject; }

        // runLocals
        public void rawSetRunLocals(IoObject v) { slots["runLocals"] = v; }
        public IoObject rawRunLocals() { return slots["runLocals"] as IoObject; }
       
        // parent
        public void rawSetParentCoroutine(IoObject v) { slots["parentCoroutine"] = v; }
        public IoObject rawParentCoroutine() { return slots["parentCoroutine"] as IoObject; }

        // result
        public void rawSetResult(IoObject v) { slots["result"] = v; }
        public IoObject rawResult() { return slots["result"] as IoObject; }

        // exception
        public void rawSetSetException(IoObject v) { slots["exception"] = v; }
        public IoObject rawException() { return slots["exception"] as IoObject; }

        public override void print()
        {
        }

    }

}
