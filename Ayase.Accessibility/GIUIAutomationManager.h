#pragma once
#ifdef GIUIAUTOMATIONMANAGER_EXPORTS
#define GIUIAUTOMATIONMANAGER_API __declspec(dllexport)
#else
#define GIUIAUTOMATIONMANAGER_API __declspec(dllimport)
#endif

#include "GUIElement.h"

extern "C" GIUIAUTOMATIONMANAGER_API int InitializeUIAutomation();

extern "C" GIUIAUTOMATIONMANAGER_API wchar_t* GetForegroundWindowName();

extern "C" GIUIAUTOMATIONMANAGER_API GUIElement* test();
