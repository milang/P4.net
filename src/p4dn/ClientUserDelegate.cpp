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
#include "ClientUserDelegate.h"
#include "diff.h"


using namespace System::Runtime::InteropServices;
using namespace System::Collections;
using namespace System::Collections::Specialized;

using namespace p4dn;

ClientUserDelegate::ClientUserDelegate( gcroot<p4dn::ClientUser^> ManagedClientUser, gcroot<System::Text::Encoding^> encoding )
{
	mcu = ManagedClientUser;
	_encoding = encoding;
}

ClientUserDelegate::~ClientUserDelegate() 
{  
	delete mcu;
}

void ClientUserDelegate::InputData( StrBuf *strbuf, ::Error* err )
{    

	p4dn::Error^ e = gcnew p4dn::Error( err, _encoding );
	System::String^ s;
	mcu->InputData(s, e );
	P4String::StringToStrBuf(strbuf, s, _encoding);
	delete e;

}

void ClientUserDelegate::HandleError( ::Error *err )
{ 
    p4dn::Error^ e = gcnew p4dn::Error( err, _encoding);
    mcu->HandleError( e );
	delete e;
}

void ClientUserDelegate::Message( ::Error *err )
{        
    p4dn::Error^ e = gcnew p4dn::Error( err , _encoding);
    mcu->Message( e );
	delete e;
    
}

void ClientUserDelegate::OutputError( const_char *errBuf )
{
	System::String^ s = P4String::CharArrToString(errBuf, _encoding);
    mcu->OutputError( s );    
}

void ClientUserDelegate::OutputInfo( char level, const_char *data )
{
    System::String^ s = P4String::CharArrToString(data, _encoding);
    mcu->OutputInfo( level, s );    
}

void ClientUserDelegate::OutputBinary( const_char *data, int length )
{
	array<System::Byte>^ b = gcnew array<System::Byte>(length);
	Marshal::Copy(IntPtr((void*)data), b, 0, length);
	mcu->OutputContent(b, false);
}

void ClientUserDelegate::OutputText( const_char *data, int length )
{
	array<System::Byte>^ b = gcnew array<System::Byte>(length);
	Marshal::Copy(IntPtr((void*)data), b, 0, length);
	mcu->OutputContent(b, true);
}

void ClientUserDelegate::OutputStat( StrDict *varList )
{

	System::Collections::Generic::Dictionary<System::String^, System::String^>^  dict; 
	::SpecDataTable specData;
	
	StrPtr* data = varList->GetVar("data");
	StrPtr* specdef = varList->GetVar("specdef");
		
	// I'm not sure what the intention of the specFormatted field is
	// can't we just look for the "data" field?
	// StrPtr* specformatted = varList->GetVar("specFormatted");

	StrDict* Dict;

	int i = 0;
	::StrRef var, val;   

	dict = gcnew System::Collections::Generic::Dictionary<System::String^, System::String^>();


	if (specdef)
	{
		// Send the SpecDef to the ClientUser so it can save it if it wants
		System::String^ ManagedSpecDef = P4String::StrPtrToString(specdef, _encoding);
		mcu->SetSpecDef(ManagedSpecDef);
	}

	if (data)
	{
		// We have a form, not pre-parsed (i.e. pre-2005.2 server version)
		::Error e;
		::Spec s(specdef->Text(), "", &e);
		s.ParseNoValid(data->Text(), &specData, &e);
		Dict = specData.Dict();

	}
	else
	{
		// No form, just use the raw dictionary
		Dict = varList;
	}
	while (Dict->GetVar(i,var,val) != -0) 
	{   
		if (!( var == "specdef" || var == "func" || var == "specFormatted" ))
		{
			System::String^ key = P4String::CharArrToString(var.Text(), _encoding);
			System::String^ value = P4String::CharArrToString(val.Text(), _encoding);

			dict->Add( key, value );
			
		}
		i++;
	}
	mcu->OutputStat( dict );
}

void ClientUserDelegate::Prompt( const StrPtr& msg, StrBuf& rsp, int noEcho, ::Error *err )
{
    String^ response;
	String^ message = P4String::CharArrToString(msg.Text(), _encoding);
    bool bEcho = ( noEcho != 0 );
    p4dn::Error^ e = gcnew p4dn::Error( err, _encoding );

    mcu->Prompt( message, response, bEcho, e );
    
   	P4String::StringToStrBuf(&rsp, response, _encoding);
	delete e;
}

void ClientUserDelegate::ErrorPause( char *errBuf, ::Error *err )
{
    System::String^ s = P4String::CharArrToString(errBuf, _encoding);
    p4dn::Error^ e = gcnew p4dn::Error( err, _encoding );
    mcu->ErrorPause( s, e ); 
	delete e;
}

void ClientUserDelegate::Edit( FileSys *f1, ::Error *err )
{    
    p4dn::Error^ e = gcnew p4dn::Error( err , _encoding);
    System::String^ name = P4String::CharArrToString(f1->Name(), _encoding);
    System::IO::FileInfo^ info = gcnew System::IO::FileInfo( name );
    mcu->Edit( info, e );
	delete e;
}

void ClientUserDelegate::Diff( FileSys *f1, FileSys *f2, int doPage, char *diffFlags, ::Error *e )
{
    //
    // Duck binary files. Much the same as ClientUser::Diff, we just
    // put the output into Ruby space rather than stdout.
    //
    if( !f1->IsTextual() || !f2->IsTextual() )
    {
		if ( f1->Compare( f2, e ) )
		{
			OutputInfo( 0, "(... files differ ...)" );
		}
		return;
    }

    // Time to diff the two text files. Need to ensure that the
    // files are in binary mode, so we have to create new FileSys
    // objects to do this.

	::FileSys *f1_bin = FileSys::Create( ::FST_BINARY );
	::FileSys *f2_bin = FileSys::Create( ::FST_BINARY );
    ::FileSys *t = FileSys::CreateGlobalTemp( f1->GetType() );

    f1_bin->Set( f1->Name() );
    f2_bin->Set( f2->Name() );

    {
		//
		// In its own block to make sure that the diff object is deleted
		// before we delete the FileSys objects.
		//
		::Diff d;

		d.SetInput( f1_bin, f2_bin, diffFlags, e );
		if ( ! e->Test() ) d.SetOutput( t->Name(), e );
		if ( ! e->Test() ) d.DiffWithFlags( diffFlags );
		d.CloseOutput( e );

		// OK, now we have the diff output, read it in and add it to 
		// the output.
		if ( ! e->Test() ) t->Open( FOM_READ, e );
		if ( ! e->Test() ) 
		{
			StrBuf 	b;
			while( t->ReadLine( &b, e ) )
			{
				OutputInfo(0, b.Text() );
			}
		}
    }
	
    delete t;
    delete f1_bin;
    delete f2_bin;
    if ( e->Test() ) HandleError( e );
}


void ClientUserDelegate::Merge( FileSys *base, FileSys *leg1, FileSys *leg2, FileSys *result, ::Error *e )
{
		ClientUser::Merge(base, leg1, leg2, result, e);
}

int ClientUserDelegate::Resolve(ClientMerge *m, ::Error *e)
{
	p4dn::P4MergeData^ mergeData;
	try
	{
		mergeData = gcnew p4dn::P4MergeData(m, this, _encoding);
		P4MergeStatus status = mcu->Resolve(mergeData);
		switch(status)
		{
			case P4MergeStatus::CMS_EDIT:
				return ::CMS_QUIT;
			case P4MergeStatus::CMS_MERGED:
				return ::CMS_MERGED;
			case P4MergeStatus::CMS_QUIT:
				return ::CMS_QUIT;
			case P4MergeStatus::CMS_SKIP:
				return ::CMS_SKIP;
			case P4MergeStatus::CMS_THEIRS:
				return ::CMS_THEIRS;
			case P4MergeStatus::CMS_YOURS:
				return ::CMS_YOURS;
		}
	}
	catch(System::Exception^ exception)
	{
		mcu->OutputError(exception->Message);
		return ::CMS_QUIT;
	}
	finally
	{
		if (mergeData != nullptr) delete mergeData;		
	}
	return ::CMS_QUIT;
	
}

void ClientUserDelegate::Help( const_char *const *help )
{
    System::String^ s = P4String::CharArrToString(*help, _encoding);
    mcu->Help( s );    
}

FileSys* ClientUserDelegate::File( FileSysType type )
{        
    return FileSys::Create( type );    
}

void ClientUserDelegate::Finished() 
{    
    mcu->Finished();    
}
