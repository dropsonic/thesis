//--------------------------------------------------------------------
// file database.h
//
// Stephen D. Bay
// COPYRIGHT 2003
//
// Support for the UCI file format.
//
//--------------------------------------------------------------------
#ifndef __DATABASE_H
#define __DATABASE_H


#include <iostream>
#include <fstream>
#include <strstream>
#include <string>
#include <vector>
#include <algorithm>
#include <assert.h>

using namespace std;

class DataTable;


//--------------------------------------------------------------------
// Constant data type
//--------------------------------------------------------------------
const int DISCRETE = 0;
const int DISCRETE_C = 1; // values compiled from data
const int CONTINUOUS = 2;
const int IGNORE_FEATURE = 3;
 
const float MISSING_C = -989898;
const int MISSING_D = -1;


//--------------------------------------------------------------------
// Struct Field 
//--------------------------------------------------------------------
struct Field
{
	string name_;
	int type_;
	vector<string> values_;

  Field(vector<string> &S);
  void print(void);
	void print_short(void);
};



//--------------------------------------------------------------------
// Struct Record 
//--------------------------------------------------------------------
struct Record
{
	int index_;
	vector<float> real_;
	vector<int> discrete_;

	void print(DataTable &D, float missing_r, int missing_d);
	void print(void);
};




//--------------------------------------------------------------------
// class DataTable
//--------------------------------------------------------------------

class DataTable
{
	private:
		vector<Field> fields_; 
		//vector<Record> records_;

		// for sequential data access to data files on disk
		//
		ifstream *infile_;
		unsigned example_;

		string data_file_;
		string field_file_;

		float missing_r_;
		int missing_d_;

	public:

		DataTable(string data, string fields, float missing_r, int missing_d);
		~DataTable();

		void load_fields(string FieldFile);

		//int num_records(void) {return records_.size(); };
		int num_fields(void) {return fields_.size(); };

		//--------------------
		// field functions
		//
		unsigned get_field_index(string name);
		string   get_field_string(unsigned index);
		unsigned get_field_type(string name);
		unsigned get_field_type(unsigned index);
	
		string get_field_value_string(unsigned field, unsigned value);
		unsigned get_field_value_index(unsigned field, string value, bool &valid);

		unsigned add_field_value(unsigned field, string value);

		vector<int> get_fields(int type);

		int num_real(void);
		int num_discrete(void);


		//--------------------
		// print functions
		//
		void print_fields(bool long_form);
		void print_records(int num);
	

		//-------------------------------
		// sequential data access on disk
		//
		void reset_file_counter(void);

		int get_next_record(Record &R, bool &valid);

		bool load_record(vector<string> tokens, Record &R,int lineno);


		//-------------------------------
		// convert DB to binary
		//
		int convert_to_binary(string file);
		void write_weight_file(string file);
};



//--------------------------------------------------------------------
// Helper functions
//--------------------------------------------------------------------
vector<string> tokenize(string line,vector<string> delimters);



#endif // ends __DATABASE_H
