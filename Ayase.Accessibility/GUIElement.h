#pragma once
#ifdef GUIELEMENT_EXPORTS
#define GUIELEMENT_API __declspec(dllexport)
#else
#define GUIELEMENT_API __declspec(dllimport)
#endif

#include "GConstant.h"

struct GUIElement {
	double x = 0, y = 0, w = 0, h = 0;
	wchar_t* name = 0;
};

extern "C" GUIELEMENT_API int GetBoundingRectangle(GUIElement* element, double* x, double* y, double* w, double* h);

extern "C" GUIELEMENT_API int GetName(GUIElement* element, wchar_t** name);

extern "C" GUIELEMENT_API int DeleteUIElement(GUIElement * element);

extern "C" GUIELEMENT_API bool Compare(GUIElement* a, GUIElement* b);
