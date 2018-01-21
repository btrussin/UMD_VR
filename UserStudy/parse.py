#requires pyhon2

import csv, os, sys

#python parser.py <data file>

	# 0     1     2       3     4     5     6     7     8     9     10    11    12    13   14   15   16   17   18   19   20   21   22    23
record = ['PID','VIZ','LINKS','T1A','T1T','T2A','T2T','T3A','T3T','T4A','T4T','T5A','T5T','Q1','Q2','Q3','Q4','Q5','Q6','Q7','Q8','Q9','Q10','TS']

i = 0

#print(record_header)

with open(sys.argv[1], 'rb') as myfile:
	filereader = csv.reader(myfile)
	for row in filereader:

		# Participant ID
		if 'PARTICIPANT' in row[0]:
			# Print the previous participant's record and reset for next participant
			#print(record)
			for x in range(23):
				print record[x], '\t',

				if x is 22:
					print '\n',

			# Start new participant
			record[0] = row[0][11:]
		
		# Condition
		if 'Sphere' in row[0]: record[1] = 'Sphere'

		elif 'Node' in row[0]: record[1] = 'Node'

		if 'true' in row[0]: record[2] = 'Links'	

		elif 'false' in row[0]:	record[2] = 'NoLinks'

		# Task
		if 'QNumT:0' in row[0]:
                    record[3] = row[0][20:]
                    i = 3

		elif 'QNumT:1' in row[0]:
                    record[5] = row[0][20:].replace('Movie: ', '')
		    i = 5

		elif 'QNumT:2' in row[0]:
		    # parse
		    record[7] = row[0][20:].replace('Movie: ', '', 1).replace('Movie: ', ', ')
		    i = 7

		elif 'QNumT:3' in row[0]:
		    record[9] = row[0][20:].replace('Movie: ', '')
		    i = 9

		elif 'QNumT:4' in row[0]:
		    # parse
		    record[11] = row[0][20:].replace('Movie: ', '', 1).replace('Movie: ', ', ')
		    i = 11

		# Time
		if 'Time Elapsed' in row[0]:
		    if i == 3: record[4] = row[1]
			
		    elif i == 5: record[6] = row[1]

		    elif i == 7: record[8] = row[1]

		    elif i == 9: record[10] = row[1]

		    elif i == 11: record[12] = row[1]
		
		# Survey
		if 'QNumS:0' in row[0]: record[13] = row[0][20:]

                elif 'QNumS:1' in row[0]: record[14] = row[0][20:]

                elif 'QNumS:2' in row[0]: record[15] = row[0][20:]

                elif 'QNumS:3' in row[0]: record[16] = row[0][20:]

                elif 'QNumS:4' in row[0]: record[17] = row[0][20:]

                elif 'QNumS:5' in row[0]: record[18] = row[0][20:]

                elif 'QNumS:6' in row[0]: record[19] = row[0][20:]

                elif 'QNumS:7' in row[0]: record[20] = row[0][20:]

                elif 'QNumS:8' in row[0]: record[21] = row[0][20:]

                elif 'QNumS:9' in row[0]: record[22] = row[0][20:]
	
		# Timestamp
		if '2017' in row[0]: record[23] = row[0]
