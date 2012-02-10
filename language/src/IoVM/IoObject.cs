
using System;
using System.Collections;

namespace io
{
    // DEBUG HELPER

    public class IoObjectArrayList : ArrayList
    {
        public override string ToString()
        {
            string s = " (";
            for (int i = 0; i < Count; i++)
            {
                s += base[i].ToString();
                if (i != Count - 1)
                    s += ",";
            }
            s += ")";
            return s;
        }
    }

    // SYMBOL HANDLING HELPER

    public class IoSeqObjectHashtable : Hashtable
    {
        public IoState state = null;
        public IoSeqObjectHashtable(IoState s) { state = s; }
        public override object this[object key]
        {
            get
            {
                return base[state.IOSYMBOL(key.ToString())];
            }
            set
            {
                base[state.IOSYMBOL(key.ToString())] = value;
            }
        }
    }

    public class IoStateProto
    {
        public string name;
        public IoStateProtoFunc func;
        public IoObject proto;
        public IoStateProto(string name, IoObject proto, IoStateProtoFunc func)
        {
            this.name = name;
            this.func = func;
            this.proto = proto;
        }
    }

    public delegate IoObject IoStateProtoFunc(IoState state);

    public class IoObject
    {
		public IoState _state = null;
		public IoState state { set { _state = value; 
			if (slots != null) slots.state = value; } get { return _state; } }
        public static long uniqueIdCounter = 0;
        public long uniqueId = 0;
        public virtual string name { get { return "Object"; } }
        public IoSeqObjectHashtable slots;
        public IoObjectArrayList listeners;
        public IoObjectArrayList protos;
        public bool hasDoneLookup;
        public bool isActivatable;
        public bool isLocals;

        public static IoObject createProto(IoState state)
        {
            IoObject pro = new IoObject();
            return pro.proto(state);
        }

        public static IoObject createObject(IoState state)
        {
            IoObject pro = new IoObject();
            return pro.clone(state);
        }
		
        public virtual IoObject proto(IoState state)
        {
            IoObject pro = new IoObject();
            pro.state = state;
            pro.createSlots();
            pro.createProtos();
            pro.uniqueId = 0;
            state.registerProtoWithFunc(name, new IoStateProto(pro.name, pro, new IoStateProtoFunc(pro.proto)));
            return pro;
        }

        public virtual IoObject clone(IoState state)
        {
            IoObject proto = state.protoWithInitFunc(name);
            IoObject o = Activator.CreateInstance(this.GetType()) as IoObject;//typeof(this)new IoObject();
            uniqueIdCounter++;
            o.uniqueId = uniqueIdCounter;
			o.state = proto.state;
            o.createSlots();
            o.createProtos();
            o.protos.Add(proto);
			cloneSpecific(this, o);
            return o;
        }

		public virtual void cloneSpecific(IoObject from, IoObject to) 
		{
		}

        // proto finish must be called only before first Sequence proto created

        public IoObject protoFinish(IoState state)
        {
            IoCFunction[] methodTable = new IoCFunction[] {
                new IoCFunction("compare", new IoMethodFunc(IoObject.slotCompare)),
                new IoCFunction("==", new IoMethodFunc(IoObject.slotEquals)),
                new IoCFunction("!=", new IoMethodFunc(IoObject.slotNotEquals)),
                new IoCFunction(">=", new IoMethodFunc(IoObject.slotGreaterThanOrEqual)),
                new IoCFunction("<=", new IoMethodFunc(IoObject.slotLessThanOrEqual)),
                new IoCFunction(">", new IoMethodFunc(IoObject.slotGreaterThan)),
                new IoCFunction("<", new IoMethodFunc(IoObject.slotLessThan)),
                new IoCFunction("-", new IoMethodFunc(IoObject.slotSubstract)),
                new IoCFunction("", new IoMethodFunc(IoObject.slotEevalArg)),
                new IoCFunction("self", new IoMethodFunc(IoObject.slotSelf)),
                new IoCFunction("clone", new IoMethodFunc(IoObject.slotClone)),
                new IoCFunction("return", new IoMethodFunc(IoObject.slotReturn)),
                new IoCFunction("cloneWithoutInit", new IoMethodFunc(IoObject.slotCloneWithoutInit)),
                new IoCFunction("doMessage", new IoMethodFunc(IoObject.slotDoMessage)),
                new IoCFunction("print", new IoMethodFunc(IoObject.slotPrint)),
                new IoCFunction("println", new IoMethodFunc(IoObject.slotPrintln)),
                new IoCFunction("slotNames", new IoMethodFunc(IoObject.slotSlotNames)),
                new IoCFunction("type", new IoMethodFunc(IoObject.slotType)),
                new IoCFunction("evalArg", new IoMethodFunc(IoObject.slotEevalArg)),
                new IoCFunction("evalArgAndReturnSelf", new IoMethodFunc(IoObject.slotEevalArgAndReturnSelf)),
                new IoCFunction("do", new IoMethodFunc(IoObject.slotDo)),
                new IoCFunction("getSlot", new IoMethodFunc(IoObject.slotGetSlot)),
                new IoCFunction("updateSlot", new IoMethodFunc(IoObject.slotUpdateSlot)),
                new IoCFunction("setSlot", new IoMethodFunc(IoObject.slotSetSlot)),
                new IoCFunction("setSlotWithType", new IoMethodFunc(IoObject.slotSetSlotWithType)),
                new IoCFunction("message", new IoMethodFunc(IoObject.slotMessage)),
                new IoCFunction("method", new IoMethodFunc(IoObject.slotMethod)),
                new IoCFunction("block", new IoMethodFunc(IoObject.slotBlock)),
                new IoCFunction("init", new IoMethodFunc(IoObject.slotSelf)),
                new IoCFunction("thisContext", new IoMethodFunc(IoObject.slotSelf)),
                new IoCFunction("thisMessage", new IoMethodFunc(IoObject.slotThisMessage)),
                new IoCFunction("thisLocals", new IoMethodFunc(IoObject.slotThisLocals)),
                new IoCFunction("init", new IoMethodFunc(IoObject.slotSelf)),
                new IoCFunction("if", new IoMethodFunc(IoObject.slotIf)),
                new IoCFunction("yield", new IoMethodFunc(IoObject.slotYield)),
                new IoCFunction("@@", new IoMethodFunc(IoObject.slotAsyncCall)),
                new IoCFunction("yieldingCoros", new IoMethodFunc(IoObject.slotYieldingCoros)),
                new IoCFunction("while", new IoMethodFunc(IoObject.slotWhile))
            };
            IoObject o = state.protoWithInitFunc(name);
            o.addTaglessMethodTable(state, methodTable);
            return o;
        }

        // Published Slots

        public static IoObject slotCompare(IoObject self, IoObject locals, IoObject message)
        {
            IoMessage m = message as IoMessage;
            IoObject o = m.localsValueArgAt(locals, 0);
            return IoNumber.newWithDouble(self.state, Convert.ToDouble(self.compare(o)));
        }

        public virtual int compare(IoObject v)
        {
			return uniqueId.CompareTo(v.uniqueId);
        }

        public static IoObject slotEquals(IoObject self, IoObject locals, IoObject message)
        {
            IoMessage m = message as IoMessage;
            IoObject o = m.localsValueArgAt(locals, 0);
            return self.compare(o) == 0 ? self.state.ioTrue : self.state.ioFalse;
        }

        public static IoObject slotNotEquals(IoObject self, IoObject locals, IoObject message)
        {
            IoMessage m = message as IoMessage;
            IoObject o = m.localsValueArgAt(locals, 0);
            return self.compare(o) != 0 ? self.state.ioTrue : self.state.ioFalse;
        }

        public static IoObject slotGreaterThanOrEqual(IoObject self, IoObject locals, IoObject message)
        {
            IoMessage m = message as IoMessage;
            IoObject o = m.localsValueArgAt(locals, 0);
            return self.compare(o) >= 0 ? self.state.ioTrue : self.state.ioFalse;
        }

        public static IoObject slotLessThanOrEqual(IoObject self, IoObject locals, IoObject message)
        {
            IoMessage m = message as IoMessage;
            IoObject o = m.localsValueArgAt(locals, 0);
            return self.compare(o) <= 0 ? self.state.ioTrue : self.state.ioFalse;
        }

        public static IoObject slotLessThan(IoObject self, IoObject locals, IoObject message)
        {
            IoMessage m = message as IoMessage;
            IoObject o = m.localsValueArgAt(locals, 0);
            return self.compare(o) < 0 ? self.state.ioTrue : self.state.ioFalse;
        }

        public static IoObject slotSubstract(IoObject self, IoObject locals, IoObject message)
        {
            IoMessage m = message as IoMessage;
            IoNumber o = m.localsNumberArgAt(locals, 0);
            return IoNumber.newWithDouble(self.state, - o.asDouble());
        }

        public static IoObject slotGreaterThan(IoObject self, IoObject locals, IoObject message)
        {
            IoMessage m = message as IoMessage;
            IoObject o = m.localsValueArgAt(locals, 0);
            return self.compare(o) > 0 ? self.state.ioTrue : self.state.ioFalse;
        }

        public static IoObject slotSelf(IoObject self, IoObject locals, IoObject m)
        {
            return self;
        }

        public static IoObject slotThisMessage(IoObject self, IoObject locals, IoObject m)
        {
            return m;
        }

        public static IoObject slotThisLocals(IoObject self, IoObject locals, IoObject m)
        {
            return locals;
        }

        public static IoObject slotClone(IoObject target, IoObject locals, IoObject m)
        {
            //IoObject newObject = target.tag.cloneFunc(target.state);
            IoObject newObject = target.clone(target.state);
            //newObject.protos.Clear();
            newObject.protos.Add(target);
            return target.initClone(target, locals, m as IoMessage, newObject);
        }

        public static IoObject slotReturn(IoObject target, IoObject locals, IoObject message)
        {
            IoMessage m = message as IoMessage;
            IoObject v = m.localsValueArgAt(locals, 0);
            target.state.Return(v);
            return target;
        }

        public static IoObject slotCloneWithoutInit(IoObject target, IoObject locals, IoObject m)
        {
            return target.clone(target.state);
        }

        public static IoObject slotDoMessage(IoObject self, IoObject locals, IoObject m)
        {
            IoMessage msg = m as IoMessage;
            IoMessage aMessage = msg.localsMessageArgAt(locals, 0) as IoMessage;
            IoObject context = self;
            if (msg.args.Count >= 2)
            {
                context = msg.localsValueArgAt(locals, 1);
            }
            return aMessage.localsPerformOn(context, self);
        }

        public static IoObject slotPrint(IoObject target, IoObject locals, IoObject m)
        {
            target.print();
            return target;
        }

        public static IoObject slotPrintln(IoObject target, IoObject locals, IoObject m)
        {
            target.print();
            Console.WriteLine();
            return target;
        }

        public static IoObject slotSlotNames(IoObject target, IoObject locals, IoObject message)
        {
            if (target.slots == null || target.slots.Count == 0) return target;
            foreach (object key in target.slots.Keys)
            {
                Console.Write(key.ToString() + " ");
            }
            Console.WriteLine();
            return target;
        }

        public static IoObject slotType(IoObject target, IoObject locals, IoObject message)
        {
			return IoSeq.createObject(target.state, target.name);
        }

        public static IoObject slotEevalArg(IoObject target, IoObject locals, IoObject message)
        {
            IoMessage m = message as IoMessage;
            return m.localsValueArgAt(locals, 0);
        }

        public static IoObject slotEevalArgAndReturnSelf(IoObject target, IoObject locals, IoObject message)
        {
            IoObject.slotEevalArg(target, locals, message);
            return target;
        }

        public static IoObject slotDo(IoObject target, IoObject locals, IoObject message)
        {
            IoMessage m = message as IoMessage;
            if (m.args.Count != 0)
            {
                IoMessage argMessage = m.rawArgAt(0);
                argMessage.localsPerformOn(target, target);
            }
            return target;
        }

        public static IoObject slotGetSlot(IoObject target, IoObject locals, IoObject message)
        {
            IoMessage m = message as IoMessage;
            IoSeq slotName = m.localsSymbolArgAt(locals, 0);
            IoObject slot = target.rawGetSlot(slotName);
            return slot == null ? target.state.ioNil : slot;
        }

        public static IoObject slotSetSlot(IoObject target, IoObject locals, IoObject message)
        {
            IoMessage m = message as IoMessage;
            IoSeq slotName = m.localsSymbolArgAt(locals, 0);
            IoObject slotValue = m.localsValueArgAt(locals, 1);
            if (slotName == null) return target;
            target.slots[slotName] = slotValue;
            return slotValue;
        }

        public static IoObject localsUpdateSlot(IoObject target, IoObject locals, IoObject message)
        {
            IoMessage m = message as IoMessage;
            IoSeq slotName = m.localsSymbolArgAt(locals, 0);
            if (slotName == null) return target;
            IoObject obj = target.rawGetSlot(slotName);
            if (obj != null)
            {
                IoObject slotValue = m.localsValueArgAt(locals, 1);
                target.slots[slotName] = slotValue;
                return slotValue;
            }
            else
            {
                IoObject theSelf = target.rawGetSlot(target.state.selfMessage.messageName);
                if (theSelf != null)
                {
                    return theSelf.perform(theSelf, locals, m);
                }
            }
            return target.state.ioNil;
        }

        public static IoObject slotUpdateSlot(IoObject target, IoObject locals, IoObject message)
        {
            IoMessage m = message as IoMessage;
            IoSeq slotName = m.localsSymbolArgAt(locals, 0);
            IoObject slotValue = m.localsValueArgAt(locals, 1);
            if (slotName == null) return target;

            if (target.rawGetSlot(slotName) != null)
            {
                target.slots[slotName] = slotValue;
            }
            else
            {
                Console.WriteLine("Slot {0} not found. Must define slot using := operator before updating.", slotName.value);
            }
            
            return slotValue;
        }

        public static IoObject slotSetSlotWithType(IoObject target, IoObject locals, IoObject message)
        {
            IoMessage m = message as IoMessage;
            IoSeq slotName = m.localsSymbolArgAt(locals, 0);
            IoObject slotValue = m.localsValueArgAt(locals, 1);
            target.slots[slotName] = slotValue;
            if (slotValue.slots[target.state.typeSymbol] == null)
            {
                slotValue.slots[target.state.typeSymbol] = slotName;
            }
            return slotValue;
        }

        public static IoObject slotMessage(IoObject target, IoObject locals, IoObject message)
        {
            IoMessage m = message as IoMessage;
            return m.args.Count > 0 ? m.rawArgAt(0) : target.state.ioNil;
        }

        public static IoObject slotMethod(IoObject target, IoObject locals, IoObject message)
        {
            return IoBlock.slotMethod(target, locals, message);
        }

        public static IoObject slotBlock(IoObject target, IoObject locals, IoObject message)
        {
            return IoBlock.slotBlock(target, locals, message);
        }

        public static IoObject slotLocalsForward(IoObject target, IoObject locals, IoObject message)
        {
            IoObject o = target.slots[target.state.selfSymbol] as IoObject;
            if (o != null && o != target)
                return target.perform(o, locals, message);
            return target.state.ioNil;
        }

        public static IoObject slotIf(IoObject target, IoObject locals, IoObject message)
        {
            IoMessage m = message as IoMessage;
            IoObject r = m.localsValueArgAt(locals, 0);
            bool condition = r != target.state.ioNil && r != target.state.ioFalse;
	        int index = condition ? 1 : 2;
	        if (index < m.args.Count) 
		        return m.localsValueArgAt(locals, index);
            return condition ? target.state.ioTrue : target.state.ioFalse;
        }

        public static IoObject slotAsyncCall(IoObject target, IoObject locals, IoObject message)
        {
            IoMessage msg = message as IoMessage;
            IoMessage aMessage = msg.rawArgAt(0);
            IoObject context = target;
            if (msg.args.Count >= 2)
            {
                context = msg.localsValueArgAt(locals, 1);
            }

            IoBlock o = target.rawGetSlot(aMessage.messageName) as IoBlock;
            if (o != null)
            {
                IoMessage mmm = o.containedMessage;
                mmm.async = true;


                IoContext ctx = new IoContext();
                ctx.target = context;
                ctx.locals = target;
                ctx.message = mmm;
                mmm.async = true;
                IoState state = target.state;
                IoObject future = IoObject.createObject(state);
                IEnumerator e = IoMessage.asyncCall(ctx, future);
                state.contextList.Add(e);
                return future;
            }
            else
            {
                IoCFunction cf = target.rawGetSlot(aMessage.messageName) as IoCFunction;
                if (cf != null)
                {
                    cf.async = true;
                    return cf.activate(target, locals, aMessage, null);
                }
            }
            return aMessage.localsPerformOn(target, locals);
        }

        public static IoObject slotYieldingCoros(IoObject target, IoObject locals, IoObject message) {
            return IoNumber.newWithDouble(target.state, target.state.contextList.Count);
        }

        public static IoObject slotYield(IoObject target, IoObject locals, IoObject message)
        {
            IoState state = target.state;
            ArrayList toDeleteThread = new ArrayList();
            for (int i = 0; i < state.contextList.Count; i++) {
                IEnumerator e  = state.contextList[i] as IEnumerator;
                bool end = e.MoveNext();
                if (!end) toDeleteThread.Add(e);
            }
            foreach (object e in toDeleteThread)
                state.contextList.Remove(e);
            return IoNumber.newWithDouble(state, state.contextList.Count);
        }

        public class EvaluateArgsEventArgs : EventArgs
        {
            public int Position = 0;
            public EvaluateArgsEventArgs(int pos) { Position = pos;  }
        }

        public delegate void EvaluateArgsEventHandler(IoMessage msg, EvaluateArgsEventArgs e, out IoObject res);

        public static IEnumerator slotAsyncWhile(IoObject target, IoObject locals, IoObject message, IoObject future)
        {
            IoMessage m = message as IoMessage;
            IoObject result = target.state.ioNil;
            IoObject cond = null;

            while (true)
            {
                cond = m.localsValueArgAt(locals, 0);
                //evaluateArgs(m, new EvaluateArgsEventArgs(0), out cond);

                if (cond == target.state.ioFalse || cond == target.state.ioNil)
                {
                    break;
                }

                //result = m.localsValueArgAt(locals, 1);
                //evaluateArgs(m, new EvaluateArgsEventArgs(1), out result);

                IoMessage msg = 1 < m.args.Count ? m.args[1] as IoMessage : null;
                if (msg != null)
                {
                    if (msg.cachedResult != null && msg.next == null)
                    {
                        result = msg.cachedResult;
                        yield return result;
                    }
                    //result = localMessage.localsPerformOn(locals, locals);

                    result = target;
                    IoObject cachedTarget = target;
                    IoObject savedPrevResultAsYieldResult = null;

                    do
                    {
                        if (msg.messageName.Equals(msg.state.semicolonSymbol))
                        {
                            target = cachedTarget;
                        }
                        else
                        {
                            result = msg.cachedResult;
                            if (result == null)
                            {
                                if (msg.messageName.value.Equals("yield"))
                                {
                                    yield return result;
                                }
                                else
                                {
                                    result = target.perform(target, locals, msg);
                                }
                            }
                            if (result == null)
                            {
                                target = cachedTarget;
                                //result = savedPrevResultAsYieldResult;
                            }
                            else
                            {
                                target = result;
                            }
                            savedPrevResultAsYieldResult = result;
                        }
                    } while ((msg = msg.next) != null);
                    future.slots["future"] = result;

                    yield return null;
                }

                result = m.state.ioNil;

                if (target.state.handleStatus() != 0)
                {
                    goto done;
                }

            }
        done:
            yield return null;
        }

        public static IoObject slotWhile(IoObject target, IoObject locals, IoObject message)
        {
            IoMessage m = message as IoMessage;

            if (m.async)
            {
                IoState state = target.state;
                IoObject future = IoObject.createObject(state);
                IEnumerator e = IoObject.slotAsyncWhile(target, locals, message, future);
                state.contextList.Add(e);
                return future;
            }

            IoObject result = target.state.ioNil;
            while (true)
            {
                bool sasync = m.async;
                m.async = false;
                IoObject cond = m.localsValueArgAt(locals, 0);
                if (cond == target.state.ioFalse || cond == target.state.ioNil)
                {
                    break;
                }
                m.async = sasync;
                result = m.localsValueArgAt(locals, 1);
                if (target.state.handleStatus() != 0)
                {
                    goto done;
                }

            }
done:
            return result;
        }

        // Object Public Raw Methods
        
        public IoObject initClone(IoObject target, IoObject locals, IoMessage m, IoObject newObject)
        {
            IoObject context = null;
            IoObject initSlot = target.rawGetSlotContext(target.state.initMessage.messageName, out context);
            if (initSlot != null)
               initSlot.activate(initSlot, newObject, locals, target.state.initMessage, context);
            return newObject;
        }

        public void addTaglessMethodTable(IoState state, IoCFunction[] table)
        {
            //foreach (IoMethodTableEntry entry in table)
            //    slots[entry.name] = new IoCFunction(state, entry.name, entry.func);
            foreach (IoCFunction entry in table)
            {
                entry.state = state;
                slots[entry.funcName] = entry;
            }
        }

        public IoObject forward(IoObject target, IoObject locals, IoObject message)
        {
            IoMessage m = message as IoMessage;
            IoObject context = null;
            IoObject forwardSlot = target.rawGetSlotContext(target.state.forwardMessage.messageName, out context);
            
            //if (forwardSlot != null)
            //    return forwardSlot.activate(forwardSlot, locals, m, context);

            Console.WriteLine("'{0}' does not respond to message '{1}'",
                target.name, m.messageName.ToString());
            return target;
        }

	    public IoObject perform(IoObject target, IoObject locals, IoObject message)
	    {
	    	IoMessage msg = message as IoMessage;
            IoObject context = null;
            IoObject slotValue = target.rawGetSlotContext(msg.messageName, out context);
            
            if (slotValue == null)
                slotValue = target.clrGetSlot(msg);
	    	
            if (slotValue != null)
	    	{
                return slotValue.activate(slotValue, target, locals, msg, context);
	    	}
	    	if (target.isLocals)
	    	{
	    		return IoObject.slotLocalsForward(target, locals, message);
	    	}
	    	return target.forward(target, locals, message);
	    }

        public IoObject localsProto(IoState state)
        {
            
            IoObject obj = IoObject.createObject(state);
            IoObject firstProto = obj.protos[0] as IoObject;
            foreach (object key in firstProto.slots.Keys)
                obj.slots[key] = firstProto.slots[key];
            firstProto.protos.Clear();
            obj.slots["setSlot"] = new IoCFunction(state, "setSlot", new IoMethodFunc(IoObject.slotSetSlot));
            obj.slots["setSlotWithType"] = new IoCFunction(state, "setSlotWithType", new IoMethodFunc(IoObject.slotSetSlotWithType));
            obj.slots["updateSlot"] = new IoCFunction(state, "updateSlot", new IoMethodFunc(IoObject.localsUpdateSlot));
            obj.slots["thisLocalContext"] = new IoCFunction(state, "thisLocalContext", new IoMethodFunc(IoObject.slotThisLocals));
            obj.slots["forward"] = new IoCFunction(state, "forward", new IoMethodFunc(IoObject.slotLocalsForward));
            return obj;
        }

		
        public virtual IoObject activate(IoObject self, IoObject target, IoObject locals, IoMessage m, IoObject slotContext)
        {
			return self.isActivatable ? self.activate(self, target, locals, m) : self;
        }
		
        public IoObject activate(IoObject self, IoObject target, IoObject locals, IoMessage m)
        {
            if (self.isActivatable)
            {
                IoObject context = null;
                IoObject slotValue = self.rawGetSlotContext(self.state.activateMessage.messageName, out context);
                if (slotValue != null)
                {
					// ?? мы шо в цикле ???
					return activate(slotValue, target, locals, m, context); 
                }
				return state.ioNil;
            } else
				return self;
        }

        public void createSlots()
        {
            if (slots == null)
                slots = new IoSeqObjectHashtable(state);
			if (state == null)
			{
				int x = 0;
			}
        }

        public void createProtos()
        {
            if (protos == null)
                protos = new IoObjectArrayList();
        }

		public IoObject slotsBySymbol(IoSeq symbol)
		{
            IoSeq s = this.state.symbols[symbol.value] as IoSeq;
            if (s == null) return null;
            return slots[s] as IoObject;
		}

        public IoObject rawGetSlot(IoSeq slot)
        {
            IoObject context = null;
            IoObject v = rawGetSlotContext(slot, out context);
            return v;
        }

        public IoObject clrGetSlot(IoMessage message)
        {
            IoObject v = null;
            if (this is IoCLRObject)
            {
                v = (this as IoCLRObject).getMethod(message);
            }
            if (v == null)
                v = this.state.clrProto.getType(this.state, message.messageName.value);
            return v;
        }

        public IoObject rawGetSlotContext(IoSeq slot, out IoObject context)
        {
            if (slot == null)
            {
                context = null;
                return null;
            }
            IoObject v = null;
            context = null;
            if (slotsBySymbol(slot) != null)
            {
                v = slotsBySymbol(slot) as IoObject;
                if (v != null)
                {
                    context = this;
                    return v;
                }
            }
            hasDoneLookup = true;
            foreach (IoObject proto in protos)
            {
                if (proto.hasDoneLookup)
                    continue;
                v = proto.rawGetSlotContext(slot, out context);
                if (v != null) break;
            }
            hasDoneLookup = false;

            return v;
        }

		public virtual void print()
		{
            //IoSeq type = this.slots["type"] as IoSeq;
			//if (type == null)
			//		type = (this.rawGetSlot(state.typeMessage.messageName) as IoCFunction).func(this, this, this) as IoSeq;
            //string printedName = type == null ? ToString() : type.value;
			Console.Write(this);
		}

		public override string ToString()
		{
			return name + "+" + uniqueId;
		}
	}

}
