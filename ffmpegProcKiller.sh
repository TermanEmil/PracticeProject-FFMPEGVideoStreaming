#!/bin/bash

while true
do
	_pid="$(pgrep -f dot)"
	if [ -z "$_pid" ]
	then
		if [[ $* != *-s* ]]
		then
			echo "killing all ffmpeg processes"
		fi
		
		pkill -TERM ffmpeg 2>&-

		if [[ $* != *--continuous* ]]
		then
			break
		fi
	else
		if [[ $* != *-s* ]]
		then
			echo "Running" $_pid
		fi
	fi
	sleep 1
done
