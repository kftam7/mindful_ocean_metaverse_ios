import fileinput
import sys

for line in fileinput.input(sys.argv[1], inplace=1):
    print line.replace(sys.argv[2], sys.argv[3]),

