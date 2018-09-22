

#ifdef ARRAY_4_USED
void Array_4_ctor(Array_4 _this, int Length)
{
	_this->base.etype = Array_4_TypeId;
	_this->Length = Length;
}
#endif

void Array_1_ctor(Array_1 _this, int Length)
{
	_this->base.etype = Array_1_TypeId;
	_this->Length = Length;
}

// todo use common array method instead
int Array_1_GetLength(Array_1 _this)
{
	return _this->Length;
}

void Array_ref_ctor(Array_ref _this, int Length)
{
	_this->base.etype = Array_ref_TypeId;
	_this->Length = Length;
}

Array_1 Array_1_new(int Length)
{
	Array_1 _this = (Array_1)CMalloc_ArrayMalloc_1(Length);
	Array_1_ctor(_this, Length);
	return _this;
}
#ifdef ARRAY_4_USED
Array_4 Array_4_new(int Length)
{
	Array_4 _this = (Array_4)CMalloc_ArrayMalloc_4(Length);
	Array_4_ctor(_this, Length);
	return _this;
}
#endif

Array_ref Array_ref_new(int Length)
{
	Array_ref _this = (Array_ref)CMalloc_ArrayMalloc_ref(Length);
	Array_ref_ctor(_this, Length);
	return _this;
}