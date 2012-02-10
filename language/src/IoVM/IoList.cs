
using System.Collections;
using System;

namespace io {

	public class IoList : IoObject
    {
		public override string name { get { return "List"; } }
        public IoObjectArrayList list = new IoObjectArrayList();

		public new static IoList createProto(IoState state)
		{
			IoList m = new IoList();
			return m.proto(state) as IoList;
		}

        public new static IoList createObject(IoState state)
        {
            IoList m = new IoList();
            return m.clone(state) as IoList;
        }

		public override IoObject proto(IoState state)
		{
			IoList pro = new IoList();
            pro.state = state;
         //   pro.tag.cloneFunc = new IoTagCloneFunc(pro.clone);
            pro.createSlots();
            pro.createProtos();
            pro.list = new IoObjectArrayList();
			state.registerProtoWithFunc(pro.name, new IoStateProto(pro.name, pro, new IoStateProtoFunc(pro.proto)));
			pro.protos.Add(state.protoWithInitFunc("Object"));

            IoCFunction[] methodTable = new IoCFunction[] {
				new IoCFunction("indexOf", new IoMethodFunc(IoList.slotIndexOf)),
				new IoCFunction("capacity", new IoMethodFunc(IoList.slotSize)),
				new IoCFunction("size", new IoMethodFunc(IoList.slotSize)),
				new IoCFunction("removeAll", new IoMethodFunc(IoList.slotRemoveAll)),
				new IoCFunction("append", new IoMethodFunc(IoList.slotAppend)),
                new IoCFunction("appendSeq", new IoMethodFunc(IoList.slotAppendSeq)),
                new IoCFunction("with", new IoMethodFunc(IoList.slotWith)),
				new IoCFunction("prepend", new IoMethodFunc(IoList.slotPrepend)),
				new IoCFunction("push", new IoMethodFunc(IoList.slotAppend)),
				new IoCFunction("at", new IoMethodFunc(IoList.slotAt)),
                new IoCFunction("last", new IoMethodFunc(IoList.slotLast)),
                new IoCFunction("pop", new IoMethodFunc(IoList.slotPop)),
				new IoCFunction("removeAt", new IoMethodFunc(IoList.slotRemoveAt)),
                new IoCFunction("reverseForeach", new IoMethodFunc(IoList.slotReverseForeach)),
			};

			pro.addTaglessMethodTable(state, methodTable);
			return pro;
		}


        public override IoObject clone(IoState state)
        {
            IoObject proto = state.protoWithInitFunc(name);
            IoList result = new IoList();
            uniqueIdCounter++;
            result.uniqueId = uniqueIdCounter;
            result.list = new IoObjectArrayList();
            result.state = state;
            result.createProtos();
            result.createSlots();
            result.protos.Add(proto);
            return result;
        }

        // Published Slots

        public static IoObject slotIndexOf(IoObject target, IoObject locals, IoObject m)
		{
			IoList o = target as IoList;
            IoObject value = (m as IoMessage).localsValueArgAt(locals, 1);
            try
            {
                return IoNumber.newWithDouble(target.state, o.list.IndexOf(value));
            }
            catch(ArgumentOutOfRangeException aoore)
            {
                object ex = aoore;
			    return target.state.ioNil;
            }
		}

        public static IoObject slotRemoveAll(IoObject target, IoObject locals, IoObject m)
		{
			IoList o = target as IoList;
			if (o.list != null)
            {
                o.list.Clear();
            }
			return target;
		}

        public static IoObject slotCapacity(IoObject target, IoObject locals, IoObject m)
		{
			IoList o = target as IoList;
			return IoNumber.newWithDouble(target.state, o.list.Capacity);
		}

        public static IoObject slotSize(IoObject target, IoObject locals, IoObject m)
		{
			IoList o = target as IoList;
            return IoNumber.newWithDouble(target.state, o.list.Count);
		}

        public void append(IoObject o)
        {
            this.list.Add(o);
        }

		public static IoObject slotAppend(IoObject target, IoObject locals, IoObject message)
		{
			IoMessage m = message as IoMessage;
			IoList o = target as IoList;

            for (int i = 0; i < m.args.Count; i++)
            {
			    IoObject obj = m.localsValueArgAt(locals, i);
                o.list.Add(obj);
            }
			return o;		
        }

        public static IoObject slotAppendSeq(IoObject target, IoObject locals, IoObject message)
        {
            IoMessage m = message as IoMessage;
            IoList o = target as IoList;

            for (int i = 0; i < m.args.Count; i++)
            {
                IoList obj = m.localsValueArgAt(locals, i) as IoList;
                for (int j = 0; j < obj.list.Count; j++)
                {
                    IoObject v = obj.list[j] as IoObject;
                    o.list.Add(v);
                }
            }
            return o;
        }

        public static IoObject slotWith(IoObject target, IoObject locals, IoObject message)
        {
            IoMessage m = message as IoMessage;
            IoList o = IoList.createObject(target.state) as IoList;

            for (int i = 0; i < m.args.Count; i++)
            {
                IoObject obj = m.localsValueArgAt(locals, i);
                o.list.Add(obj);
            }
            return o;
        }

        public static IoObject slotPrepend(IoObject target, IoObject locals, IoObject message)
        {
			return target;		
        }

        public static IoObject slotAt(IoObject target, IoObject locals, IoObject message)
        {
            IoMessage m = message as IoMessage;
            IoNumber ind = m.localsNumberArgAt(locals, 0);
            IoList o = target as IoList;
            IoObject v = o.list[ind.asInt()] as IoObject;
            return v == null ? target.state.ioNil : v;
        }

        public static IoObject slotLast(IoObject target, IoObject locals, IoObject message)
        {
            IoMessage m = message as IoMessage;
            IoList o = target as IoList;
            if (o.list.Count > 0)
            {
                IoObject e = o.list[o.list.Count - 1] as IoObject;
                return e;
            }
            return target.state.ioNil;
        }

        public static IoObject slotPop(IoObject target, IoObject locals, IoObject message)
        {
            IoMessage m = message as IoMessage;
            IoList o = target as IoList;
            if (o.list.Count > 0)
            {
                IoObject e = o.list[o.list.Count - 1] as IoObject;
                o.list.RemoveAt(o.list.Count - 1);
                return e;
            }
            else
            {
                return target.state.ioNil;
            }
        }

        /*
                public static IoObject slotAtInsert(IoObject target, IoObject locals, IoObject message)
                {
                }

                public static IoObject slotAtPut(IoObject target, IoObject locals, IoObject message)
                {
                }

                public static IoObject slotAtIfAbsentPut(IoObject target, IoObject locals, IoObject message)
                {
                }
        */
        public static IoObject slotContains(IoObject target, IoObject locals, IoObject message)
		{
            return null; // TODO: return IoBool
        }

        public static IoObject slotForeach(IoObject target, IoObject locals, IoObject message)
        {
            return null; // TODO: return IoBool
        }

        public static IoObject slotReverseForeach(IoObject target, IoObject locals, IoObject message)
        {
            return target; // TODO: return IoBool
        }

        public static IoObject slotRemoveAt(IoObject target, IoObject locals, IoObject message)
		{
			IoMessage m = message as IoMessage;
    		IoNumber ind = m.localsNumberArgAt(locals, 0);
			IoList o = target as IoList;
            try
            {
                o.list.RemoveAt(ind.asInt());
			    return target;
            }
            catch(ArgumentOutOfRangeException aoore)
            {
                object ex = aoore;
			    return target.state.ioNil;
            }
		}

		public override string ToString()
		{
			return uniqueId.ToString();
		}
	}
}
