using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Collections;

namespace io
{
    public class IoCLRObject : IoObject
    {
        public override string name { get { return "CLRObject"; } }
        public Type clrType;
        public object clrInstance;
        public IoCLRObject() : base() { isActivatable = false; }

        public new static IoCLRObject createProto(IoState state)
        {
            IoCLRObject cf = new IoCLRObject();
            return cf.proto(state) as IoCLRObject;
        }

        public new static IoCLRObject createObject(IoState state)
        {
            IoCLRObject cf = new IoCLRObject();
            return cf.proto(state).clone(state) as IoCLRObject;
        }

        public IoCLRObject(IoState state, string name)
        {
            isActivatable = true;
            this.state = state;
            createSlots();
            createProtos();
            uniqueId = 0;
        }

        public override IoObject proto(IoState state)
        {
            IoCLRObject pro = new IoCLRObject();
            pro.state = state;
            pro.uniqueId = 0;
            pro.createSlots();
            pro.createProtos();
            pro.isActivatable = true;
            state.registerProtoWithFunc(pro.name, new IoStateProto(pro.name, pro, new IoStateProtoFunc(pro.proto)));
			pro.protos.Add(state.protoWithInitFunc("Object"));

            IoCFunction[] methodTable = new IoCFunction[] {
				new IoCFunction("type", new IoMethodFunc(IoObject.slotType))
			};

            pro.addTaglessMethodTable(state, methodTable);
            return pro;
        }

        public override IoObject clone(IoState state)
        {
            IoCLRObject proto = state.protoWithInitFunc(name) as IoCLRObject;
            IoCLRObject result = new IoCLRObject();
            result.isActivatable = true;
            uniqueIdCounter++;
            result.uniqueId = uniqueIdCounter;
            result.state = state;
            result.createProtos();
            result.createSlots();
            result.protos.Add(proto);
            return result;
        }

        public IoCLRFunction getMethod(IoMessage message)
        {
            string methodName = message.messageName.value;
            if (clrType == null) return null;
            ConstructorInfo[] searchConstructors = null;
            Type[] parameters = null;
            ArrayList args = null;
            MethodBase mb = null;

            args = new ArrayList();
            parameters = new Type[message.args.Count];

            for (int i = 0; i < message.args.Count; i++)
            {
                IoObject o = message.localsValueArgAt(message, i);
                args.Add(o);
                Type t = null;
                switch (o.name)
                {
                    case "Number": t = typeof(double); break;
                    case "Object": t = typeof(object); break;
                    case "CLRObject": t = (o as IoCLRObject).clrType; break;
                    case "Sequence": t = typeof(string); break;
                }
                parameters[i] = t;
            }

            if (methodName.Equals("new"))
            {
                searchConstructors = this.clrType.GetConstructors();
                if (searchConstructors.Length > 0)
                    mb = searchConstructors[0];
            }
            else
            {
                try
                {
                    mb = this.clrType.GetMethod(methodName, parameters);
                }
                catch { }
            }
            
            IoCLRFunction clrFunction = IoCLRFunction.createObject(this.state);
            clrFunction.methodInfo = mb;
            clrFunction.parametersTypes = parameters;
            clrFunction.evaluatedParameters = args;
            return clrFunction;
        }

        public override IoObject activate(IoObject self, IoObject target, IoObject locals, IoMessage m, IoObject slotContext)
        {
            return self;
        }

        public override string ToString()
        {
			if (clrInstance == null) {
				if (clrType == null) return name;
				return clrType.ToString();
			}
			return clrInstance.ToString();
        }
    }
}
