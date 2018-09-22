
// todo this doesn't work yet and is not included.
// created boxed type on the fly when needed. Don't generate object header for value types.
#ifdef BOXEDINT32_USED
EObject BoxedInt32_box(BoxedInt32 _vt) {
	EObject o = CMalloc_Malloc(BoxedInt32_TypeId);
	memcpy((uint8_t*)o + sizeof(EObject_struct), (uint8_t*)&_vt + sizeof(EObject_struct), sizeof(_vt) - sizeof(EObject_struct));
	return o;
}
BoxedInt32* BoxedInt32_unbox(EObject _o) {
	return (BoxedInt32 *)_o;
}
BoxedInt32 BoxedInt32_unbox_any(EObject _o) {
	return *((BoxedInt32 *)_o);
}
#endif