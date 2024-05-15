#!/bin/bash
# .Net Core bootstrap script

# Move to script's directory
cd "`dirname "$0"`"

# Get the system architecture
UNAME=`uname`

# Setup LD path to the script current path
if [ "$UNAME" == "Darwin" ]; then
	# macOS
	export DYLD_LIBRARY_PATH=$DYLD_LIBRARY_PATH:./

	if [ "$STEAM_DYLD_INSERT_LIBRARIES" != "" ] && [ "$DYLD_INSERT_LIBRARIES" == "" ]; then
		export DYLD_INSERT_LIBRARIES="$STEAM_DYLD_INSERT_LIBRARIES"
	fi
else
	# Linux
	LD_LIBRARY_PATH=./
	export LD_LIBRARY_PATH
fi

./GreedyKid $@
