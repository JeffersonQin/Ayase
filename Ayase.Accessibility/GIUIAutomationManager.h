#pragma once
#ifdef GIUIAUTOMATIONMANAGER_EXPORTS
#define GIUIAUTOMATIONMANAGER_API __declspec(dllexport)
#else
#define GIUIAUTOMATIONMANAGER_API __declspec(dllimport)
#endif

extern "C" GIUIAUTOMATIONMANAGER_API int InitializeUIAutomation();

extern "C" GIUIAUTOMATIONMANAGER_API wchar_t* GetForeGroundWindowName();
