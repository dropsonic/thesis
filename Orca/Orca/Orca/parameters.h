//--------------------------------------------------------------------
// file parameters.h
//
// Stephen D. Bay
// COPYRIGHT 2003
//--------------------------------------------------------------------

#ifndef __PARAMETERS_H
#define __PARAMETERS_H

#include <vector>
#include <string>

using namespace std;


//--------------------------------------------------------------------
// Struct Parameters
//--------------------------------------------------------------------
struct Parameters
{
  Parameters(int argc,char **argv);

  void print(void) const;
	void check(void);

	// files
	string data_file_;
	string reference_file_;
	string weight_file_;
	bool not_same_; // are data file and reference file the same
	
	// outlier options 
	int scoref_;    // average or kth nearest neighbor
	int distf_;     // default weighting or weight file 
	int num_outliers_; 
	int k_;
	double cutoff_;

  // computation parameters
	int start_batch_size_;
	int batch_size_;

	// miscellaneous parameters
	bool record_neighbors_;
	float missing_;
	float dist_m_r_;

};




#endif // ends __PARAMETERS_H
