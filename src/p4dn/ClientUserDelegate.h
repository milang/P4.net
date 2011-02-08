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
#include "Error_m.h"
#include "ClientUser_m.h"
#include <vcclr.h>

//================================================================
// wrapper for callbacks into into the ClienUser class
//
//
// Implements callbacks to interface the clientapi with the .NET interface
//================================================================
class ClientUserDelegate : public ::ClientUser 
{
	private:
		gcroot<p4dn::ClientUser^> mcu;
		gcroot<System::Text::Encoding^> _encoding;
	public:            
		ClientUserDelegate( gcroot<p4dn::ClientUser^> ManagedClientUser, gcroot<System::Text::Encoding^> encoding );
		~ClientUserDelegate();
		void InputData( StrBuf *strbuf, ::Error *e );
		void HandleError( ::Error *err );
		void Message( ::Error *err );
		void OutputError( const_char *errBuf );
		void OutputInfo( char level, const_char *data );
		void OutputBinary( const_char *data, int length );
		void OutputText( const_char *data, int length );
		void OutputStat( StrDict *varList );
		void Prompt( const StrPtr &msg, StrBuf &rsp, int noEcho, ::Error *e );
		void ErrorPause( char *errBuf, ::Error *e );
		void Edit( FileSys *f1, ::Error *e );
		void Diff( FileSys *f1, FileSys *f2, int doPage, char *diffFlags, ::Error *e );
		void Merge( FileSys *base, FileSys *leg1, FileSys *leg2, FileSys *result, ::Error *e );
		void Help( const_char *const *help );
		FileSys *File( FileSysType type );
		void Finished();
		int	Resolve( ClientMerge *m, Error *e );
};
