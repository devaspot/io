using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Collections;

namespace io
{
    public class IoCLRAssembly : IoObject
    {
        public override string name { get { return "CLRAssembly"; } }
        public Assembly assembly;
        public string assemblyName;
        public Type[] assemblyTypes;
        public Hashtable assemblyNamespaces;
        public IoCLRAssembly() : base() { isActivatable = false; }

        public new static IoCLRAssembly createProto(IoState state)
        {
            IoCLRAssembly cf = new IoCLRAssembly();
            return cf.proto(state) as IoCLRAssembly;
        }

        public new static IoCLRAssembly createObject(IoState state)
        {
            IoCLRAssembly cf = new IoCLRAssembly();
            return cf.proto(state).clone(state) as IoCLRAssembly;
        }

        public IoCLRAssembly(IoState state, string name)
        {
            isActivatable = true;
            this.state = state;
            createSlots();
            createProtos();
            uniqueId = 0;
        }

        public override IoObject proto(IoState state)
        {
            IoCLRAssembly pro = new IoCLRAssembly();
            pro.state = state;
            pro.uniqueId = 0;
            pro.createSlots();
            pro.createProtos();
            pro.isActivatable = true;
            state.registerProtoWithFunc(pro.name, new IoStateProto(pro.name, pro, new IoStateProtoFunc(pro.proto)));
            //pro.protos.Add(state.protoWithInitFunc("Object"));

            IoCFunction[] methodTable = new IoCFunction[] {
			    new IoCFunction("namespaces", new IoMethodFunc(IoCLRAssembly.slotNamespaces)),
            };

            pro.addTaglessMethodTable(state, methodTable);
            return pro;
        }

        // published slots

        public static IoObject slotNamespaces(IoObject target, IoObject locals, IoObject message)
        {
            IoCLRAssembly self = target as IoCLRAssembly;
            IoMessage m = message as IoMessage;
            foreach (string s in self.assemblyNamespaces.Keys)
            {
                Console.Write(s + " ");
            }
            Console.WriteLine();
            return self;
        }
    }
}
