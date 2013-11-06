//--------------------------------------------------------------------
// file parameters.h
//
// Stephen D. Bay
// Copyright 2003
//--------------------------------------------------------------------

#ifndef __PARAMETERS_H
#define __PARAMETERS_H

#include <vector>
#include <string>

using namespace std;

const int SCALE_01 = 0;
const int SCALE_STD = 1;
const int SCALE_NONE = 2;

const int CLEAN_FINAL = 0;
const int CLEAN_DURING = 1;
const int CLEAN_NONE = 2;

//--------------------------------------------------------------------
// Struct Parameter
//--------------------------------------------------------------------
struct Parameters
{
  Parameters(int argc,char **argv);

	void check(void);
  void print(void) const;


	// files
	string data_file_;
	string names_file_;
	string dest_file_;

//	float missing_;

	// scale parameters
	int scale_;
	string scale_file_;

	// randomization parameters
	bool randomize_;
	int iterations_;
	int randfiles_;

	// file parameters
	string tmp_file_stem_;

	// random number seed
	long int seed_;

	// misc parameters
	int clean_;

	// missing values
	float missing_r_;
	int missing_d_;
};




#endif // ends __PARAMETERS_H
