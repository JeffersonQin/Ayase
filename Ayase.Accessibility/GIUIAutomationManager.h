#pragma once
#ifdef GIUIAUTOMATIONMANAGER_EXPORTS
#define GIUIAUTOMATIONMANAGER_API __declspec(dllexport)
#else
#define GIUIAUTOMATIONMANAGER_API __declspec(dllimport)
#endif

#include <vector>
#include "GUIElement.h"

extern "C" GIUIAUTOMATIONMANAGER_API int InitializeUIAutomation();

extern "C" GIUIAUTOMATIONMANAGER_API int GetLeafElementsFromForegroundWindow(std::vector<GUIElement*>** leafElements, int* elementCount);

extern "C" GIUIAUTOMATIONMANAGER_API int DeleteLeafElements(std::vector<GUIElement*>* leafElements);

extern "C" GIUIAUTOMATIONMANAGER_API int GetGUIElement(std::vector<GUIElement*>* leafElements, int index, GUIElement** element);
