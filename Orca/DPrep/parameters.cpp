//--------------------------------------------------------------------
// file parameters.cpp
//
// Stephen D. Bay
// Copyright 2003
//--------------------------------------------------------------------


#include <iostream>
#include <fstream>
#include <strstream>
#include <string>
#include <vector>
#include <algorithm>
#include <time.h>

#include "parameters.h"

//--------------------------------------------------------------------
// Parameters
//--------------------------------------------------------------------
Parameters::Parameters(int argc, char **argv)
{
  // setup the default values of parameters
	scale_ = SCALE_01;

	randomize_ = true;
	iterations_ = 5;
	randfiles_ = 10;
	seed_ = time(NULL);

	missing_r_ = -989898;
	missing_d_ = -1;

	tmp_file_stem_ = string("tmp");

	clean_ = CLEAN_FINAL;

  // set up files 
  data_file_ = string(argv[1]);
  names_file_ = string(argv[2]);
  dest_file_ = string(argv[3]);


  // process the options
  for (int i=4;i<argc;i++) {
    string option(argv[i]);
    if (option == "-a") {
      //num_anomalies_ = atoi(argv[i+1]);
      //i++;
    }
		else if (option == "-cleanf") {
			clean_ = CLEAN_FINAL;
		}
		else if (option == "-cleand") {
			clean_ = CLEAN_DURING;
		}
		else if (option == "-cleann") {
			clean_ = CLEAN_NONE;
		}
		else if (option == "-m") {
			missing_r_ = atof(argv[i+1]);
			i++;
		}
		else if (option == "-sf") {
			scale_file_ = string(argv[i+1]);
			i++;
    }
		else if (option == "-s01") {
			scale_ = SCALE_01;
    }
		else if (option == "-sstd") {
			scale_ = SCALE_STD;
    }
		else if (option == "-snone") {
			scale_ = SCALE_NONE;
    }
		else if (option == "-i") {
			iterations_ = atoi(argv[i+1]);
			i++;
    }
		else if (option == "-rand") {
			randomize_ = true;
		}
		else if (option == "-norand") {
			randomize_ = false;
		}
		else if (option == "-rf") {
			randfiles_ = atoi(argv[i+1]);
			i++;
    }
		else if (option == "-seed") {
			seed_ = atoi(argv[i+1]);
			i++;
    }
		else if (option == "-tmp") {
      tmp_file_stem_ = string(argv[i+1]);
      i++;
    }
    else if (string::size_type pos = option.find_first_of("-",0) == 0) {
      cerr << "Error: unknown option " << option << endl;
      exit(-1);
    }
  }
};

//--------------------------------------------------------------------
// Parameters::check
//--------------------------------------------------------------------
void Parameters::check(void)
{

	// check randomization parameters
	if (iterations_ < 1) {
		cout << iterations_ << endl;
		cerr << "error: iterations_ must be an integer greater than 0" << endl;
		exit(-1);
	}

	if (randfiles_ < 1) {
		cout << randfiles_ << endl;
		cerr << "error: randfiles_ must be an integer greater than 0" << endl;
		exit(-1);
	}

	// check scaling parameters


};






//--------------------------------------------------------------------
// Parameters::print
//--------------------------------------------------------------------
void Parameters::print(void) const
{
  cout << "Parameters:" << endl;
  cout << "  data_file: " << data_file_ << endl;
  cout << "  names_file: " << names_file_ << endl;
  cout << "  output_file: " << dest_file_ << endl;

	// scaling parameters
	//
	cout << "  scale : ";
	if (scale_ == SCALE_01) {
		cout << "[0,1]" << endl;
	}
	else if (scale_ == SCALE_STD) {
		cout << "std" << endl;
	}
	else {
		cout << "none" << endl;
	}

	// randomization parameters
	//
	cout << "  randomize: " << randomize_ << endl;
	cout << "  iterations: " << iterations_ << endl;
	cout << "  number of files: " << randfiles_ << endl;
	cout << "  seed: " << seed_ << endl;

	// file parameters
	//
	cout << "  tmp_file_stem: " << tmp_file_stem_ << endl;

	// missing values 
	//
	cout << "  missing_real_: " << missing_r_ << endl;
	cout << "  missing_discrete_: " << missing_d_ << endl;
};




 
