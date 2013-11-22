//--------------------------------------------------------------------
// file anomaly.h
//
// Stephen D. Bay
// COPYRIGHT 2003
//--------------------------------------------------------------------
#ifndef __OUTLIERS_H
#define __OUTLIERS_H


#include <iostream>
#include <vector>
#include <algorithm>

#include "database.h"
#include "misc.h"

using namespace std;

//--------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------
const double big_distance = 10000000000.0;

//--------------------------------------------------------------------
// Struct Outlier
//--------------------------------------------------------------------
struct Outlier
{
	int index_;
	double score_;
	vector<int> neighbors_;
};

class comp_outlier{
	public:
		bool operator() (const Outlier &O1, const Outlier &O2) {
			return(O1.score_ > O2.score_);
		};
};


//--------------------------------------------------------------------
// Outlier Detection Functions
//--------------------------------------------------------------------

void find_outliers(MTable &M, DTable &D, int k, real cutoff, int scoref, int distf, vector<double> &Rw, vector<double> &Dw, vector<Outlier> &O, bool not_same);

void find_outliers_index(MTable &M, DTable &D, int k, real cutoff, int scoref, int distf, vector<double> &Rw, vector<double> &Dw, vector<Outlier> &O, bool not_same);


//--------------------------------------------------------------------
// Distance functions
//--------------------------------------------------------------------
double distance(vector<real> &Areal, vector<integer> &Aint, vector<real> &Breal, vector<integer> &Bint);

double distance(vector<real> &Areal, vector<integer> &Aint, vector<real> &Breal, vector<integer> &Bint, vector<double> &Rweights, vector<double> &Dweights);


//--------------------------------------------------------------------
// Misc functions
//--------------------------------------------------------------------
void distance_contribution(DTable &Dref, DTable &Dtest, Outlier &O, int scoref, vector<double> &Rw, vector<double> &Dw, vector<double> &Rc, vector<double> &Dc);



#endif // ends __OUTLIERS_H
