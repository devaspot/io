
using System;
using System.Text;
using System.Collections;

namespace io
{
    public class IoBlock : IoObject
    {
        public bool async = false;
        public override string name { get { return "Block"; } }
        public IoMessage containedMessage;
        public IoObjectArrayList argNames;
        public IoObject scope; // if 0x0, then use target as the locals proto
        public IoCallStatus passStops;

        // Prototypes and Clone

        public new static IoBlock createProto(IoState state)
        {
            IoBlock number = new IoBlock();
            return number.proto(state) as IoBlock;
        }

        public new static IoBlock createObject(IoState state)
        {
            IoBlock number = new IoBlock();
            return number.clone(state) as IoBlock;
        }

        public override IoObject proto(IoState state)
        {
            IoBlock pro = new IoBlock();
            pro.state = state;
            pro.createSlots();
            pro.createProtos();
            pro.containedMessage = state.nilMessage;
            pro.argNames = new IoObjectArrayList();
            state.registerProtoWithFunc(name, new IoStateProto(name, pro, new IoStateProtoFunc(this.proto)));
            pro.protos.Add(state.protoWithInitFunc("Object"));

            IoCFunction[] methodTable = new IoCFunction[] {
                new IoCFunction("call", new IoMethodFunc(IoBlock.slotCall)),
                new IoCFunction("code", new IoMethodFunc(IoBlock.slotCode)),
                new IoCFunction("block", new IoMethodFunc(IoBlock.slotBlock)),
                new IoCFunction("method", new IoMethodFunc(IoBlock.slotMethod)),
    	    };

            pro.addTaglessMethodTable(state, methodTable);
            return pro;
        }

        public override void cloneSpecific(IoObject _from, IoObject _to)
        {
			IoBlock to = _to as IoBlock;
			IoBlock from = _from as IoBlock;
            to.isActivatable = from.isActivatable;
            to.containedMessage = from.containedMessage;
            to.argNames = new IoObjectArrayList();
        }

        // Published Slots

        public new static IoObject slotMethod(IoObject target, IoObject locals, IoObject message)
        {
            IoState state = target.state;
	        IoBlock self = IoBlock.createObject(state);
            IoMessage m = message as IoMessage;
	        int nargs = m.args.Count;
	        IoMessage lastArgAsMessage = (nargs > 0) ? m.rawArgAt(nargs - 1) : state.nilMessage;
	        int i;

            self.containedMessage = lastArgAsMessage;
            self.isActivatable = true;

	        for (i = 0; i < nargs - 1; i ++)
	        {
		        IoMessage argMessage = m.rawArgAt(i);
		        IoSeq name = argMessage.messageName;
                self.argNames.Add(name);
	        }

	        return self;
        }

        public new static IoObject slotBlock(IoObject target, IoObject locals, IoObject m)
        {
            IoBlock self = target as IoBlock;
            self = IoBlock.slotMethod(target, locals, m) as IoBlock;
            self.scope = locals;
            self.isActivatable = false;
            return self;
        }

        public static IoObject slotCode(IoObject target, IoObject locals, IoObject m)
        {
            string s = "";
            IoBlock self = target as IoBlock;
            if (self.scope != null)
                s += "block(";
            else
                s += "method(";
            int nargs = self.argNames.Count;
            for (int i = 0; i < nargs; i++)
            {
                IoSeq name = self.argNames[i] as IoSeq;
                s += name.value + ", ";
            }

            IoMessage msg = self.containedMessage;
            IoSeq seq = IoMessage.slotCode(msg, locals, m) as IoSeq;
            s += seq.value + ")";

            return IoSeq.createObject(target.state, s);
        }

        public static IoObject slotCall(IoObject target, IoObject locals, IoObject message)
        {
			return target.activate(target, locals, locals, message as IoMessage, locals);
        }

        // Call Public Raw Methods

        public override IoObject activate(IoObject sender, IoObject target, IoObject locals, IoMessage m, IoObject slotContext)
        {
            IoState state = sender.state;
            IoBlock self = sender as IoBlock;

            IoObjectArrayList argNames = self.argNames;
            IoObject scope = self.scope;

            IoObject blockLocals = state.localsProto.clone(state);
            IoObject result = null;
            IoObject callObject = null;
            
            blockLocals.isLocals = true;

            if (scope == null)
                scope = target;

            blockLocals.createSlots();

            callObject = IoCall.with(state, locals, target, m, slotContext, self, null/*state.currentCoroutine*/);

            IoSeqObjectHashtable bslots = blockLocals.slots;
            bslots["call"] = callObject;
            bslots["self"] = scope;
            bslots["updateSlot"] = state.localsUpdateSlotCFunc;

            if (argNames != null)
            for (int i = 0; i < argNames.Count; i++)
            {
                IoSeq name = argNames[i] as IoSeq;
                IoObject arg = m.localsValueArgAt(locals, i);
                blockLocals.slots[name] = arg;
            }

            if (self.containedMessage != null)
            {
                result = self.containedMessage.localsPerformOn(blockLocals, blockLocals);
            }

            if (self.passStops == IoCallStatus.MESSAGE_STOP_STATUS_NORMAL)
            {

            }

            return result;
        }

    }
}
