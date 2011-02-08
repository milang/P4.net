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


#pragma once

#include "StdAfx.h"
#include <vcclr.h>
#include "options.h"

namespace p4dn {

	

	public ref class Options : public System::IDisposable 
    {

    public:

		enum class Flags {
		OPT_ONE = 0x01,		/// exactly one
		OPT_TWO = 0x02,		/// exactly two
		OPT_THREE = 0x04,	/// exactly three
		OPT_MORE = 0x08,	/// more than two
		OPT_NONE = 0x10,	/// require none
		OPT_MAKEONE = 0x20,	/// if none, make one that points to null

		// combos of the above

		OPT_OPT = 0x11,		/// NONE, or ONE
		OPT_ANY = 0x1F,		/// ONE, TWO, THREE, MORE, or NONE
		OPT_DEFAULT = 0x2F,	/// ONE, TWO, THREE, MORE, or MAKEONE
		OPT_SOME = 0x0F		/// ONE, TWO, THREE, or MORE
		};

        Options( void );
		~Options();			// : System::IDisposable::Dispose();
     	void Parse(array<System::String^>^ args, System::String^ opts, Flags flags);

		System::String^	GetValue(System::Char opt);
  
    private:
		::Options* _options;
		char** _argsArray;
		char*  _optionDefinition;
		bool   _parseSuccess;
		void CleanUp();
		int _argc;

    };

} // end namespace




