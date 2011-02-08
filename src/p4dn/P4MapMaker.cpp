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

// Ported from P4Python, copyright below.
/*******************************************************************************

Copyright (c) 2008, Perforce Software, Inc.  All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

1.  Redistributions of source code must retain the above copyright
notice, this list of conditions and the following disclaimer.

2.  Redistributions in binary form must reproduce the above copyright
notice, this list of conditions and the following disclaimer in the
documentation and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
ARE DISCLAIMED. IN NO EVENT SHALL PERFORCE SOFTWARE, INC. BE LIABLE FOR ANY
DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

*******************************************************************************/

#include "StdAfx.h"
#include "mapapi.h"
#include "P4MapMaker.h"

using namespace p4dn;
P4MapMaker::P4MapMaker(System::Text::Encoding^ encoding)
{
	_encoding = encoding;
	map = new MapApi;
}

P4MapMaker::~P4MapMaker()
{
	delete map;
}

P4MapMaker^
P4MapMaker::Join( P4MapMaker^ l, P4MapMaker^ r)
{
	P4MapMaker^ m = gcnew P4MapMaker(l->_encoding);
	delete m->map;

	m->map = MapApi::Join( l->map, r->map );
	return m;
}

void
P4MapMaker::Insert( String^ m )
{
	StrBuf	in;
	StrBuf	lbuf;
	StrBuf	r;
	StrRef	l;
	MapType	t = MapInclude;

	P4String::StringToStrBuf(&in, m, _encoding);
	SplitMapping( in, lbuf, r );

	l = lbuf.Text();

	// Look for mapType in lhs only.
	if( l[ 0 ] == '-' )
	{
		l += 1;
		t = MapExclude;
	}
	else if( l[ 0 ] == '+' )
	{
		l += 1;
		t = MapOverlay;
	}

	map->Insert( l, r, t );
}


void
P4MapMaker::Insert(String^ l, String^ r )
{
	StrBuf		left;
	StrBuf		right;
	StrBuf *	dest = &left;
	int			quoted = 0;
	int			index = 0;

	const char *p;
	MapType	t = MapInclude;

	StrBuf l_in;
	StrBuf r_in;
	P4String::StringToStrBuf(&l_in, l, _encoding);
	p = l_in.Text();
	for( ; ; )
	{
		for( index = 0; *p; p++ )
		{
			switch( *p )
			{
			case '"':
				quoted = !quoted;
				break;

			case ' ':
			case '\t':
				// Embedded whitespace ok; leading not.
				if( quoted || index )
				{
					dest->Extend( *p );
					index++;
				}
				break;

			case '-':
				if( !index )
					t = MapExclude;
				index++;
				break;

			case '+':
				if( !index )
					t = MapOverlay;
				index++;
				break;

			default:
				dest->Extend( *p );
				index++;
			}
		}
		if( dest == &right )
			break;

		dest = &right;
		P4String::StringToStrBuf(&r_in, r, _encoding);
		p = r_in.Text();
		quoted = 0;
	}
	left.Terminate();
	right.Terminate();

	map->Insert( left, right, t );
}

int
P4MapMaker::Count()
{
	return map->Count();
}

void
P4MapMaker::Clear()
{
	map->Clear();
}

void
P4MapMaker::Reverse()
{
	MapApi *		nmap = new MapApi;
	const StrPtr *	l;
	const StrPtr *	r;
	MapType		t;

	for( int i = 0; i < map->Count(); i++ )
	{
		l = map->GetLeft( i );
		r = map->GetRight( i );
		t = map->GetType( i );

		nmap->Insert( *r, *l, t );
	}

	delete map;
	map = nmap;
}

String^ 
P4MapMaker::Translate( String^ p, bool fwd )
{
	StrBuf	from;
	StrBuf	to;
	MapDir	dir = MapLeftRight;

	if( !fwd )
		dir = MapRightLeft;

	P4String::StringToStrBuf(&from, p, _encoding);
	if( map->Translate( from, to, dir ) )
	{
		return P4String::StrPtrToString(&to, _encoding);
	}
	return nullptr;
}

array<System::String^>^	P4MapMaker::Lhs()
{
	array<System::String^>^ a = gcnew array<String^>(map->Count());
	StrBuf		s;
	const StrPtr *	l;
	MapType		t;
	int			quote;

	for( int i = 0; i < map->Count(); i++ )
	{
		s.Clear();
		quote = 0;

		l = map->GetLeft( i );
		t = map->GetType( i );

		if( l->Contains( StrRef( " " ) ) )
		{
			quote++;
			s << "\"";
		}

		switch( t )
		{
		case MapInclude:
			break;
		case MapExclude:
			s << "-";
			break;
		case MapOverlay:
			s << "+";
		};

		s << l->Text();
		if( quote ) s << "\"";

		a[i] = P4String::StrPtrToString(&s, _encoding);
	}
	return a;

}

array<System::String^>^	P4MapMaker::Rhs()
{

	array<System::String^>^ a = gcnew array<String^>(map->Count());
	StrBuf		s;
	const StrPtr *	r;
	int			quote;

	for( int i = 0; i < map->Count(); i++ )
	{
		s.Clear();
		quote = 0;

		r = map->GetRight( i );

		if( r->Contains( StrRef( " " ) ) )
		{
			quote++;
			s << "\"";
		}

		s << r->Text();
		if( quote ) s << "\"";
		a[i] = P4String::StrPtrToString(&s, _encoding);
	}
	return a;

}

array<System::String^>^	P4MapMaker::ToA()
{
	array<System::String^>^	a = gcnew array<String^>(map->Count());
    StrBuf		s;
    const StrPtr *	l;
    const StrPtr *	r;
    MapType		t;
    int			quote;

    for( int i = 0; i < map->Count(); i++ )
    {
		s.Clear();
		quote = 0;

		l = map->GetLeft( i );
		r = map->GetRight( i );
		t = map->GetType( i );

		if( l->Contains( StrRef( " " ) ) ||
			r->Contains( StrRef( " " ) ) )
		{
			quote++;
			s << "\"";
		}

		switch( t )
		{
		case MapInclude:
			break;
		case MapExclude:
			s << "-";
			break;
		case MapOverlay:
			s << "+";
		};

		s << l->Text();

		if( quote ) s << "\" \"";
		else s << " ";

		s << r->Text();
		if( quote ) s << "\"";

		a[i] = P4String::StrPtrToString(&s, _encoding);
    }
    return a;
}


String^ P4MapMaker::Inspect()
{
	StrBuf b;

	b << "P4.Map object: ";

	if( !map->Count() )
	{
		b << "(empty)";
		return P4String::StrPtrToString(&b, _encoding);
	}

	const StrPtr *l, *r;
	int	  t;

	b << "\n";

	for( int i = 0; i < map->Count(); i++ )
	{

		l = map->GetLeft( i );
		r = map->GetRight( i );
		t = map->GetType( i );

		b << "\t";
		switch( t )
		{
		case MapExclude:
			b << "-";
			break;

		case MapOverlay:
			b << "+";
			break;

		case MapInclude:
			break;
		}

		b << l->Text();
		b << " ";
		b << r->Text();
		b << "\n";
	}
	return P4String::StrPtrToString(&b, _encoding);

}

//
// Take a single string containing either a half-map, or both halves of
// a mapping and split it in two. If there's only one half of a mapping in
// the input, then l, and r are set to the same value as 'in'. If 'in'
// contains two halves, then they are split.
//
void
P4MapMaker::SplitMapping( const StrPtr &in, StrBuf &l, StrBuf &r )
{
	char *	pos;
	int		quoted = 0;
	int		split = 0;
	StrBuf *	dest = &l;

	pos = in.Text();

	l.Clear();
	r.Clear();

	while( *pos )
	{
		switch( *pos )
		{
		case '"':
			quoted = !quoted;
			break;

		case ' ':
			if( !quoted && !split )
			{
				// whitespace in the middle. skip it, and start updating
				// the destination
				split = 1;
				dest->Terminate();
				dest = &r;
			}
			else if( !quoted )
			{
				// Trailing space on rhs. ignore
			}
			else
			{
				// Embedded space
				dest->Extend( *pos );
			}
			break;

		default:
			dest->Extend( *pos );
		}
		pos++;
	}
	l.Terminate();
	r.Terminate();

	if( !r.Length() )
		r = l;

}
