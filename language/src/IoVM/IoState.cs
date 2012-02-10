
using System;
using System.Collections;
using System.IO;

namespace io {

    public class IoContext
    {
        public IoObject target;
        public IoObject locals;
        public IoMessage message;
        public IoObject slotContext;
    }

	public class IoState {

        public ArrayList contextList = new ArrayList();

		public Hashtable primitives = new Hashtable(); // keys are raw strings
        public Hashtable symbols = new Hashtable(); // keys are raw strings

		// coroutines
		public IoObject objectProto;
       // public IoCoroutine mainCoroutine;    // the object that represents the main "thread"
       // public IoCoroutine currentCoroutine; // the object whose coroutine is active
		public Stack currentIoStack;      // quick access to current coro's retain stack
        public IoCLR clrProto;

		// quick access objects
		public IoSeq activateSymbol;
        public IoSeq callSymbol;
        public IoSeq forwardSymbol;
        public IoSeq noShufflingSymbol;
		public IoSeq opShuffleSymbol;
		public IoSeq semicolonSymbol;
		public IoSeq selfSymbol;
		public IoSeq setSlotSymbol;
		public IoSeq setSlotWithTypeSymbol;
		public IoSeq stackSizeSymbol;
		public IoSeq typeSymbol;

		public IoSeq updateSlotSymbol;
		public IoObject setSlotBlock;
		public IoObject localsUpdateSlotCFunc;
		public IoObject localsProto;

		public IoMessage asStringMessage;
		public IoMessage collectedLinkMessage;
		public IoMessage compareMessage;
		public IoMessage initMessage;
        public IoMessage selfMessage;
		public IoMessage mainMessage;
		public IoMessage nilMessage;
        public IoMessage forwardMessage;
        public IoMessage activateMessage;
        public IoMessage opShuffleMessage;
		public IoMessage printMessage;
		public IoMessage referenceIdForObjectMessage;
		public IoMessage objectForReferenceIdMessage;
		public IoMessage runMessage;
		public IoMessage willFreeMessage;
		public IoMessage yieldMessage;
		public IoMessage typeMessage;
		
		public IoObjectArrayList cachedNumbers;

		// singletons
		public IoObject ioNil;
		public IoObject ioTrue;
		public IoObject ioFalse;

		// Flow control singletons
		public IoObject ioNormal;
		public IoObject ioBreak;
		public IoObject ioContinue;
		public IoObject ioReturn;
		public IoObject ioEol;

		// execution context
		public IoObject lobby;
		public IoObject core;

		// current execution state
        public IoStopStatus stopStatus;
		public object returnValue;

		// debugger
		public int debugOn;
		public IoObject debugger;
		public IoMessage vmWillSendMessage;

		// SandBox limits
		public int messageCountLimit;
		public int messageCount;
		public double timeLimit;
		public double endTime;

		// tail calls
		public IoMessage tailCallMessage;

		// exiting
		public int shouldExit;
		public int exitResult;

        public IoSeq IOSYMBOL(string name)
        {
            return IoSeq.createSymbolInMachine(this, name);
        }

		public void registerProtoWithFunc(string name, IoStateProto stateProto)
		{
			primitives[name] = stateProto;
		}

		public IoObject protoWithInitFunc(string name)
		{
			IoStateProto stateProto = primitives[name] as IoStateProto;
			return stateProto.proto;
		}

        public void error(IoMessage m, string s)
        {
        }

		public IoState()
		{
            

			objectProto = IoObject.createProto(this);
			core = objectProto.clone(this);
			lobby = objectProto.clone(this);

            IoSeq seqProto = IoSeq.createProto(this);

            setupSingletons();
            setupSymbols();

            objectProto.protoFinish(this);

			IoMessage messageProto = IoMessage.createProto(this);

            nilMessage = IoMessage.createObject(this) as IoMessage;
            nilMessage.cachedResult = ioNil;
            nilMessage.messageName = IOSYMBOL("nil");

            IoMap mapProto = IoMap.createProto(this);
			IoNumber numProto = IoNumber.createProto(this);
			IoCFunction cfProto = IoCFunction.createProto(this);
            IoBlock blockProto = IoBlock.createProto(this);
            //IoCoroutine coroProto = IoCoroutine.createProto(this);
            //mainCoroutine = coroProto;
            //currentCoroutine = coroProto;
            IoCall callProto = IoCall.createProto(this);
            IoList listProto = IoList.createProto(this);
            clrProto = IoCLR.createProto(this);
            IoCLRAssembly asmProto = IoCLRAssembly.createProto(this);
            IoCLRObject clrObjProto = IoCLRObject.createProto(this);

            IoObject protos = objectProto.clone(this);
			protos.slots["Core"] = core;
			protos.slots["Addons"] = null;

			lobby.slots["Lobby"] = lobby;
			lobby.slots["Protos"] = protos;

			core.slots["Object"] = objectProto;
			core.slots["Map"] = mapProto;
           // core.slots["Coroutine"] = coroProto;
			core.slots["Message"] = messageProto;
			core.slots["CFunction"] = cfProto;
			core.slots["Number"] = numProto;
            core.slots["Block"] = blockProto;
            core.slots["Call"] = callProto;
            core.slots["Locals"] = localsProto = objectProto.localsProto(this);
            core.slots["List"] = listProto;
            core.slots["Sequence"] = seqProto;
            core.slots["CLR"] = clrProto;
            core.slots["CLRAssembly"] = asmProto;
            core.slots["CLRObject"] = clrObjProto;
			
			objectProto.protos.Add(lobby);
            lobby.protos.Add(protos);
            protos.protos.Add(core);

            localsUpdateSlotCFunc = new IoCFunction(this, "localsUpdate", IoObject.localsUpdateSlot);

            initMessage = IoMessage.newWithName(this, IOSYMBOL("init"));
            forwardMessage = IoMessage.newWithName(this, IOSYMBOL("forward"));
            activateMessage = IoMessage.newWithName(this, IOSYMBOL("activate"));
            selfMessage = IoMessage.newWithName(this, IOSYMBOL("self"));
            opShuffleMessage = IoMessage.newWithName(this, IOSYMBOL("opShuffle"));
            mainMessage = IoMessage.newWithName(this, IOSYMBOL("main"));
			typeMessage = IoMessage.newWithName(this, IOSYMBOL("type"));
		}

      

        public void Return(IoObject v)
        {
            stopStatus = IoStopStatus.MESSAGE_STOP_STATUS_RETURN;
            returnValue = v;
        }

        public void resetStopStatus()
        {
            stopStatus = IoStopStatus.MESSAGE_STOP_STATUS_RETURN;
        }

        public int handleStatus()
        {
            switch (stopStatus)
            {
                case IoStopStatus.MESSAGE_STOP_STATUS_RETURN:
                    return 1;

                case IoStopStatus.MESSAGE_STOP_STATUS_BREAK:
                    resetStopStatus();
                    return 1;

                case IoStopStatus.MESSAGE_STOP_STATUS_CONTINUE:
                    resetStopStatus();
                    return 0;

                default:
                    return 0;
            }
        }


        public void setupSymbols()
        {
            activateSymbol = IOSYMBOL("activate");
            callSymbol = IOSYMBOL("call");
            forwardSymbol = IOSYMBOL("forward");
            noShufflingSymbol = IOSYMBOL("__noShuffling__");
            opShuffleSymbol = IOSYMBOL("opShuffle");
            semicolonSymbol = IOSYMBOL(";");
            selfSymbol = IOSYMBOL("self");
            setSlotSymbol = IOSYMBOL("setSlot");
            setSlotWithTypeSymbol = IOSYMBOL("setSlotWithType");
            stackSizeSymbol = IOSYMBOL("stackSize");
            typeSymbol = IOSYMBOL("type");
            updateSlotSymbol = IOSYMBOL("updateSlot");
        }

		public void setupSingletons()
		{
			ioNil = objectProto.clone(this);
            ioNil.slots["type"] = IOSYMBOL("nil");
			core.slots["nil"] = ioNil;
			
			ioTrue = IoObject.createObject(this);
            ioTrue.slots["type"] = IOSYMBOL("true");
			core.slots["true"] = ioTrue;

			ioFalse = IoObject.createObject(this);
            ioFalse.slots["type"] = IOSYMBOL("false");
			core.slots["false"] = ioFalse;
		}

        public void error(IoMessage self, string p, string p_3)
        {
        }

		public IoObject onDoCStringWithLabel(IoObject target, string code, string label)
		{
			IoMessage msg = new IoMessage();
			msg = msg.clone(this) as IoMessage;
			msg = msg.newFromTextLabel(this, code, label);
			return msg.localsPerformOn(target, target);
		}

        public IoObject loadFile(string fileName)
        {
            StreamReader sr = new StreamReader(fileName);
			IoObject result = null;
			string s = sr.ReadToEnd();
				result = onDoCStringWithLabel(lobby, s, fileName);
            return result;
        }

        public IoObject processBootstrap()
        {
            string[] ios = null;
            try
            {
                ios = Directory.GetFiles("../io/bootstrap");
            }
            catch {
            }
            if (ios == null || ios.Length == 0)
            {
                Console.WriteLine("Bootstrap not found. Processing raw Io.");
                return null;
            }
            else
                Console.WriteLine("Bootstrap successfully loaded.");
            ArrayList iosa = new ArrayList(ios);
            iosa.Sort();
            IoObject result = null;
            foreach (string s in iosa)
            {
                result = loadFile(s);
            }
            return result;
        }

		public void prompt(IoState state)
		{
			IoObject result = null;
            processBootstrap();
            while (true)
            {
                Console.Write("Io> ");
                string s = Console.ReadLine();
                if (s.Equals("quit") || s.Equals("exit")) break;
                result = onDoCStringWithLabel(lobby, s, "prompt:");
                Console.Write("==> ");
                if (result != null)
                    result.print();
                else Console.WriteLine("why null?");
                Console.WriteLine();

            }
		}

    }

    public enum IoStopStatus
    {
        MESSAGE_STOP_STATUS_NORMAL = 0,
        MESSAGE_STOP_STATUS_BREAK = 1,
        MESSAGE_STOP_STATUS_CONTINUE = 2,
        MESSAGE_STOP_STATUS_RETURN = 4,
        MESSAGE_STOP_STATUS_EOL = 8
    }
}
