#!/bin/bash
mkdir -p /var/ldt/log/
LOG_FULL_NAME="/var/ldt/log/zabbix.log" #ajust log name acrodently
LDT_VERBOSE="false" # when false only PrintAndLog print messages else set to true for all echo to be seen
#LDT_MASTER_PATH="" #set to use a diffrent ldtMaster path

ldtMaster_url="https://linuxinfra.wdf.sap.corp/$LDT_MASTER_PATH/ldtMaster.sh"
echo "Sourcing ldtMaster.sh, Please wait..."
source /dev/stdin <<< "$(curl -k -s -x '' $ldtMaster_url)"
if  ! InitialRunArguments ;then
	echo "Sourcing remote ldtMaster...Failed!"
	echo "Script Exist!"
	echo "For any issues please contact asaf.magen@sap.com"
	exit 14
fi


InstallGit
InstallChef 12.14

[[ -z $REALM_BRANCH ]] && REALM_BRANCH=master
mkdir -p /var/chef/cookbooks

cd /var/chef/cookbooks
[[ -d /var/chef/cookbooks/zabbix_agent ]] && rm -rf /var/chef/cookbooks/zabbix_agent
if ! git clone https://github.wdf.sap.corp/i028498/zabbix_agent -b $REALM_BRANCH ; then
	echo "LDT_ERROR: faild to clone git repo."
	exit 1
fi

chef-solo -o 'recipe[zabbix_agent]'
exit $?




#case ${OSNAME} in 
# SLES)
#	SRV="zabbix-agentd"
# ;;
# *)
# SRV="zabbix-agent"
# ;;
#esac
#
#count=20
#
#echo "Checking Zabbix-agentd service status ($count retries): "
#for i in $(seq 1 $count)
#do
#  sleep 1
#  service $SRV status && exit 0
#  printf '.'
#done
#echo
#
#exit 1

