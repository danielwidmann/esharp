#include "CSharp.h"

int main(int argc, char* argv[])
{
	Array_ref array_ref;
	int ret = 0;
	e_lock_init();
	heap_init();
	array_ref = GetArgs(argc, argv);
	// todo pass array_ref as parameter to main; 
	ret = $ENTRY();
	EObject_RemoveRef((EObject)array_ref);

	Scheduler_Cleanup();
	EObject_NullStaticFields();
	CMalloc_Check_();
	return ret;
}

