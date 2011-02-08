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

#include "StdAfx.h"
#include "clientapi_m.h"
#include "i18napi.h"

using namespace System::Runtime::InteropServices;

p4dn::ClientApi::ClientApi()
{
	_Disposed = false;
    _clientApi = new ::ClientApi();
    _keepAliveDelegate = NULL;
	
	// default to non-unicode server use ANSI encoding
	_encoding = System::Text::Encoding::GetEncoding(1252);
}

p4dn::ClientApi::~ClientApi()
{
	_Disposed = true;
	CleanUp();
}
::ClientApi* p4dn::ClientApi::getClientApi()
{
	// Oops someone called dispose too early!!!
	if (_Disposed)
	{
		//used to throw an exception here, but it seems to just cause other errror
		return NULL;
	}
	return _clientApi;
}
void p4dn::ClientApi::CleanUp()
{
	if (_clientApi != NULL) delete _clientApi;
	if (_keepAliveDelegate != NULL) delete _keepAliveDelegate;
	_clientApi = NULL;
	_keepAliveDelegate = NULL;
}

void p4dn::ClientApi::SetMaxResults(int maxResults)
{
	if( maxResults  )	getClientApi()->SetVar( "maxResults",  maxResults  );
}
void p4dn::ClientApi::SetMaxScanRows(int maxScanRows)
{
	if( maxScanRows )	getClientApi()->SetVar( "maxScanRows", maxScanRows );
}
void p4dn::ClientApi::SetMaxLockTime(int maxLockTime)
{
	if( maxLockTime )	getClientApi()->SetVar( "maxLockTime", maxLockTime );
}
void p4dn::ClientApi::SetTrans( int output, int content, int fnames, int dialog )
{
    getClientApi()->SetTrans( output, content, fnames, dialog );
}

void p4dn::ClientApi::SetProtocol( System::String^ p, System::String^ v )
{   
	StrBuf str_p; StrBuf str_v;
	P4String::StringToStrBuf(&str_p, p, _encoding);
    P4String::StringToStrBuf(&str_v, v , _encoding);
    getClientApi()->SetProtocol( str_p.Text(), str_v.Text() );    
}

void p4dn::ClientApi::SetArgv( array<System::String^>^ args )
{  
	StrBuf s;
	for (int i = 0; i < args->Length; ++i) {
		s.Clear();
		P4String::StringToStrBuf(&s, args[i], _encoding);
        getClientApi()->SetVar( "", &s );
    }
}


void p4dn::ClientApi::SetProtocolV( System::String^ p ) 
{ 
	StrBuf s;
	P4String::StringToStrBuf(&s, p, _encoding);
    getClientApi()->SetProtocolV( s.Text() );
}


System::String^ p4dn::ClientApi::GetProtocol( System::String^ v ) 
{ 
    StrBuf s;
	P4String::StringToStrBuf(&s, v, _encoding);
    StrPtr* ptr = getClientApi()->GetProtocol( s.Text() );
	return P4String::StrPtrToString(ptr, _encoding);
}

 void p4dn::ClientApi::Init( p4dn::Error^ e ) 
 { 
	if(getClientApi()->GetCharset().Length() > 0)
	{
		// unicode server use UTF-8
		_encoding = gcnew System::Text::UTF8Encoding();

		// set the translations (use UTF-8 for everything but content).
		CharSetApi::CharSet content = CharSetApi::Lookup(getClientApi()->GetCharset().Text());
		getClientApi()->SetTrans(CharSetApi::UTF_8, content, 
			CharSetApi::UTF_8, CharSetApi::UTF_8);
	}
	else
	{
		// non-unicode server use ANSI encoding
		_encoding = System::Text::Encoding::GetEncoding(1252);
	}
    getClientApi()->Init( e->InternalError );
	if (_keepAliveDelegate == NULL) _keepAliveDelegate = new KeepAliveDelegate();
	
	// Always set the KeepAlive... only do something if a managed KeepAlive is present.
	getClientApi()->SetBreak(_keepAliveDelegate);
 }

 void p4dn::ClientApi::Run( System::String^ func, p4dn::ClientUser^ ui ) 
 {
     StrBuf cmd;
	 P4String::StringToStrBuf(&cmd, func, _encoding);
	 ClientUserDelegate cud = ClientUserDelegate(ui, _encoding);
     getClientApi()->Run(cmd.Text(), &cud);              
 }

 p4dn::Error^ p4dn::ClientApi::CreateError()
 {
	 return gcnew p4dn::Error(_encoding);
 }

 p4dn::Spec^  p4dn::ClientApi::CreateSpec(System::String^ specDef)
 {
	 return gcnew p4dn::Spec(specDef, _encoding);
 }

 int p4dn::ClientApi::Final( p4dn::Error^ e ) 
 {      
 	::ClientApi* api = getClientApi();
	if(NULL != api)
	{
		return api->Final( e->InternalError );
	}
	return 1;
 }

 int p4dn::ClientApi::Dropped() 
 { 
     return getClientApi()->Dropped();
 }

 //
 // These functions are disabled.  There will be a lot of work to enable
 // asynchronous execution and ensure that all of references are released
 // so managed objects are garbage collected
 //
 //void p4dn::ClientApi::RunTag( String* func, p4dn::ClientUser *ui ) 
 //{      
 //   char* f = (char *)(void *) Marshal::StringToHGlobalAnsi( func );  
	//ClientUserDelegate __nogc* cud = new ClientUserDelegate(ui);
 //   _clientApi->RunTag( f, cud);
	//delete cud;
 //   Marshal::FreeHGlobal( f );
 //}


 //void p4dn::ClientApi::WaitTag( p4dn::ClientUser *ui )
 //{ 
	//ClientUserDelegate __nogc* cud = new ClientUserDelegate(ui);
 //   _clientApi->WaitTag( cud );
	//delete cud;
 //}

 void p4dn::ClientApi::SetTag()
 {
	 getClientApi()->SetVar( "tag", "");
	 //getClientApi()->SetVar( "specstring", "");
 }

 void p4dn::ClientApi::SetVar(System::String^ var, System::String^ val)
 {
	 StrBuf sbVar, sbVal;
	 P4String::StringToStrBuf(&sbVar, var, _encoding);
	 P4String::StringToStrBuf(&sbVal, val, _encoding);
     getClientApi()->SetVar( sbVar, sbVal);

 }
 void p4dn::ClientApi::SetCharset( String^ c ) 
 { 
     StrBuf f;
	 P4String::StringToStrBuf(&f, c, _encoding);
     getClientApi()->SetCharset( f.Text() );
 }

 void p4dn::ClientApi::SetClient( System::String^ c ) 
 { 
     StrBuf f;
	 P4String::StringToStrBuf(&f, c, _encoding);
     getClientApi()->SetClient( f.Text() );
 }

 void p4dn::ClientApi::SetCwd( System::String^ c ) 
 {
     StrBuf f;
	 P4String::StringToStrBuf(&f, c, _encoding);
     getClientApi()->SetCwd( f.Text() );
 }

 void p4dn::ClientApi::SetHost( System::String^ c ) 
 {
     StrBuf f;
	 P4String::StringToStrBuf(&f, c, _encoding);
     getClientApi()->SetHost( f.Text() );
 }

 void p4dn::ClientApi::SetLanguage( System::String^ c ) 
 {
     StrBuf f;
	 P4String::StringToStrBuf(&f, c, _encoding);
     getClientApi()->SetLanguage( f.Text() );
 }

 void p4dn::ClientApi::SetPassword( System::String^ c ) 
 {
	StrBuf f;
	P4String::StringToStrBuf(&f, c, _encoding);    
	getClientApi()->SetPassword( f.Text() );
 }

 void p4dn::ClientApi::SetPort( System::String^ c ) 
 {
	StrBuf f;
	P4String::StringToStrBuf(&f, c, _encoding);    
	getClientApi()->SetPort( f.Text() );
 }

 void p4dn::ClientApi::SetProg( System::String^ c ) 
 {
    StrBuf f;
	P4String::StringToStrBuf(&f, c, _encoding);    
	 getClientApi()->SetProg( f.Text() );
 }

 void p4dn::ClientApi::SetVersion( System::String^ c ) 
 {
    StrBuf f;
	P4String::StringToStrBuf(&f, c, _encoding);    
	 getClientApi()->SetVersion( f.Text() );
 }

 void p4dn::ClientApi::SetTicketFile( System::String^ c ) 
 {
    StrBuf f;
	P4String::StringToStrBuf(&f, c, _encoding);    
	getClientApi()->SetTicketFile( f.Text() );
 }

 void p4dn::ClientApi::SetUser( System::String^ c ) 
 {
    StrBuf f;
	P4String::StringToStrBuf(&f, c, _encoding);    
	 getClientApi()->SetUser( f.Text() );
 }

 void p4dn::ClientApi::SetBreak( p4dn::KeepAlive^ keepAlive )
 {
     _keepAliveDelegate->SetKeepAlive(keepAlive);
	 //getClientApi()->SetBreak(_keepAliveDelegate);
 }

 void p4dn::ClientApi::DefineCharset( System::String^ c, p4dn::Error^ e ) 
 { 
    StrBuf f;
	P4String::StringToStrBuf(&f, c, _encoding);    
    getClientApi()->DefineCharset( f.Text(), e->InternalError );

 }

 void p4dn::ClientApi::DefineClient( System::String^ c, p4dn::Error^ e ) 
 { 
    StrBuf f;
	P4String::StringToStrBuf(&f, c, _encoding);    
     getClientApi()->DefineClient( f.Text(), e->InternalError );
 }

 void p4dn::ClientApi::DefineHost( System::String^ c, p4dn::Error^ e ) 
 {
    StrBuf f;
	P4String::StringToStrBuf(&f, c, _encoding);    
    getClientApi()->DefineHost( f.Text(), e->InternalError );
 }

 void p4dn::ClientApi::DefineLanguage( System::String^ c, p4dn::Error^ e ) 
 { 
    StrBuf f;
	P4String::StringToStrBuf(&f, c, _encoding);    
    getClientApi()->DefineLanguage( f.Text(), e->InternalError );
 }

 void p4dn::ClientApi::DefinePassword( System::String^ c, p4dn::Error^ e ) 
 { 
    StrBuf f;
	P4String::StringToStrBuf(&f, c, _encoding);    
    getClientApi()->DefinePassword( f.Text(), e->InternalError );
 }

 void p4dn::ClientApi::DefinePort( System::String^ c, p4dn::Error^ e ) 
 { 
    StrBuf f;
	P4String::StringToStrBuf(&f, c, _encoding);    
    getClientApi()->DefinePort( f.Text(), e->InternalError );
 }

 void p4dn::ClientApi::DefineUser( System::String^ c, p4dn::Error^ e ) 
 { 
    StrBuf f;
	P4String::StringToStrBuf(&f, c, _encoding);    
    getClientApi()->DefineUser(f.Text(), e->InternalError );
 }
