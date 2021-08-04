#pragma once
#ifdef GUIELEMENT_EXPORTS
#define GUIELEMENT_API __declspec(dllexport)
#else
#define GUIELEMENT_API __declspec(dllimport)
#endif

struct GUIElement {
	double x, y, w, h;
	wchar_t* name;
};

extern "C" GUIELEMENT_API int GetBoundingRectangle(GUIElement* element, double* x, double* y, double* w, double* h);

extern "C" GUIELEMENT_API int GetName(GUIElement* element, wchar_t** name);

extern "C" GUIELEMENT_API int DeleteUIElement(GUIElement * element);
