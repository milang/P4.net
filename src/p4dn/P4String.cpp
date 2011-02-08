/*
 * P4.Net *
Copyright (c) 2007-2010 Shawn Hladky

Permission is hereby granted, free of charge, to any person obtaining a copy of this software 
and associated documentation files (the "Software"), to deal in the Software without 
restriction, including without limitation the rights to use, copy, modify, merge, publish, 
distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the 
Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or 
substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING 
BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, 
DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
*/


#include "StdAfx.h"
#include "P4String.h"

using namespace p4dn;


System::String^ P4String::StrPtrToString(::StrPtr* buffer, System::Text::Encoding^ encoding)
{
	if (!buffer)
	{
		return nullptr;
	}
	else
	{
		return gcnew System::String(buffer->Text(), 0, buffer->Length(), encoding);
	}
}

System::String^ P4String::CharArrToString(const char* buffer, System::Text::Encoding^ encoding)
{
	if (!buffer)
	{
		return nullptr;
	}
	else
	{
		return gcnew System::String(buffer, 0, (int)strlen(buffer), encoding);
	}
}

System::String^ P4String::ErrorToString(::Error* e, System::Text::Encoding^ encoding)
{
	StrBuf err;
	e->Fmt(&err);
	System::String^ errMsg = StrPtrToString(&err, encoding);
	return errMsg;
}

void P4String::StringToStrBuf(::StrBuf* buffer, System::String^ str, System::Text::Encoding^ encoding)
{
	if(str != nullptr && str->Length > 0)
	{
		array<unsigned char>^ b = encoding->GetBytes(str);
		pin_ptr<unsigned char> ptr = &(b[0]);

		//StrBuf makes a copy, so we're golden here.  ptr is unpinned when it goes out of scope.
		buffer->Set((const char*)ptr);
	
	}
	else
	{
		buffer->Set("");
	
	}
}



















////p4dn::P4String::P4String(System::String* str, System::Text::Encoding* encoding)
////{
////	_bytes = encoding->GetBytes(str);
////}
////
////p4dn::P4String::P4String(::StrPtr* str)
////{
////	_bytes = new System::Byte[str->Length()];
////	Marshal::Copy(System::IntPtr(str->Text()), _bytes, 0, str->Length());	
////}
////
////void p4dn::P4String::SetStrBuf(::StrBuf* str)
////{
////}
////
////System::String* p4dn::P4String::ToManagedString(System::Text::Encoding* encoding)
////{
////	return encoding->GetString(_bytes);
////}
////
