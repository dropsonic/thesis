//--------------------------------------------------------------------
// misc.h 
//
// Stephen Bay
// Copyright 2003 
//--------------------------------------------------------------------
#ifndef __MISC_H
#define __MISC_H

#include <iostream>
#include <string>
#include <vector>

using namespace std;

void printrecord(vector<real> &R, vector<integer> &D);

//--------------------------------------------------------------------
// Template Functions
//--------------------------------------------------------------------


//--------------------------------------------------------------------
// printvector
//--------------------------------------------------------------------
template<class T> void printvector(vector<T> &V)
{
  for (int i=0;i<V.size();i++) {
    cout << V[i] << " ";
  }
}

//--------------------------------------------------------------------
// printlongvector
//--------------------------------------------------------------------
template<class T> void printlongvector(vector<T> &V, int rowsize, int indent)
{
	int rowcount = 0;
  for (int i=0;i<V.size();i++) {
		if (rowcount == 0)
			spaces(indent);
    cout << V[i] << " ";
		if (++rowcount == rowsize) {
			if (i != V.size()-1) 
				cout << endl;
			rowcount = 0;
		}
  }
	cout << endl;
}


//--------------------------------------------------------------------
// Index
//--------------------------------------------------------------------
struct ScoreIndex
{
	int index_;
	double score_;

	ScoreIndex(int i, double s) { index_ = i; score_ = s;};
	bool operator< (const ScoreIndex &rhs)
	{
		return (score_ < rhs.score_);
	}
};

class comp{
	public:
		bool operator() (const ScoreIndex &T1, const ScoreIndex &T2) {
			return(T1.score_ > T2.score_);
		};
};

class comp_lt{
	public:
		bool operator() (const ScoreIndex &T1, const ScoreIndex &T2) {
			return(T1.score_ < T2.score_);
		};
};

 
//--------------------------------------------------------------------
// misc functions
//--------------------------------------------------------------------
int spacing(int number);
void spaces(int numspaces);
double defuzz(double number);


#endif // ends __MISC_H
