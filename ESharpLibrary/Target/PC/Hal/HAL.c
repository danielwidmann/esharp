

#ifdef __linux__ 
#include <time.h>
#include <sys/times.h>
uint32_t Target_GetTickCount()
{
	struct timespec now;
	if (clock_gettime(CLOCK_MONOTONIC, &now))
		return 0;
	return now.tv_sec * 1000.0 + now.tv_nsec / 1000000.0;
}
#elif _WIN32
#define WIN32_LEAN_AND_MEAN
#include "Windows.h"
#include "stdint.h"
#include "stdio.h"

uint32_t Target_GetTickCount()
{
	return (uint32_t)GetTickCount();
}
#elif defined(__EMSCRIPTEN__)
#include <time.h>
#include <sys/times.h>
uint32_t Target_GetTickCount()
{
	struct timespec now;
	if (clock_gettime(CLOCK_MONOTONIC, &now))
		return 0;
	return now.tv_sec * 1000.0 + now.tv_nsec / 1000000.0;
}
#elif defined(__CYGWIN__)
#include "stdint.h"
#include "stdio.h"

#include <time.h>
#include <sys/times.h>
uint32_t Target_GetTickCount()
{
	struct timespec now;
	if (clock_gettime(CLOCK_MONOTONIC, &now))
		return 0;
	return now.tv_sec * 1000.0 + now.tv_nsec / 1000000.0;
}

#else
#error OS NOT SUPPORTED 
#endif

void Target_PutChar(char c)
{
	printf("%c", c);
	fflush(stdout);
}

HANDLE ghMutex;


void e_lock_init() 
{
	// Create a mutex with no initial owner

	ghMutex = CreateMutex(
		NULL,              // default security attributes
		FALSE,             // initially not owned
		NULL);             // unnamed mutex
}

void e_lock() 
{
	WaitForSingleObject(
		ghMutex,    // handle to mutex
		INFINITE);  // no time-out interval
}

void e_unlock()
{
	ReleaseMutex(ghMutex);
}

uint16_t e_inc_16(uint16_t* value) {
	return _InterlockedIncrement16(value);

	//InterlockedIncrement() and friends(see MSDN).

	//	Together with the gcc builtins __sync_fetch_and_add()
}

uint16_t e_dec_16(uint16_t* value) {
	return _InterlockedDecrement16(value);
}

//todo: use this:
//private:
//	LONG m_counter;
//	HANDLE m_semaphore;
//
//public:
//	Benaphore()
//	{
//		m_counter = 0;
//		m_semaphore = CreateSemaphore(NULL, 0, 1, NULL);
//	}
//
//	~Benaphore()
//	{
//		CloseHandle(m_semaphore);
//	}
//
//	void Lock()
//	{
//		if (_InterlockedIncrement(&m_counter) > 1) // x86/64 guarantees acquire semantics
//		{
//			WaitForSingleObject(m_semaphore, INFINITE);
//		}
//	}
//
//	void Unlock()
//	{
//		if (_InterlockedDecrement(&m_counter) > 0) // x86/64 guarantees release semantics
//		{
//			ReleaseSemaphore(m_semaphore, 1, NULL);
//		}
//	}


