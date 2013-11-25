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
#include <vector>
#include <algorithm>

#include <stdio.h>

#include "database.h"
#include "misc.h"



//--------------------------------------------------------------------
// Field::Field
//
// This function takes a string representing a valid line in an
// field file and converts it into an field definition.
//--------------------------------------------------------------------
Field::Field(vector<string> &S) 
{
  assert(S.size() >= 1);
  name_ = S[0];

  if (S.size() == 1) { 
    type_ = IGNORE_FEATURE;
  }

  if (S.size() > 1) {
		if (S[1] == "ignore") {
			type_ = IGNORE_FEATURE;
		}
    // continuous
		else if (S[1] == "continuous") {
			type_ = CONTINUOUS;
		}
    // string 
		else if (S[1] == "discrete") {
			type_ = DISCRETE_C;
		}
    // categorical
    else {
      type_ = DISCRETE;
      for (int i=1;i<S.size();i++) {
        values_.push_back(S[i]);
      }
    }
  }
};




//--------------------------------------------------------------------
// Field::print
//--------------------------------------------------------------------
void Field::print(void)
{
	cout << name_ << ": ";

	if (type_ == CONTINUOUS) {
		cout << "continuous." << endl;
	}
	else if ((type_ == DISCRETE) || ((type_ == DISCRETE_C) && (values_.size() > 0))) {
		cout << endl;
    for (unsigned i=0;i<values_.size();i++) {
      cout << "        " << values_[i];
			if (i < values_.size()-1) 
				cout << ", " << endl;
    }
		cout << "." << endl;
  }

	else if (type_ == DISCRETE_C) {
		cout << "discrete." << endl;
	}
	else if (type_ == IGNORE_FEATURE) {
		cout << "ignore." << endl;
	}
};



//--------------------------------------------------------------------
// Field::print_short
//--------------------------------------------------------------------
void Field::print_short(void)
{
	cout << name_ << ": ";

	if (type_ == CONTINUOUS) {
		cout << "continuous." << endl;
	}
	else if ((type_ == DISCRETE) || (type_ == DISCRETE_C)) {
		cout << "discrete." << endl;
	}
	else if (type_ == IGNORE_FEATURE) {
		cout << "ignore." << endl;
	}
};







//--------------------------------------------------------------------
// DataTable
//--------------------------------------------------------------------
DataTable::DataTable(string data, string fields, float missing_r, int missing_d)
{
	data_file_ = data;
	field_file_ = fields;
	missing_r_ = missing_r;
	missing_d_ = missing_d;

	load_fields(field_file_);
	
	infile_ = new ifstream(data_file_.c_str());
	example_ = 0;
  if ( ! infile_) {
    cerr << "error: unable to open input file: " << data_file_ << endl;
    exit(-1);
  } 
};


//--------------------------------------------------------------------
// DataTable
//--------------------------------------------------------------------
DataTable::~DataTable()
{
	infile_->close();
	delete infile_;
};




//--------------------------------------------------------------------
// DataTable::load_fields
//--------------------------------------------------------------------
void DataTable::load_fields(string file)
{
  ifstream infile(file.c_str());
  if ( ! infile) {
    cerr << "error: unable to open input file: " << file << endl;
    exit(-1);
  } 

  string buffer;

	vector<string> delimiters;
	delimiters.push_back(".");
	delimiters.push_back(",");
	delimiters.push_back(":");
	delimiters.push_back(";");

  while (getline(infile,buffer)) {

		vector<string> tokens = tokenize(buffer,delimiters);

    // if we have a valid line in our attribute file
    if (tokens.size() > 0) {
      Field new_field(tokens);
      fields_.push_back(new_field);
    }
  }
};



//--------------------------------------------------------------------
// DataTable::print_fields
//--------------------------------------------------------------------
void DataTable::print_fields(bool long_form)
{
	if (long_form) {
		for (int i=0;i<fields_.size();i++) {
			fields_[i].print();
		}
	}
	else {
		for (int i=0;i<fields_.size();i++) {
			cout << "  ";
			fields_[i].print_short();
		}
	}
};



//--------------------------------------------------------------------
// print_records
//--------------------------------------------------------------------
void DataTable::print_records(int num) 
{
	assert(num > 0);
	
	reset_file_counter();

	Record R;

	int status = 1;
//	int count = 0;
	while (status) {
		Record R;
		bool valid;
		status = get_next_record(R,valid);
		if ((status == 1) && (valid== true)) {
 			R.print();
			cout << endl;
		};
	};

	
}





//--------------------------------------------------------------------
// get_field_index
//--------------------------------------------------------------------
unsigned DataTable::get_field_index(string name)
{
	for (unsigned i=0;i<fields_.size();i++) {
		if (fields_[i].name_ == name) {
			return i;
		}
	}

	cerr << "Error: unknown field" << name << endl;
	exit(0);
	return 0;
};


//--------------------------------------------------------------------
// get_field_string
//--------------------------------------------------------------------
string DataTable::get_field_string(unsigned index)
{
	//assert(index >= 0);
	assert(index < fields_.size());
	return fields_[index].name_;
};


//--------------------------------------------------------------------
// get_field_type 
//--------------------------------------------------------------------
unsigned DataTable::get_field_type(string name)
{
	unsigned index = get_field_index(name);
	return fields_[index].type_;
};

//--------------------------------------------------------------------
// get_field_type
//--------------------------------------------------------------------
unsigned DataTable::get_field_type(unsigned index)
{
	assert(index < fields_.size());
	return fields_[index].type_;				
};


//--------------------------------------------------------------------
// get_field_value_string
//
//--------------------------------------------------------------------
string DataTable::get_field_value_string(unsigned field, unsigned value)
{
	assert(field < fields_.size());
	assert(value < fields_[field].values_.size());

	return fields_[field].values_[value];
				
};

//--------------------------------------------------------------------
// get_field_value_index
//
// Returns the index for value in field.
//--------------------------------------------------------------------
unsigned DataTable::get_field_value_index(unsigned field, string value, bool &valid)
{
	assert(field < fields_.size());

	vector<string> &V = fields_[field].values_;
	for (unsigned i=0;i<V.size();i++) {
		if (value == V[i]) {
			valid = true;
			return i;
		}
	}

	valid = false;
	return(0);
};



//--------------------------------------------------------------------
// add_field_value
//
// Adds a new value to the field description.
//--------------------------------------------------------------------
unsigned DataTable::add_field_value(unsigned field, string value)
{
	// check to make sure value is not already there
	// check to make sure field is one that we can add a value to

	Field &F = fields_[field];
	F.values_.push_back(value);
	return (F.values_.size() -1);
};



//--------------------------------------------------------------------
// get_fields
//--------------------------------------------------------------------
vector<int> DataTable::get_fields(int type)
{
	vector<int> fields;
	for (int i=0;i<fields_.size();i++) {
		if (fields_[i].type_ == type) {
			fields.push_back(i);
		}
	}
	return(fields);
};



//--------------------------------------------------------------------
// print
//--------------------------------------------------------------------
void Record::print(DataTable &D, float missing_r, int missing_d)
{
	unsigned rptr = 0;
	unsigned dptr = 0;

	unsigned n = D.num_fields();
	for (unsigned i=0;i<n;i++) {
		int type = D.get_field_type(i);
		if (type == CONTINUOUS) {
			if (real_[rptr] != missing_r)	
				cout << real_[rptr++];
			else
				cout << "?";
		}
		else if ((type == DISCRETE) || (type == DISCRETE_C)) {
			if (discrete_[dptr] != missing_d) 
				cout << D.get_field_value_string(i,((int) discrete_[dptr++]));
			else
				cout << "?";
		}

		if (i < n-1) {
			cout << ",";
		}
	}
	cout << endl;
};


//--------------------------------------------------------------------
// print
//--------------------------------------------------------------------
void Record::print(void)
{
	printvector(real_);
	printvector(discrete_);
	cout << endl;

};



//--------------------------------------------------------------------
// reset_file_counter
//--------------------------------------------------------------------
void DataTable::reset_file_counter(void)
{
	infile_->clear();
	infile_->seekg(0, ios::beg);
};




//--------------------------------------------------------------------
// get_next_record
//
// valid -- 1 if the record was correctly loaded
//       -- 0 if if the record had errors and was ignored
//
// returns 1 if able to retrieve the next record
//         0 if unable to get the next record
//--------------------------------------------------------------------
int DataTable::get_next_record(Record &R,bool &valid)
{
  // load in data one line at a time
  string buffer;
	vector<string> delimiters;

  if (getline(*infile_,buffer)) {
		vector<string> tokens = tokenize(buffer,delimiters);
		
		valid = load_record(tokens,R,example_);

		// update the example number
		example_++;
		return 1;
  }
	else {
		return 0;
	}
}; 


//--------------------------------------------------------------------
// load_record 
//--------------------------------------------------------------------
bool DataTable::load_record(vector<string> tokens, Record &R, int lineno)
{
  // get information on field indexes
	vector<int> real = get_fields(CONTINUOUS);
	vector<int> discrete = get_fields(DISCRETE);
	vector<int> discrete_data = get_fields(DISCRETE_C);

	// check to make sure there are the correct number of tokens
	// if there are an incorrect number ignore the line
	if (tokens.size() != fields_.size()) {
		cerr << "Error in data file on line " << lineno << ":";
		cerr << "  incorrect number of fields, line ignored." << endl;
		// skip to next iteration of loop
		return false;
	}

	// get Continuous fields
	for (int i=0;i<real.size();i++) {
		int index = real[i];
		if (tokens[index] == "?") {
			R.real_.push_back(missing_r_);
		}
		else {
			float value = atof(tokens[index].c_str());
			R.real_.push_back(value);
		}
	}

	// get Discrete fields
	for (int i=0;i<discrete.size();i++) {
		int index = discrete[i];
		if (tokens[index] == "?") {
			R.discrete_.push_back(missing_d_);
		}
		else {
			bool valid;
     	unsigned value = get_field_value_index(index,tokens[index],valid);
			R.discrete_.push_back(value);
			if (valid == false) {
				cerr << "Error in data file on line " << lineno << ": ";
				cerr << "  unknown value " << tokens[index] << " in field " << get_field_string(index) << ","; 
				cerr << " line ignored." << endl;
				return false;
			}
		}
	}
		
	// get Discrete Data driven
	for (int i=0;i<discrete_data.size();i++) {
		int index = discrete_data[i];
		if (tokens[index] == "?") {
			R.discrete_.push_back(missing_d_);
		}
		else {
			bool valid;
   	 	unsigned value = get_field_value_index(index,tokens[index],valid);
			if (valid == false) {
   	 		value = add_field_value(index,tokens[index]);
			}
			R.discrete_.push_back(value);
		}
	}
	return true;
};



//--------------------------------------------------------------------
// num_real
//
// Returns the number of real fields.
//--------------------------------------------------------------------
int DataTable::num_real(void)
{
	int num=0;
	for (int i=0;i<fields_.size();i++) {
		if (fields_[i].type_ == CONTINUOUS) {
			num++;
		}
	}
	return num;
};

//--------------------------------------------------------------------
// num_discrete
//
// Returns the number of discrete fields.
//--------------------------------------------------------------------
int DataTable::num_discrete(void)
{
	int num=0;
	for (int i=0;i<fields_.size();i++) {
		if ((fields_[i].type_ == DISCRETE) || (fields_[i].type_ == DISCRETE_C)) {
			num++;
		}
	}
	return num;
};

//--------------------------------------------------------------------
// convert_to_binary
//
// Converts the data set to a binary file.
//
// returns the number of converted records.
//--------------------------------------------------------------------
int DataTable::convert_to_binary(string file)
{
	reset_file_counter();

	ofstream outfile (file.c_str(),ios::out|ios::binary);
	if ( ! outfile ) {
		cerr << "Error: cannot open file " << file << endl;
		exit(0);
	}

	int status = 1;

	int num_records = 0;
	int num_r = num_real();
	int num_d = num_discrete();

	
	//-------------------------------
	// just allocating space for the
	// header information 
	outfile.write((const char *) &num_records,sizeof(int));
	outfile.write((const char *) &num_r,sizeof(int));
	outfile.write((const char *) &num_d,sizeof(int));
	
	//----------------------
	// write the example to the file
	//
	int record_number = 1;
	while (status) {
		Record R;
		bool valid;
		status = get_next_record(R,valid);
		if ((status == 1) && (valid==true)) {
			// write index number
			outfile.write((const char *) &record_number,sizeof(int));
			if (num_r > 0) {
				outfile.write((const char *) &R.real_[0], sizeof(float) * R.real_.size());
			}
			if (num_d > 0) {
				outfile.write((const char *) &R.discrete_[0], sizeof (int) * R.discrete_.size());
			}
			num_records++;
			record_number++;
		};
	};
	
	//-----------------------------
	// go back to the beginning and 
	// write header information
	//
	outfile.seekp(0, ios::beg);
	outfile.write((const char *) &num_records,sizeof(int));
	outfile.close();

	return num_records;
};

//--------------------------------------------------------------------
// write_weight_file
//--------------------------------------------------------------------
void DataTable::write_weight_file(string file)
{
  // get information on field indexes
	vector<int> real = get_fields(CONTINUOUS);
	vector<int> discrete = get_fields(DISCRETE);
	vector<int> discrete_data = get_fields(DISCRETE_C);

	// open output file
	ofstream outfile (file.c_str());
	if ( ! outfile ) {
		cerr << "Error: cannot open file " << file << endl;
		exit(0);
	}


	for (int i=0;i<real.size();i++) {
		cout << "  " << get_field_string(real[i]) << endl;
		outfile << get_field_string(real[i]) << " 1.0" << endl;
	}

	for (int i=0;i<discrete.size();i++) {
		cout << "  " << get_field_string(discrete[i]) << endl;
		outfile << get_field_string(discrete[i]) << " 0.4" << endl;
	}

	for (int i=0;i<discrete_data.size();i++) {
		cout << "  " << get_field_string(discrete_data[i]) << endl;
		outfile << get_field_string(discrete_data[i]) << " 0.4" << endl;
	}
				
};

//--------------------------------------------------------------------
// tokens
//
// Converts an input line from a data file (either data, type, or model
// file and strips out delimiters and comments.
//--------------------------------------------------------------------
vector<string> tokenize(string line,vector<string> delimiters)
{
  //--------------------
  // strip comments
  //
  bool comment = false;
  for (unsigned i=0;i<line.size();i++) {
    if (line[i] == '%')
      comment = true;
    if (comment == true)
      line[i] = ' ';
  }


  //--------------------
  // strip delimiters 
  //
  // convert delimiters in string to whitespace
	// remove all ".,:;" from the attribute file
	
	if (delimiters.size() == 0) {
 		replace(line.begin(),line.end(),',',' ');
 		replace(line.begin(),line.end(),':',' ');
 		replace(line.begin(),line.end(),';',' ');
	}
	else {
		for (unsigned i=0;i<delimiters.size();i++) {
			char d = *delimiters[i].c_str();
			replace(line.begin(),line.end(),d,' ');
		}
	}


  //--------------------
  // break string into tokens
  //
  istrstream input(line.c_str());
  vector<string> tokens;

  while (!input.eof()) {
    string word;
    input >> word;

    // check to see if line is empty or filled with blank spaces
    if (word != "")
      tokens.push_back(word);
  }

  return(tokens);
};

