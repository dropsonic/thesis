//--------------------------------------------------------------------
// file bfile.h
//
// Stephen D. Bay
// Copyright 2003
//
// Support for the binary file format used by Orca.
//--------------------------------------------------------------------
#ifndef __BFILE_H
#define __BFILE_H


#include <iostream>
#include <fstream>
#include <strstream>
#include <string>
#include <vector>
#include <algorithm>
#include <assert.h>

using namespace std;

//--------------------------------------------------------------------
// Typedef
//--------------------------------------------------------------------
typedef float real;
typedef int integer;


//--------------------------------------------------------------------
// RStats
//--------------------------------------------------------------------
struct RStats
{
	vector<real> max_;
	vector<real> min_;
	vector<real> mean_;
	vector<real> std_;

	RStats(int size);
	void load(string file);
	void print(int which);
};


//--------------------------------------------------------------------
// class BFile 
//--------------------------------------------------------------------
class BFile 
{
	private:

		// for sequential data access to data files on disk
		//
		ifstream *infile_;
		unsigned example_;

		string data_file_;

		int index_;

		int records_;
		int rvars_;
		int dvars_;

		float missing_r_;
		int missing_d_;

	public:

		BFile(string file, float missing_r, int missing_d);
		~BFile();


		//--------------------
		// state functions
		//
		int num_records(void) {return records_;};
		int num_real(void) {return rvars_;}; 
		int num_discrete(void) {return dvars_;}; 

		//---------------------
		// Scaling functions
		// 
		// get_scale_stats
		void get_max_min(vector<real> &max, vector<real> &min); 
		void get_mean_std(vector<real> &mean, vector<real> &std); 
		void scale_01(string file, vector<real> &max, vector<real> &min);
		void scale_std(string file, vector<real> &mean, vector<real> &std);

		//---------------------
		// Shuffling functions
		void shuffle(string file, int blocksize, int tmpfiles);
		void multi_shuffle(string file, int iterations, int tmpfiles);


		//--------------------
		// print functions
		//
		void print_header(void);
		void print_records(int num);
	

		//-------------------------------
		// sequential data access on disk
		//
		void set_file_ptr(string file);
		void get_next(int &ID, vector<real> &R, vector<integer> &D);
		void read_header(void);
		void seek_position(int pos);

};

#endif // ends __BFILE_H
