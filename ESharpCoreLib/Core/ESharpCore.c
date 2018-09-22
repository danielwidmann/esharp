

//#include "CSharp.h"
//#include "PC.h"

int s_allocCount = 0;

void error()
{
	printf("obj error\n");
}

void ESharpRT_Error() {
	printf("Fatal Error\n");	
}

void CMalloc_Check_()
{
	if (s_allocCount != 0)
	{
		printf("Error Malloc Check: Not all memory freed %d\r\n", s_allocCount);
		obj_heap_print(true);
		//make test fail
		exit(1);
	}
}


void EObject_Finalize(EObject _this)
{

}


void EObject_DerefCheck(EObject _this)
{
	if (_this->refCount <= 0)
	{
		// todo print
		printf("Error Deref\r\n");
		fflush(stdout);
		while (1){};
		//assert(false);
		//throw new Exception();
	}
}

void EObject_ctor(EObject _this)
{
}

void EObject_AddRef(EObject _this)
{
	if (_this != NULL && is_heap_variable(_this))
	{
		CheckEObjectObject(_this);
		EObject_DerefCheck(_this);
		//_this->refCount += 1;
		e_inc_16(&_this->refCount);
	}
}

void EObject_Finalize(EObject _this);

void EObject_RemoveRef(EObject _this)
{
	if (_this != NULL && is_heap_variable(_this))
	{
		CheckEObjectObject(_this);
		EObject_DerefCheck(_this);

		uint16_t after = e_dec_16(&_this->refCount);

		//if (_this->refCount <= 1)
		if(after == 0)
		{
			EObject_Finalize_virtual(_this);

			//free(_this);
			obj_free(_this);
			s_allocCount--;

			//CheckHeap();
		}
		/*else
		{
			_this->refCount -= 1;
		}*/
	}
}

EObject Array_ref_Get(Array_ref this_, int idx)
{
	EObject value = this_->data[idx];
	EObject_AddRef(value);
	return value;
}

void Array_ref_Set(Array_ref this_, int idx, EObject value)
{
	EObject_RemoveRef(this_->data[idx]);
	EObject_AddRef(value);
	this_->data[idx] = value;
}


void Array_ref_Finalize(Array_ref this_)
{
	int i;
	for (i = 0; i < this_->Length; i++)
	{
		EObject_RemoveRef(this_->data[i]);
	}
}


void Console_WriteLine_EString(EString s)
{
	printf("%.*s\n", s->Length, s->data);
	fflush(stdout);
}

void Console_Write_EString(EString s)
{
	printf("%.*s", s->Length, s->data);
	fflush(stdout);
}

string CharPtr_To_String(const char* str)
{
	const char* s;
	for (s = str; *s; ++s)
		;

	size_t len = (s - str);

	Array_1 newArray = (Array_1)CMalloc_ArrayMalloc_1((short)len);
	memcpy(newArray->data, str, len);

	return newArray;
}

void Console_WriteLine_Int32(int i)
{
	printf("%d\n", i);
	fflush(stdout);
}

void Console_Write_Int32(int i)
{
	printf("%d", i);
	fflush(stdout);
}

void Console_Write_EObject(EObject o)
{
	printf("eobject type %d@%d", o != NULL ? o->etype : 0, (int)(uintptr_t)o);
	fflush(stdout);
}

void Console_WriteLine_EObject(EObject o)
{
	Console_Write_EObject(o);
	printf("\n");
	fflush(stdout);
}

Array_ref GetArgs(int count, char* argv[])
{
	int i;
	Array_ref array_ref = (Array_ref)CMalloc_ArrayMalloc_ref(count);
	Array_ref_ctor(array_ref, count);
	// skip the called command line
	for (i = 1; i < count; i++)
	{
		EObject s = (EObject)CharPtr_To_String(argv[i]);
		Array_ref_Set(array_ref, i, s);
		EObject_RemoveRef(s);
	}
	Array_ref result = array_ref;
	return result;
}

void Array_CopyTo_1(Array_1 _this, Array_1 dst, unsigned short idx)
{
	if (_this->Length + idx > dst->Length)
	{
		ECS_ERROR("Array1 dest too small");
		return;
	}
	memcpy(&dst->data[idx], _this->data, _this->Length);
}

void RuntimeHelpers_InitializeArray(EObject array, EObject data)
{
	if (array->etype == Array_1_TypeId) {
		Array_CopyTo_1((Array_1)data, (Array_1)array, 0);
	}
}

EObject emalloc(short type)
{
	emalloc_len_t size = TypeSizes[type];
	emalloc_block_t block = obj_alloc(size);

	if (block == NULL)
	{ // out of memory
		return NULL;
	}

	block->etype = type;
	block->refCount = 1;

	ASSERT(obj_size(block) == size);

	s_allocCount++;

	/*static uint8_t printed;
	if (heap_max_usage() >= 360 && !printed) {
		obj_heap_print(false);
		printed = 1;
	}*/

	return block;
}

Array_1 emalloc_array_1(short elements)
{
	uint16_t len = 1 * elements + sizeof(Array_1_struct);
	Array_1 block = (Array_1)obj_alloc(len);

	if (block == NULL)
	{ // out of memory
		return NULL;
	}

	block->base.etype = Array_1_TypeId;
	block->base.refCount = 1;
	block->Length = elements;

	ASSERT(obj_size((EObject)block) == len);

	s_allocCount++;

	return block;
}
#ifdef ARRAY_4_USED
Array_4 emalloc_array_4(short elements)
{
	uint16_t len = 4 * elements + sizeof(Array_4_struct);
	Array_4 block = (Array_4)obj_alloc(len);

	if (block == NULL)
	{ // out of memory
		return NULL;
	}

	block->base.etype = Array_4_TypeId;
	block->base.refCount = 1;
	block->Length = elements;

	ASSERT(obj_size((EObject)block) == len);

	s_allocCount++;

	return block;
}
#endif

Array_ref emalloc_array_ref(short elements)
{
	uint16_t len = sizeof(void*) * elements + sizeof(Array_ref_struct);
	Array_ref block = (Array_ref)obj_alloc(len);

	if (block == NULL)
	{ // out of memory
		return NULL;
	}

	block->base.etype = Array_ref_TypeId;
	block->base.refCount = 1;
	block->Length = elements;

	ASSERT(obj_size((EObject)block) == len);

	s_allocCount++;

	return block;
}

typedef struct struct_ObjectCheck
{
	EObject obj;
	struct struct_ObjectCheck* last;
} ObjectCheck_t;

static void CheckEObjectObject_internal(EObject obj, ObjectCheck_t* last, bool print)
{
	if (obj == NULL)
		return;

	// check for cycles
	ObjectCheck_t* prev_obj = last;
	while (prev_obj != NULL) {
		if (prev_obj->obj == obj) {
			// cycle detected 
			if (print) {
				printf("%s(%d)", TypeNames[obj->etype], obj->etype);
				for (prev_obj = last; prev_obj->obj != obj;prev_obj = prev_obj->last) {
					printf("<-%s(%d)", TypeNames[prev_obj->obj->etype], prev_obj->obj->etype);
				}
				printf(" loop\n");
			}
			return;
		}
		prev_obj = prev_obj->last;
	}
	/*if (!is_heap_variable(obj))
	{
	error();
	return;
	}*/

	if ((((uintptr_t)obj) % 4) != 0)
	{
		error();
		return;
	}

	if (obj->etype >= type_count)
	{
		error();
		return;
	}

	if (obj->refCount > 10 || obj->refCount <= 0)
	{
		error();
		return;
	}

	int i;
	for (i = 0; i < type_meta_count; i++)
	{
		type_meta_t cur = TypeMeta[i];
		if (cur.etypeid != obj->etype)
			continue;
		if (cur.etypeid != obj->etype)
			continue;

		EObject* pfield = (EObject*)((uint8_t*)obj + cur.offset);
		EObject field = *pfield;

		ObjectCheck_t o = { obj, last };
		CheckEObjectObject_internal(field, &o, print);
	}
}

void CheckEObjectObject(EObject obj)
{
	ObjectCheck_t last = {NULL, NULL};
	CheckEObjectObject_internal(obj, &last, false);

	//CheckHeap();
}


void _VisitBlock(EObject obj)
{
	ObjectCheck_t last = { NULL, NULL };
	CheckEObjectObject_internal(obj, &last, false);
}

void CheckHeap() 
{
	obj_heap_visit(&_VisitBlock);
}


// END HEADER
