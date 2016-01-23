#!/bin/bash

# output the name of script ( http://stackoverflow.com/questions/192319/how-do-i-know-the-script-file-name-in-a-bash-script )
echo `basename "$0"`
echo Looks like this step is not executed on free account (may be because it uses S3 storage and it is paid only?)
