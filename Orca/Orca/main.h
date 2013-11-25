//--------------------------------------------------------------------
// file main.h
//
// Stephen D. Bay
// COPYRIGHT 2003
//--------------------------------------------------------------------
#ifndef __MAIN_H
#define __MAIN_H

#include <vector>
#include <string>

using namespace std;

typedef pair<double,string> mypair;

// Descending sorting function
struct DescendingSort
{
	bool operator()(const mypair &A, const mypair &B) {
		return (A > B); 
	};
};


#endif // ends __MAIN_H
