//--------------------------------------------------------------------
// file database.cpp
//
// Stephen D. Bay
// COPYRIGHT 2003
//--------------------------------------------------------------------

#include <iostream>
#include <fstream>
#include <strstream>
#include <string>
#include <list>
#include <vector>
#include <algorithm>
#include <stdio.h>
#include <assert.h>

#include "database.h"
#include "misc.h"


//--------------------------------------------------------------------
// MTable
//--------------------------------------------------------------------
MTable::MTable(string file, int start_batch_size, int batch_size)
{

	infile_ = new fstream(file.c_str(), ios::in | ios::binary);
	if (!infile_) {
		cerr << "error: unable to open input file: " << file << endl;
		exit(0);
	}

	// read header information
	infile_->read((char *) &records_,sizeof(int));
	infile_->read((char *) &rvars_,sizeof(int));
	infile_->read((char *) &dvars_,sizeof(int));

	record_index_ = 0;
	start_batch_size_ = start_batch_size;
	batch_size_ = batch_size;
	offset_ = 0;
	last_offset_ = start_batch_size;

	int nr = min(records_,start_batch_size_);

	for (int i=0;i<nr;i++) {
		vector<real> rr(rvars_,0);
		vector<integer> dd(dvars_,0);
		int id;

		infile_->read((char *) &id, sizeof(int));
		infile_->read((char *) &rr[0], sizeof(real)*rvars_);
		infile_->read((char *) &dd[0], sizeof(integer)*dvars_);

		ID_.push_back(id);
		R_.push_back(rr);
		D_.push_back(dd);
	}

	record_index_ += nr;
	nr_ = nr;
};

//--------------------------------------------------------------------
// ~MTable
//--------------------------------------------------------------------
MTable::~MTable()
{
	infile_->close();
	delete infile_;
};


//--------------------------------------------------------------------
// get_next_batch
//
// Loads the next block of records from data into memory.
//--------------------------------------------------------------------
bool MTable::get_next_batch(void)
{

	int nr = min(records_ - record_index_,batch_size_);

	// erase old records
	ID_.erase(ID_.begin(),ID_.end());
	R_.erase(R_.begin(),R_.end());
	D_.erase(D_.begin(),D_.end());


	vector<real> rr(rvars_,0);
	vector<integer> dd(dvars_,0);
	for (int i=0;i<nr;i++) {
		int id;
		infile_->read((char *) &id, sizeof(int));
		infile_->read((char *) &rr[0], sizeof(real)*rvars_);
		infile_->read((char *) &dd[0], sizeof(integer)*dvars_);

		ID_.push_back(id);
		R_.push_back(rr);
		D_.push_back(dd);
	}

	record_index_ += nr;
	nr_ = nr;
	//offset_ += nr;
	offset_ += last_offset_;
	last_offset_ = nr;

	if (nr > 0) { 
		return true;
	}
	else {
		return false;
	}

};


//--------------------------------------------------------------------
// DTable
//--------------------------------------------------------------------
DTable::DTable(string file)
{
	infile_ = new fstream(file.c_str(), ios::in | ios::binary);
	if (!infile_) {
		cerr << "error: unable to open input file: " << file << endl;
		exit(0);
	}

	// read header information
	infile_->read((char *) &records_,sizeof(int));
	infile_->read((char *) &rvars_,sizeof(int));
	infile_->read((char *) &dvars_,sizeof(int));

};

//--------------------------------------------------------------------
// ~DTable
//--------------------------------------------------------------------
DTable::~DTable()
{
	infile_->close();
	delete infile_;
};


//--------------------------------------------------------------------
// reset_file_ptr
//
// Resets the file pointer to the location where the data starts in
// the binary file (i.e., after the header information).
//
// Note the first 12 bytes of the binary file are devoted to header
// information containing the number of records, number of real 
// variables and the number of discrete variables.
//--------------------------------------------------------------------
void DTable::reset_file_ptr(void)
{
	infile_->seekg(12);

};


//--------------------------------------------------------------------
// seek_position
//
// Move the iostream file pointer to the record at pos (0 based 
// index).
//--------------------------------------------------------------------
void DTable::seek_position(int pos)
{
	// check if position valid
	assert(pos >= 0);
	assert(pos < records_);

	// change int filepos to long filepos
	long filepos = 12 + pos * sizeof(int) + pos * rvars_ * sizeof(real) + pos * dvars_ * sizeof(integer);
	infile_->seekg(filepos);
};


//--------------------------------------------------------------------
// get_next_object
//--------------------------------------------------------------------
void DTable::get_next_object(vector<real> &R, vector<integer> &D) 
{
	int id;
	infile_->read((char *) &id, sizeof(int));
	infile_->read((char *) &R[0], sizeof(real)*rvars_);
	infile_->read((char *) &D[0], sizeof(integer)*dvars_);
};


//--------------------------------------------------------------------
// get_object
//--------------------------------------------------------------------
int  DTable::get_object(int pos, vector<real> &R, vector<integer> &D)
{
	seek_position(pos);
	
	int id;
	infile_->read((char *) &id,sizeof(int));
	infile_->read((char *) &R[0], sizeof(real)*rvars_);
	infile_->read((char *) &D[0], sizeof(integer)*dvars_);

	return 0;
};


//--------------------------------------------------------------------
// get_object
//--------------------------------------------------------------------
int  DTable::get_object(int pos, int &ID, vector<real> &R, vector<integer> &D)
{
	seek_position(pos);
	
	infile_->read((char *) &ID,sizeof(int));
	infile_->read((char *) &R[0], sizeof(real)*rvars_);
	infile_->read((char *) &D[0], sizeof(integer)*dvars_);

	return 0;
};


//--------------------------------------------------------------------
// get_object
//--------------------------------------------------------------------
int  DTable::get_object(int pos, int &ID)
{
	seek_position(pos);
	infile_->read((char *) &ID,sizeof(int));
	return 0;
};




//--------------------------------------------------------------------
// Weights
//--------------------------------------------------------------------
Weights::Weights(int R, int D, string file) 
{
	fstream infile(file.c_str(), ios::in);
	if (!infile) {
		cerr << "error: unable to open input file: " << file << endl;
		exit(0);
	}

	// read header information
	for (int i=0;i<R;i++) {
		string name;
		double weight;
		infile >> name;
		infile >> weight;
		Rname_.push_back(name);
		Rw_.push_back(weight);
	}

	// read header information
	for (int i=0;i<D;i++) {
		string name;
		double weight;
		infile >> name;
		infile >> weight;
		Dname_.push_back(name);
		Dw_.push_back(weight);
	}
};


//--------------------------------------------------------------------
// print
//--------------------------------------------------------------------
void Weights::print(void)
{
	for (int i=0;i<Rw_.size();i++) {
		spaces(2);
		cout << Rname_[i] << ": " << Rw_[i] << endl;
	}
	for (int i=0;i<Dw_.size();i++) {
		spaces(2);
		cout << Dname_[i] << ": " << Dw_[i] << endl;
	}
};


