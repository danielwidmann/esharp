
#include "stdint.h"

struct struct_Empty
{
	// size including header
	emalloc_len_t size;
	uint16_t next_offset;

};
typedef struct struct_Empty* EmptyBlock;
typedef EmptyBlock emalloc_empty_t;

// dummy entry point
//struct struct_Empty empty_first_struct;
emalloc_empty_t empty_first = NULL;
//emalloc_block_t block_first;
//emalloc_len_t alloc_size;

uint8_t heap[1024];
#define heap_size sizeof(heap)
const uint8_t* heap_end = (uint8_t*)heap + sizeof(heap);
void* largest_heap;

//#include "assert.h"
//#define ASSERT(x) if(!(x)) { printf("assert failed\n"); assert(x);}




//#define CMalloc_ArrayMalloc_1(size) CMalloc_Malloc_((1 * size) + sizeof(Array_1_struct))

emalloc_len_t obj_size(emalloc_block_t block)
{
	if (block->etype == Array_1_TypeId) {
		Array_1 array_1 = (Array_1)block;
		return array_1->Length + sizeof(Array_1_struct);
	}
#ifdef ARRAY_4_USED
	if (block->etype == Array_4_TypeId) {
		Array_4 array_4 = (Array_4)block;
		return array_4->Length*4 + sizeof(Array_4_struct);
	}
#endif
	if (block->etype == Array_ref_TypeId) {
		Array_ref array_ref = (Array_ref)block;
		return array_ref->Length*sizeof(void*) + sizeof(Array_ref_struct);
	}

	return TypeSizes[block->etype];
}

emalloc_block_t obj_next(emalloc_block_t block);

void obj_print(emalloc_block_t block)
{
	printf("%p object id %d(%s) size %d refcount %d\n", (void*)((uint8_t*)block - heap), block->etype, TypeNames[block->etype], obj_size(block), block->refCount);

}

void empty_print(emalloc_empty_t empty)
{
	printf("%p empty size 0x%x\n", (void*)((uint8_t*)empty - heap), empty->size);
}


emalloc_len_t obj_size_from_type(emalloc_block_type_t type)
{
	return type;
}

emalloc_empty_t empty_get_next_empty(emalloc_empty_t before)
{
	return (emalloc_empty_t)((uint8_t*)before + before->next_offset);
}

void empty_set_next_empty(emalloc_empty_t current, emalloc_empty_t next)
{
	current->next_offset = ((uint8_t*)next - (uint8_t*)current);
}

void empty_set_size(emalloc_empty_t empty, emalloc_len_t len)
{
	empty->size = len;
}

emalloc_len_t empty_get_size(emalloc_empty_t empty)
{
	return empty->size;
}

emalloc_block_t empty_next_block(emalloc_empty_t empty)
{
	emalloc_len_t len = empty_get_size(empty);
	return (emalloc_block_t)((uint8_t*)empty + len);
}

// true if can be joined
bool obj_joint(emalloc_empty_t empty_1, emalloc_empty_t empty_2)
{
	if ((emalloc_empty_t)empty_next_block(empty_1) != empty_2)
	{ //not adjacent, can't join
		return false;
	}

	empty_1->size += empty_2->size;
	empty_set_next_empty(empty_1, empty_get_next_empty(empty_2));

	return true;
}

bool obj_can_alloc(emalloc_empty_t empty, emalloc_len_t size)
{
	if (empty->size == size)
		return true;

	if (size + emalloc_min_block_size <= empty->size)
		return true;

	return false;
}

// return remaining empty block or null if no empty
emalloc_empty_t obj_split(emalloc_empty_t to_alloc, emalloc_len_t size)
{
	if (to_alloc->size == size)
	{ // nothing to split
		return NULL;
	}

	emalloc_empty_t remaining = (emalloc_empty_t)((uint8_t*)to_alloc + size);

	remaining->size = to_alloc->size - size;
	empty_set_next_empty(remaining, empty_get_next_empty(to_alloc));

	to_alloc->size = size;

	return remaining;
}

void heap_init()
{
	empty_first = (emalloc_empty_t)heap;
	emalloc_empty_t all_heap = (emalloc_empty_t)heap + 1;
	
	empty_set_size(empty_first, 0);
	empty_set_next_empty(empty_first, all_heap);
	
	empty_set_size(all_heap, sizeof(heap) - sizeof(struct struct_Empty));
	empty_set_next_empty(all_heap, (emalloc_empty_t)((uint8_t*)heap + sizeof(heap)));

	largest_heap = heap;
}

bool is_heap_variable(emalloc_block_t block)
{
	return (uint8_t*)block >= heap && (uint8_t*)block < heap_end;
}

// helper functions

static emalloc_len_t align_size(emalloc_len_t s)
{
	if (s < emalloc_min_block_size) {
		s = emalloc_min_block_size;
	}

	while ((s % emalloc_alignment) != 0)
		s++;

	return s;
}

emalloc_block_t obj_next(emalloc_block_t block)
{
	emalloc_len_t len = obj_size(block);
	len = align_size(len);
	return (emalloc_block_t)((uint8_t*)block + len);
}

void obj_print_member(emalloc_block_t obj)
{
	int i;
	for (i = 0; i < type_meta_count; i++)
	{
		type_meta_t cur = TypeMeta[i];
		if (cur.etypeid != obj->etype)
			continue;		

		EObject* pfield = (EObject*)((uint8_t*)obj + cur.offset);
		EObject field = *pfield;
		if (field == NULL)
			continue;

		printf("\t%p object id %d(%s) refcount %d\n", (void*)((uint8_t*)field - heap), field->etype, TypeNames[field->etype], field->refCount);
	}
	
}

void obj_heap_print(bool show_members)
{
	emalloc_block_t current_block = (emalloc_block_t)(emalloc_empty_t)heap + 1;
	emalloc_empty_t current_empty = empty_get_next_empty(empty_first);
	do
	{
		if (current_block == (emalloc_block_t)current_empty)
		{
			empty_print(current_empty);
			current_block = empty_next_block(current_empty);
			current_empty = empty_get_next_empty(current_empty);
		}
		else
		{
			obj_print(current_block);
			if (show_members)
				obj_print_member(current_block);
			current_block = obj_next(current_block);
		}
	} while ((uint8_t*)current_block < (uint8_t*)heap + heap_size);
}

void obj_heap_visit(visit_heap_callback_t callback)
{
	emalloc_block_t current_block = (emalloc_block_t)(emalloc_empty_t)heap + 1;
	emalloc_empty_t current_empty = empty_get_next_empty(empty_first);
	do
	{
		if (current_block == (emalloc_block_t)current_empty)
		{			
			current_block = empty_next_block(current_empty);
			current_empty = empty_get_next_empty(current_empty);
		}
		else
		{
			callback(current_block);
			current_block = obj_next(current_block);
		}
	} while ((uint8_t*)current_block < (uint8_t*)heap + heap_size);
}



emalloc_empty_t obj_find_empty_before(emalloc_block_t block)
{
	emalloc_empty_t prev;
	emalloc_empty_t current = empty_first;

	do
	{
		prev = current;
		current = empty_get_next_empty(current);

	} while ((uint8_t*)current < heap_end && (uint8_t*)current <= (uint8_t*)block);

	ASSERT((uint8_t*)prev != (uint8_t*)block);

	return prev;
}

emalloc_block_t obj_alloc(emalloc_len_t size)
{
	e_lock();

	size = align_size(size);

	emalloc_empty_t prev;
	emalloc_empty_t current = empty_first;

	do
	{
		prev = current;
		current = empty_get_next_empty(current);

		// some sanity checks
		ASSERT(empty_get_size(current) >= emalloc_min_block_size);
		ASSERT(empty_get_next_empty(current) != current);
		ASSERT((uint8_t*)current >= heap);

		// out of memory 
		if ((uint8_t*)current >= heap_end) {
			e_unlock();
			return NULL;
		}

	} while (!obj_can_alloc(current, size));

	// now we can alloc the object
	emalloc_empty_t next = empty_get_next_empty(current);
	emalloc_empty_t remaining = obj_split(current, size);

	if (remaining == NULL)
	{
		empty_set_next_empty(prev, next);
	}
	else
	{
		empty_set_next_empty(prev, remaining);
		empty_set_next_empty(remaining, next);
	}

	// keep track of the largest buffer usage
	void* current_obj_end = (uint8_t*)current + size;
	if (current_obj_end > largest_heap)
	{
		largest_heap = current_obj_end;
	}

	e_unlock();

	// make sure the new memory is initialized with zero.
	// otherwise we can have dangling pointers.
	memset(current, 0, size);
	return (emalloc_block_t)current;
}

void obj_free(emalloc_block_t block)
{
	e_lock();
	emalloc_empty_t empty = (emalloc_empty_t)block;
	empty_set_size(empty, align_size(obj_size(block)));

	emalloc_empty_t before = obj_find_empty_before(block);
	emalloc_empty_t after = empty_get_next_empty(before);

	empty_set_next_empty(empty, after);
	empty_set_next_empty(before, empty);

	// see if we can join any of the empty blocks
	if (obj_joint(before, empty))
		empty = before;
	obj_joint(empty, after);

	e_unlock();
}


emalloc_len_t heap_total_free()
{
	emalloc_len_t count = 0;
	emalloc_empty_t current = empty_get_next_empty(empty_first);

	do
	{
		count += empty_get_size(current);
		current = empty_get_next_empty(current);

	} while (current < (emalloc_empty_t)heap_end);

	return count;
}

emalloc_len_t heap_empty_block_count()
{
	emalloc_len_t count = 0;
	emalloc_empty_t current = empty_get_next_empty(empty_first);

	do
	{
		count += 1;
		current = empty_get_next_empty(current);

	} while (current < (emalloc_empty_t)heap_end);

	return count;
}

emalloc_len_t heap_max_usage()
{
	return  ((uint8_t*)largest_heap - (uint8_t*)heap);
}



#ifdef MALLOC_TEST
int _tmain(int argc, _TCHAR* argv[])
{
	ASSERT(emalloc_min_block_size >= sizeof(emalloc_empty_t));

	{
		printf("init\n");
		heap_init();
		ASSERT(heap_total_free() == heap_size);
		ASSERT(heap_empty_block_count() == 1);
		//obj_heap_print();
	}

	{
		printf("alloc\n");
		heap_init();
		malloc(16);
		ASSERT(heap_total_free() == heap_size - 16);
		ASSERT(heap_empty_block_count() == 1);
		//obj_heap_print();
	}

	{
		printf("free\n");
		heap_init();
		emalloc_block_t b = malloc(16);
		obj_free(b);
		ASSERT(heap_total_free() == heap_size);
		ASSERT(heap_empty_block_count() == 1);
		//obj_heap_print();
	}


	{
		printf("gap\n");
		heap_init();
		emalloc_block_t b = malloc(16);
		emalloc_block_t c = malloc(32);
		obj_free(b);
		ASSERT(heap_total_free() == heap_size - 32);
		ASSERT(heap_empty_block_count() == 2);
		//obj_heap_print();
	}

	{
		printf("join\n");
		heap_init();
		emalloc_block_t b = malloc(16);
		emalloc_block_t c = malloc(32);
		obj_free(b);
		obj_free(c);

		//obj_heap_print();
	}

	{
		printf("unaligned\n");
		heap_init();
		emalloc_block_t b = malloc(17);
		emalloc_block_t c = malloc(32);

		ASSERT(((intptr_t)c % emalloc_alignment) == 0);
		//obj_heap_print();
	}

	{
		printf("unaligned free\n");
		heap_init();
		emalloc_block_t b = malloc(17);
		emalloc_block_t c = malloc(32);
		obj_free(b);
		obj_free(c);

		ASSERT(heap_total_free() == heap_size);
		ASSERT(heap_empty_block_count() == 1);

		//obj_heap_print();
	}

	{
		printf("out of mem blocks\n");
		heap_init();
		emalloc_block_t b = malloc(heap_size);
		emalloc_block_t c = malloc(32);

		//obj_free(c);

		ASSERT(c == NULL);

		//obj_heap_print();
	}

	{
		printf("out of mem space\n");
		heap_init();
		emalloc_block_t b = malloc(heap_size + 1);

		ASSERT(b == NULL);

		//obj_heap_print();
	}

	return 0;
}
#endif

