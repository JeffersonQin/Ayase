#pragma once
#ifdef GIUIAUTOMATIONMANAGER_EXPORTS
#define GIUIAUTOMATIONMANAGER_API __declspec(dllexport)
#else
#define GIUIAUTOMATIONMANAGER_API __declspec(dllimport)
#endif

#include <vector>
#include "GUIElement.h"
#include <combaseapi.h>
#include <comdef.h>
#include <comutil.h>
#include <WinUser.h>
#include <UIAutomation.h>

extern "C" GIUIAUTOMATIONMANAGER_API HRESULT InitializeUIAutomation();

extern "C" GIUIAUTOMATIONMANAGER_API HRESULT GetForegroundWindowElement
    (IUIAutomationElement * *foundWindow);

extern "C" GIUIAUTOMATIONMANAGER_API HRESULT GetElementName
    (IUIAutomationElement * element, BSTR * name);

extern "C" GIUIAUTOMATIONMANAGER_API HRESULT GetElementBounds
    (IUIAutomationElement * element, DOUBLE * x, DOUBLE * y, DOUBLE * w, DOUBLE * h);

extern "C" GIUIAUTOMATIONMANAGER_API HRESULT GetLeafElementsFromWindow
    (IUIAutomationElement * window, std::vector<GUIElement*>**leafElements, int* elementCount, int* stopFlag);

extern "C" GIUIAUTOMATIONMANAGER_API HRESULT DeleteLeafElements
    (std::vector<GUIElement*>*leafElements);

extern "C" GIUIAUTOMATIONMANAGER_API HRESULT GetGUIElement
    (std::vector<GUIElement*>*leafElements, int index, GUIElement * *element);
