#pragma once

// WARNING: includes belong funrther down
// pre-header

#include "stdbool.h"
#include "stdint.h"

#define string Array_1
#define IntPtr void*
#define ldftn(X) ((void*)&(X))
#define ldvirtftn(X) ldftn(X)
#define Void void
#ifndef bool
	#define bool unsigned char
#endif // !bool

//typedef unsigned char bool;
#define false 0
#define true 1
#define inline

#define EObject_s_AddRef(x) EObject_AddRef((EObject) x)
#define EObject_s_RemoveRef(x) EObject_RemoveRef((EObject) x)

uint16_t e_inc_16(uint16_t* value);
uint16_t e_dec_16(uint16_t* value);

struct struct_EObject;
typedef struct struct_EObject * EObject;
typedef struct struct_EObject EObject_struct;

struct struct_EObject
{
	short refCount;
	short etype;
};

void EObject_Finalize_virtual(EObject _this);
void EObject_Finalize(EObject _this);


struct sturct_Array_4;
typedef struct struct_Array_4* Array_4;
typedef struct struct_Array_4 Array_4_struct;
//todo move to seperate file, best attached to csharp file
struct struct_Array_4
{
	EObject_struct base;
	short Length;
	int data[];
};


struct sturct_Array_1;
typedef struct struct_Array_1* Array_1;
typedef struct struct_Array_1 Array_1_struct;
//todo move to seperate file, best attached to csharp file
struct struct_Array_1
{
	EObject_struct base;
	short Length;
	unsigned char data[];
};

struct struct_Array
{
	EObject_struct base;
	short Length;
};

struct sturct_Array_ref;
typedef struct struct_Array_ref* Array_ref;
typedef struct struct_Array_ref Array_ref_struct;
//todo move to seperate file, best attached to csharp file
struct struct_Array_ref
{
	EObject_struct base;
	short Length;
	EObject data[];
};


typedef short emalloc_len_t;
typedef EObject emalloc_block_t;
typedef short emalloc_block_type_t;

// todo: should emalloc_block_t be renamed to emalloc_used_t
// todo: should this be MAX(sizeof(EObject_struct), sizeof(emalloc_empty_t)) ?? So we can fit an emply or a used block there
static emalloc_len_t emalloc_min_block_size = sizeof(EObject_struct) + sizeof(short);
static emalloc_len_t emalloc_alignment = 4;

void heap_init();
bool is_heap_variable(emalloc_block_t block);
void obj_heap_print(bool show_members);

typedef void *visit_heap_callback_t(emalloc_block_t block);
void obj_heap_visit(visit_heap_callback_t callback);

EObject emalloc(short type);
Array_1 emalloc_array_1(short elements);
Array_4 emalloc_array_4(short elements);
Array_ref emalloc_array_ref(short elements);

#define CMalloc_Malloc(X) emalloc(X)
#define CMalloc_ArrayMalloc_1(size) emalloc_array_1(size)
#define CMalloc_ArrayMalloc_4(size) emalloc_array_4(size)
#define CMalloc_ArrayMalloc_ref(size) emalloc_array_ref(size)

// todo move to source file

emalloc_block_t obj_alloc(emalloc_len_t size);
void obj_free(emalloc_block_t block);
emalloc_len_t obj_size(emalloc_block_t block);
#ifndef NULL
	#define NULL 0
#endif

void Array_CopyTo_1(Array_1 _this, Array_1 dst, unsigned short idx);


static inline int Array_4_Get(Array_4 this_, int idx)
{
	return this_->data[idx];
}

static inline void Array_4_Set(Array_4 this_, int idx, int value)
{
	this_->data[idx] = value;
}

static inline void Array_4_Finalize()
{

}

static inline unsigned char Array_1_Get(Array_1 this_, int idx)
{
	return this_->data[idx];
}

static inline void Array_1_Set(Array_1 this_, int idx, unsigned char value)
{
	this_->data[idx] = value;
}

static inline void Array_1_Finalize(Array_1 _this)
{

}



// todo move to earray.h



EObject Array_ref_Get(Array_ref this_, int idx);

void Array_ref_Set(Array_ref this_, int idx, EObject value);

void Array_ref_Finalize(Array_ref this_);

void Array_4_ctor(Array_4 _this, int Length);
void Array_ref_ctor(Array_ref _this, int Length);
void Array_1_ctor(Array_1 _this, int Length);


Array_1 Array_1_new(int Length);
Array_4 Array_4_new(int Length);
Array_ref Array_ref_new(int Length);

// internal includes


// external includes

#include "stdio.h"
#include <stdlib.h>
#include "string.h"
#include "stdint.h"


//#define CMalloc_Malloc(X) (X*)CMalloc_Malloc_(sizeof(X))
void CMalloc_Check_();

static inline void ECS_ERROR(char* x) { printf("\r\n"); printf("%s", x); printf("\r\n"); exit(1); }

Array_ref GetArgs(int count, char* argv[]);

void EObject_RemoveRef(EObject _this);
void EObject_ctor(EObject _this);
void EObject_AddRef(EObject _this);

//extern int mallocCount;

#if defined(_MSC_VER)
#define ALIGNED_(x) __declspec(align(x))
#else
#if defined(__GNUC__)
#define ALIGNED_(x) __attribute__ ((aligned(x)))
#endif
#endif

#define _ALIGNED_TYPE(t,x) typedef t ALIGNED_(x)
#define ALIGN ALIGNED_(8)

#define PAD_0 
#define PAD_1 0
#define PAD_2 0,0
#define PAD_3 0,0,0

void error();
void ESharpRT_Error();

void CheckEObjectObject(EObject obj);

void CheckHeap();

string CharPtr_To_String(const char* str);

#include "assert.h"
#define ASSERT(x) if(!(x)) { printf("assert failed\n"); assert(0);}

typedef unsigned char byte;


