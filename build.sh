#! /bin/sh

splat=$*
root=$(pwd)
nant="$root/lib/Nant/nant.exe"
settings="$root/Settings/UppercuT.config"

usage() {
	echo
	echo Usage: build.sh
	echo
}

go() {
	$nant -logger:NAnt.Core.DefaultLogger -quiet -f:build\\default.build -D:build.config.settings=$settings $splat
	exit $?
}

if [ "$1" == "/?" ]; then usage;
elif [ "$1" == "-?" ]; then usage;
elif [ "$1" == "?" ]; then usage;
elif [ "$1" == "/help" ]; then usage;
elif [ "$1" == "--help" ]; then usage;
elif [ "$1" == "-h" ]; then usage;
else go;
fi
