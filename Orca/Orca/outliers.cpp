//--------------------------------------------------------------------
// file outliers.cpp
//
// Stephen D. Bay
// COPYRIGHT 2003
//--------------------------------------------------------------------

#include <iostream>
#include <vector>
#include <algorithm>

#include <functional>

#include "outliers.h"
#include "database.h"
#include "misc.h"

extern float MISSING_C;
extern float DIST_M_R;

//-------------------------------------------------------------------
// distance
//
// Calculates the distance between two examples assuming a fixed, 
// default weighting.
//--------------------------------------------------------------------
double distance(vector<real> &Areal, vector<integer> &Aint, vector<real> &Breal, vector<integer> &Bint)
{
	int nr = Areal.size();
	double d = 0;

	for (int i=0;i<nr;i++) {
		// check for missing values
		int nm = 0;
		if (Areal[i] == MISSING_C) {
			nm++;
		}
		if (Breal[i] == MISSING_C) {
			nm++;
		}
		if (nm == 0) {	
			double diff = (Areal[i] - Breal[i]);
			d += diff * diff;
		}
		// one value is missing
		else if (nm == 1) {
			d += DIST_M_R;
		}
	}

	int nd = Aint.size();
	for (int i=0;i<nd;i++) {
		if (Aint[i] != Bint[i]) {
			d += 0.4;
		}
	}
	return d;
};



//--------------------------------------------------------------------
// distance
//
// Calculates the distance between two examples with weights specified
// in Rweights and Dweights.
//--------------------------------------------------------------------
double distance(vector<real> &Areal, vector<integer> &Aint, vector<real> &Breal, vector<integer> &Bint, vector<double> &Rweights, vector<double> &Dweights)
{
	int nr = Areal.size();
	double d = 0;

	// real 
	for (int i=0;i<nr;i++) {
		// check for missing values
		int nm = 0;
		if (Areal[i] == MISSING_C) {
			nm++;
		}
		if (Breal[i] == MISSING_C) {
			nm++;
		}
		if (nm == 0) {	
			double diff = (Areal[i] - Breal[i]);
			d += diff * diff * Rweights[i];
		}
		// one value is missing
		else if (nm == 1) {
			d += DIST_M_R;
		}
	}

	// discrete
	int nd = Aint.size();
	for (int i=0;i<nd;i++) {
		if (Aint[i] != Bint[i]) {
			d += Dweights[i];
		}
	}
	return d;
};





//--------------------------------------------------------------------
// find_outliers
//
//--------------------------------------------------------------------
void find_outliers(MTable &M, DTable &D, int k, real cutoff, int scoref, int distf, vector<double> &Rw, vector<double> &Dw, vector<Outlier> &O, bool not_same)
{

	// iterators to the test cases: real and discrete parts
	list<vector<real> > &Real = M.R_;
	list<vector<integer> > &Discrete= M.D_;

	list<vector<real> >::iterator R_itr = Real.begin();
	list<vector<integer> >::iterator D_itr = Discrete.begin();

	int n = D.num_records();
	int m = M.nr_;

	// vectors to store distance of nearest neighbors
	// initialize distance score with value big_distance
	list<vector<double> > kdist;
	vector<double> dtemp(k,big_distance);
	make_heap(dtemp.begin(),dtemp.end());
	for (int i=0;i<m;i++) {
		kdist.push_back(dtemp);
	}

	vector<real> r(M.num_rvars(),0);
	vector<integer> d(M.num_dvars(),0);
	list<vector<double> >::iterator kdist_itr = kdist.begin();

	// vector to store furthest nearest neighbour
	list<double> minkdist;
	for (kdist_itr = kdist.begin();kdist_itr != kdist.end();kdist_itr++) {
		minkdist.push_back((*kdist_itr)[k-1]);
	}

	// candidates stores the integer index
	list<int> candidates;
	for (int i=0;i<m;i++) {
		candidates.push_back(i);
	}
	list<int>::iterator candidates_itr = candidates.begin();
	list<double>::iterator minkdist_itr = minkdist.begin();

	// loop over objects in reference table
	for (int i=0;i<n;i++) {
		D.get_next_object(r,d);

		R_itr = Real.begin();
		D_itr = Discrete.begin();
		kdist_itr = kdist.begin();
		minkdist_itr = minkdist.begin();
		candidates_itr = candidates.begin();

		// loop over objects in test table
		for (R_itr=Real.begin();R_itr != Real.end();R_itr++) {
			double dist = 0;
			switch (distf) {
				case 0: dist = distance(*R_itr,*D_itr,r,d); break;
				case 1: dist = distance(*R_itr,*D_itr,r,d,Rw,Dw); break;
			}

			if (dist < *minkdist_itr) {
				if ((M.offset_ + *candidates_itr != i) || (not_same)) {
					vector<double> &kvec = *kdist_itr;
					kvec.push_back(dist);
					push_heap(kvec.begin(),kvec.end());
					pop_heap(kvec.begin(),kvec.end());
					kvec.pop_back();
					*minkdist_itr = kvec[0];

					//--------------------------------------
					// Score Function for determing outliers
					// 
					real score=0;
					switch (scoref) {
						case 0:  // average distance to k neighbors 
							for (int it=0;it<k;it++) {
								score += kvec[it];
							}
							break;
						case 1:  // distance to kth neighbor
							score = kvec[0];
							break;
					}

					if (score <= cutoff) {
						candidates.erase(candidates_itr--);
						Real.erase(R_itr--);
						Discrete.erase(D_itr--);
						kdist.erase(kdist_itr--);
						minkdist.erase(minkdist_itr--);
						if (candidates.begin() == candidates.end() ) {
							i = n;
							// exit loop and return
						}
					}
				}
			}
			candidates_itr++;
			D_itr++;
			kdist_itr++;
			minkdist_itr++;
		}
	}

	//--------------------------------
	// update the list of top outliers 
	// 
	candidates_itr = candidates.begin();
	for (kdist_itr=kdist.begin();kdist_itr != kdist.end();kdist_itr++) {
		double sum = 0;
		switch (scoref) {
			case 0:  // average distance to k neighbors
				for (int j=0;j<kdist_itr->size();j++) {
					sum += (*kdist_itr)[j];
				}
				break;
			case 1:  // distance to kth neighbor
					sum = (*kdist_itr)[kdist_itr->size()-1];
					break;
		}

		Outlier outlier;
		outlier.index_ = M.offset_ + *candidates_itr++;
		outlier.score_ = sum;
		O.push_back(outlier);

	}
};


//--------------------------------------------------------------------
// find_outliers_index
//
// Records the index of the nearest neighbors.
//
//--------------------------------------------------------------------
void find_outliers_index(MTable &M, DTable &D, int k, real cutoff, int scoref, int distf, vector<double> &Rw, vector<double> &Dw, vector<Outlier> &O, bool not_same)
{

	// iterators to the test cases: real and discrete parts
	list<vector<real> > &Real = M.R_;
	list<vector<integer> > &Discrete= M.D_;

	list<vector<real> >::iterator R_itr = Real.begin();
	list<vector<integer> >::iterator D_itr = Discrete.begin();

	int n = D.num_records();
	int m = M.nr_;

	// vectors to store distance of nearest neighbors
	// initialize distance score with value big_distance

	// vectors to store index and distance of nearest neighbors
	list<vector<ScoreIndex> > kdist;
	ScoreIndex I(-1,big_distance);
	vector<ScoreIndex> dtemp(k,I);
	make_heap(dtemp.begin(),dtemp.end());
	for (int i=0;i<m;i++) {
		kdist.push_back(dtemp);
	}

	vector<real> r(M.num_rvars(),0);
	vector<integer> d(M.num_dvars(),0);
	list<vector<ScoreIndex> >::iterator kdist_itr = kdist.begin();

	// vector to store furthest nearest neighbour
	list<double> minkdist;
	for (kdist_itr = kdist.begin();kdist_itr != kdist.end();kdist_itr++) {
		minkdist.push_back((*kdist_itr)[k-1].score_);
	}

	// candidates stores the integer index
	list<int> candidates;
	for (int i=0;i<m;i++) {
		candidates.push_back(i);
	}
	list<int>::iterator candidates_itr = candidates.begin();
	list<double>::iterator minkdist_itr = minkdist.begin();

	// loop over objects in reference table
	for (int i=0;i<n;i++) {
		D.get_next_object(r,d);

		R_itr = Real.begin();
		D_itr = Discrete.begin();
		kdist_itr = kdist.begin();
		minkdist_itr = minkdist.begin();
		candidates_itr = candidates.begin();

		// loop over objects in test table
		for (R_itr=Real.begin();R_itr != Real.end();R_itr++) {
			double dist = 0;
			switch (distf) {
				case 0: dist = distance(*R_itr,*D_itr,r,d); break;
				case 1: dist = distance(*R_itr,*D_itr,r,d,Rw,Dw); break;
			}
			if (dist < *minkdist_itr) {
				if ((M.offset_ + *candidates_itr != i) || (not_same)) {
					
					vector<ScoreIndex> &kvec = *kdist_itr;
					ScoreIndex I(i,dist);
					kvec.push_back(I);
					push_heap(kvec.begin(),kvec.end());
					pop_heap(kvec.begin(),kvec.end());
					kvec.pop_back();
					*minkdist_itr = kvec[0].score_;
					

					//--------------------------------------
					// Score Function for determing outliers
					// 
					real score=0;
					switch (scoref) {
						case 0:  // average distance to k neighbors 
							for (int it=0;it<k;it++) {
								score += kvec[it].score_;
							}
							break;
						case 1:  // distance to kth neighbor
							score = kvec[0].score_;
							break;
					}
					if (score <= cutoff) {
						candidates.erase(candidates_itr--);
						Real.erase(R_itr--);
						Discrete.erase(D_itr--);
						kdist.erase(kdist_itr--);
						minkdist.erase(minkdist_itr--);
						if (candidates.begin() == candidates.end() ) {
							i = n;
							// exit loop and return
						}
					}
				}
			}
			candidates_itr++;
			D_itr++;
			kdist_itr++;
			minkdist_itr++;
		}
	}

	//--------------------------------
	// update the list of top outliers 
	// 
	candidates_itr = candidates.begin();
	for (kdist_itr=kdist.begin();kdist_itr != kdist.end();kdist_itr++) {
		double sum = 0;
		switch (scoref) {
			case 0:  // average distance to k neighbors
				for (int j=0;j<kdist_itr->size();j++) {
					sum += (*kdist_itr)[j].score_;
				}
				break;
			case 1:  // distance to kth neighbor
					sum = (*kdist_itr)[kdist_itr->size()-1].score_;
					break;
		}

		Outlier outlier;
		outlier.index_ = M.offset_ + *candidates_itr++;
		outlier.score_ = sum;
		for (int i=0;i<kdist_itr->size();i++) {
			outlier.neighbors_.push_back((*kdist_itr)[i].index_);
		}
		O.push_back(outlier);
	}
};


//--------------------------------------------------------------------
// distance_contribution
//
// For a given outlier, this function calculates the contribution of
// each feature to the outlier's distance score.  
//
// Rw  weight vector for real features
// Dw  weight vector for discrete features
// Rc  vector to store contributions for real features
// Dc  vector to store contributions for discrete features
//--------------------------------------------------------------------

void distance_contribution(DTable &Dref, DTable &Dtest, Outlier &O, int scoref, vector<double> &Rw, vector<double> &Dw, vector<double> &Rc, vector<double> &Dc)
{
	// the outlier
	vector<real> Ro(Dtest.num_rvars(),0); 
	vector<integer> Do(Dtest.num_dvars(),0);
	Dtest.get_object(O.index_,Ro,Do);
	
	// vectors to store neighbors
	vector<real> Rnn(Dref.num_rvars(),0); 
	vector<integer> Dnn(Dref.num_dvars(),0);

	// for each real feature
	for (int i=0;i<Rc.size();i++) {
		vector<double> distv;
		vector<double> Rwi = Rw;
		Rwi[i] = 0;

		// for each neighbor calculate the distance if feature i had 0 weight
		for (int j=0;j<O.neighbors_.size();j++) {
			Dref.get_object(O.neighbors_[j],Rnn,Dnn);
			double dist_score=0;
			dist_score = distance(Ro,Do,Rnn,Dnn,Rwi,Dw);
			distv.push_back(dist_score);
		}

		// calculate the score
		double score = 0;
		switch (scoref) {
			case 0:  // average distance to k neighbors
				for (int k=0;k<distv.size();k++) {
					score += distv[k];
				}
				break;
			case 1:  // distance to kth neighbor
				//sort(distv.begin(),distv.end(),greater<double>());
				sort(distv.begin(),distv.end(),greater<double>());
				score = distv[0];
				break;
		}

		// the contribution is the drop in score
		Rc[i] = O.score_ - score;
	}

	// for each discrete feature
	for (int i=0;i<Dc.size();i++) {
		vector<double> distv;
		vector<double> Dwi = Dw;
		Dwi[i] = 0;

		// for each neighbor calculate the distance if feature i had 0 weight
		for (int j=0;j<O.neighbors_.size();j++) {
			Dref.get_object(O.neighbors_[j],Rnn,Dnn);
			double dist_score=0;
			dist_score = distance(Ro,Do,Rnn,Dnn,Rw,Dwi);
			distv.push_back(dist_score);
		}

		// calculate the score
		double score = 0;
		switch (scoref) {
			case 0:  // average distance to k neighbors
				for (int k=0;k<distv.size();k++) {
					score += distv[k];
				}
				break;
			case 1:  // distance to kth neighbor
				sort(distv.begin(),distv.end());
				score = distv[0];
				break;
		}

		// the contribution is the drop in score
		Dc[i] = O.score_ - score;
	}
};


