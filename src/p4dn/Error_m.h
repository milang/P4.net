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

//
// Copyright 2010 Jacob Gladish. All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification, are
// permitted provided that the following conditions are met:
// 
//    1. Redistributions of source code must retain the above copyright notice, this list of
//       conditions and the following disclaimer.
// 
//    2. Redistributions in binary form must reproduce the above copyright notice, this list
//       of conditions and the following disclaimer in the documentation and/or other materials
//       provided with the distribution.
// 
// THIS SOFTWARE IS PROVIDED BY Jacob Gladish ``AS IS'' AND ANY EXPRESS OR IMPLIED
// WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL <COPYRIGHT HOLDER> OR
// CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON
// ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
// NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
// ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
// 
// The views and conclusions contained in the software and documentation are those of the
// authors and should not be interpreted as representing official policies, either expressed
// or implied, of <copyright holder>.
// 
#pragma once

#include "StdAfx.h"
#include <vcclr.h>
#include "error.h"
using namespace System;
using namespace System::Collections::Generic;

namespace p4dn {

	public ref class ManagedErrorID
	{
	public:
		int		code;		// ErrorOf
		System::String^ fmt;

		int	SubCode() 		{ return (code >> 0) & 0x3ff; }
		int	Subsystem()		{ return (code >> 10) & 0x3f; }
		int	Generic()		{ return (code >> 16) & 0xff; }
		int	ArgCount()		{ return (code >> 24) & 0x0f; }
		int	Severity()		{ return (code >> 28) & 0x0f; }
		int	UniqueCode()	{ return code & 0xffff; }

		System::Collections::Generic::SortedDictionary<System::String^, System::String^>^ GetArgs()
		{
			return _vars;
		}

	internal:
		ManagedErrorID(::ErrorId *e, System::Text::Encoding^ encoding)
		{
			code = e->code;
			fmt = P4String::CharArrToString(e->fmt, encoding);
			_vars = gcnew System::Collections::Generic::SortedDictionary<System::String^, System::String^>();
		}
		
		void AddVar(StrPtr* var, StrPtr* val, System::Text::Encoding^ encoding)
		{
			System::String^ mvar = P4String::StrPtrToString(var, encoding);
			System::String^ mval = P4String::StrPtrToString(val, encoding);
			_vars->Add(mvar, mval);
		}
		
	private:
		System::Collections::Generic::SortedDictionary<System::String^, System::String^>^ _vars;

	} ;

	public ref class Error : public System::IDisposable 
    {

    public:

		Error( System::Text::Encoding^ encoding);

		/*
			From error.h:
			E_EMPTY = 0,	// nothing yet
			E_INFO = 1,	// something good happened
			E_WARN = 2,	// something not good happened
			E_FAILED = 3,	// user did somthing wrong
			E_FATAL = 4	// system broken -- nothing can continue

		*/
		enum class ErrorSeverity {
			Empty = 0,
			Info = 1,
			Warning = 2,
			Failed = 3,
			Fatal = 4
		};

		~Error();	 // : System::IDisposable::Dispose();
        void Clear();
				
        bool Test();
        bool IsInfo();
        bool IsWarning();
		bool IsFailed();
        bool IsFatal();

		int  GetID();
		
		void			GetVariables(SortedDictionary<String^, String^>^ ht);
		ManagedErrorID^ GetErrorID();
        
        System::String^ Fmt();

        property ErrorSeverity Severity {
            ErrorSeverity get() {
                return static_cast< ErrorSeverity >( _err->GetSeverity() );
			}
        }

		property int GenericCode {
			int get() { return  _err->GetGeneric(); }
		}

    private:          
        ::Error* _err;
        bool _requiresFree;
		bool _Disposed;
		System::String^ _Instance;
		System::Text::Encoding^ _encoding;
		void CleanUp();

    internal:        
        Error( ::Error* e, System::Text::Encoding^ encoding);
		property ::Error* InternalError {
			::Error* get();
        }
    };

} // end namespace




