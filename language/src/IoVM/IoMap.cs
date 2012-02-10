
using System.Collections;
using System;

namespace io {

	public class IoMap : IoObject
    {
		public override string name { get { return "Map"; } }
		public Hashtable map = new Hashtable();

		public new static IoMap createProto(IoState state)
		{
			IoMap m = new IoMap();
			return m.proto(state) as IoMap;
		}

		public override IoObject proto(IoState state)
		{
			IoMap pro = new IoMap();
            pro.state = state;
            pro.createSlots();
            pro.createProtos();
            pro.map = new Hashtable(); 
            state.registerProtoWithFunc(pro.name, new IoStateProto(pro.name, pro, new IoStateProtoFunc(pro.proto)));
			pro.protos.Add(state.protoWithInitFunc("Object"));

            IoCFunction[] methodTable = new IoCFunction[] {
				new IoCFunction("at", new IoMethodFunc(IoMap.slotAt)),
				new IoCFunction("atPut", new IoMethodFunc(IoMap.slotAtPut)),
				new IoCFunction("atIfAbsentPut", new IoMethodFunc(IoMap.slotAtIfAbsentPut)),
				new IoCFunction("empty", new IoMethodFunc(IoMap.slotEmpty)),
				new IoCFunction("size", new IoMethodFunc(IoMap.slotSize)),
				new IoCFunction("removeAt", new IoMethodFunc(IoMap.slotRemoveAt)),
				new IoCFunction("hasKey", new IoMethodFunc(IoMap.slotHasKey)),
				new IoCFunction("hasValue", new IoMethodFunc(IoMap.slotHasValue)),
			};

			pro.addTaglessMethodTable(state, methodTable);
			return pro;
		}

		public override void cloneSpecific(IoObject from, IoObject to)
		{
			(to as IoMap).map = (from as IoMap).map.Clone() as Hashtable;
		}

		public static IoObject slotEmpty(IoObject target, IoObject locals, IoObject m)
		{
			IoMap dict = target as IoMap;
			if (dict.map != null) dict.map.Clear();
			return target;
		}

        public static IoObject slotSize(IoObject target, IoObject locals, IoObject m)
		{
			IoMap dict = target as IoMap;
			if (dict.map != null) return IoNumber.newWithDouble(dict.state, dict.map.Count);
			return dict.state.ioNil;
		}

		public object lookupMap(object k)
		{
			foreach (object key in map.Keys)
				if (key.ToString().Equals(k.ToString())) return map[key];
			return null;
		}

		public object lookupMapValues(object v)
		{
			foreach (object val in map.Values)
				if (val.Equals(v)) return val;
			return null;
		}

        public static IoObject slotAt(IoObject target, IoObject locals, IoObject message)
		{
			IoMessage m = message as IoMessage;
			IoObject result = null;
			IoObject symbol = m.localsValueArgAt(locals, 0);
			IoMap dict = target as IoMap;
			result = dict.lookupMap(symbol) as IoObject;
			if (result == null && m.args.Count > 1) {
				result = m.localsValueArgAt(locals, 1);
			}
			return result == null ? dict.state.ioNil : result;
		}

        public static IoObject slotAtPut(IoObject target, IoObject locals, IoObject message)
		{
			IoMessage m = message as IoMessage;
			IoObject key = m.localsValueArgAt(locals, 0);
			IoObject value = m.localsValueArgAt(locals, 1);
			IoMap dict = target as IoMap;
			dict.map[key.ToString()] = value;
			return target;
		}

        public static IoObject slotAtIfAbsentPut(IoObject target, IoObject locals, IoObject message)
		{
			IoMessage m = message as IoMessage;
			IoObject key = m.localsValueArgAt(locals, 0);
			IoObject value = m.localsValueArgAt(locals, 1);
			IoMap dict = target as IoMap;
			if (dict.lookupMap(key) == null) 
				dict.map[key.ToString()] = value;
			return target;
		}

        public static IoObject slotRemoveAt(IoObject target, IoObject locals, IoObject message)
		{
			IoMessage m = message as IoMessage;
			IoObject key = m.localsSymbolArgAt(locals, 0);
			IoMap dict = target as IoMap;
			dict.map[key.ToString()] = null;
			return target;
		}

        public static IoObject slotHasKey(IoObject target, IoObject locals, IoObject message)
		{
			IoMap dict = target as IoMap;
			IoMessage m = message as IoMessage;
			IoObject key = m.localsValueArgAt(locals, 0);
            if (dict.lookupMap(key) == null)
            {
				return dict.state.ioFalse;
			}
			return dict.state.ioTrue;
		}

		public static IoObject slotHasValue(IoObject target, IoObject locals, IoObject message)
		{
			IoMap dict = target as IoMap;
			IoMessage m = message as IoMessage;
			IoObject val = m.localsValueArgAt(locals, 0);
			if (dict.lookupMapValues(val) == null)
			{
				return dict.state.ioFalse;
			}
			return dict.state.ioTrue;
		}

	}

}
