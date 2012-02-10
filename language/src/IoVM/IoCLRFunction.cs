using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Collections;

namespace io
{
    public class IoCLRFunction : IoObject
    {
        public bool async = false;
        public override string name { get { return "CLRFunction"; } }
        public MethodBase methodInfo;
        public Type[] parametersTypes;
        public ArrayList evaluatedParameters;
        public IoCLRFunction() : base() { isActivatable = true; }

        public new static IoCLRFunction createProto(IoState state)
        {
            IoCLRFunction cf = new IoCLRFunction();
            return cf.proto(state) as IoCLRFunction;
        }

        public new static IoCLRFunction createObject(IoState state)
        {
            IoCLRFunction cf = new IoCLRFunction();
            return cf.proto(state).clone(state) as IoCLRFunction;
        }

        public IoCLRFunction(IoState state, string name)
        {
            isActivatable = true;
            this.state = state;
            createSlots();
            createProtos();
            uniqueId = 0;
        }

        public override IoObject proto(IoState state)
        {
            IoCLRFunction pro = new IoCLRFunction();
            pro.state = state;
            pro.uniqueId = 0;
            pro.createSlots();
            pro.createProtos();
            pro.isActivatable = true;
            state.registerProtoWithFunc(pro.name, new IoStateProto(pro.name, pro, new IoStateProtoFunc(pro.proto)));
            //pro.protos.Add(state.protoWithInitFunc("Object"));

            IoCFunction[] methodTable = new IoCFunction[] {
			};

            pro.addTaglessMethodTable(state, methodTable);
            return pro;
        }

        public override IoObject activate(IoObject self, IoObject target, IoObject locals, IoMessage m, IoObject slotContext)
        {
            IoCLRFunction method = self as IoCLRFunction;
            IoCLRObject obj = target as IoCLRObject;
            object result = null;

            object[] parameters = new object[method.evaluatedParameters.Count];
            for (int i = 0; i < method.evaluatedParameters.Count; i++)
            {
                IoObject ep = method.evaluatedParameters[i] as IoObject;
                switch (ep.name)
                {
                    case "Object": parameters[i] = ep; break;
                    case "Number":
                        {
                            IoNumber num = ep as IoNumber;
                            if (num.isInteger)
                            {
                                parameters[i] = num.longValue;
                            }
                            else
                            {
                                parameters[i] = num.doubleValue;
                            }

                        }
                        break;
                    case "Sequence": parameters[i] = (ep as IoSeq).value; break;
                    case "CLRObject": parameters[i] = (ep as IoCLRObject).clrInstance; break;
                }

            }

            IoCLRObject clr = IoCLRObject.createObject(self.state);

            try
            {
                if (method.methodInfo is ConstructorInfo)
                {
                    ConstructorInfo ci = method.methodInfo as ConstructorInfo;
                    result = ci.Invoke(parameters);
                }
                else if (method.methodInfo is MethodInfo)
                {
                    MethodInfo mi = method.methodInfo as MethodInfo;
                    result = mi.Invoke(obj.clrInstance, parameters);
                }
                clr.clrType = result != null ? result.GetType() : null;
                clr.clrInstance = result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message.ToString());
                clr.clrType = null;
                clr.clrInstance = null;
            }
            
            return clr;
        }

    }
}
