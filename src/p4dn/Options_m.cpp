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
#include "Options_m.h"
#include <stdlib.h>
using namespace System::Runtime::InteropServices;

p4dn::Options::Options(void)
{
	_options = new ::Options();
	_parseSuccess = false;
}

p4dn::Options::~Options(void)
{
	CleanUp();
}

void p4dn::Options::CleanUp()
{
	// cleanup all the dynamic memory
	if (_argsArray != NULL)
	{
		for (int i = 0; i < _argc; ++i) {
			Marshal::FreeHGlobal( System::IntPtr(_argsArray[i]) );
		}	
		delete _argsArray;
	}
	_argsArray = NULL;

	if (_optionDefinition != NULL)
	{
		Marshal::FreeHGlobal(System::IntPtr(_optionDefinition));
	}
	
	if (_options != NULL ) delete _options;
	_options = NULL;
}

void p4dn::Options::Parse(array<System::String^>^ args, System::String^ opts, Flags flags)
{

	// reset everything if this is a repeat call
	if (_argsArray != NULL)
	{
		CleanUp();
		_options = new ::Options();
	}

	// convert managed array to char**
	_argsArray = new char* [ args->Length ];
	_argc = args->Length;
    for (int i = 0; i < args->Length; ++i) {
        _argsArray[i] = (char *)(void *) Marshal::StringToHGlobalAnsi( args[i] );
    }

	// convert options string
	_optionDefinition = (char *)(void *) Marshal::StringToHGlobalAnsi(opts);

	int argc = _argc;
	::Error* e = new ::Error();
	ErrorId usage = { E_FAILED, "Usage: parse optionstring flag args" };

	// we need to store off the original pointer, b/c it will get lost
	char** args2 = _argsArray;
	_options->Parse(argc, args2, _optionDefinition, (int) flags, usage, e );

   if( e->Test() )
   {
       _parseSuccess = false;
		CleanUp();
		_options = new ::Options();
   }
   else
   {
	   _parseSuccess = true;
   }

	delete e;

}

System::String^	p4dn::Options::GetValue(System::Char opt)
{
	System::String^ ret = nullptr;
	if (!_parseSuccess)
	{
		throw gcnew System::Exception("Can't obtain value.  Parse was unsuccessful.");
	}
	return gcnew System::String(_options->GetValue(System::Convert::ToByte(opt),0)->Text());
}