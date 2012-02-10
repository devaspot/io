using System;
using System.Collections.Generic;
using System.Text;

namespace io
{
	public delegate IoObject IoMethodFunc(IoObject target, IoObject locals, IoObject message);

	public class IoCFunction : IoObject
	{
        public bool async = false;
		public override string name { get { return "CFunction";  } }
		public IoMethodFunc func;
		public string funcName;
		public IoCFunction() : base() { isActivatable = true; }

		public new static IoCFunction createProto(IoState state)
		{
			IoCFunction cf = new IoCFunction();
			return cf.proto(state) as IoCFunction;
		}

		public new static IoCFunction createObject(IoState state)
		{
			IoCFunction cf = new IoCFunction();
			return cf.proto(state).clone(state) as IoCFunction;
		}

        public IoCFunction(string name, IoMethodFunc func) : this(null, name, func) {}

        public IoCFunction(IoState state, string name, IoMethodFunc func)
        {
            isActivatable = true;
            this.state = state;
            createSlots();
            createProtos();
            uniqueId = 0;
            funcName = name;
            this.func = func;
        }

        public override IoObject proto(IoState state)
        {
            IoCFunction pro = new IoCFunction();
            pro.state = state;
            pro.uniqueId = 0;
            pro.createSlots();
            pro.createProtos();
            pro.isActivatable = true; 
            state.registerProtoWithFunc(pro.name, new IoStateProto(pro.name, pro, new IoStateProtoFunc(pro.proto)));
            pro.protos.Add(state.protoWithInitFunc("Object"));

            IoCFunction[] methodTable = new IoCFunction[] {
                //new IoCFunction("perform", new IoMethodFunc(pro.slotPerform)),
			};

			pro.addTaglessMethodTable(state, methodTable);
			return pro;
        }

        public override void cloneSpecific(IoObject _from, IoObject _to)    
        {
            IoCFunction from = _from as IoCFunction;
            IoCFunction to = _to as IoCFunction;
            to.isActivatable = true;
			to.funcName = from.funcName;
			to.func = from.func;
		}

        public override IoObject activate(IoObject self, IoObject target, IoObject locals, IoMessage m, IoObject slotContext)
		{
            if (func == null) return self;
			return func(target, locals, m);
		}
	}
}
