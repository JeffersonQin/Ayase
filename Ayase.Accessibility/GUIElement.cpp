#include "pch.h" // use stdafx.h in Visual Studio 2017 and earlier
#include "GUIElement.h"

int GetBoundingRectangle(GUIElement* element, double* x, double* y, double* w, double* h) {
	*x = element->x;
	*y = element->y;
	*w = element->w;
	*h = element->h;
	return 1;
}

int GetName(GUIElement* element, wchar_t** name) {
	*name = element->name;
	return 1;
}

int DeleteUIElement(GUIElement* element) {
	delete element;
	return 1;
}
