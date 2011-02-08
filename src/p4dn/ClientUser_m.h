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
#include "Error_m.h"
#include <vcclr.h>


using namespace System;
using namespace System::Collections;

namespace p4dn {

    enum FileSystemType {
        FST_TEXT =	    0x0001,	// file is text
        FST_BINARY =	0x0002,	// file is binary
        FST_GZIP =	    0x0003,	// file is gzip
        FST_DIRECTORY =	0x0005,	// it's a directory
        FST_SYMLINK =	0x0006,	// it's a symlink
        FST_RESOURCE =	0x0007,	// Macintosh resource file
        FST_SPECIAL =	0x0008,	// not a regular file
        FST_MISSING =	0x0009,	// no file at all
        FST_CANTTELL =	0x000A,	// can read file to find out
        FST_EMPTY =	    0x000B,	// file is empty
        FST_UNICODE =	0x000C,	// file is unicode

        FST_MASK =	    0x000F,	// mask for types

        // Modifiers
        FST_M_APPEND =	0x0010,	// open always append
        FST_M_EXCL =	0x0020,	// open exclusive create
        FST_M_SYNC =	0x0040,	// fsync on close

        FST_M_EXEC = 	0x0100,	// file is executable
        FST_M_APPLE =	0x0200,	// apple single/double encoding
        FST_M_COMP =	0x0400, // file is somehow compressed

        FST_M_MASK =	0x0ff0,	// mask for modifiers

        // Line ending types, loosely mapped to LineType

        FST_L_LOCAL =	0x0000,	// LineTypeLocal
        FST_L_LF =	    0x1000,	// LineTypeRaw
        FST_L_CR =	    0x2000,	// LineTypeCr
        FST_L_CRLF =	0x3000,	// LineTypeCrLf
        FST_L_LFCRLF =	0x4000,	// LineTypeLfcrlf

        FST_L_MASK =	0xf000,	// mask for LineTypes

        // Composite types, for filesys.cc        
        FST_ATEXT =	      0x0011,	// append-only text
        FST_XTEXT =	      0x0101,	// executable text
        FST_RTEXT =	      0x1001,	// raw text
        FST_RXTEXT =	  0x1101,	// executable raw text
        FST_CBINARY =	  0x0402,	// pre-compressed binary
        FST_XBINARY =	  0x0102,	// executable binary
        FST_APPLETEXT =	  0x0201,	// apple format text
        FST_APPLEFILE =	  0x0202,	// apple format binary
        FST_XAPPLEFILE =  0x0302,	// executable apple format binary
        FST_XUNICODE =	  0x010C,	// executable unicode text
        FST_RCS =	      0x1041 	// RCS temporary file: raw text, sync on close
    };

    public ref class ClientUser {

    public:
		ClientUser();
		~ClientUser(){}
        virtual void InputData(	String^% buff, p4dn::Error^ err );
		virtual void InputForm(	Hashtable^% varList, String^% specdef, p4dn::Error^ err );
		virtual void HandleError( p4dn::Error^ err );
        virtual void Message( p4dn::Error^ err );
        virtual void OutputError(String^ errString	);
        virtual void OutputInfo(Char level, String^ data );
        virtual void OutputContent(array<System::Byte>^ b, bool text);
		virtual void SetSpecDef(String^ specdef);
        virtual void OutputStat(System::Collections::Generic::Dictionary<System::String^, System::String^>^  varList );

        virtual void Prompt( const String^ msg,  
                             String^% rsp,
                             bool noEcho,
                             p4dn::Error^ err );
        virtual void ErrorPause( String^ errBuf, Error^ err );
        virtual void Edit( IO::FileInfo^ f1, Error^ err );
        virtual void Merge(	IO::FileInfo^ base, 
                            IO::FileInfo^ leg1, 
                            IO::FileInfo^ leg2, 
                            IO::FileInfo^ result,	
                            p4dn::Error^ err );

		virtual P4MergeStatus Resolve(P4MergeData^ mergeData);
        virtual void Help( String^ help	);       
        virtual void Finished()	{}
	};
}
