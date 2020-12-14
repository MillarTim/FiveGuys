package main

import (
	"fileparser"
	"fmt"
)

func main() {
	fileparser.ParseFile(`Products.txt`, recordBlockProcessor)
}

func recordBlockProcessor(records []fileparser.Record) {
	for _, record := range records {
		if record.RecordType == "H01" {
			// How to access an individual field:
			fileDate := record.FieldMap["FilDat"] // Get field names from RecordHierarchy "resource" file
			fmt.Println(fileDate)
		}
		if record.RecordType == "D01" {
			fmt.Println("Record type: ", record.RecordType, "Physical file record number: ", record.RecordNumber)
			// How to access all fields within a record:
			for k, v := range record.FieldMap {
				fmt.Println(k + ": " + v)
			}
		}
	}
}
