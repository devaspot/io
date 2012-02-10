
using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

namespace io {

	public class IoMessage : IoObject
    {
        public bool async = false;
		public override string name { get { return "Message"; } }
        public IoSeq  messageName;
        public IoObjectArrayList args;
		public IoMessage next;
		public IoObject cachedResult;
		public int lineNumber;
		public IoSeq label;

		public new static IoMessage createProto(IoState state)
		{
			IoMessage m = new IoMessage();
			return m.proto(state) as IoMessage;
		}

        public new static IoMessage createObject(IoState state)
        {
            IoMessage pro = new IoMessage();
            return pro.clone(state) as IoMessage;
        }

		public override IoObject proto(IoState state)
		{
			IoMessage pro = new IoMessage();
            pro.state = state;
          //  pro.tag.cloneFunc = new IoTagCloneFunc(this.clone);
			//pro.tag.activateFunc = new IoTagActivateFunc(this.activate);
            pro.createSlots();
            pro.createProtos();
            pro.uniqueId = 0;
            pro.messageName = IoSeq.createSymbolInMachine(state, "anonymous");
            pro.label = IoSeq.createSymbolInMachine(state, "unlabeled");
            pro.args = new IoObjectArrayList();
            state.registerProtoWithFunc(name, new IoStateProto(name, pro, new IoStateProtoFunc(this.proto)));
			pro.protos.Add(state.protoWithInitFunc("Object"));

            IoCFunction[] methodTable = new IoCFunction[] {
                new IoCFunction("name", new IoMethodFunc(IoMessage.slotName)),
                new IoCFunction("setName", new IoMethodFunc(IoMessage.slotSetName)),
                new IoCFunction("next", new IoMethodFunc(IoMessage.slotNext)),
                new IoCFunction("setNext", new IoMethodFunc(IoMessage.slotSetNext)),
                new IoCFunction("code", new IoMethodFunc(IoMessage.slotCode)),
                new IoCFunction("arguments", new IoMethodFunc(IoMessage.slotArguments)),
                new IoCFunction("appendArg", new IoMethodFunc(IoMessage.slotAppendArg)),
                new IoCFunction("argAt", new IoMethodFunc(IoMessage.slotArgAt)),
                new IoCFunction("argCount", new IoMethodFunc(IoMessage.slotArgCount)),
                new IoCFunction("asString", new IoMethodFunc(IoMessage.slotCode)),
                new IoCFunction("cachedResult", new IoMethodFunc(IoMessage.slotCachedResult)),
	            new IoCFunction("setCachedResult", new IoMethodFunc(IoMessage.slotSetCachedResult)),
                new IoCFunction("removeCachedResult", new IoMethodFunc(IoMessage.slotRemoveCachedResult)),
                new IoCFunction("hasCachedResult", new IoMethodFunc(IoMessage.slotHasCachedResult)),

            };

            pro.addTaglessMethodTable(state, methodTable);
			return pro;
		}

		public override void cloneSpecific(IoObject _from, IoObject _to)
		{
			IoMessage from = _from as IoMessage;
			IoMessage to = _to as IoMessage;
			to.messageName = from.messageName;
			to.label = from.label;
			to.args = new IoObjectArrayList();
		}

        // Published Slots

        public static IoObject slotName(IoObject target, IoObject locals, IoObject message)
        {
            IoMessage self = target as IoMessage;
            return self.messageName;
        }

        public static IoObject slotSetName(IoObject target, IoObject locals, IoObject message)
        {
            IoMessage self = target as IoMessage;
            IoMessage msg = message as IoMessage;
            IoSeq s = msg.localsSymbolArgAt(locals, 0);
            self.messageName = s;
            return self;
        }

        public static IoObject slotNext(IoObject target, IoObject locals, IoObject message)
        {
            IoMessage self = target as IoMessage;
            return self.next == null ? target.state.ioNil : self.next;
        }

        public static IoObject slotSetNext(IoObject target, IoObject locals, IoObject message)
        {
            IoMessage self = target as IoMessage;
            IoMessage msg = message as IoMessage;
            IoObject m = msg.localsMessageArgAt(locals, 0) as IoObject;
            IoMessage mmm = null;
            if (m == target.state.ioNil)
            {
                mmm = null;
            }
            else if (m.name.Equals("Message"))
            {
                mmm = m as IoMessage;
            }
            else
            {
                Console.WriteLine("argument must be Message or Nil");
            }
            self.next = mmm;
            return self;
        }

        public static IoObject slotCode(IoObject target, IoObject locals, IoObject message)
        {
            IoMessage self = target as IoMessage;
            string s = ""; 
            s = self.descriptionToFollow(true);
            return IoSeq.createObject(self.state, s);
        }

        public static IoObject slotArguments(IoObject target, IoObject locals, IoObject message)
        {
            IoMessage self = target as IoMessage;
            IoList list = IoList.createObject(target.state);
            foreach (IoObject o in self.args)
            {
                list.append(o);
            }
            return list;
        }

        public static IoObject slotAppendArg(IoObject target, IoObject locals, IoObject message)
        {
            IoMessage self = target as IoMessage;
            IoMessage msg = message as IoMessage;
            IoMessage m = msg.localsMessageArgAt(locals, 0) as IoMessage;
            self.args.Add(m);
            return self;
        }

        public static IoObject slotArgCount(IoObject target, IoObject locals, IoObject message)
        {
            IoMessage self = target as IoMessage;
            return IoNumber.newWithDouble(target.state, Convert.ToDouble(self.args.Count));
        }

        public static IoObject slotArgAt(IoObject target, IoObject locals, IoObject message)
        {
            IoMessage self = target as IoMessage;
            IoMessage m = message as IoMessage;
            int index = m.localsNumberArgAt(locals, 0).asInt();
            IoObject v = self.args[index] as IoObject;
            return v != null ? v : self.state.ioNil;
        }
        
        public static IoObject slotCachedResult(IoObject target, IoObject locals, IoObject message)
        {
            IoMessage m = target as IoMessage;
            return m.cachedResult == null ? target.state.ioNil : m.cachedResult;
        }

        public static IoObject slotSetCachedResult(IoObject target, IoObject locals, IoObject message)
        {
            IoMessage self = target as IoMessage;
            IoMessage msg = message as IoMessage;
            self.cachedResult = msg.localsValueArgAt(locals, 0);
            return self;
        }

        public static IoObject slotRemoveCachedResult(IoObject target, IoObject locals, IoObject message)
        {
            IoMessage self = target as IoMessage;
            self.cachedResult = null;
            return self;
        }

        public static IoObject slotHasCachedResult(IoObject target, IoObject locals, IoObject message)
        {
            IoMessage self = target as IoMessage;
            return self.cachedResult == null ? target.state.ioFalse : target.state.ioTrue;
        }

        // Message Public Raw Methods

        public static IoMessage newWithName(IoState state, IoSeq ioSymbol)
        {
            IoMessage msg = IoMessage.createObject(state);
            msg.messageName = ioSymbol;
            return msg;
        }

        public IoMessage newFromTextLabel(IoState state, string code, string label)
        {
            IoSeq labelSymbol = IoSeq.createSymbolInMachine(state, label);
            return newFromTextLabelSymbol(state, code, labelSymbol);
        }

        public string descriptionToFollow(bool follow)
        {
            IoMessage m = this;
            string s = "";
            do {
                s +=  m.messageName;

                if (m.args.Count > 0)
                {
                    s += "(";

                    for (int i = 0; i < m.args.Count; i++)
                    {
                        IoMessage arg = m.args[i] as IoMessage;
                        s += arg.descriptionToFollow(true);
                        if (i != m.args.Count - 1)
                        {
                            s += ", ";
                        }
                    }

                    s += ")";
                }

                if (!follow)
                {
                    return s;
                }

                if (m.next != null && !m.messageName.value.Equals(";"))
                {
                    s += " ";
                }
                if (m.messageName.value.Equals(";"))
                {
                    s += "\n";
                }
                
            } while ((m = m.next) != null);

            return s;
        }

        public static IEnumerator asyncCall(IoContext ctx, IoObject future)
        {
            IoObject target = ctx.target;
            IoObject locals = ctx.locals;
            IoObject result = target;
            IoObject cachedTarget = target;
            IoMessage msg = ctx.message;
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
                        result = savedPrevResultAsYieldResult;
                    }
                    target = result;
                    savedPrevResultAsYieldResult = result;
                }
            } while ((msg = msg.next) != null);
            future.slots["future"] = result;
            yield return null;
            //yield return result;
        }

        public IoObject localsPerformOn(IoObject target, IoObject locals)
        {
            if (async)
            {
                IoContext ctx = new IoContext();
                ctx.target = target;
                ctx.locals = locals;
                ctx.message = this;
                IoState state = target.state;
                IoObject future = IoObject.createObject(state);
                IEnumerator e = IoMessage.asyncCall(ctx, future);
                state.contextList.Add(e);
                return future;
            }

            IoObject result = target;
            IoObject cachedTarget = target;
            IoMessage msg = this;
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
                        //if (target.tag.performFunc == null)
                            result = target.perform(target, locals, msg);
                        //else
                        //    result = target.tag.performFunc(target, locals, msg);
                    }
                    if (result == null)
                    {
                        Console.WriteLine("Message chains intermediate mustn't be null");
                    }
                    target = result;
                }
            } while ((msg = msg.next) != null);
            return result;
        }

        public override string ToString()
        {
            return messageName.ToString();// +args.ToString();
        }

        public override void print()
        {
            string code = this.descriptionToFollow(true);
            Console.Write(code);
            //Console.Write(ToString());
        }

        public IoMessage rawArgAt(int p)
        {
            IoMessage argIsMessage = args[p] as IoMessage;
            return argIsMessage;
        }

        public IoObject localsValueArgAt(IoObject locals, int i)
        {
            IoMessage m = i < args.Count ? args[i] as IoMessage : null;
            //m.async = async;
            if (m != null)
            {
                if (m.cachedResult != null && m.next == null)
                {
                    return m.cachedResult;
                }
                
                return m.localsPerformOn(locals, locals);
            }
            return this.state.ioNil;
        }

        // TODO: possible folding of following functions

        public IoSeq localsSymbolArgAt(IoObject locals, int i)
        {
			IoObject o = localsValueArgAt(locals, i);
			if (!o.name.Equals("Sequence"))
			{
				localsNumberArgAtErrorForType(locals, i, "Sequence");

			}
            return o as IoSeq;
        }

        public IoObject localsMessageArgAt(IoObject locals, int n)
        {
            IoObject v = localsValueArgAt(locals, n);
            if (!v.name.Equals("Message") && v != state.ioNil)
            {
                localsNumberArgAtErrorForType(locals, n, "Message");

            }
            return v;
        }

        public IoNumber localsNumberArgAt(IoObject locals, int i)
        {
            IoObject o = localsValueArgAt(locals, i);
            if (o == null || !o.name.Equals("Number"))
            {
                localsNumberArgAtErrorForType(locals, i, "Number");

            }
            return o as IoNumber;
        }

        // Private Methods

        void localsNumberArgAtErrorForType(IoObject locals, int i, string p)
        {
            IoObject v = localsValueArgAt(locals, i);
            Console.WriteLine("argument {0} to method '{1}' must be a {2}, not a '{3}'",
                i, this.messageName, p, v.name);
        }

        IoMessage newParse(IoState state, IoLexer lexer)
        {
            if (lexer.errorToken != null)
            {
            }

            if (lexer.topType() == IoTokenType.TERMINATOR_TOKEN)
            {
                lexer.pop();
            }

            if (lexer.top() != null && lexer.top().isValidMessageName())
            {
                IoMessage self = newParseNextMessageChain(state, lexer);
                if (lexer.topType() != IoTokenType.NO_TOKEN)
                {
                    state.error(self, "compile error: %s", "unused tokens");
                }
                return self;
            }

            return newWithNameReturnsValue(state, IoSeq.createSymbolInMachine(state, "nil"), state.ioNil);

        }

        IoMessage newWithNameReturnsValue(IoState state, IoSeq symbol, IoObject v)
        {
            IoMessage self = clone(state) as IoMessage;
            self.messageName = symbol;
            self.cachedResult = v;
            return self;
        }

        IoMessage newParseNextMessageChain(IoState state, IoLexer lexer)
        {
            IoMessage msg = clone(state) as IoMessage;

            if (lexer.top() != null && lexer.top().isValidMessageName())
            {
                msg.parseName(state, lexer);
            }

            if (lexer.topType() == IoTokenType.OPENPAREN_TOKEN)
            {
                msg.parseArgs(lexer);
            }

            if (lexer.top() != null && lexer.top().isValidMessageName())
            {
                msg.parseNext(lexer);
            }

            while (lexer.topType() == IoTokenType.TERMINATOR_TOKEN)
            {
                lexer.pop();

                if (lexer.top() != null && lexer.top().isValidMessageName())
                {
                    IoMessage eol = IoMessage.newWithName(state, state.semicolonSymbol);
                    msg.next = eol;
                    eol.parseNext(lexer);
                }
            }

            return msg;
        }

        void parseName(IoState state, IoLexer lexer)
        {
            IoToken token = lexer.pop();
            messageName = IoSeq.createSymbolInMachine(state, token.name);
            ifPossibleCacheToken(token);
            //rawSetLineNumber(token.lineNumber);
            //rawSetCharNumber(token.charNumber);
        }

        void ifPossibleCacheToken(IoToken token)
        {
			IoSeq method = this.messageName;
			IoObject r = null;
			switch (token.type)
			{
				case IoTokenType.TRIQUOTE_TOKEN:
					break;
				case IoTokenType.MONOQUOTE_TOKEN:
                    r = IoSeq.createSymbolInMachine(
                            method.state,
                            IoSeq.rawAsUnescapedSymbol(
                                IoSeq.rawAsUnquotedSymbol(
                                    IoSeq.createObject(method.state, method.value)
                                )
                            ).value
                        );
					break;
				case IoTokenType.NUMBER_TOKEN:
					r = IoNumber.newWithDouble(this.state, Convert.ToDouble(method.value, CultureInfo.InvariantCulture));
					break;
                default:
                    if (method.value.Equals("nil"))
                    {
                        r = state.ioNil;
                    }
                    else if (method.value.Equals("true"))
                    {
                        r = state.ioTrue;
                    }
                    else if (method.value.Equals("false"))
                    {
                        r = state.ioFalse;
                    }
                    break;


			}
			this.cachedResult = r;
        }

        void parseNext(IoLexer lexer)
        {
            IoMessage nextMessage = newParseNextMessageChain(this.state, lexer);
            this.next = nextMessage;
        }

        void parseArgs(IoLexer lexer)
        {
            lexer.pop();

            if (lexer.top() != null && lexer.top().isValidMessageName())
            {
                IoMessage arg = newParseNextMessageChain(this.state, lexer);
                addArg(arg);

                while (lexer.topType() == IoTokenType.COMMA_TOKEN)
                {
                    lexer.pop();

                    if (lexer.top() != null && lexer.top().isValidMessageName())
                    {
                        IoMessage arg2 = newParseNextMessageChain(this.state, lexer);
                        addArg(arg2);
                    }
                    else
                    {
                    }
                }
            }

            if (lexer.topType() != IoTokenType.CLOSEPAREN_TOKEN)
            {
                // TODO: Exception, missing close paren
            }
            lexer.pop();
        }

        void addArg(IoMessage arg)
        {
			args.Add(arg);
        }

		IoMessage newFromTextLabelSymbol(IoState state, string code, IoSeq labelSymbol)
		{
			IoLexer lexer = new IoLexer();
			IoMessage msg = new IoMessage();
			msg = msg.clone(state) as IoMessage;
			lexer.s = code;
			lexer.lex();
			msg = this.newParse(state, lexer);
			msg.opShuffle();
			msg.label = labelSymbol;
			return msg;
		}

		IoObject opShuffle()
		{
            IoObject context = null;
            IoObject m = this.rawGetSlotContext(state.opShuffleMessage.messageName, out context);
            if (m != null)
            {
                state.opShuffleMessage.localsPerformOn(this, state.lobby);
            }
			return this;
		}

    }

}
