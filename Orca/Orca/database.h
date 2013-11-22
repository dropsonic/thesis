//--------------------------------------------------------------------
// file database.h
//
// Stephen D. Bay
// COPYRIGHT 2003
//--------------------------------------------------------------------
#ifndef __DATABASE_H
#define __DATABASE_H


#include <iostream>
#include <fstream>
#include <strstream>
#include <string>
#include <vector>
#include <algorithm>
#include <list>

using namespace std;

//--------------------------------------------------------------------
// Typedef
//--------------------------------------------------------------------
typedef float real;
typedef int integer;

class DataTable;


//--------------------------------------------------------------------
// Constant data type
//--------------------------------------------------------------------
const int DISCRETE = 0;
const int CONTINUOUS = 1;
 
//const double MISSING_C = -989898;
//const double DIST_M_R = -0.4;



//--------------------------------------------------------------------
// MTable
//
// Provides random access to a block of records for a data set that
// resides on disk. Each block of records is loaded into main memory
// from disk.
//--------------------------------------------------------------------
class MTable
{
	private:

		int records_;
		int rvars_;
		int dvars_;

		int start_batch_size_;
		int batch_size_;
		int record_index_;

		// for sequential data access on disk
		fstream *infile_;

	public:
		list<int> ID_;
		list<vector<real> > R_;
		list<vector<integer> > D_;

		int nr_;
		int offset_;
		int last_offset_;
		
		MTable(string DataFile, int start_batch_size, int batch_size);
		~MTable();

		int num_records(void) {return records_;};
		int num_rvars(void) {return rvars_;};
		int num_dvars(void) {return dvars_;};

		bool get_next_batch(void);
};


//--------------------------------------------------------------------
// DTable
//
// Provides sequential access to data that resides on disk.
//--------------------------------------------------------------------
class DTable
{
	private: 
		// for sequential data access on disk
		//
		fstream *infile_;

		unsigned example_;

		int records_;
		int rvars_;
		int dvars_;

	public:
    DTable(string DataFile);
		~DTable();

		void reset_file_ptr(void);
		void seek_position(int pos);

		void get_next_object(vector<real> &R, vector<integer> &D);
		int  get_object(int pos, vector<real> &R, vector<integer> &D);
		int  get_object(int pos, int &ID, vector<real> &R, vector<integer> &D);
		int  get_object(int pos, int &ID);

		int num_records(void) {return records_;};
		int num_rvars(void) {return rvars_;};
		int num_dvars(void) {return dvars_;};

};


//--------------------------------------------------------------------
// Weights
//--------------------------------------------------------------------
struct Weights
{
	vector<string> Rname_;
	vector<string> Dname_;
	vector<double> Rw_;
	vector<double> Dw_;

	Weights(int R, int D, string file);
	void print(void);
};


#endif // ends __DATABASE_H

