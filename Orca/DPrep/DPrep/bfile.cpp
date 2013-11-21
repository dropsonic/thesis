//--------------------------------------------------------------------
// file bfile.cpp
//
// Stephen D. Bay
// Copyright 2003
//--------------------------------------------------------------------

#include <iostream>
#include <fstream>
//#include <sstream>
#include <strstream>
#include <string>
#include <vector>
#include <algorithm>

#include <stdio.h>
#include <math.h>

#include <stdlib.h>

#include "bfile.h"
#include "misc.h"
#include "database.h" // for tokenize





//--------------------------------------------------------------------
// BFile 
//--------------------------------------------------------------------
BFile::BFile(string file, float missing_r, int missing_d) : infile_(0)
{
  data_file_ = file;
	missing_r_ = missing_r;
	missing_d_ = missing_d;

	set_file_ptr(file);
	read_header();
};


//--------------------------------------------------------------------
// ~BFile
//--------------------------------------------------------------------
BFile::~BFile()
{
	if (infile_ != 0) {
		infile_->close();
		delete infile_;
	}
};


//--------------------------------------------------------------------
// scale_01 
//
// Scales real values to the range [0,1].
//--------------------------------------------------------------------
void BFile::scale_01(string file,vector<real> &max, vector<real> &min) 
{

	//-------------------------------
	// open file for writing
	//
	ofstream outfile (file.c_str(),ios::out|ios::binary);
	if ( ! outfile ) {
		cerr << "Error: cannot open file " << file << endl;
		exit(0);
	};
	
	// write header information
	outfile.write((const char *) &records_,sizeof(int));
	outfile.write((const char *) &rvars_,sizeof(int));
	outfile.write((const char *) &dvars_,sizeof(int));

	vector<real> range(rvars_,0);

	get_max_min(max,min);
	for (int i=0;i<max.size();i++) {
		range[i] = max[i] - min[i];
	};

	cout << "  Scaling data file" << endl;
	//--------------------------------
	// read in file and scale it
	//
	seek_position(0);

	vector<real> R(rvars_,0);
	vector<integer> D(dvars_,0);
	vector<real> Rscale(rvars_,0);
	int ID;
	while (index_ < records_) {
		get_next(ID,R,D);
		for (int i=0;i<R.size();i++) {
			if (R[i] == missing_r_) {
				Rscale[i] = missing_r_;
			}
			else if (range[i] != 0) {
				Rscale[i] = (R[i] - min[i]) / range[i]; 
			}
			else {
				Rscale[i] = 0;
			}
		}
		outfile.write((const char *) &ID,sizeof(int));
		outfile.write((const char *) &Rscale[0],sizeof(real)*rvars_);
		outfile.write((const char *) &D[0],sizeof(integer)*dvars_);
	};

	outfile.close();
};


//--------------------------------------------------------------------
// scale_std
//
// Scales real values to standard deviations from the mean.
//--------------------------------------------------------------------
void BFile::scale_std(string file, vector<real> &mean, vector<real> &std) 
{

	//-------------------------------
	// open file for writing
	//
	ofstream outfile (file.c_str(),ios::out|ios::binary);
	if ( ! outfile ) {
		cerr << "Error: cannot open file " << file << endl;
		exit(0);
	};
	
	// write header information
	outfile.write((const char *) &records_,sizeof(int));
	outfile.write((const char *) &rvars_,sizeof(int));
	outfile.write((const char *) &dvars_,sizeof(int));

	cout << "  Scaling data file" << endl;
	//--------------------------------
	// read in file and scale it
	//
	seek_position(0);

	vector<real> R(rvars_,0);
	vector<integer> D(dvars_,0);
	vector<real> Rscale(rvars_,0);
	int ID;
	while (index_ < records_) {
		get_next(ID,R,D);
		for (int i=0;i<R.size();i++) {
			if (R[i] == missing_r_) {
				Rscale[i] = missing_r_;
			}
			else if (std[i] != 0) {
				Rscale[i] = (R[i] - mean[i]) / std[i]; 
			}
			else {
				Rscale[i] = 0;
			}
		}
		outfile.write((const char *) &ID,sizeof(int));
		outfile.write((const char *) &Rscale[0],sizeof(real)*rvars_);
		outfile.write((const char *) &D[0],sizeof(integer)*dvars_);
	};

	outfile.close();
};





//--------------------------------------------------------------------
// get_max_min
//
// Makes a pass through the data and calculates the max and min values
// for real variables.
//--------------------------------------------------------------------
void BFile::get_max_min(vector<real> &max, vector<real> &min)
{
	seek_position(0);

	vector<real> R(rvars_,0);
	vector<integer> D(dvars_,0);
	int ID;

	real bignum = 10000000;

	// initialize max and min vectors 
	for (int i=0;i<R.size();i++) {
		min[i] = bignum;
		max[i] = -bignum;
	}

	// process rest of examples
	while (index_ < records_) {
		get_next(ID,R,D);

		for (int i=0;i<R.size();i++) {
			if (R[i] != missing_r_) {
				if (R[i] < min[i]) {
					min[i] = R[i];
				}
				else if (R[i] > max[i]) {
					max[i] = R[i];
				}
			}
		};
	}

};



//--------------------------------------------------------------------
// get_mean_std
//
// Makes a pass through the data and calculates the mean and standard
// deviation for real variables.
//--------------------------------------------------------------------
void BFile::get_mean_std(vector<real> &mean, vector<real> &std)
{
	seek_position(0);

	vector<real> R(rvars_,0);
	vector<integer> D(dvars_,0);
	int ID;

	vector<double> sumv(rvars_,0);
	vector<double> sumsqv(rvars_,0);
	vector<int> num(rvars_,0);

	// process rest of examples
	while (index_ < records_) {
		get_next(ID,R,D);

		for (int i=0;i<R.size();i++) {
			if (R[i] != missing_r_) {
				double r = ((double) R[i]);
				sumv[i] += r;
				sumsqv[i] += r*r;
				num[i]++;
			}
		};
	}

	cout << "  sum: ";
	printvector(sumv);
	cout << endl;
	cout << "  sum^2: ";
	printvector(sumsqv);
	cout << endl;

	for (int i=0;i<rvars_;i++) {
		if (num[i] > 1) {
			double mean_value = sumv[i] / num[i];
			mean[i] = ((real) mean_value);

			double std_value = sqrt((sumsqv[i] - sumv[i] * sumv[i] / num[i]) / (num[i] - 1));
			std[i] = ((real) std_value);
		}
		else {
			mean[i] = 0;
			std[i] = 0;
		}
		// error checkin
		// check mean /std are not NaN
	}
};



//--------------------------------------------------------------------
// multi_shuffle
//--------------------------------------------------------------------
void BFile::multi_shuffle(string destfile, int iterations, int tmpfiles)
{
	assert(tmpfiles > 0);
	for (int i=0;i<iterations;i++) {
		shuffle(destfile,10000,tmpfiles);
		set_file_ptr(destfile);
	};
};


//--------------------------------------------------------------------
// shuffle
//
// file
// blocksize
// ntmpfiles
//--------------------------------------------------------------------
void BFile::shuffle(string file, int blocksize, int ntmpfiles)
{

	//-------------------------
	// set up tmp file names
	//
	vector<string> tmpfiles;
	for (int i=0;i<ntmpfiles;i++) {
		//ostringstream filename;
		strstream filename;
		filename << "tmp." << i << ends;
		//cout << "filename: " << filename.str() << endl;
		//cout << filename.str() << endl;
		tmpfiles.push_back(filename.str());
	}


	//-------------------------------
	// open file for writing
	//

	vector<ofstream*> file_ptrs;
	for (int i=0;i<tmpfiles.size();i++) {
		ofstream *outfile = new ofstream(tmpfiles[i].c_str(),ios::out|ios::binary);

		if ( ! *outfile ) {
			cerr << "Error: cannot open file " << file << endl;
			exit(0);
		}

		file_ptrs.push_back(outfile);
	};


	//--------------------------------
	// read in data file and randomly shuffle examples to
	// temporay files
	//
	cout << "  Splitting data set randomly into temporary files" << endl;
	seek_position(0);

	vector<real> R(rvars_,0);
	vector<integer> D(dvars_,0);
	int ID;
	while (index_ < records_) {
		get_next(ID,R,D);
		
		// for linux gcc version
		//double rnum = floor(drand48() * ((double) tmpfiles.size()));

		// for MINGW version
		double drand = (((double) rand()) / ((double) RAND_MAX+1));
		double rnum = floor(drand * ((double) tmpfiles.size()));

//		cout << drand << "   " << rnum << endl;

		int index = ((int) rnum); 
		
		file_ptrs[index]->write((const char *) &ID,sizeof(int));	
		file_ptrs[index]->write((const char *) &R[0],sizeof(real)*rvars_);	
		file_ptrs[index]->write((const char *) &D[0],sizeof(integer)*dvars_);	
	};

	// close temporary file pointers and delete ofstream objects
	for (int i=0;i<file_ptrs.size();i++) {
		file_ptrs[i]->close();
		delete file_ptrs[i];
	}


	//-------------------------------
	// open tmpfiles for reading 
	//

	vector<ifstream*> ifile_ptrs;
	for (int i=0;i<tmpfiles.size();i++) {
		ifstream *infile = new ifstream(tmpfiles[i].c_str(), ios::in | ios::binary);

		if ( ! *infile) {
			cerr << "Error: cannot open file " << file << endl;
			exit(0);
		}

		ifile_ptrs.push_back(infile);
	};


	//-----------------------------------
	// open final destination file
	//
	
	ofstream outfile (file.c_str(),ios::out|ios::binary);
	if ( ! outfile ) {
		cerr << "Error: cannot open file " << file << endl;
		exit(0);
	};
	
	// write header information
	outfile.write((const char *) &records_,sizeof(int));
	outfile.write((const char *) &rvars_,sizeof(int));
	outfile.write((const char *) &dvars_,sizeof(int));


	//--------------------------------------
	// concatenate tmp files in random order
	// 

	vector<int> order;
	for (int i=0;i<tmpfiles.size();i++) {
		order.push_back(i);
	}
	random_shuffle(order.begin(),order.end());
	cout << "  Combining temporary files in order: ";
	printvector(order);
	cout << endl;
	

	for (int i=0;i<order.size();i++) {
		int count = blocksize;
		ifstream *infile = ifile_ptrs[order[i]];
		while(count == blocksize) {
			//char buffer[blocksize];
			char *buffer = new char[blocksize];
			infile->read(buffer,blocksize);
			count = infile->gcount();
			outfile.write((const char *) buffer,count);
			delete [] buffer;
		}
	}
	
	
	//-----------------------------
	// close file pointers
	//
	
	outfile.close();

	// close temporary file pointers and delete ifstream objects
	for (int i=0;i<file_ptrs.size();i++) {
		ifile_ptrs[i]->close();
		delete ifile_ptrs[i];
	}

	//-----------------------------
	// clean temporary files
	//
	for (int i=0;i<tmpfiles.size();i++) {
		if (remove(tmpfiles[i].c_str()) == -1) {
			cerr << "Error deleting file " << tmpfiles[i] << endl;
		}
	}
};




//--------------------------------------------------------------------
// get_next 
//
// WARNING does not check to make sure read command succeeded.
//--------------------------------------------------------------------
void BFile::get_next(int &ID, vector<real> &R, vector<integer> &D)
{
	
	infile_->read((char *) &ID, sizeof(int));
	infile_->read((char *) &R[0], sizeof(real) * rvars_);
	infile_->read((char *) &D[0], sizeof(integer) * dvars_);

	index_++;
};

//--------------------------------------------------------------------
// set_file_ptr
//--------------------------------------------------------------------
void BFile::set_file_ptr(string file)
{
	// if infile_ already points to a file, close it and
	// free the memory for the ifstream object
	if (infile_ != 0) {
		infile_->close();
		delete infile_;
	}

	// create a new ifstream object
  infile_ = new ifstream(file.c_str(), ios::in | ios::binary);
	index_ = 0;
	if ( ! infile_) {
		cerr << "error: unabile to open input file: " << file << endl;
		exit(-1);
	}
};



//--------------------------------------------------------------------
// read_header
//--------------------------------------------------------------------
void BFile::read_header(void)
{
	// check file ptr != 0
	int data[3];
	infile_->read((char *) data,3*sizeof(int));

	records_ = data[0];
	rvars_ = data[1];
	dvars_ = data[2];

};


//--------------------------------------------------------------------
// seek_position
//--------------------------------------------------------------------
void BFile::seek_position(int pos)
{
	// check if position valie
	assert(pos >= 0);
	assert(pos < records_);
				
	//int filepos = 3 * sizeof(int) + pos * sizeof(real) + pos * sizeof(integer);
	long filepos = 3 * sizeof(int) + pos * sizeof(real) + pos * sizeof(integer);
	infile_->seekg(filepos);
	index_ = pos;
				
};

//--------------------------------------------------------------------
// print_fields
//--------------------------------------------------------------------
void BFile::print_header(void)
{
	cout << "Records: " << records_ << endl;
	cout << "Real Variables: " << rvars_ << endl;
	cout << "Discrete Variables: " << dvars_ << endl;
};


//--------------------------------------------------------------------
// print_records
//--------------------------------------------------------------------
void BFile::print_records(int num)
{
	seek_position(0);
	
	vector<real> R(rvars_,0);
	vector<integer> D(dvars_,0);
	int ID;
	if (num == -1) {
		num = records_;
	}

	int count = 0;
	while ((index_ < records_) && (count < num)) {
		get_next(ID,R,D);
		
		cout << ID << " ";
		printvector(R);
		cout << " | " ;
		printvector(D);
		cout << endl;
		count++;
	}
	
};

//--------------------------------------------------------------------
// RStats
//--------------------------------------------------------------------
RStats::RStats(int size)
{
	vector<real> zero(size,0);
	max_ = zero;
	min_ = zero;
	mean_ = zero;
	std_ = zero;
};


//--------------------------------------------------------------------
// load
//--------------------------------------------------------------------
void RStats::load(string file)
{
	// open file
	fstream infile(file.c_str(), ios::in);
	if (!infile) {
		cerr << "error: unable to open input file: " << file << endl;
		exit(0);
	}
	
	string buffer;

	vector<string> delimiters;
	delimiters.push_back(":");
	delimiters.push_back(";");
	delimiters.push_back(",");

	while (getline(infile,buffer)) {
		vector<string> tokens = tokenize(buffer,delimiters);

		// if we have a valid line
		vector<real> *V;
		V = &std_;
		if (tokens.size() > 0) {
			if (tokens[0] == "max") 
				V = &max_;
			else if (tokens[0] == "min")
				V = &min_;
			else if (tokens[0] == "mean")
				V = &mean_;
			else if (tokens[0] == "std")
				V = &std_;
			else {
				cerr << "error: unknown statistic " << tokens[0] << " in file " << file << endl;
				exit(0);
			}
		}
				
		// get numbers
		for (int i=1; i<tokens.size();i++) {
			//V->push_back(atof(tokens[i].c_str()));
			(*V)[i-1] = atof(tokens[i].c_str());
		}
 	}

};


//--------------------------------------------------------------------
// print
//--------------------------------------------------------------------
void RStats::print(int which) 
{
	if (which == 0) {
		if (max_.size() != 0) {
			cout << "  max: ";
			printvector(max_);
			cout << endl;
		}
		if (min_.size() != 0) {
			cout << "  min: ";
			printvector(min_);
			cout << endl;
		}
	}
	else {
		if (mean_.size() != 0) {
			cout << "  mean: ";
			printvector(mean_);
			cout << endl;
		}
		if (std_.size() != 0) {
			cout << "  std: ";
			printvector(std_);
			cout << endl;
		}
	}
};

