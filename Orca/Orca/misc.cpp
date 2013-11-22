//--------------------------------------------------------------------
// file misc.cpp
//
// Stephen D. Bay
// COPYRIGHT 2003
//--------------------------------------------------------------------

#include <iostream>
#include <vector>
#include <algorithm>
#include <math.h>

#include "outliers.h"
#include "database.h"
#include "misc.h"

extern float MISSING_C;

//--------------------------------------------------------------------
// printrecord
//--------------------------------------------------------------------
void printrecord(vector<real> &R, vector<integer> &D) 
{
	for (int i=0;i<R.size();i++) {
		if (R[i] == MISSING_C) {
			cout << "? ";
		}
		else {
			cout << R[i] << " ";
		}
	}

	cout << "| ";
	printvector(D);
	cout << endl;
};

//--------------------------------------------------------------------
// spacing
//--------------------------------------------------------------------
int spacing(int number)
{
	int s = (int) floor(log10(((double) number)));
	return (s+1); 
};


//--------------------------------------------------------------------
// spaces
//--------------------------------------------------------------------
void spaces(int numspaces)
{
	for (int i=0;i<numspaces;i++) {
		cout << " ";
	}
};

//--------------------------------------------------------------------
// defuzz
//--------------------------------------------------------------------
double defuzz(double number)
{
	if (fabs(number) < 0.000001)
		return 0;
	else
		return number;
};
 

