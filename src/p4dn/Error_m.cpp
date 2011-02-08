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
#include "Error_m.h"
#include <stdlib.h>
#include "strtable.h"

p4dn::Error::Error( ::Error* e, System::Text::Encoding^ encoding)
{
	_encoding = encoding;
	_err = e;
    _requiresFree = false;
	_Disposed = false;
}

p4dn::Error::Error( System::Text::Encoding^ encoding )
{   
	_err = new ::Error();
	_encoding = encoding;
	_requiresFree = true;
	_Disposed = false;
}

p4dn::Error::~Error( void )
{
	_Disposed = true;
	CleanUp();
}
void p4dn::Error::CleanUp()
{
	if (_requiresFree && _err != NULL ) delete _err;
	_err = NULL;
	_requiresFree = false;
}

::Error* p4dn::Error::InternalError::get()
{
	// Silly programer called Dispose() too early
	// I will be nice to them and reset the object
	if(_Disposed)
	{
		_err = new ::Error();
		_Disposed = false;
		System::GC::ReRegisterForFinalize(this);
	}
	return _err;
}

 void p4dn::Error::Clear()
 {
	 InternalError->Clear();
 }

 bool p4dn::Error::Test() 
 {
    return InternalError->Test() != 0;
 }

 bool p4dn::Error::IsInfo()
 {
     return InternalError->GetSeverity() == E_INFO;   
 }

 bool p4dn::Error::IsWarning()
 {
     return InternalError->GetSeverity() == E_WARN;     
 }

 bool p4dn::Error::IsFailed()
 {
     return InternalError->GetSeverity() == E_FAILED;     
 }

 p4dn::ManagedErrorID^ p4dn::Error::GetErrorID()
 {
	 ErrorId* id = InternalError->GetId(0);
	 if (!id) return nullptr;
	 return gcnew p4dn::ManagedErrorID(id, _encoding);
 }

 /*
 array<System::String^>^ p4dn::Error::GetVars()
 {
	StrPtrDict sd;
	InternalError->Marshall1(sd);

	StrRef var, val;

	for(int i = 0; sd.GetVar( i, var, val); i++ )
	{
		//seems like a total hack :-)
		if(var.Length() >  0 && var[0] >= 'a' && var[0] <= 'z')
		{
			//StrOps::Dump(var);
			fprintf(stdout, "[%d] %s : [%d] %s\n", var.Length(), var.Text(), val.Length(), val.Text()); 
		}
	}

	 return gcnew array<System::String^>();
 }
 */

 int p4dn::Error::GetID()
 {
	return InternalError->GetId(0)->UniqueCode();
 }

 void p4dn::Error::GetVariables(SortedDictionary<System::String^, System::String^>^ ht)
 {
	int i = 0;
	::StrRef var, val;   

	while (InternalError->GetDict()->GetVar(i,var,val) != -0) 
	{   
	
		System::String^ key = P4String::CharArrToString(var.Text(), _encoding);
		System::String^ value = P4String::CharArrToString(val.Text(), _encoding);

		if (!ht->ContainsKey(key))
		{
			ht->Add( key, value );
		}
		i++;
		
	}
 } 

 bool p4dn::Error::IsFatal()
 {
     return InternalError->GetSeverity() == E_FATAL;     
 }

 System::String^ p4dn::Error::Fmt()
 {     
     ::StrBuf buf;
     InternalError->Fmt( &buf ); 
	 System::String^ temp = P4String::StrPtrToString(&buf, _encoding);
     return temp->Trim();
 }

