#!/bin/bash

# output the name of script ( http://stackoverflow.com/questions/192319/how-do-i-know-the-script-file-name-in-a-bash-script )
echo `basename "$0"`
# echo version of debian http://linuxconfig.org/check-what-debian-version-you-are-running-on-your-linux-system
echo `cat /etc/issue`
