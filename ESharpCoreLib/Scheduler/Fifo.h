/*
* PosFifo.h
*
*  Created on: Oct 25, 2013
*      Author: Daniel
*/

// pre-header
// internal includes
// external includes
// header


#ifndef FIFO_H_
#define FIFO_H_


#ifdef TYPE
#undef TYPE
#endif

#ifdef PREFIX
#undef PREFIX
#endif

#define TYPE void*
#define PREFIX(x) Fifo_ ## x







/*
* GenericFifo.h
*
*  Created on: Oct 25, 2013
*      Author: Daniel
*/
// no guard, should be guarded in containing file
//#ifndef GENERICFIFO_H_
//#define GENERICFIFO_H_

void PREFIX(QueueInit) (void);
int PREFIX(QueueEnqueue) (TYPE* _new);
int PREFIX(QueueDequeue) (TYPE* old);
int PREFIX(QueuePeek) (TYPE* old);
int PREFIX(QueueElements) ();
int PREFIX(QuereFreeElements) ();

//#endif /* GENERICFIFO_H_ */

#endif /* RESPONSEFIFO_H_ */