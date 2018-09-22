EString EString_Concat(EString a, EString b)
{
	int size = a->Length + b->Length;
	Array_1 newArray = (Array_1)CMalloc_ArrayMalloc_1(size);
	Array_1_ctor(newArray, size);
	Array_CopyTo_1((Array_1)a, newArray, 0);
	Array_CopyTo_1((Array_1)b, newArray, a->Length);

	return (EString)newArray;
}

bool EString_op_Equality(string a, string b)
{
	if (a->Length != b->Length)
	{
		return false;
	}

	if (memcmp(a->data, b->data, a->Length) != 0)
	{
		return false;
	}

	return true;
}

void EString_Finalize(EString _this)
{

}