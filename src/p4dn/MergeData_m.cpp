/*
 * P4.Net *
Copyright (c) 2007 Shawn Hladky

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


#using <mscorlib.dll>
#include "StdAfx.h"
#include "MergeData_m.h"


p4dn::P4MergeData::P4MergeData(ClientMerge *m, ClientUser* cu, System::Text::Encoding^ encoding)
{
	_disposed = false;
	_encoding = encoding;
	this->_basePath = gcnew System::IO::FileInfo(
		P4String::StrPtrToString(m->GetBaseFile()->Path(), encoding));
	this->_thierPath = gcnew System::IO::FileInfo(
		P4String::StrPtrToString(m->GetTheirFile()->Path(), encoding));
	this->_resultPath = gcnew System::IO::FileInfo(
		P4String::StrPtrToString(m->GetResultFile()->Path(), encoding));
	this->_yourPath = gcnew System::IO::FileInfo(
		P4String::StrPtrToString(m->GetYourFile()->Path(), encoding));

	// Extract (forcibly) the paths from the RPC buffer.
    StrPtr *t;
    if( ( t = cu->varList->GetVar( "baseName" ) ) ) 
		_baseName = P4String::StrPtrToString(t, encoding);
    if( ( t = cu->varList->GetVar( "yourName" ) ) ) 
		_yourName = P4String::StrPtrToString(t, encoding);
    if( ( t = cu->varList->GetVar( "theirName" ) ) ) 
		_thierName = P4String::StrPtrToString(t, encoding);

	MergeStatus status = m->AutoResolve(CMF_FORCE);

	switch( status )
	{
	case CMS_QUIT:	
		_mergeHint = P4MergeStatus::CMS_QUIT;
		break;
	case CMS_SKIP:	
		_mergeHint = P4MergeStatus::CMS_SKIP;
		break;
	case CMS_MERGED:	
		_mergeHint = P4MergeStatus::CMS_MERGED;
		break;
	case CMS_EDIT:	
		_mergeHint = P4MergeStatus::CMS_EDIT;
		break;
	case CMS_YOURS:	
		_mergeHint = P4MergeStatus::CMS_YOURS;
		break;
	case CMS_THEIRS:	
		_mergeHint = P4MergeStatus::CMS_THEIRS;
		break;
	}

	_clientUser = cu;
	_clientMerge = m;
}

System::String^ p4dn::P4MergeData::GetBaseName()
{
	if (_disposed) throw gcnew System::ObjectDisposedException("P4MergeData");
	return _baseName;
}

System::String^ p4dn::P4MergeData::GetYourName()
{
	if (_disposed) throw gcnew System::ObjectDisposedException("P4MergeData");
	return _yourName;
}

System::String^ p4dn::P4MergeData::GetTheirName()
{
	if (_disposed) throw gcnew System::ObjectDisposedException("P4MergeData");
	return _thierName;
}


int	p4dn::P4MergeData::GetYourChunks()
{
	if (_disposed) throw gcnew System::ObjectDisposedException("P4MergeData");
	return _clientMerge->GetYourChunks();
}

int	p4dn::P4MergeData::GetTheirChunks()
{
	if (_disposed) throw gcnew System::ObjectDisposedException("P4MergeData");
	return _clientMerge->GetTheirChunks();
}

int	p4dn::P4MergeData::GetBothChunks()
{
	if (_disposed) throw gcnew System::ObjectDisposedException("P4MergeData");
	return _clientMerge->GetBothChunks();
}

int	p4dn::P4MergeData::GetConflictChunks()
{
	if (_disposed) throw gcnew System::ObjectDisposedException("P4MergeData");
	return _clientMerge->GetConflictChunks();
}

System::String^	p4dn::P4MergeData::GetMergeDigest()
{
	if (_disposed) throw gcnew System::ObjectDisposedException("P4MergeData");
	return P4String::StrPtrToString((StrPtr*)_clientMerge->GetTheirDigest(), _encoding);
}

System::String^	p4dn::P4MergeData::GetYourDigest()
{
	if (_disposed) throw gcnew System::ObjectDisposedException("P4MergeData");
	return P4String::StrPtrToString((StrPtr*)_clientMerge->GetTheirDigest(), _encoding);
}

System::String^	p4dn::P4MergeData::GetTheirDigest()
{
	if (_disposed) throw gcnew System::ObjectDisposedException("P4MergeData");
	return P4String::StrPtrToString((StrPtr*)_clientMerge->GetTheirDigest(), _encoding);
}


System::IO::FileInfo^ p4dn::P4MergeData::GetYourPath()
{
	if (_disposed) throw gcnew System::ObjectDisposedException("P4MergeData");
	return _yourPath;
}

System::IO::FileInfo^ p4dn::P4MergeData::GetTheirPath()
{
	if (_disposed) throw gcnew System::ObjectDisposedException("P4MergeData");
	return _thierPath;
}

System::IO::FileInfo^ p4dn::P4MergeData::GetBasePath()
{
	if (_disposed) throw gcnew System::ObjectDisposedException("P4MergeData");
	return _basePath;
}

System::IO::FileInfo^ p4dn::P4MergeData::GetResultPath()
{
	if (_disposed) throw gcnew System::ObjectDisposedException("P4MergeData");
	return _resultPath;
}

p4dn::P4MergeStatus	p4dn::P4MergeData::GetMergeHint()
{
	if (_disposed) throw gcnew System::ObjectDisposedException("P4MergeData");
	return _mergeHint;
}

bool p4dn::P4MergeData::RunMergeTool()
{
	if (_disposed) throw gcnew System::ObjectDisposedException("P4MergeData");
	::Error e;
    _clientUser->Merge( _clientMerge->GetBaseFile(), _clientMerge->GetTheirFile(),
	       _clientMerge->GetYourFile(), _clientMerge->GetResultFile(), &e );

    if( e.Test() ) return false;
    return true;

}

p4dn::P4MergeData::~P4MergeData()
{
	_disposed = true;
	_clientUser = NULL;
	_clientMerge = NULL;
}