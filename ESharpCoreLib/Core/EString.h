// pre-header

struct struct_EString;
typedef struct struct_EString EString_struct;
typedef struct struct_EString * EString;

// internal includes
// external includes
// header


// needs to be binary compatible with Array_1
struct struct_EString
{
	EObject_struct base;
	short Length;
	unsigned char data[];
};



#define ESTRING_(x) { 1 /*ref count*/, EString_TypeId, sizeof(x) - 1, { x } }
#define ESTRING(NAME, X) \
	const EString_struct NAME##_ = ESTRING_(X); \
	const EString NAME = (EString)&(NAME##_);

void EString_Finalize(EString _this);
EString EString_Concat(EString a, EString b);
bool EString_op_Equality(string a, string b);
