//--------------------------------------------------------------------
// file parameters.cpp
//
// Stephen D. Bay
// COPYRIGHT 2003
//--------------------------------------------------------------------


#include <iostream>
#include <fstream>
#include <strstream>
#include <string>
#include <vector>
#include <algorithm>

#include "parameters.h"

//--------------------------------------------------------------------
// Parameters
//--------------------------------------------------------------------
Parameters::Parameters(int argc, char **argv)
{
	//---------------------------------------
  // setup the default values of parameters
	//
	
	// outlier options
	scoref_ = 0; // average
	distf_ = 1; // weighted
	num_outliers_ = 30;
	k_ = 5;
	cutoff_ = 0;

	// computation parameters
	start_batch_size_ = 1000;
	batch_size_ = 1000;

	// misc parameters
	record_neighbors_ = false;
	missing_ = -989898;
	dist_m_r_ = 0.4;


  // set up filestem 
  data_file_ = string(argv[1]);
  reference_file_ = string(argv[2]);
	weight_file_ = string(argv[3]);
			
	if (data_file_ == reference_file_) {
		not_same_ = false;
	}

  // process the options
  for (int i=3;i<argc;i++) {
    string option(argv[i]);

    if (option == "-n") {
      num_outliers_ = atoi(argv[i+1]);
      i++;
    }
		else if (option == "-b") {
      batch_size_ = atoi(argv[i+1]);
      i++;
    }
		else if (option == "-avg") {
			scoref_ = 0;
		}
		else if (option == "-c") {
      cutoff_ = atof(argv[i+1]);
      i++;
    }
		else if (option == "-k") {
			k_ = atoi(argv[i+1]);
			i++;
		}
		else if (option == "-kth") {
			scoref_ = 1;
		}
		else if (option == "-m") {
			missing_ = atof(argv[i+1]);
			i++;
		}
		else if (option == "-dmr") {
			dist_m_r_ = atof(argv[i+1]);
			i++;
		}
		else if (option == "-rn") {
      record_neighbors_ = true;
    }
		else if (option == "-s") {
      start_batch_size_ = atoi(argv[i+1]);
      i++;
    }
		else if (option == "-woff") {
      distf_ = 0; 
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

	if (num_outliers_ < 1) {
		cerr << "error: number of outliers must be an integer greater than 0" << endl; 
		exit(-1);
	}

	if (k_ < 1) {
		cerr << "error: k must be an integer greater than 0" << endl;
		exit(-1);
	}

	if (cutoff_ < 0) {
		cerr << "warning: cutoff should be greater than or equal to 0" << endl;
		cerr << "warning: reseting cutoff value to 0" << endl;
		cutoff_ = 0;
	}

	if (batch_size_ < 1) {
		cerr << "error: batch size must be an integer greater than 0" << endl;
		exit(-1);
	}

	if (start_batch_size_ < 1) {
		cerr << "error: starting batch size must be an integer greater than 0" << endl;
		exit(-1);
	}

};



//--------------------------------------------------------------------
// Parameters::print
//--------------------------------------------------------------------
void Parameters::print(void) const
{
  cout << "Parameters:" << endl;
  cout << "  data_file: " << data_file_ << endl;
  cout << "  reference_file: " << reference_file_ << endl;
	cout << "  weight_file: " << weight_file_ << endl;

	cout << "  distf: " << distf_ << endl;
	cout << "  scoref: " << scoref_ << endl;
	cout << "  number of outliers: " << num_outliers_ << endl;
	cout << "  k: " << k_ << endl;
	cout << "  cutoff: " << cutoff_ << endl;

	cout << "  batch size: " << batch_size_ << endl;
	cout << "  starting batch size: " << start_batch_size_ << endl;

	cout << "  record neighbors: " << record_neighbors_ << endl;
	cout << "  missing: " << missing_ << endl;
	cout << "  distance missing real: " << dist_m_r_ << endl;

};




 
