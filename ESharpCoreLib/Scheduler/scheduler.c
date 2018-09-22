
//#include "Fifo.h"

int Scheduler_m_Stop = 0;

void Scheduler_Dispatch(Action t)
{
	if (t == NULL)
	{
		ECS_ERROR("Dispatch NULL");
	}
	Fifo_QueueEnqueue((void**)&t);
	EObject_AddRef(&t->_base._base);
}
void Scheduler_ProcessLoop()
{
	void* t;
	if (0 == Fifo_QueueDequeue(&t))
	{ // element available
		Action a = (Action)t;
		Action_Invoke(a);
		EObject_RemoveRef(&a->_base._base);
	}

}

void Scheduler_Cleanup()
{
	void* t;
	while (Fifo_QueueElements())
	{
		if (0 == Fifo_QueueDequeue(&t))
		{ // element available
			Action a = (Action)t;
			EObject_RemoveRef(&a->_base._base);
		}
	}
}

//#include <unistd.h>
//#include <emscripten.h>
void Scheduler_MainLoop()
{
	while (!Scheduler_m_Stop)
	{		
		Scheduler_ProcessLoop();
		//Sleep(1);
		//sleep(1);
		//emscripten_sleep(1);
	}
	Scheduler_Cleanup();
}

void Scheduler_Stop()
{
	Scheduler_m_Stop = 1;
}

