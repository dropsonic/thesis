//--------------------------------------------------------------------
// file main.cpp
//
// Stephen D. Bay
// COPYRIGHT 2003
//
//--------------------------------------------------------------------

#include <iostream>

#include <algorithm>
#include <vector>
#include <list>
#include <stdio.h>

#include "main.h"
#include "bfile.h"
#include "database.h"
#include "parameters.h"
#include "misc.h"


//--------------------------------------------------------------------
// print_usage
//--------------------------------------------------------------------
void print_usage(void) {
	cout << "Usage: dprep data_file fields_file dest_file [options]" << endl;
	cout << endl;
	cout << "Options: " << endl;
	cout << endl;

	cout << "  Scaling Parameters" << endl;
	cout << "    -snone   no scaling of continuous fields" << endl;
	cout << "    -s01     scale continuous fields to range [0,1]" << endl;
	cout << "    -sstd    scale continuous fields to zero mean and unit standard devation" << endl;
	cout << endl;

	cout << "  Disk Based Randomization Parameters" << endl;
	cout << "    -rand    randomize" << endl;
	cout << "    -norand  do not randomize the file" << endl;
	cout << "    -i X     execute X iterations of shuffling (5)" << endl;
	cout << "    -rf X    use X tempory files for disk shuffling (10)" << endl;
	cout << "    -seed X  random number seed X (time based)" << endl;
	cout << endl;

	cout << "  Miscellaneous Parameters" << endl;
	cout << "    -m X     real number for encoding continuous missing values" << endl;      
	cout << "    -cleanf  clean temporary files at end" << endl;      
	cout << "    -cleand  clean temporary files during execution" << endl;      
	cout << "    -cleann  do not clean temporary fiels" << endl;
	cout << endl;
};


//--------------------------------------------------------------------
// print_run_info
//--------------------------------------------------------------------
void print_run_info(int argc, char **argv)
{
	// print the comman line call
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

	cout << "DPrep: Mining Distance Based Outliers" << endl;
	cout << endl;

	print_run_info(argc,argv);
	cout << endl;

	// get the parameters
	Parameters P(argc,argv);
	P.check();
	P.print();
	cout << endl;


	// initialize random number seed
	
	// for linux gcc version
	//srand48(P.seed_);

	// for MINGW version
	srand(P.seed_);

	vector<string> files;
	files.push_back(P.data_file_);

	//-------------------------------------------------------------
	// Create the DataTable
	//

	DataTable D(files.back(),P.names_file_,P.missing_r_,P.missing_d_);


	//-------------------------------------------------------------
	// Load the Names File 
	//
	cout << "Fields" << endl;
	D.print_fields(false);
	cout << endl;

//	cout << "Records" << endl;
//	D.print_records(10);
//	cout << endl;

	cout << "Real variables: " << D.num_real() << endl;
	cout << "Discrete variables: " << D.num_discrete() << endl;
	cout << endl;

	//-------------------------------------------------------------
	// Write weight file
	//
	cout << "Writing feature and weight file" << endl;
	D.write_weight_file("weights");
	cout << endl;

	//-------------------------------------------------------------
	// Load the Scale File 
	//
	RStats RS(D.num_real());	
	if (P.scale_file_ != "") {
		cout << "Loading statistics for scaling" << endl;
		RS.load(P.scale_file_);
		RS.print(P.scale_);
		cout << endl;
	}


	//-------------------------------------------------------------
	// Convert Data set to binary format
	//
	
	cout << "Converting data set to binary format" << endl;
	string output(P.tmp_file_stem_ + ".out");
	files.push_back(output);
	int converted_records = D.convert_to_binary(files.back());
	cout << "  Converted " << converted_records << " records" << endl;


	if (converted_records == 0) {
		cerr << "Error: no records converted; exiting program." << endl;
		exit(0);
	}
	cout << endl;



	//-------------------------------------------------------------
	// Scale data set 
	//
	if (P.scale_ != SCALE_NONE) {
  	cout << "Scaling data set" << endl;

  	BFile B(files.back(),P.missing_r_,P.missing_d_);

  	string scale_output(P.tmp_file_stem_ + ".scale");
  	files.push_back(scale_output);
  	if (P.scale_ == SCALE_01) {
			if (P.scale_file_ == "") {
				B.get_max_min(RS.max_,RS.min_);
			}
			RS.print(P.scale_);
  		B.scale_01(files.back(),RS.max_,RS.min_);
  	}
  	else if (P.scale_ == SCALE_STD) {
			if (P.scale_file_ == "") {
				B.get_mean_std(RS.mean_,RS.std_);
			}
			RS.print(P.scale_);
  		B.scale_std(files.back(),RS.mean_,RS.std_);
  	}
  	cout << "  Scaling finished" << endl;
	}
	cout << endl;


	//-------------------------------------------------------------
	// Randomize data set 
	//
	
	if (P.randomize_ == true) {
  	cout << "Randomizing data set" << endl;
  	BFile Bscale(files.back(),P.missing_r_,P.missing_d_);

  	string rand_output(P.tmp_file_stem_ + ".rand"); 
  	files.push_back(rand_output);

  	Bscale.multi_shuffle(files.back(),P.iterations_,P.randfiles_);
  	cout << "  Randomization finished" << endl;
	}
	cout << endl;


	//-------------------------------------------------------------
	// rename last temporary file to destination file
	//
	if (rename(files.back().c_str(),P.dest_file_.c_str()) != 0) {
		cerr << "Error renaming file " << files.back() << " to " << P.dest_file_ << endl;
		exit(0);
	}


	//-------------------------------------------------------------
	// clean temporary files 
	//
	if (P.clean_ == CLEAN_FINAL) {
		cout << "Cleaning temporary files" << endl;
		for (int i=1;i<files.size()-1;i++) {
			if (remove(files[i].c_str()) == -1) {
				cerr << "Error deleting file " << files[i] << endl;
			}
			else {
				cout << "  deleting temporary file " << files[i] << endl;
			}
		}
		cout << "  cleaning finished" << endl;
	}
	cout << endl;



	cout << "Processing finished" << endl;
	return 0;
};




