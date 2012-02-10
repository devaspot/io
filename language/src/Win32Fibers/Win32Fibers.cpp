// This is the main DLL file.


#define _WIN32_WINNT 0x400

#using <mscorlib.dll>
#include <windows.h>
#include <mscoree.h>

#if defined(Yield)
#undef Yield
#endif

#define CORHOST

namespace io {

typedef System::Runtime::InteropServices::GCHandle GCHandle;

VOID CALLBACK unmanaged_fiberproc(PVOID pvoid);

__gc private struct StopFiber {};

enum FiberStateEnum {
	FiberCreated, FiberRunning, FiberStopPending, FiberStopped, FiberNotCreated
};

#pragma unmanaged

#if defined(CORHOST) 
ICorRuntimeHost *corhost;

void initialize_corhost() {
	CorBindToCurrentRuntime(0, CLSID_CorRuntimeHost,
		IID_ICorRuntimeHost, (void**) &corhost);
}

#endif

	void rawCorSwitchToFiber(void *fiber)
	{
#if defined(CORHOST)
		DWORD *cookie;
		corhost->SwitchOutLogicalThreadState(&cookie);
#endif
		SwitchToFiber(fiber);
#if defined(CORHOST)
		corhost->SwitchInLogicalThreadState(cookie);
#endif
	}



#pragma managed

public __delegate System::Object* Coroutine();

__gc __abstract public class AjaiFiber : public System::IDisposable {
public:
#if defined(CORHOST)
	static AjaiFiber() { initialize_corhost(); }
#endif

	AjaiFiber() : retval(0), state(FiberCreated) {
		void *objptr = (void*) GCHandle::op_Explicit(GCHandle::Alloc(this));
		fiber = CreateFiber(/*4096, 4096, */0, unmanaged_fiberproc, objptr);
		if (!fiber)
			state = FiberNotCreated;
	}

	__property bool get_IsRunning() {
		return state != FiberStopped;
	}

	//static Coroutine* op_Implicit(AjaiFiber *obj) {
	//	return new Coroutine(obj, &AjaiFiber::Resume);
	//}

	void CorSwitchToFiber(io::AjaiFiber *f) {
		rawCorSwitchToFiber(f->fiber);
	}

	__property int get_State() {
		return state;
	}

	System::Object* Resume() {
		if(!fiber || state == FiberStopped || state == FiberNotCreated)
			return NULL;
		initialize_thread();
		void *current = GetCurrentFiber();
		if(fiber == current)
			return NULL;
		previousfiber = current;
		rawCorSwitchToFiber(fiber);
		return retval;
	}

	void Dispose() {
		if(fiber) {
			if(state  == FiberRunning) {
				initialize_thread();
				void *current = GetCurrentFiber();
				if(fiber == current)
					return;
				previousfiber = current;
				state = FiberStopPending;
				rawCorSwitchToFiber(fiber);
			} else if(state == FiberCreated) {
				state = FiberStopped;
			}
			DeleteFiber(fiber);
			fiber = 0;
		}
	}
	virtual void Run() = 0;
	void Yield(System::Object *obj) {
		retval = obj;
		rawCorSwitchToFiber(previousfiber);
		if(state == FiberStopPending)
			throw new StopFiber;
	}
		void *fiber, *previousfiber;
private:
	[System::ThreadStatic] static bool thread_is_fiber;


	FiberStateEnum state;
	System::Object *retval;

	static void initialize_thread() {
		if(!thread_is_fiber) {
			ConvertThreadToFiber(0);
			thread_is_fiber = true;
		}
	}
private public:
	void* main() {
		state = FiberRunning;
		try {
			Run();
		} catch(System::Object *x) {
			System::Console::Error->WriteLine(
				S"\nFIBERS.DLL: main Caught {0}", x);
		}
		state = FiberStopped;
		retval = 0;
		return previousfiber;
	}
};

void* fibermain(void* objptr) {
	System::IntPtr ptr = (System::IntPtr) objptr;
	GCHandle g = GCHandle::op_Explicit(ptr);
	AjaiFiber *fiber = static_cast<AjaiFiber*>(g.Target);
	g.Free();
	return fiber->main();
}

#pragma unmanaged

VOID CALLBACK unmanaged_fiberproc(PVOID objptr) {
#if defined(CORHOST)
	corhost->CreateLogicalThreadState();
#endif
	void *previousfiber = fibermain(objptr);
#if defined(CORHOST)
	corhost->DeleteLogicalThreadState();
#endif
	SwitchToFiber(previousfiber);
}

} // namespace fibers

