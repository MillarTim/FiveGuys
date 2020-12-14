package fileparser

import (
	"bufio"
	"log"
	"os"
	"regexp"
	"strconv"
	"strings"
)

// Record including type and data
type Record struct {
	RecordNumber         int
	RecordHierarchyLevel int
	RecordType           string
	Data                 string
	FieldMap             map[string]string // field key/value (name/value) pair
}

// RecordHierarchyEntry describing how the records relate to each other; Key is Record Type
type RecordHierarchyEntry struct {
	RecordType string
	Level      int
	isHeader   bool
	IsBlock    bool
	FieldRegex string
}

// FieldFormat describes a fields type and how to format; Key is RecTyp and Field Name
type FieldFormat struct {
	FieldName string // record type + '~' + fieldname from field regex
	Regex     string
	Replace   string
	FieldType string
	Format    string
}

var (
	_currentRecordNumber int

	_recordsInBlock []Record

	_recordHierarchy = make(map[string]RecordHierarchyEntry)

	_fieldFormatting = make(map[string]FieldFormat)

	_recordTypeRegexes = []string{}

	_blockProcessingFunction = func([]Record) {}
)

// ParseFile will read a file and parse records into blocks and fields out of records based on:
// FieldFormatting.txt, RecordHierarchy.txt, RecordTypeRegexes.txt in the caller's folder
func ParseFile(file string, blockProcessingFunction func([]Record)) {
	_blockProcessingFunction = blockProcessingFunction
	initialize()
	parseFile(file, ProcessRecord)
	_blockProcessingFunction(_recordsInBlock) // at eof, process the last block
}

func parseFile(file string, processRecord func(string)) {
	/*
		fptr := flag.String("fpath", file, "file path to read from")
		flag.Parse()
	*/
	//	f, err := os.Open(*fptr)
	f, err := os.Open(file)
	if err != nil {
		log.Fatal(err)
	}
	defer func() {
		if err = f.Close(); err != nil {
			log.Fatal(err)
		}
	}()
	s := bufio.NewScanner(f)
	for s.Scan() {
		_currentRecordNumber++
		processRecord(s.Text())
	}
	err = s.Err()
	if err != nil {
		log.Fatal(err)
	}
}

// ProcessRecord blah
func ProcessRecord(record string) {
	for _, v := range _recordTypeRegexes {
		recTypRegex := regexp.MustCompile(v)
		groups := recTypRegex.FindStringSubmatch(record) // look for record type of record
		if groups == nil {
			continue // no match, go to next expression
		} else {
			var recordTypeOfRecordJustRead string
			for i, v := range groups { // concatenate all captured groups to determine record type
				if i != 0 {
					recordTypeOfRecordJustRead += v
				}
			}
			// If record just-read defines a block (or eof) & the last record in block is not a header, process previous block
			recordTypeOfLastRecordInBlock := RecordTypeOfLastRecordInBlock()
			if recordTypeOfLastRecordInBlock != "" &&
				_recordHierarchy[recordTypeOfRecordJustRead].IsBlock &&
				!_recordHierarchy[recordTypeOfLastRecordInBlock].isHeader {
				_blockProcessingFunction(_recordsInBlock)

				// Remove previous block of detail records already processed
				for i := len(_recordsInBlock); i >= 0; i-- {
					recordTypeOfLastRecordInBlock = RecordTypeOfLastRecordInBlock()
					if recordTypeOfLastRecordInBlock != "" &&
						_recordHierarchy[recordTypeOfLastRecordInBlock].Level >= _recordHierarchy[recordTypeOfRecordJustRead].Level {
						_recordsInBlock = _recordsInBlock[:i-1]
					} else {
						break
					}
				}
			}
			fields := ExtractFieldsFromRecord(recordTypeOfRecordJustRead, record)
			hierarchyOfCurrentRecord := _recordHierarchy[recordTypeOfRecordJustRead].Level
			//  Add record just read
			_recordsInBlock = append(_recordsInBlock, Record{_currentRecordNumber, hierarchyOfCurrentRecord, recordTypeOfRecordJustRead, record, fields}) // accumulate records in current block
			return
		}
		// if record type not found, ignore the record
	}
}

// ExtractFieldsFromRecord blah
func ExtractFieldsFromRecord(recordType string, record string) map[string]string {
	fieldMap := make(map[string]string)
	fieldsRegex := regexp.MustCompile(_recordHierarchy[recordType].FieldRegex)
	groups := fieldsRegex.FindStringSubmatch(record) // extract field values
	for i, v := range groups {
		if i != 0 {
			fieldName := fieldsRegex.SubexpNames()[i]
			fieldValue := FormatField(recordType, fieldName, v)
			fieldMap[fieldName] = fieldValue
		}
	}
	return fieldMap
}

// FormatField blah
func FormatField(recordType string, fieldName string, fieldValue string) string {
	fieldFormatting := _fieldFormatting[recordType+"~"+fieldName]
	if fieldFormatting.FieldName == "" {
		return fieldValue // no formatting directives exist, so return orig string
	}
	formatRegex := regexp.MustCompile(fieldFormatting.Regex)
	formattedValue := formatRegex.ReplaceAllString(fieldValue, fieldFormatting.Replace) // extract field values
	return formattedValue
}

// RecordTypeOfLastRecordInBlock blah
func RecordTypeOfLastRecordInBlock() string {
	if len(_recordsInBlock) == 0 {
		return ""
	}
	return _recordsInBlock[len(_recordsInBlock)-1].RecordType
}

func initialize() {
	parseFile("RecordHierarchy.txt", loadHierarchyRecord)
	parseFile("RecordTypeRegexes.txt", loadRecordTypeRegexes)
	parseFile("FieldFormatting.txt", loadFieldFormatting)
}

func loadHierarchyRecord(record string) {
	fields := strings.Split(record, "¤")
	level, _ := strconv.Atoi(fields[1])
	isHeader, _ := strconv.ParseBool(fields[2])
	isBlock, _ := strconv.ParseBool(fields[3])
	//	_recordHierarchy = append(_recordHierarchy, RecordHierarchyEntry{fields[0], fields[1], fields[2], fields[3], fields[4]})
	_recordHierarchy[fields[0]] = RecordHierarchyEntry{fields[0], level, isHeader, isBlock, fields[4]}
}

func loadRecordTypeRegexes(record string) {
	_recordTypeRegexes = append(_recordTypeRegexes, record)
}

func loadFieldFormatting(record string) {
	fields := strings.Split(record, "¤")
	_fieldFormatting[fields[0]] = FieldFormat{fields[0], fields[1], fields[2], fields[3], fields[4]}
}
