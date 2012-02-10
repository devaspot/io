using System;
using System.Text;
using System.Diagnostics;

namespace io
{
    public class IoSeq : IoObject
    {
        public override string name { get { return "Sequence"; } }
        public string value = String.Empty;

        public char[] asCharArray { get { return value.ToCharArray(); } }

		public new static IoSeq createProto(IoState state)
		{
			IoSeq s = new IoSeq();
			return s.proto(state) as IoSeq;
		}

        public new static IoSeq createObject(IoState state)
        {
            IoSeq s = new IoSeq();
            return s.clone(state) as IoSeq;
        }

        public static IoSeq createObject(IoSeq symbol)
        {
            IoSeq seq = new IoSeq();
            seq = seq.clone(symbol.state) as IoSeq;
            seq.value = symbol.value;
            return seq;
        }

        public static IoSeq createObject(IoState state, string symbol)
        {
            IoSeq seq = new IoSeq();
            seq = seq.clone(state) as IoSeq;
            seq.value = symbol;
            return seq;
        }

        public static IoSeq createSymbolInMachine(IoState state, string symbol)
        {
            if (state.symbols[symbol] == null)
                state.symbols[symbol] = IoSeq.createObject(state, symbol);
            return state.symbols[symbol] as IoSeq;
        }

        public override IoObject proto(IoState state)
		{
			IoSeq pro = new IoSeq();
            pro.state = state;
		//	pro.tag.cloneFunc = new IoTagCloneFunc(this.clone);
        //    pro.tag.compareFunc = new IoTagCompareFunc(this.compare);
            pro.createSlots();
            pro.createProtos();
            state.registerProtoWithFunc(name, new IoStateProto(name, pro, new IoStateProtoFunc(this.proto)));
			pro.protos.Add(state.protoWithInitFunc("Object"));

            IoCFunction[] methodTable = new IoCFunction[] {
                new IoCFunction("appendSeq", new IoMethodFunc(IoSeq.slotAppendSeq)),
                new IoCFunction("at", new IoMethodFunc(IoSeq.slotAt)),
                new IoCFunction("reverse", new IoMethodFunc(IoSeq.slotReverse)),
			};

			pro.addTaglessMethodTable(state, methodTable);
			return pro;
		}

        public static IoObject slotAppendSeq(IoObject target, IoObject locals, IoObject message)
        {
            IoMessage m = message as IoMessage;
            IoSeq o = target as IoSeq;
            IoSeq arg = m.localsSymbolArgAt(locals, 0);
            o.value += arg.value.Replace(@"\""", "\"");
            return o;
        }

        public static IoObject slotAt(IoObject target, IoObject locals, IoObject message)
        {
            IoMessage m = message as IoMessage;
            IoSeq o = target as IoSeq;
            IoSeq res = IoSeq.createObject(target.state);
            IoNumber arg = m.localsNumberArgAt(locals, 0);
            res.value += o.value.Substring(arg.asInt(),1);
            return res;
        }

        public static IoObject slotReverse(IoObject target, IoObject locals, IoObject message)
        {
            IoMessage m = message as IoMessage;
            IoSeq o = target as IoSeq;
            IoSeq res = IoSeq.createObject(target.state);
            char[] A = o.asCharArray;
            Array.Reverse(A);
            res.value = new string(A);
            return res;
        }

		public override IoObject clone(IoState state)
		{
			IoSeq proto = state.protoWithInitFunc(name) as IoSeq;
			IoSeq result = new IoSeq();
			result.state = state;
            result.value = proto.value;
			result.createProtos();
			result.createSlots();
			result.protos.Add(proto);
			return result;
		}

        public override int compare(IoObject v)
        {
			if (v is IoSeq) return this.value.CompareTo((v as IoSeq).value);
            return base.compare(v);
        }

        public override void print()
        {
            Console.Write("{0}", value);
        }

        public override string ToString()
        {
            return value.ToString();
        }

        public static IoSeq rawAsUnquotedSymbol(IoSeq s)
        {
            string str = "";
            if (s.value.StartsWith("\"")) str = s.value.Substring(1, s.value.Length - 1);
            if (s.value.EndsWith("\"")) str = str.Substring(0,s.value.Length-2);
            return IoSeq.createObject(s.state, str);
        }

        public static IoSeq rawAsUnescapedSymbol(IoSeq s)
        {
            string str = "";
            int i = 0;
            while (i < s.value.Length)
            {
                char c = s.value[i];
                if (c != '\\')
                {
                    str += c;
                }
                else
                {
                    c = s.value[i];
                    switch (c)
                    {
                        case 'a': c = '\a'; break;
                        case 'b': c = '\b'; break;
                        case 'f': c = '\f'; break;
                        case 'n': c = '\n'; break;
                        case 'r': c = '\r'; break;
                        case 't': c = '\t'; break;
                        case 'v': c = '\v'; break;
                        case '\0': c = '\\'; break;
                        default:
                            if (c > '0' && c < '9')
                            {
                                c -= '0';
                            }
                            break;
                    }
                    str += c;
                }

                i++;
            }
            return IoSeq.createObject(s.state, str);
        }
    }
}
