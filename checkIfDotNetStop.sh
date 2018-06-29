while true
do
	_pid="$(pgrep -f dot)"
	if [ -z "$_pid" ]
	then
 	  echo "|$_pid| is not running"
	  echo "killing all ffmpeg processes"
	  pkill -f ffmpeg
	  pkill -f checkIfDotNetStop
	  break
  	 # Do something knowing the pid exists, i.e. the process with $PID is running
	  else
	  echo "Running"
	fi
	sleep 10
done
