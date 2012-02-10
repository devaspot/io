using System.Collections;
using System;
using System.Globalization;

namespace io {

    public class IoNumber : IoObject
    {
 		public override string name { get { return "Number"; } }
        public object value
        {
            get
            {
                if (isInteger)
                {
                    return longValue;
                }
                else
                {
                    return doubleValue;
                }
            }
        }
        public int accuracy = 0;
        public double doubleValue;
        public bool isInteger = true;
        public int longValue;

		public new static IoNumber createProto(IoState state)
		{
			IoNumber number = new IoNumber();
			return number.proto(state) as IoNumber;
		}

		public override IoObject proto(IoState state)
		{
			IoNumber pro = new IoNumber();
			pro.state = state;
            pro.createSlots();
            pro.createProtos();
            pro.doubleValue = 0;
            pro.longValue = 0;
            pro.isInteger = true;
            state.registerProtoWithFunc(name, new IoStateProto(pro.name, pro, new IoStateProtoFunc(pro.proto)));
			pro.protos.Add(state.protoWithInitFunc("Object"));

            IoCFunction[] methodTable = new IoCFunction[] {
                new IoCFunction("asNumber", new IoMethodFunc(IoNumber.slotAsNumber)),
                new IoCFunction("+", new IoMethodFunc(IoNumber.slotAdd)),
                new IoCFunction("-", new IoMethodFunc(IoNumber.slotSubstract)),
                new IoCFunction("*", new IoMethodFunc(IoNumber.slotMultiply)),                  
                new IoCFunction("/", new IoMethodFunc(IoNumber.slotDivide)),
                new IoCFunction("log10", new IoMethodFunc(IoNumber.slotLog10)),
                new IoCFunction("log2", new IoMethodFunc(IoNumber.slotLog2)),
                new IoCFunction("log", new IoMethodFunc(IoNumber.slotLog)),
                new IoCFunction("pow", new IoMethodFunc(IoNumber.slotPow)),
                new IoCFunction("pi", new IoMethodFunc(IoNumber.slotPi)),
                new IoCFunction("e", new IoMethodFunc(IoNumber.slotE)),
                new IoCFunction("minPositive", new IoMethodFunc(IoNumber.slotMinPositive)),
                new IoCFunction("exp", new IoMethodFunc(IoNumber.slotExp)),
                new IoCFunction("round", new IoMethodFunc(IoNumber.slotRound)),
//                new IoCFunction("asString", new IoMethodFunc(this.asString))
            };

			pro.addTaglessMethodTable(state, methodTable);
			return pro;
		}

        public static IoNumber newWithDouble(IoState state, double n)
        {
			IoNumber fab = new IoNumber();
			IoNumber num = state.protoWithInitFunc(fab.name) as IoNumber;
            num = num.clone(state) as IoNumber;
            num.isInteger = false;
            num.doubleValue = n;

            if (Double.Equals(n, 0) ||
                (!Double.IsInfinity(n) && !Double.IsNaN(n) &&
                !n.ToString(CultureInfo.InvariantCulture).Contains(".") &&
                !n.ToString(CultureInfo.InvariantCulture).Contains("E") &&
                !n.ToString(CultureInfo.InvariantCulture).Contains("e")
                )
            )
            {
                try
                {
                    num.longValue = Convert.ToInt32(n);
                    num.isInteger = true;
                }
                catch (OverflowException oe)
                {

                }
            }
			return num;
        }

		public override int GetHashCode()
		{
			return Convert.ToInt32(uniqueIdCounter);
		}

		public override string ToString()
		{
			return isInteger ? longValue.ToString(CultureInfo.InvariantCulture)
                : doubleValue.ToString("G",CultureInfo.InvariantCulture);
		}

        public override int compare(IoObject v)
        {
            IoNumber o = this as IoNumber;
            if (v is IoNumber)
            {
                if (Convert.ToDouble((v as IoNumber).value) == Convert.ToDouble(o.value))
                {
                    return 0;
                }
                double d = (v as IoNumber).isInteger ? (v as IoNumber).longValue : (v as IoNumber).doubleValue;
                double thisValue = o.isInteger ? o.longValue : o.doubleValue;

                return thisValue < d ? -1 : 1;
            }
            return base.compare(v);
        }

        public long asLong()
        {
            return Convert.ToInt64(value);
        }

        public int asInt()
        {
            return Convert.ToInt32(value);
        }

        public float asFloat()
        {
            return Convert.ToSingle(value);
        }

        public double asDouble()
        {
            return Convert.ToDouble(value);
        }

        public override void print()
        {
            Console.Write("{0}", this.ToString());
        }

        public static IoObject slotAsNumber(IoObject target, IoObject locals, IoObject message)
        {
            return target;
        }

        public static IoObject slotAdd(IoObject target, IoObject locals, IoObject message)
        {
            IoNumber other = (message as IoMessage).localsNumberArgAt(locals, 0);
            IoNumber self = target as IoNumber;
			if (other == null) return self;
            return IoNumber.newWithDouble(target.state,
                (self.isInteger ? self.longValue : self.doubleValue) +
                (other.isInteger ? other.longValue : other.doubleValue)
                    );
        }

        public static IoObject slotSubstract(IoObject target, IoObject locals, IoObject message)
        {
            IoNumber other = (message as IoMessage).localsNumberArgAt(locals, 0);
            IoNumber self = target as IoNumber;
            return IoNumber.newWithDouble(target.state,
                (self.isInteger ? self.longValue : self.doubleValue) -
                (other.isInteger ? other.longValue : other.doubleValue) 
                );
        }

        public static IoObject slotMultiply(IoObject target, IoObject locals, IoObject message)
        {
            IoNumber other = (message as IoMessage).localsNumberArgAt(locals, 0);
            IoNumber self = target as IoNumber;
            return IoNumber.newWithDouble(target.state,
                (self.isInteger ? self.longValue : self.doubleValue) *
                (other.isInteger ? other.longValue : other.doubleValue) 
                );
        }

        public static IoObject slotDivide(IoObject target, IoObject locals, IoObject message)
        {
            IoNumber other = (message as IoMessage).localsNumberArgAt(locals, 0);
            IoNumber self = target as IoNumber;
            return IoNumber.newWithDouble(target.state,
                (self.isInteger ? self.longValue : self.doubleValue) /
                (other.isInteger ? other.longValue : other.doubleValue) 
                );
        }

        public static IoObject slotLog10(IoObject target, IoObject locals, IoObject message)
        {
           // IoNumber other = (message as IoMessage).localsNumberArgAt(locals, 0);
            IoNumber self = target as IoNumber;
            return IoNumber.newWithDouble(target.state,
                Math.Log10(self.isInteger ? self.longValue : self.doubleValue)
                );
        }

        // setSlot("A", 32.45 + (20*(5836 log10)) + (20*(8.5 log10)))
        // !link-budget 22 0 13 126.3606841787141377 8 0


        public static IoObject slotLog2(IoObject target, IoObject locals, IoObject message)
        {
           // IoNumber other = (message as IoMessage).localsNumberArgAt(locals, 0);
            IoNumber self = target as IoNumber;
            return IoNumber.newWithDouble(target.state,
                Math.Log(self.isInteger ? self.longValue : self.doubleValue, 2)
                );
        }

        public static IoObject slotPi(IoObject target, IoObject locals, IoObject message)
        {
            IoNumber self = target as IoNumber;
            return IoNumber.newWithDouble(target.state, Math.PI);
        }

        public static IoObject slotMinPositive(IoObject target, IoObject locals, IoObject message)
        {
            IoNumber self = target as IoNumber;
            return IoNumber.newWithDouble(target.state, Double.Epsilon);
        }

        public static IoObject slotE(IoObject target, IoObject locals, IoObject message)
        {
            IoNumber self = target as IoNumber;
            return IoNumber.newWithDouble(target.state, Math.E);
        }

        public static IoObject slotLog(IoObject target, IoObject locals, IoObject message)
        {
            IoNumber other = (message as IoMessage).localsNumberArgAt(locals, 0);
            IoNumber self = target as IoNumber;
            return IoNumber.newWithDouble(target.state,
                Math.Log(self.isInteger ? self.longValue : self.doubleValue,
                other.isInteger ? other.longValue : other.doubleValue)
                );
        }

        public static IoObject slotPow(IoObject target, IoObject locals, IoObject message)
        {
            IoNumber other = (message as IoMessage).localsNumberArgAt(locals, 0);
            IoNumber self = target as IoNumber;
            return IoNumber.newWithDouble(target.state,
                Math.Pow(self.isInteger ? self.longValue : self.doubleValue,
                other.isInteger ? other.longValue : other.doubleValue)
                );
        }

        public static IoObject slotExp(IoObject target, IoObject locals, IoObject message)
        {
            IoNumber self = target as IoNumber;
            return IoNumber.newWithDouble(target.state,
                Math.Exp(self.isInteger ? self.longValue : self.doubleValue)
                );
        }

        public static IoObject slotRound(IoObject target, IoObject locals, IoObject message)
        {
            IoNumber self = target as IoNumber;
            return IoNumber.newWithDouble(target.state,
                Math.Round(self.isInteger ? self.longValue : self.doubleValue)
                );
        }

    }
}