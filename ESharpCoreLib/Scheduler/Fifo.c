/*
* PosFifo.c
*
*  Created on: Oct 27, 2013
*      Author: Daniel
*/


#define QUEUE_ELEMENTS (10)



/*
* GenericFifo.c
*
*  Created on: Oct 27, 2013
*      Author: Daniel
*/


/* Very simple queue
* These are FIFO queues which discard the new data when full.
*
* Queue is empty when in == out.
* If in != out, then
*  - items are placed into in before incrementing in
*  - items are removed from out before incrementing out
* Queue is full when in == (out-1 + QUEUE_SIZE) % QUEUE_SIZE;
*
* The queue will hold QUEUE_ELEMENTS number of items before the
* calls to QueuePut fail.
*/

/* Queue structure */


#define QUEUE_SIZE (QUEUE_ELEMENTS + 1)

TYPE PREFIX(Queue)[QUEUE_SIZE];
int PREFIX(QueueIn), PREFIX(QueueOut) = 0;

void PREFIX(QueueInit) (void)
{
	PREFIX(QueueIn) = PREFIX(QueueOut) = 0;
}

int PREFIX(QueueElements) ()
{

	return ((PREFIX(QueueIn) + QUEUE_SIZE - PREFIX(QueueOut)) % QUEUE_SIZE);
}

int PREFIX(QuereFreeElements) ()
{
	return QUEUE_ELEMENTS - PREFIX(QueueElements)();
}

int PREFIX(QueueEnqueue) (TYPE* _new)
{
	if (PREFIX(QueueIn) == ((PREFIX(QueueOut) - 1 + QUEUE_SIZE) % QUEUE_SIZE))
	{
		return -1; /* Queue Full*/
	}

	PREFIX(Queue)[PREFIX(QueueIn)] = *_new;

	PREFIX(QueueIn) = (PREFIX(QueueIn) + 1) % QUEUE_SIZE;

	return 0; // No errors
}

int PREFIX(QueueDequeue) (TYPE* old)
{
	if (PREFIX(QueueIn) == PREFIX(QueueOut))
	{
		return -1; /* Queue Empty - nothing to get*/
	}

	*old = PREFIX(Queue)[PREFIX(QueueOut)];

	PREFIX(QueueOut) = (PREFIX(QueueOut) + 1) % QUEUE_SIZE;

	return 0; // No errors
}

/**
* Read without dequeue
*/
int PREFIX(QueuePeek) (TYPE* old)
{
	if (PREFIX(QueueIn) == PREFIX(QueueOut))
	{
		return -1; /* Queue Empty - nothing to get*/
	}

	*old = PREFIX(Queue)[PREFIX(QueueOut)];

	return 0; // No errors
}
