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


#pragma once

#include "StdAfx.h"
#include "Error_m.h"
#include <vcclr.h>


using namespace System;
using namespace System::Collections;

namespace p4dn {

    public enum class P4MergeStatus {
    	CMS_QUIT,	// user wants to quit
		CMS_SKIP,	// skip the integration record
		CMS_MERGED,	// accepted merged theirs and yours
		CMS_EDIT,	// accepted edited merge
		CMS_THEIRS,	// accepted theirs
		CMS_YOURS	// accepted yours,
    };

	public ref class P4MergeData : public System::IDisposable 
	{

	internal:
		P4MergeData(ClientMerge *m, ::ClientUser* cu, System::Text::Encoding^ encoding);

	public:
		System::String^	GetYourName();
		System::String^	GetTheirName();
		System::String^	GetBaseName();

		System::IO::FileInfo^ GetYourPath();
		System::IO::FileInfo^ GetTheirPath();
		System::IO::FileInfo^ GetBasePath();
		System::IO::FileInfo^ GetResultPath();

		int	GetYourChunks();
	    int	GetTheirChunks();
		int	GetBothChunks();
		int	GetConflictChunks();

		System::String^	GetMergeDigest();
		System::String^	GetYourDigest();
		System::String^	GetTheirDigest();


		P4MergeStatus	GetMergeHint();

		bool RunMergeTool();

		~P4MergeData();

	private:
		System::String^ _yourName;
		System::String^ _baseName;
		System::String^ _thierName;

		System::IO::FileInfo^ _yourPath;
		System::IO::FileInfo^ _thierPath;
		System::IO::FileInfo^ _basePath;
		System::IO::FileInfo^ _resultPath;

		P4MergeStatus	_mergeHint;

		System::Text::Encoding^ _encoding;

		bool _disposed;

		::ClientUser*  _clientUser;
		::ClientMerge* _clientMerge;

	};
}
