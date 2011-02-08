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
#include "Spec_m.h"
#include "P4String.h"

p4dn::Spec::Spec(System::String^ specDef, System::Text::Encoding^ encoding)
{
	_specDef = specDef;
	_encoding = encoding;
}

System::String^ p4dn::Spec::Format(System::Collections::Generic::Dictionary<System::String^, System::String^>^ sd, p4dn::Error^ err)
{
	StrBuf sSpecDef;
	
	P4String::StringToStrBuf(&sSpecDef, _specDef, _encoding);
	
	//Build our Spec objects
	::SpecDataTable	specData;
	::Spec		    spec(sSpecDef.Text(), "",  err->InternalError);

	if(err->InternalError->IsError())
	{
		return System::String::Empty;
	}

	//Spin the managed dictionary
	System::Collections::IEnumerator^ myEnum = sd->Keys->GetEnumerator();
	while (myEnum->MoveNext())
	{
		// Convert the key and values
		System::String^ key = static_cast<System::String^>(myEnum->Current);
		System::String^ val = static_cast<System::String^>(sd[key]);

		StrBuf sKey; StrBuf sVal;
		P4String::StringToStrBuf(&sKey, key, _encoding);
		P4String::StringToStrBuf(&sVal, val, _encoding);
		
		// Create the unmanaged Dict entry
		specData.Dict()->SetVar(sKey, sVal);
	}

	StrBuf strbuf;

	spec.Format(&specData, &strbuf);

	System::String^ SpecFormated = P4String::StrPtrToString(&strbuf, _encoding);

	return SpecFormated;
}

System::Collections::Generic::Dictionary<System::String^, System::String^>^ p4dn::Spec::Parse(System::String^ formated, p4dn::Error^ err)
{
	System::Collections::Generic::Dictionary<System::String^, System::String^>^ managedDict;
	
	::StrBuf specFormated;
	::StrBuf specdef;
	P4String::StringToStrBuf(&specFormated, formated, _encoding);
	P4String::StringToStrBuf(&specdef, _specDef, _encoding);
	
	managedDict = gcnew System::Collections::Generic::Dictionary<System::String^, System::String^>();

	::Spec s(specdef.Text(), "", err->InternalError);
	
	if(err->InternalError->IsError())
	{
		// dictionary is empty... caller needs to look at err
		return managedDict;
	}

	::SpecDataTable specData;
	s.Parse(specFormated.Text(), &specData, err->InternalError);
	if (err->InternalError->IsError())
	{
		// dictionary is empty... caller needs to look at err
		return managedDict;
	}
	
	StrRef var, val;
	int i = 0;
	while (specData.Dict()->GetVar(i, var, val) != -0) 
	{
		System::String^ key = P4String::CharArrToString(var.Text(), _encoding);
		System::String^ value = P4String::CharArrToString(val.Text(), _encoding);
		managedDict->Add( key, value );
		i++;
	}

	return managedDict;
}