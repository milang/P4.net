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
#include "DiffEngine.h"
#include "diff.h"
//#include "filesys.h"

p4dn::DiffEngine::DiffEngine()
{
	_encoding = System::Text::Encoding::GetEncoding(1252);
}


array<System::String^>^ p4dn::DiffEngine::RunDiff (IO::FileInfo^ f1, IO::FileInfo^ f2)
{
	List<String^> output = gcnew List<String^>();
	::Error* e = new ::Error();
    //
    // Duck binary files. Much the same as ClientUser::Diff, we just
    // put the output into Ruby space rather than stdout.
    //
  //  if( !f1->IsTextual() || !f2->IsTextual() )
  //  {
		//if ( f1->Compare( f2, e ) )
		//{
		//	output.Add("(... files differ ...)");
		//	//OutputInfo( 0, "(... files differ ...)" );
		//}
		//return;
  //  }

    // Time to diff the two text files. Need to ensure that the
    // files are in binary mode, so we have to create new FileSys
    // objects to do this.

	::FileSys *f1_bin = FileSys::Create( ::FST_BINARY );
	::FileSys *f2_bin = FileSys::Create( ::FST_BINARY );
	StrBuf f1_name, f2_name;
	P4String::StringToStrBuf(&f1_name, f1->FullName, _encoding);
	P4String::StringToStrBuf(&f2_name, f2->FullName, _encoding);
	f1_bin->Set(f1_name);
    f2_bin->Set(f2_name);

	::FileSys *t = FileSys::CreateGlobalTemp(::FST_TEXT );

	StrBuf flags;
	P4String::StringToStrBuf(&flags, _flags, _encoding);


    {
		//
		// In its own block to make sure that the diff object is deleted
		// before we delete the FileSys objects.
		//
		::Diff d;
		::DiffFlags diffFlags(&flags);

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
			//while( t->ReadLine( &b, e ) )
			//OutputInfo(0, b.Text() );
		}
    }
	
    delete t;
    delete f1_bin;
    delete f2_bin;
	delete e;
    //if ( e->Test() ) HandleError( e );


	return output.ToArray();
}