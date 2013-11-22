//--------------------------------------------------------------------
// file main.cpp
//
// Stephen D. Bay
// COPYRIGHT 2003
//--------------------------------------------------------------------

#include <iostream>

#include <algorithm>
#include <vector>
#include <list>

#include "main.h"
#include "database.h"
#include "outliers.h"
#include "parameters.h"

#include "misc.h"

//--------------------------------------------------------------------
// Global variables
//
// Used for missing values
//--------------------------------------------------------------------
float MISSING_C; // the number representing continuous missing values
float DIST_M_R;  // the distance between a continuous and missing value

//--------------------------------------------------------------------
// print_usage
//--------------------------------------------------------------------
void print_usage(void) {
	cout << "Usage: orca test_cases reference_cases weights [options]" << endl;
	cout << "Options: " << endl;
	cout << endl;

	cout << "Outlier options " << endl;
	cout << "  -avg   average distance to k nearest neighbors (default)" << endl;
	cout << "  -kth   distance to the kth nearest neighbor" << endl;
	cout << "  -n X   find top X outliers (30)" << endl;
	cout << "  -k X   use X nearest neighbors (5)" << endl;
	cout << "  -c X   initial cutoff (5)" << endl;
	cout << endl;

	cout << "Computation options" << endl;
	cout << "  -b X   batch size (1000)" << endl;
	cout << "  -s X   start batch size (1000)" << endl;
	cout << endl;

	cout << "Miscellaneous options" << endl;
	cout << "  -rn    record nearest neighbors of outliers" << endl;
	cout << "  -m X   use X to represent missing values (-989898)" << endl;
	cout << "  -woff  ignore weights" << endl;
	cout << endl;

};


//--------------------------------------------------------------------
// print_run_info
//--------------------------------------------------------------------
void print_run_info(int argc, char **argv)
{
	// print the command line call
	cout << "Command line call: " << endl;
	cout << " ";
	for (int i=0;i<argc;i++) {
		cout << argv[i] << " ";
	}
	cout << endl;
};


//--------------------------------------------------------------------
// MAIN
//--------------------------------------------------------------------
int main(int argc, char **argv) 
{
	if (argc < 4) {
		print_usage();
		exit(0);
	}

	// print report header
	cout << "ORCA: Mining Distance Based Outliers" << endl;
	cout << endl;

	print_run_info(argc,argv);
	cout << endl;

	// get the parameters
	Parameters P(argc,argv);
	P.check();
	P.print();
	cout << endl;

	// WARNING -- I'm using global values to remove the need to pass
	// a parameter to distance functions for the number that represents
	// a missing value
	MISSING_C = P.missing_;
	DIST_M_R = P.dist_m_r_;

	// load the test cases
	cout << "Setting up test cases from " << P.data_file_ << endl;
	MTable M(P.data_file_,P.start_batch_size_,P.batch_size_);
	cout << "  " << M.num_records() << " records" << endl;
	cout << "  " << M.num_rvars()   << " real variables"  << endl;
	cout << "  " << M.num_dvars()   << " discrete variables" << endl;
	cout << endl;

	//-------------------------------------------------------------
	// print several cases to make sure the file was read correctly
	//
	int r = min(M.num_records(),10);
	cout << "Printing first " << r << " records" << endl; 

	list<vector<real> >::iterator R_iter = M.R_.begin();
	list<vector<integer> >::iterator D_iter = M.D_.begin();

	for (int i=0;i<r;i++) {
		cout << "  ";
		printrecord(*R_iter++,*D_iter++);
	}
	cout << endl;

	//-----------------------------
	// setup the reference database
	//
	cout << "Setting up reference cases from file " << P.reference_file_ << endl;
  DTable D(P.reference_file_);
	cout << "  " << D.num_records() << " records" << endl;
	cout << "  " << D.num_rvars()   << " real variables"  << endl;
	cout << "  " << D.num_dvars()   << " discrete variables" << endl;
	cout << endl;


	//-----------------------------------------------------
	// check to make sure that reference and test databases 
	// have same structure
	
	if (M.num_rvars() != D.num_rvars()) {
		cerr << "error: test and reference cases must have same number of real variables" << endl;
		exit(0);
	}

	if (M.num_dvars() != D.num_dvars()) {
		cerr << "error: test and reference cases must have same number of discrete variables" << endl;
		exit(0);
	}

	//-------------------------------------------------------------
	// Setup weights
	//
	cout << "Loading features and weights" << endl;
	Weights W(M.num_rvars(),M.num_dvars(),P.weight_file_);
	W.print();
	if (P.distf_ == 0) {
		cout << "weights ignored" << endl;
		// set W.Rw_ and W.Dw_ to default values for feature analysis
		for (int i=0;i<W.Rw_.size();i++) {
			W.Rw_[i] = 1;
		}
		for (int i=0;i<W.Dw_.size();i++) {
			W.Dw_[i] = 0.4;
		}
	}
	cout << endl;


	//-----------------------
	// run the outlier search 
	//
	cout << "Computing outliers" << endl;
	vector<Outlier> O;

	bool done = false;
	int dot_count = 0;

	// main computational loop
	while (!done) {

		int size = O.size();

		if (P.record_neighbors_ == false) {
			find_outliers(M,D,P.k_,P.cutoff_,P.scoref_,P.distf_,W.Rw_,W.Dw_,O,P.not_same_);
		}
		else {
			find_outliers_index(M,D,P.k_,P.cutoff_,P.scoref_,P.distf_,W.Rw_,W.Dw_,O,P.not_same_);
		}

		D.reset_file_ptr();

	 	done = ! M.get_next_batch();

		//----------------
		// print dots
		//
		if (size == O.size()) {  // no outliers found in block
			if (dot_count == 0) 
				cout << "  ";
			cout << "." << flush;
			dot_count++;
		}
		else {  // outliers found
			if (dot_count != 0) {
				cout << endl;
			}
			for (int i=size;i<O.size();i++) {
				cout << "  " << O[i].index_ << ": " << O[i].score_ << endl;
				dot_count = 0;
			}
		}

		//-------------------------------
		// sort the current best outliers 
		// and keep the best
		//
		sort(O.begin(),O.end(),comp_outlier());
		int no = P.num_outliers_;
		if (O.size() > no) {
			if (O[no-1].score_ > P.cutoff_) {
				P.cutoff_ = O[no-1].score_;
				cout << "  new cutoff: " << P.cutoff_ << endl;
			}
		}
		if (dot_count == 50) {
			dot_count = 0;
			cout << endl;
		}
	}	

	// add proper line spacing after this output section
	if (dot_count != 0)
		cout << endl;
	cout << endl;

	//---------------
	// print outliers 
	//
	cout << "Top outliers: " << endl << endl;

	DTable TestCases(P.data_file_);

	int no = min((int) O.size(),P.num_outliers_);
	int ID;
	vector<real> rr(TestCases.num_rvars(),0);
	vector<integer> dd(TestCases.num_dvars(),0);

	for (int i=0;i<no;i++) {
		// print rank
		cout << "  " << i+1 << ". ";

		// print index and score
		TestCases.get_object(O[i].index_,ID,rr,dd);
		cout << "Record: " << ID;
		cout << " Score: " << O[i].score_ << endl;

		// if run with rn option print contributions
		if (P.record_neighbors_ == true) {

			// print neighbors id
			spaces(spacing(i+1) + 4);
			cout << "Neighbors: ";
			vector<int> nID;
			for (int j=0;j<O[i].neighbors_.size();j++) {
				int id;
				D.get_object(O[i].neighbors_[j],id);
				nID.push_back(id);
			}
			if (O[i].neighbors_.size() < 8) {
				printvector(nID);
				cout << endl;
			}
			else {
				cout << endl;
				printlongvector(nID,10,spacing(i+1) + 6);
			}

			// print contribution
			vector<double> Rc(D.num_rvars(),0);
			vector<double> Dc(D.num_dvars(),0);

			distance_contribution(D,TestCases,O[i],P.scoref_,W.Rw_,W.Dw_,Rc,Dc);
			spaces(spacing(i+1) + 4);
			cout << "feature importance: " << endl;

			//---------------------------------------------
			// print feature contributions ordered by size
			//
			vector<mypair> results;
			for (int j=0;j<Rc.size();j++) {
				mypair a(Rc[j],W.Rname_[j]);
				results.push_back(a);
			}
			for (int j=0;j<Dc.size();j++) {
				mypair a(Dc[j],W.Dname_[j]);
				results.push_back(a);
			}
			sort(results.begin(),results.end(),DescendingSort());
			for (int j=0;j<results.size();j++) {
				spaces(spacing(i+1) + 6);
				int old_precision = cout.precision();
				cout.precision(2);
				cout.setf(ios::fixed);
				cout << results[j].second << ": ";
				cout << defuzz(results[j].first) << endl;
				cout.precision(old_precision);
				cout.unsetf(ios::fixed);
			}
			
		}
		cout << endl;
	}

	return 0;
};

