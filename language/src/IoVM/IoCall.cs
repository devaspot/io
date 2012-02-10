
namespace io {

    public enum IoCallStatus
    {
        MESSAGE_STOP_STATUS_NORMAL = 0,
        MESSAGE_STOP_STATUS_BREAK = 1,
        MESSAGE_STOP_STATUS_CONTINUE = 2,
        MESSAGE_STOP_STATUS_RETURN = 4,
        MESSAGE_STOP_STATUS_EOL = 8
    }

	public class IoCall : IoObject
    {
        public override string name { get { return "Call"; } }
        public IoObject sender;
        public IoObject msg;
        public IoObject target;
        public IoObject slotContext;
        public IoObject activated;
        public IoObject coroutine;
        public IoCallStatus stopStatus;

        public new static IoCall createProto(IoState state)
        {
            IoCall call = new IoCall();
            return call.proto(state) as IoCall;
        }

        public new static IoCall createObject(IoState state)
        {
            IoCall call = new IoCall();
            return call.clone(state) as IoCall;
        }

        public override IoObject proto(IoState state)
        {
            IoCall pro = new IoCall();
            pro.state = state;
            pro.createSlots();
            pro.createProtos(); 
            state.registerProtoWithFunc(name, new IoStateProto(pro.name, pro, new IoStateProtoFunc(pro.proto)));
            pro.protos.Add(state.protoWithInitFunc("Object"));

            IoCFunction[] methodTable = new IoCFunction[] {
                new IoCFunction("sender", new IoMethodFunc(IoCall.slotSender)),
                new IoCFunction("target", new IoMethodFunc(IoCall.slotTarget)),
                new IoCFunction("message", new IoMethodFunc(IoCall.slotCallMessage)),
            };

            pro.addTaglessMethodTable(state, methodTable);
            return pro;
        }

        public static IoObject slotSender(IoObject target, IoObject locals, IoObject message)
        {
            IoCall self = target as IoCall;
            return self.sender;
        }

        public static IoObject slotTarget(IoObject target, IoObject locals, IoObject message)
        {
            IoCall self = target as IoCall;
            return self.target;
        }


        public static IoObject slotCallMessage(IoObject target, IoObject locals, IoObject message)
        {
            // setSlot("A", Object clone do(setSlot("B", method(call message))))
            IoCall self = target as IoCall;
            return self.msg;
        }


		//public override IoObject clone(IoState state)
		//{
		//    IoObject proto = state.protoWithInitFunc(name);
		//    IoCall result = new IoCall();
		//    uniqueIdCounter++;
		//    result.uniqueId = uniqueIdCounter;
		//    result.state = state;
		//    result.createProtos();
		//    result.createSlots();
		//    result.protos.Add(proto);
		//    return result;
		//}

        public static IoObject with(IoState state, IoObject sender, IoObject target,
            IoObject message, IoObject slotContext, IoObject activated, IoObject coro)
        {
            IoCall call = IoCall.createObject(state);
            call.sender = sender;
            call.target = target;
            call.msg = message;
            call.slotContext = slotContext;
            call.activated = activated;
            call.coroutine = coro;
            call.stopStatus = IoCallStatus.MESSAGE_STOP_STATUS_NORMAL;
            return call;
        }
    }

}

