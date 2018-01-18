#!/bin/bash
RPATH=$(df -h /mnt | grep / | awk {'print $1'})
HOST_UUID=$(/usr/sbin/dmidecode --string system-uuid  2>/dev/null | egrep -io "^[0-9A-F]{8}-[0-9A-F]{4}-[1-4][0-9A-F]{3}-[89AB][0-9A-F]{3}-[0-9A-F]{12}$" || $(which dmidecode 2>/dev/null) --string system-uuid  2>/dev/null |egrep -io "^[0-9A-F]{8}-[0-9A-F]{4}-[1-4][0-9A-F]{3}-[89AB][0-9A-F]{3}-[0-9A-F]{12}$" || cat /sys/devices/virtual/dmi/id/product_uuid 2>/dev/null) 
TIMESET=$(/sbin/tune2fs -l $RPATH | grep "Filesystem created"|sed 's/.*created://;s/[ \t]*//')
TIMESETNUMBER=$(date -d "$TIMESET" "+%s")
HASH=$(echo ${TIMESETNUMBER}${HOST_UUID} | sha256sum |awk '{print $1}')
echo $HASH
echo $HOST_UUID