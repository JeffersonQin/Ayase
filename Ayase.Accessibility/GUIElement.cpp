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

bool Compare(GUIElement* a, GUIElement* b) {
	double a_x = a->x + 0.5 * a->w;
	double a_y = a->y + 0.5 * a->h;
	double b_x = b->x + 0.5 * b->w;
	double b_y = b->y + 0.5 * b->h;
	if (a_y < b_y - GUIELEMENT_COMPARE_DISTANCE_Y_THRESHOLD) return true;
	if (a_y > b_y + GUIELEMENT_COMPARE_DISTANCE_Y_THRESHOLD) return false;
	return a->x < b->x;
}
